using AutoMapper;
using IEGen.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.IO;
using DataTables.Mvc;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;
using Microsoft.AspNet.Identity.Owin;
using System.Drawing;

namespace IEGen.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        // GET: Admin
        public ActionResult Index()
        {
            return Schools();
        }


        #region Access Groups

        public ActionResult AccessGroups()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Admin);
            if (!hVM.Has_ManageAccess) return RedirectToAction("Index", "Home");

            var model = new AccessGroupPageViewModel { AccessGroupList = context.AccessGroupList.ProjectToList<AccessGroupViewModel>() };

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _EditAccessGroup(int AccessGroupID)
        {
            var context = new IEContext();
            var model = context.AccessGroupList.Where(m => m.AccessGroupID == AccessGroupID).ProjectToFirst<AccessGroupViewModel>();

            return PartialView(model);
        }

        public ActionResult _EditGroupRoles(int AccessGroupID)
        {
            var model = new IEContext().AccessGroupList.Where(m => m.AccessGroupID == AccessGroupID).ProjectToFirst<EditGroupRolesViewModel>();
            model.AvailableRoles = new List<GroupRoleViewModel>();

            foreach (byte roleID in Enum.GetValues(typeof(UserRoles)))
            {
                if (!model.AssignedRoles.Any(l => l.RoleID == roleID))
                    model.AvailableRoles.Add(new GroupRoleViewModel { RoleID = roleID });
            }

            return PartialView(model);
        }

        public ActionResult _AddRole(int RoleID, int AccessGroupID)
        {
            var context = new IEContext();
            var gr = new AccessGroupRole() { AccessGroupID = (short)AccessGroupID, RoleID = (byte)RoleID };
            context.AccessGroupRoleList.Add(gr);

            var gp = new AccessGroup() { AccessGroupID = gr.AccessGroupID };
            context.AccessGroupList.Attach(gp);
            gp.TimeChanged = DateTime.Now;
            gp.ChangedByID = context.IEUserList.Where(l => l.Email == User.Identity.Name).Select(l => l.UserID).First();

            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert("An error occured while trying to add the role!");
            }
            return Json(new GroupRoleViewModel() { RoleID = gr.RoleID, AccessGroupID = gr.AccessGroupID }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _RemoveRole(int RoleID, int AccessGroupID)
        {
            var context = new IEContext();
            var gr = new AccessGroupRole() { AccessGroupID = (short)AccessGroupID, RoleID = (byte)RoleID };
            context.AccessGroupRoleList.Attach(gr);

            var gp = new AccessGroup() { AccessGroupID = gr.AccessGroupID };
            context.AccessGroupList.Attach(gp);
            gp.TimeChanged = DateTime.Now;
            gp.ChangedByID = context.IEUserList.Where(l => l.Email == User.Identity.Name).Select(l => l.UserID).First();

            var model = new GroupRoleViewModel() { RoleID = gr.RoleID, AccessGroupID = gr.AccessGroupID };
            context.Entry(gr).State = EntityState.Deleted;

            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert("An error occured while trying to add the role!");
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FinishEditRoles(int AccessGroupID)
        {
            return Json(new IEContext().AccessGroupList.Where(m => m.AccessGroupID == AccessGroupID).ProjectToFirst<AccessGroupViewModel>(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteAccessGroup(int AccessGroupID)
        {
            var context = new IEContext();
            var gp = new AccessGroup() { AccessGroupID = (short)AccessGroupID };
            context.AccessGroupList.Attach(gp);
            context.Entry(gp).State = EntityState.Deleted;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }
            return Json(AccessGroupID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateAccessGroup(AccessGroupViewModel model)
        {
            var context = new IEContext();

            if (context.AccessGroupList.Any(l => l.Name == model.Name && l.AccessGroupID != model.AccessGroupID))
            {
                return DefaultErrorAlert("Another Access Group with this name already exists!");
            }

            var gp = new AccessGroup() { AccessGroupID = model.AccessGroupID };
            context.AccessGroupList.Attach(gp);
            gp.Name = model.Name;

            var ua = context.IEUserList.Where(l => l.Email == User.Identity.Name).Select(l => new { l.UserID, l.Name }).First();

            gp.TimeChanged = DateTime.Now;
            gp.ChangedByID = ua.UserID;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }
            model.ChangedByName = ua.Name;
            model.TimeChanged = gp.TimeChanged.Value;
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _AddAccessGroup()
        {
            return PartialView();
        }

        public ActionResult CreateAccessGroup(AccessGroupViewModel model)
        {
            var context = new IEContext();

            if (context.AccessGroupList.Any(l => l.Name == model.Name))
            {
                return DefaultErrorAlert("An Access Group with this name already exists!");
            }

            var ua = context.IEUserList.Where(l => l.Email == User.Identity.Name).Select(l => new { l.UserID, l.Name }).First();

            var gp = new AccessGroup { Name = model.Name, ChangedByID = ua.UserID, TimeChanged = DateTime.Now };
            context.AccessGroupList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }
            model.AccessGroupID = gp.AccessGroupID;
            model.ChangedByName = ua.Name;
            model.TimeChanged = gp.TimeChanged.Value;
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Locations

        public ActionResult Locations()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Admin);
            if (!hVM.Has_ManageSchools) return RedirectToAction("Index", "Home");

            var model = new LocationPageViewModel { LocationList = context.LocationList.ProjectToList<LocationViewModel>() };

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _DownloadLocations(string Search)
        {
            var newView = new GridView
            {
                AutoGenerateColumns = false,
                ShowHeaderWhenEmpty = true
            };

            newView.Columns.Add(new BoundField { HeaderText = "#", DataField = "LocationID" });
            newView.Columns.Add(new BoundField { HeaderText = "Name", DataField = "Name" });
            newView.Columns.Add(new BoundField { HeaderText = "State", DataField = "StateName" });
            newView.Columns.Add(new BoundField { HeaderText = "# Schools", DataField = "SchoolCount" });

            var fileName = "Locations.xlsx";

            var context = new IEContext();
            var query = new IEContext().LocationList.AsQueryable();

            if (Search != string.Empty)
            {
                string search = Search.Trim();
                query = query.Where(p => p.Name.Contains(search));
            }


            newView.ItemType = "IEGen.Models.LocationViewModel";
            newView.DataSource = query.ProjectToList<LocationViewModel>().AsQueryable();

            newView.DataBind();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("IEPortal Locations");
            var totalCols = newView.Rows[0].Cells.Count;
            var totalRows = newView.Rows.Count;
            var headerRow = newView.HeaderRow;

            for (var i = 1; i <= totalCols; i++)
            {
                workSheet.Cells[1, i].Value = HttpUtility.HtmlDecode(headerRow.Cells[i - 1].Text);
                workSheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            using (var range = workSheet.Cells[1, 1, 1, totalCols])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.WhiteSmoke);
                range.Style.ShrinkToFit = false;
            }

            for (var j = 1; j <= totalRows; j++)
            {
                var aRow = newView.Rows[j - 1];
                for (var i = 1; i <= totalCols; i++)
                {
                    workSheet.Cells[j + 1, i].Value = HttpUtility.HtmlDecode(aRow.Cells[i - 1].Text);
                    workSheet.Cells[j + 1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            workSheet.Cells[1, 1, totalRows + 1, totalCols].Style.Border.BorderAround(ExcelBorderStyle.Thick);

            for (var i = 1; i <= totalCols; i++) workSheet.Column(i).AutoFit();

            var ms = new MemoryStream();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            excel.SaveAs(ms);
            ms.Position = 0;

            return File(ms, contentType, fileName);
        }

        public ActionResult _EditLocation(int LocationID)
        {
            return PartialView(new IEContext().LocationList.Where(m => m.LocationID == LocationID).ProjectToFirst<LocationViewModel>());
        }

        [HttpPost]
        public ActionResult DeleteLocation(int LocationID)
        {
            var context = new IEContext();
            var gp = new Location() { LocationID = LocationID };
            context.LocationList.Attach(gp);
            context.Entry(gp).State = EntityState.Deleted;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }
            return Json(LocationID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateLocation(LocationViewModel model)
        {
            var context = new IEContext();

            if (context.LocationList.Any(l => l.LocationID != model.LocationID && l.Name == model.Name))
                return DefaultErrorAlert("Another Location with this name already exists!");

            var gp = new Location() { LocationID = model.LocationID };
            context.LocationList.Attach(gp);
            gp.Name = model.Name;
            gp.StateID = model.StateID;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _AddLocation()
        {
            return PartialView();
        }

        public ActionResult CreateLocation(LocationViewModel model)
        {
            var context = new IEContext();

            if (context.LocationList.Any(l => l.Name == model.Name))
                return DefaultErrorAlert("A Location with this name already exists!");

            var gp = new Location { Name = model.Name, StateID = model.StateID };
            context.LocationList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            model.LocationID = gp.LocationID;
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Users 

        private List<SelectListItem> GetAccessGroupList(IEContext context, bool isFilter)
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = isFilter ? "-- All Access Groups --" : "-- Select Access Group --" }
            };

            var gpList = context.AccessGroupList
                                .Select(t => new { t.Name, t.AccessGroupID }).ToList()
                                .Select(l => new SelectListItem { Value = l.AccessGroupID.ToString(), Text = l.Name });

            list.AddRange(gpList);
            return list;
        }

        public ActionResult Users()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Admin);
            if (!hVM.Has_ManageUsers) return RedirectToAction("Index", "Home");

            var model = new UserPageViewModel
            {
                HeaderViewModel = hVM,

                AccessGroupList = GetAccessGroupList(context, true)
            };

            return View("Users", model);
        }

        public ActionResult GetUserList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, UserFilterViewModel filterModel)
        {
            var query = new IEContext().IEUserList.Where(l => l.UserID > 1);
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Name.Contains(search) || p.Email.Contains(search) || p.PhoneNumber.Contains(search));
            }

            if (filterModel.AccessGroupID.HasValue)
                query = query.Where(l => l.AccessGroupID == filterModel.AccessGroupID.Value);

            if (filterModel.TypeID.HasValue)
                query = query.Where(l => l.TypeID == filterModel.TypeID.Value);

            int filteredCount = query.Count();

            string sortStr = "";
            foreach (var col in requestModel.Columns.GetSortedColumns())
            {
                sortStr += sortStr != string.Empty ? "," : "";
                if (col.Data == "Name" || col.Data == "PhoneNumber" || col.Data == "Email")
                {
                    sortStr = sortStr + col.Data + (col.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    continue;
                }
            }

            var model = query.OrderBy(sortStr == string.Empty ? "Name asc" : sortStr).Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<UserViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _DownloadUsers(UserFilterViewModel filterModel, string Search)
        {
            GridView newView = new GridView
            {
                AutoGenerateColumns = false,
                ShowHeaderWhenEmpty = true
            };

            newView.Columns.Add(new BoundField { HeaderText = "#", DataField = "UserID" });
            newView.Columns.Add(new BoundField { HeaderText = "Name", DataField = "Name" });
            newView.Columns.Add(new BoundField { HeaderText = "Phone", DataField = "PhoneNumber" });
            newView.Columns.Add(new BoundField { HeaderText = "Email", DataField = "Email" });
            newView.Columns.Add(new BoundField { HeaderText = "Type", DataField = "Type" });
            newView.Columns.Add(new BoundField { HeaderText = "Access Group", DataField = "AccessGroupName" });

            var fileName = "IEPortalUsers.xlsx";

            var context = new IEContext();
            var query = new IEContext().IEUserList.Where(l => l.UserID > 1);

            if (Search != string.Empty)
            {
                string search = Search.Trim();
                query = query.Where(p => p.Name.Contains(search) || p.Email.Contains(search) || p.PhoneNumber.Contains(search));
            }

            if (filterModel.AccessGroupID.HasValue)
                query = query.Where(l => l.AccessGroupID == filterModel.AccessGroupID.Value);

            if (filterModel.TypeID.HasValue)
                query = query.Where(l => l.TypeID == filterModel.TypeID.Value);

            newView.ItemType = "IEGen.Models.UserViewModel";
            newView.DataSource = query.ProjectToList<UserViewModel>().AsQueryable();

            newView.DataBind();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Users");
            var totalCols = newView.Rows[0].Cells.Count;
            var totalRows = newView.Rows.Count;
            var headerRow = newView.HeaderRow;

            for (var i = 1; i <= totalCols; i++)
            {
                workSheet.Cells[1, i].Value = HttpUtility.HtmlDecode(headerRow.Cells[i - 1].Text);
                workSheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            using (var range = workSheet.Cells[1, 1, 1, totalCols])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.WhiteSmoke);
                range.Style.ShrinkToFit = false;
            }

            for (var j = 1; j <= totalRows; j++)
            {
                var aRow = newView.Rows[j - 1];
                for (var i = 1; i <= totalCols; i++)
                {
                    workSheet.Cells[j + 1, i].Value = HttpUtility.HtmlDecode(aRow.Cells[i - 1].Text);
                    workSheet.Cells[j + 1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            workSheet.Cells[1, 1, totalRows + 1, totalCols].Style.Border.BorderAround(ExcelBorderStyle.Thick);

            for (var i = 1; i <= totalCols; i++) workSheet.Column(i).AutoFit();

            var ms = new MemoryStream();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            excel.SaveAs(ms);
            ms.Position = 0;

            return File(ms, contentType, fileName);
        }

        public ActionResult _EditUser(int UserID)
        {
            var context = new IEContext();
            var model = context.IEUserList.Where(m => m.UserID == UserID).ProjectToFirst<EditUserViewModel>();

            model.OldEmail = model.Email;
            model.AccessGroupList = GetAccessGroupList(context, false);

            if (!string.IsNullOrEmpty(model.PhoneNumber) && !string.IsNullOrEmpty(model.PhoneNumber.Trim()))
            {
                var mID = context.IEUserList.Where(m => m.UserID > 1 && m.UserID != UserID && m.PhoneNumber == model.PhoneNumber).Select(l => (int?)l.UserID).FirstOrDefault();

                if (mID.HasValue)
                {
                    model.MergeUserID = mID;

                    var mg = context.IEUserList.Where(l => l.UserID == mID).Select(l => new { l.Email, l.Name }).First();
                    model.MergeName = mg.Name;
                    model.MergeEmail = mg.Email;
                }
            }

            return PartialView(model);
        }

        public ActionResult _ViewUser(int UserID)
        {
            return PartialView(new IEContext().IEUserList.Where(m => m.UserID == UserID).ProjectToFirst<UserDetailViewModel>());
        }

        [HttpPost]
        public async Task<ActionResult> MergeUsers(int UserID, int OldUserID)
        {
            var context = new IEContext();

            context.AccessGroupList.Where(l => l.ChangedByID == OldUserID).Update(s => new AccessGroup { ChangedByID = UserID });

            // delete old user
            var gp = new IEUser() { UserID = OldUserID };
            context.IEUserList.Attach(gp);
            context.Entry(gp).State = EntityState.Deleted;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert("Merge Completed Successfully!");
        }

        [HttpPost]
        public ActionResult DeleteUser(int UserID)
        {
            var context = new IEContext();
            var gp = new IEUser() { UserID = UserID };
            context.IEUserList.Attach(gp);
            context.Entry(gp).State = EntityState.Deleted;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }
            return Json(UserID, JsonRequestBehavior.AllowGet);
        }

        private EmailMessage GetUpdateUserMail(UpdateUserMailModel model, ApplicationUserManager UserManager, IEContext context)
        {
            string body = "<p>Your account on the IE Portal has been updated.</p>" +
                          "<p>Your Email is <strong>" + model.NewEmail + "</strong>.</p>";

            body += "<mb>Log In</mb>";

            return new EmailMessage
            {
                ButtonUrl = Url.Action("Index", "Home", null, Request.Url.Scheme),
                ToEmail = model.Email,
                Subject = "Account Updated by " + model.SchoolName,
                Name = model.Name,
                Body = body
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateUser(EditUserViewModel model)
        {
            var context = new IEContext();

            bool emailChanged = false;
            bool newUser = false;
            var UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            if (!string.IsNullOrEmpty(model.Email))
            {
                if (model.OldEmail != model.Email)
                {
                    if (context.IEUserList.Any(l => l.Email == model.Email))
                        return DefaultErrorAlert("The Email: " + model.Email + " is in use!");

                    var u = await UserManager.FindByEmailAsync(model.Email);
                    if (u == null)
                        emailChanged = true;
                    else
                        return DefaultErrorAlert("The Email: " + model.Email + " is in use!");
                }

                if (emailChanged)
                {
                    //create new aspnet user
                    var user = await UserManager.FindByNameAsync(model.OldEmail);
                    if (user == null)
                    {
                        //create a new user
                        newUser = true;
                        user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                        string pwd = model.Email + "123";

                        var result = await UserManager.CreateAsync(user, pwd);
                        if (!result.Succeeded)
                        {
                            var alertVM = AlertViewModel.Create("Account creation was not successful. Please refresh your browser and try again.", AlertTypes.Error);
                            foreach (string error in result.Errors)
                            {
                                alertVM.AddAlert(error, AlertTypes.Error);
                                General.AlertElmah(new Exception(error));
                            }
                            Response.StatusCode = 500;
                            return PartialView("_Alert", alertVM);
                        }
                    }
                    else
                    {
                        //update existing user
                        user.Email = model.Email;

                        var result = await UserManager.UpdateAsync(user);
                        if (!result.Succeeded)
                        {
                            var alertVM = AlertViewModel.Create("Account update was not successful. Please refresh your browser and try again.", AlertTypes.Error);
                            foreach (string error in result.Errors)
                            {
                                alertVM.AddAlert(error, AlertTypes.Error);
                                General.AlertElmah(new Exception(error));
                            }
                            Response.StatusCode = 500;
                            return PartialView("_Alert", alertVM);
                        }
                    }
                }
            }

            var gp = new IEUser() { UserID = model.UserID };
            context.IEUserList.Attach(gp);
            Mapper.Map(model, gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                if (newUser)
                {
                    var user = await UserManager.FindByNameAsync(model.Email);
                    await UserManager.DeleteAsync(user);
                }

                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            if (emailChanged)
            {
                var uModel = new UpdateUserMailModel { Email = model.Email, Name = model.Name, NewEmail = model.Email };

                var msgList = new List<EmailMessage>
                {
                    GetUpdateUserMail(uModel, UserManager, context)
                };

                uModel.Email = model.OldEmail;
                msgList.Add(GetUpdateUserMail(uModel, UserManager, context));

                try
                {
                    context.EmailMessageList.AddRange(msgList);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    General.AlertElmah(ex);
                }
            }

            return DefaultSuccessAlert("User details for " + model.Name + " [" + model.Email + "] were updated successfully!");
        }

        public ActionResult _AddUser()
        {
            var context = new IEContext();
            var model = new EditUserViewModel
            {
                AccessGroupList = GetAccessGroupList(context, false)
            };

            return PartialView(model);
        }

        private async Task<EmailMessage> GetAddUserMail(ResetPasswordMailModel model, ApplicationUserManager UserManager, IEContext context)
        {
            DateTime now = DateTime.Now;
            string expTime = General.FullTimeString(now.AddHours(2.0));
            var pr = new PasswordReset { UserID = model.UserID, RequestTime = now };
            context.PasswordResetList.Add(pr);
            context.SaveChanges();

            string code = await UserManager.GeneratePasswordResetTokenAsync(model.AspUserId);
            string body = "<p>An account has been created for you on the IE Portal.</p>" +
                          "<p>The Imavate Education Portal (IE Portal) is the central management application for the Imavate Education Program.</p>" +
                          "<p>Your email is <strong>" + model.Email + "</strong> and your password is <strong>" + model.Password + "</strong>.</p>" +
                          "Use the button below to reset it. <strong>This password reset is only valid for 2 hours till " + expTime + " (UTC).</strong></p>" +
                          "<mb>Reset Password</mb>" + 
                          "<p>If you did not request for an account to be created, you can use the button at the bottom of this email to delete this account.</p>";

            string adBtnUrl = Url.Action("Index", "Home", null, Request.Url.Scheme);

            return new EmailMessage
            {
                ButtonUrl = Url.Action("ResetPassword", "Account", new { resetId = pr.ResetID, code = code }, Request.Url.Scheme),
                DeleteAccountUrl = Url.Action("Delete", "Account", new { userId = model.UserID, email = model.Email }, Request.Url.Scheme),
                ToEmail = model.Email,
                Subject = "Account Created",
                Name = model.Name,
                Body = body
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateUser(EditUserViewModel model)
        {
            var context = new IEContext();

            var UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = null;
            bool canLogin = false;
            string pwd = model.Email + "123";

            if (!string.IsNullOrEmpty(model.Email))
            {
                if (context.IEUserList.Any(m => m.Email == model.Email))
                    return DefaultErrorAlert("The Email: " + model.Email + " is in use!");

                user = new ApplicationUser { UserName = model.Email, Email = model.Email };

                var result = await UserManager.CreateAsync(user, pwd);
                if (!result.Succeeded)
                {
                    var alertVM = AlertViewModel.Create("Account creation was not successful. Please refresh your browser and try again.", AlertTypes.Error);
                    foreach (string error in result.Errors)
                    {
                        alertVM.AddAlert(error, AlertTypes.Error);
                        General.AlertElmah(new Exception(error));
                    }
                    Response.StatusCode = 500;
                    return PartialView("_Alert", alertVM);
                }

                canLogin = true;
            }

            var gp = new IEUser();
            Mapper.Map(model, gp);
            context.IEUserList.Add(gp);

            try
            {
                await context.SaveChangesAsync();

                if (canLogin)
                {
                    var pmModel = new ResetPasswordMailModel { Email = gp.Email, UserID = gp.UserID, Name = gp.Name, Password = pwd, AspUserId = user.Id };
                    var msgList = new List<EmailMessage>
                    {
                        await GetAddUserMail(pmModel, UserManager, context)
                    };

                    try
                    {
                        context.EmailMessageList.AddRange(msgList);
                        await context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        General.AlertElmah(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                if (canLogin)
                    await UserManager.DeleteAsync(user);

                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert(model.Name + "[" + model.Email + "] was added to the database successfully!");
        }
        #endregion

        #region Schools

        public ActionResult NewRequests()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Admin);
            if (!hVM.Has_ManageSchools) return RedirectToAction("Index", "Home");

            var model = new NewRequestPageViewModel { RequestList = context.SchoolRegList.ProjectToList<NewRequestViewModel>() };

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _ViewNewRequest(int SchoolRegID)
        {
            return PartialView(new IEContext().SchoolRegList.Where(m => m.SchoolRegID == SchoolRegID).ProjectToFirst<SchoolRegViewModel>());
        }

        public ActionResult _SchoolFromRequest(int SchoolRegID)
        {
            var context = new IEContext();
            var model = context.SchoolRegList.Where(m => m.SchoolRegID == SchoolRegID).ProjectToFirst<EditSchoolViewModel>();
            model.LocationList = GetLocationList(context, false);

            return PartialView("_AddSchool", model);
        }

        public ActionResult _SelectSchool()
        {
            return PartialView(new IEContext().SchoolList.Where(m => !m.IsDisabled).ProjectToList<ChooseSchoolViewModel>());
        }

        public ActionResult AddRequestToSchool(int SchoolID, int SchoolRegID)
        {
            var context = new IEContext();
            var reg = context.SchoolRegList.Where(m => m.SchoolRegID == SchoolRegID).First();
            var gp = new SchoolRequest() { SchoolID = SchoolID, RequestDate = reg.RegDate, ContactPerson = reg.ContactPerson, Notes = reg.Notes };
            
            context.SchoolRequestList.Add(gp);

            context.Entry(reg).State = EntityState.Deleted;
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return Json(SchoolRegID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteSchoolRequest(int SchoolRegID)
        {
            var context = new IEContext();
            var reg = new SchoolReg { SchoolRegID = SchoolRegID };
            context.SchoolRegList.Attach(reg);

            context.Entry(reg).State = EntityState.Deleted;
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return Json(SchoolRegID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Schools()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Admin);
            if (!hVM.Has_ManageSchools) return RedirectToAction("Index", "Home");

            var model = new SchoolPageViewModel { DefaultSchool = context.SchoolList.Where(l => l.SchoolID == hVM.SchoolID).Select(l => l.Name).FirstOrDefault() };
            model.HeaderViewModel = hVM;

            model.LocationList = GetLocationList(context, true);

            return View("Schools", model);
        }

        public ActionResult GetSchoolList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, SchoolFilterViewModel filterModel)
        {
            var query = new IEContext().SchoolList.AsQueryable();
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Name.Contains(search) || p.Email.Contains(search) || p.Phone.Contains(search) || p.Address.Contains(search));
            }

            if (filterModel.LocationID.HasValue)
                query = query.Where(l => l.LocationID == filterModel.LocationID.Value);

            if (filterModel.TypeID.HasValue)
                query = query.Where(l => l.TypeID == filterModel.TypeID.Value);

            if (filterModel.StateID.HasValue)
                query = query.Where(l => l.Location.StateID == filterModel.StateID.Value);

            if (filterModel.StatusID.HasValue)
                query = query.Where(l => l.IsDisabled == (filterModel.StatusID.Value == 1));

            int filteredCount = query.Count();

            string sortStr = "";
            foreach (var col in requestModel.Columns.GetSortedColumns())
            {
                sortStr += sortStr != string.Empty ? "," : "";
                if (col.Data == "Name" || col.Data == "PhoneNumber" || col.Data == "Email")
                {
                    sortStr = sortStr + col.Data + (col.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    continue;
                }
            }

            var model = query.OrderBy(sortStr == string.Empty ? "Name asc" : sortStr).Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<SchoolViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _DownloadSchools(SchoolFilterViewModel filterModel, string Search)
        {
            GridView newView = new GridView
            {
                AutoGenerateColumns = false,
                ShowHeaderWhenEmpty = true
            };

            newView.Columns.Add(new BoundField { HeaderText = "#", DataField = "SchoolID" });
            newView.Columns.Add(new BoundField { HeaderText = "Name", DataField = "Name" });
            newView.Columns.Add(new BoundField { HeaderText = "Type", DataField = "Type" });
            newView.Columns.Add(new BoundField { HeaderText = "Location", DataField = "LocationDesc" });
            newView.Columns.Add(new BoundField { HeaderText = "Address", DataField = "Address" });
            newView.Columns.Add(new BoundField { HeaderText = "Email", DataField = "Email" });
            newView.Columns.Add(new BoundField { HeaderText = "Phone", DataField = "Phone" });
            newView.Columns.Add(new BoundField { HeaderText = "Website", DataField = "Website" });
            newView.Columns.Add(new BoundField { HeaderText = "Status", DataField = "Status" });
            newView.Columns.Add(new BoundField { HeaderText = "WriteUp", DataField = "WriteUp" });

            var fileName = "IEPortalSchools.xlsx";

            var context = new IEContext();
            var query = new IEContext().SchoolList.AsQueryable();

            if (Search != string.Empty)
            {
                string search = Search.Trim();
                query = query.Where(p => p.Name.Contains(search) || p.Email.Contains(search) || p.Phone.Contains(search) || p.Address.Contains(search));
            }

            if (filterModel.LocationID.HasValue)
                query = query.Where(l => l.LocationID == filterModel.LocationID.Value);

            if (filterModel.TypeID.HasValue)
                query = query.Where(l => l.TypeID == filterModel.TypeID.Value);

            if (filterModel.StateID.HasValue)
                query = query.Where(l => l.Location.StateID == filterModel.StateID.Value);

            if (filterModel.StatusID.HasValue)
                query = query.Where(l => l.IsDisabled == (filterModel.StatusID.Value == 1));

            newView.ItemType = "IEGen.Models.SchoolViewModel";
            newView.DataSource = query.ProjectToList<SchoolViewModel>().AsQueryable();

            newView.DataBind();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Schools");
            var totalCols = newView.Rows[0].Cells.Count;
            var totalRows = newView.Rows.Count;
            var headerRow = newView.HeaderRow;

            for (var i = 1; i <= totalCols; i++)
            {
                workSheet.Cells[1, i].Value = HttpUtility.HtmlDecode(headerRow.Cells[i - 1].Text);
                workSheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            using (var range = workSheet.Cells[1, 1, 1, totalCols])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.WhiteSmoke);
                range.Style.ShrinkToFit = false;
            }

            for (var j = 1; j <= totalRows; j++)
            {
                var aRow = newView.Rows[j - 1];
                for (var i = 1; i <= totalCols; i++)
                {
                    workSheet.Cells[j + 1, i].Value = HttpUtility.HtmlDecode(aRow.Cells[i - 1].Text);
                    workSheet.Cells[j + 1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            workSheet.Cells[1, 1, totalRows + 1, totalCols].Style.Border.BorderAround(ExcelBorderStyle.Thick);

            for (var i = 1; i <= totalCols; i++) workSheet.Column(i).AutoFit();

            var ms = new MemoryStream();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            excel.SaveAs(ms);
            ms.Position = 0;

            return File(ms, contentType, fileName);
        }

        public ActionResult SetDefSchool(int SchoolID, int UserID)
        {
            var context = new IEContext();
            var gp = new IEUser() { UserID = UserID };
            context.IEUserList.Attach(gp);
            gp.SchoolID = SchoolID;

            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return JSRedirect(Url.Action("Schools", "Admin"));
        }

        private List<SelectListItem> GetLocationList(IEContext context, bool isFilter)
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = isFilter ? "-- All Locations --" : "-- Select Location --" }
            };

            var gpList = context.LocationList
                                .Select(t => new { t.Name, t.LocationID, t.StateID }).ToList()
                                .Select(l => new SelectListItem { Value = l.LocationID.ToString(), Text = General.StateName(l.StateID) + " - " + l.Name });

            list.AddRange(gpList);
            return list;
        }

        public ActionResult _EditSchool(int SchoolID)
        {
            var context = new IEContext();
            var model = new IEContext().SchoolList.Where(m => m.SchoolID == SchoolID).ProjectToFirst<EditSchoolViewModel>();
            model.LocationList = GetLocationList(context, false);

            if (!string.IsNullOrEmpty(model.GuidString))
                model.LogoSrc = "data:image/png;base64," + Convert.ToBase64String(new School { GuidString = model.GuidString }.DownloadFile());

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteSchool(int SchoolID)
        {
            var context = new IEContext();
            var gp = new School() { SchoolID = SchoolID };
            context.SchoolList.Attach(gp);
            context.Entry(gp).State = EntityState.Deleted;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }
            return Json(SchoolID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateSchool(EditSchoolViewModel model)
        {
            var lfile = Request.Files["Logo"];
            int lLength = 0;
            MemoryStream lms = new MemoryStream();
            if (lfile != null)
            {
                lLength = lfile.ContentLength;
                if (lLength > 0)
                {
                    lfile.InputStream.CopyTo(lms);

                    Bitmap img = new Bitmap(lms);

                    //if (img.Height != 360 || img.Width != 360)
                    //    return DefaultErrorAlert("Please select a Logo file that is 360px by 360px");

                    if (img.Height != img.Width)
                        return DefaultErrorAlert("Please select a Logo file that is 360px by 360px");
                }
            }

            var context = new IEContext();

            if (context.SchoolList.Any(l => l.SchoolID != model.SchoolID && l.Name == model.Name))
                return DefaultErrorAlert("Another School with this name already exists!");

            var gp = new School() { SchoolID = model.SchoolID };
            context.SchoolList.Attach(gp);

            Mapper.Map(model, gp);

            if (lLength > 0)
            {
                if (!context.TermList.Any(l => l.SchoolID == gp.SchoolID && l.GuidString == gp.GuidString))
                    gp.DeleteFile();

                gp.UploadFile(lms.ToArray());
            }

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert("Details for " + model.Name + " was updated successfully!");
        }

        public ActionResult _AddSchool()
        {
            var context = new IEContext();
            var model = new EditSchoolViewModel
            {
                LocationList = GetLocationList(context, false)
            };

            return PartialView(model);
        }

        public ActionResult CreateSchool(EditSchoolViewModel model)
        {
            var lfile = Request.Files["Logo"];
            int lLength = 0;
            MemoryStream lms = new MemoryStream();
            if (lfile != null)
            {
                lLength = lfile.ContentLength;
                if(lLength > 0)
                {
                    lfile.InputStream.CopyTo(lms);

                    Bitmap img = new Bitmap(lms);

                    if (img.Height != img.Width)
                        return DefaultErrorAlert("Please select a Logo file that is 360px by 360px");
                }
            }

            var context = new IEContext();

            if (context.SchoolList.Any(l => l.Name == model.Name))
                return DefaultErrorAlert("A School with this name already exists!");

            var gp = new School();
            Mapper.Map(model, gp);

            if(lLength > 0) gp.UploadFile(lms.ToArray());

            if(model.SchoolRegID > 0)
            {
                var reg = context.SchoolRegList.Where(m => m.SchoolRegID == model.SchoolRegID).First();
                var rgp = new SchoolRequest() { RequestDate = reg.RegDate, ContactPerson = reg.ContactPerson, Notes = reg.Notes };
                gp.Requests = new List<SchoolRequest> { rgp };

                context.Entry(reg).State = EntityState.Deleted;
            }

            context.SchoolList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert(model.Name + " was added to the database successfully!");
        }
        #endregion

        #region Teachers

        public ActionResult Teachers()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Admin);
            if (!hVM.Has_ManageTeachers) return RedirectToAction("Index", "Home");

            var model = new TeacherPageViewModel
            {
                HeaderViewModel = hVM,

                LocationList = GetLocationList(context, true)
            };

            return View("Teachers", model);
        }

        public ActionResult GetAllTeachersList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, TeacherFilterViewModel filterModel)
        {
            var query = new IEContext().TeacherList.AsQueryable();
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Name.Contains(search) || p.Email.Contains(search) || p.Phone.Contains(search));
            }

            if (filterModel.LocationID.HasValue)
                query = query.Where(l => l.School.LocationID == filterModel.LocationID.Value);

            if (filterModel.SchoolTypeID.HasValue)
                query = query.Where(l => l.School.TypeID == filterModel.SchoolTypeID.Value);

            if (filterModel.StateID.HasValue)
                query = query.Where(l => l.School.Location.StateID == filterModel.StateID.Value);

            if (filterModel.SexID.HasValue)
                query = query.Where(l => l.IsMale == (filterModel.SexID.Value == 1));

            int filteredCount = query.Count();

            string sortStr = "";
            foreach (var col in requestModel.Columns.GetSortedColumns())
            {
                sortStr += sortStr != string.Empty ? "," : "";
                if (col.Data == "Name" || col.Data == "Phone" || col.Data == "Email")
                {
                    sortStr = sortStr + col.Data + (col.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    continue;
                }
            }

            var model = query.OrderBy(sortStr == string.Empty ? "SchoolID asc, Name asc" : sortStr).Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<TeacherViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _DownloadTeachers(TeacherFilterViewModel filterModel, string Search)
        {
            GridView newView = new GridView
            {
                AutoGenerateColumns = false,
                ShowHeaderWhenEmpty = true
            };

            newView.Columns.Add(new BoundField { HeaderText = "#", DataField = "TeacherID" });
            newView.Columns.Add(new BoundField { HeaderText = "Name", DataField = "Name" });
            newView.Columns.Add(new BoundField { HeaderText = "Email", DataField = "Email" });
            newView.Columns.Add(new BoundField { HeaderText = "Phone", DataField = "Phone" });
            newView.Columns.Add(new BoundField { HeaderText = "Sex", DataField = "Sex" });
            newView.Columns.Add(new BoundField { HeaderText = "School", DataField = "SchoolName" });
            newView.Columns.Add(new BoundField { HeaderText = "Location", DataField = "LocationDesc" });
            newView.Columns.Add(new BoundField { HeaderText = "IET Number", DataField = "IETNumber" });
            newView.Columns.Add(new BoundField { HeaderText = "Start Year", DataField = "StartYear" });
            newView.Columns.Add(new BoundField { HeaderText = "Qualifications", DataField = "Qualifications" });

            var fileName = "IEPortalTeachers.xlsx";

            var context = new IEContext();
            var query = new IEContext().TeacherList.AsQueryable();

            if (Search != string.Empty)
            {
                string search = Search.Trim();
                query = query.Where(p => p.Name.Contains(search) || p.Email.Contains(search) || p.Phone.Contains(search));
            }

            if (filterModel.LocationID.HasValue)
                query = query.Where(l => l.School.LocationID == filterModel.LocationID.Value);

            if (filterModel.SchoolTypeID.HasValue)
                query = query.Where(l => l.School.TypeID == filterModel.SchoolTypeID.Value);

            if (filterModel.StateID.HasValue)
                query = query.Where(l => l.School.Location.StateID == filterModel.StateID.Value);

            if (filterModel.SexID.HasValue)
                query = query.Where(l => l.IsMale == (filterModel.SexID.Value == 1));

            newView.ItemType = "IEGen.Models.TeacherViewModel";
            newView.DataSource = query.ProjectToList<TeacherViewModel>().AsQueryable();

            newView.DataBind();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Teachers");
            var totalCols = newView.Rows[0].Cells.Count;
            var totalRows = newView.Rows.Count;
            var headerRow = newView.HeaderRow;

            for (var i = 1; i <= totalCols; i++)
            {
                workSheet.Cells[1, i].Value = HttpUtility.HtmlDecode(headerRow.Cells[i - 1].Text);
                workSheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            using (var range = workSheet.Cells[1, 1, 1, totalCols])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.WhiteSmoke);
                range.Style.ShrinkToFit = false;
            }

            for (var j = 1; j <= totalRows; j++)
            {
                var aRow = newView.Rows[j - 1];
                for (var i = 1; i <= totalCols; i++)
                {
                    workSheet.Cells[j + 1, i].Value = HttpUtility.HtmlDecode(aRow.Cells[i - 1].Text);
                    workSheet.Cells[j + 1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            workSheet.Cells[1, 1, totalRows + 1, totalCols].Style.Border.BorderAround(ExcelBorderStyle.Thick);

            for (var i = 1; i <= totalCols; i++) workSheet.Column(i).AutoFit();

            var ms = new MemoryStream();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            excel.SaveAs(ms);
            ms.Position = 0;

            return File(ms, contentType, fileName);
        }

        public ActionResult _ViewTeacher(int TeacherID)
        {
            return PartialView(new IEContext().TeacherList.Where(m => m.TeacherID == TeacherID).ProjectToFirst<TeacherViewModel>());
        }

        private List<SelectListItem> GetSchoolList(IEContext context, bool isFilter)
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = isFilter ? "-- All Schools --" : "-- Select School --" }
            };

            var gpList = context.SchoolList
                                .Select(t => new { t.Name, t.SchoolID }).ToList()
                                .Select(l => new SelectListItem { Value = l.SchoolID.ToString(), Text = l.Name });

            list.AddRange(gpList);
            return list;
        }
        #endregion

        #region CTeachers

        public ActionResult CTeachers()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Admin);
            if (!hVM.Has_ManageTeachers) return RedirectToAction("Index", "Home");

            var model = new CTeacherPageViewModel
            {
                HeaderViewModel = hVM,

                LocationList = GetLocationList(context, true)
            };

            return View("CTeachers", model);
        }

        public ActionResult GetCTeacherList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, CTeacherFilterViewModel filterModel)
        {
            var query = new IEContext().CTeacherList.AsQueryable();
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Name.Contains(search) || p.Email.Contains(search) || p.Phone.Contains(search) || p.Qualifications.Contains(search) || p.Subjects.Contains(search) ||
                                         p.SchoolAddress.Contains(search) || p.SchoolName.Contains(search) || p.Designation.Contains(search));
            }

            if (filterModel.LocationID.HasValue)
                query = query.Where(l => l.LocationID == filterModel.LocationID.Value);

            if (filterModel.SchoolTypeID.HasValue)
                query = query.Where(l => l.SchoolTypeID == filterModel.SchoolTypeID.Value);

            if (filterModel.StateID.HasValue)
                query = query.Where(l => l.Location.StateID == filterModel.StateID.Value);

            if (filterModel.SexID.HasValue)
                query = query.Where(l => l.IsMale == (filterModel.SexID.Value == 1));

            if (filterModel.RegID.HasValue)
                query = query.Where(l => l.IsVerified == (filterModel.RegID.Value == 1));

            if (filterModel.DegreeID.HasValue)
                query = query.Where(l => l.MaxDegreeID == filterModel.DegreeID.Value);

            int thisYear = DateTime.Today.Year;
            if (filterModel.YearsFrom.HasValue)
                query = query.Where(l => (thisYear - l.StartYear) >= filterModel.YearsFrom.Value);

            if (filterModel.YearsTo.HasValue)
                query = query.Where(l => (thisYear - l.StartYear) <= filterModel.YearsTo.Value);

            int filteredCount = query.Count();

            string sortStr = "";
            foreach (var col in requestModel.Columns.GetSortedColumns())
            {
                sortStr += sortStr != string.Empty ? "," : "";
                if (col.Data == "Name" || col.Data == "Phone" || col.Data == "Email")
                {
                    sortStr = sortStr + col.Data + (col.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    continue;
                }
            }

            var model = query.OrderBy(sortStr == string.Empty ? "CTeacherID asc" : sortStr).Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<CTeacherViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _DownloadCTeachers(CTeacherFilterViewModel filterModel, string Search)
        {
            GridView newView = new GridView
            {
                AutoGenerateColumns = false,
                ShowHeaderWhenEmpty = true
            };

            newView.Columns.Add(new BoundField { HeaderText = "IET #", DataField = "IETNumber" });
            newView.Columns.Add(new BoundField { HeaderText = "Name", DataField = "Name" });
            newView.Columns.Add(new BoundField { HeaderText = "Email", DataField = "Email" });
            newView.Columns.Add(new BoundField { HeaderText = "Phone", DataField = "Phone" });
            newView.Columns.Add(new BoundField { HeaderText = "Sex", DataField = "Sex" });
            newView.Columns.Add(new BoundField { HeaderText = "Max. Degree", DataField = "MaxDegreeName" });
            newView.Columns.Add(new BoundField { HeaderText = "School Type", DataField = "SchoolType" });
            newView.Columns.Add(new BoundField { HeaderText = "School", DataField = "SchoolName" });
            newView.Columns.Add(new BoundField { HeaderText = "School Address", DataField = "SchoolAddress" });
            newView.Columns.Add(new BoundField { HeaderText = "Location", DataField = "LocationDesc" });
            newView.Columns.Add(new BoundField { HeaderText = "Designation", DataField = "Designation" });
            newView.Columns.Add(new BoundField { HeaderText = "Subjects", DataField = "Subjects" });
            newView.Columns.Add(new BoundField { HeaderText = "Start Year", DataField = "StartYear" });
            newView.Columns.Add(new BoundField { HeaderText = "Qualifications", DataField = "Qualifications" });
            newView.Columns.Add(new BoundField { HeaderText = "Status", DataField = "RegStatus" });

            var fileName = "IEPortalCTeachers.xlsx";

            var context = new IEContext();
            var query = new IEContext().CTeacherList.AsQueryable();

            if (Search != string.Empty)
            {
                string search = Search.Trim();
                query = query.Where(p => p.Name.Contains(search) || p.Email.Contains(search) || p.Phone.Contains(search) || p.Qualifications.Contains(search) || p.Subjects.Contains(search) ||
                                         p.SchoolAddress.Contains(search) || p.SchoolName.Contains(search) || p.Designation.Contains(search));
            }

            if (filterModel.LocationID.HasValue)
                query = query.Where(l => l.LocationID == filterModel.LocationID.Value);

            if (filterModel.SchoolTypeID.HasValue)
                query = query.Where(l => l.SchoolTypeID == filterModel.SchoolTypeID.Value);

            if (filterModel.StateID.HasValue)
                query = query.Where(l => l.Location.StateID == filterModel.StateID.Value);

            if (filterModel.SexID.HasValue)
                query = query.Where(l => l.IsMale == (filterModel.SexID.Value == 1));

            if (filterModel.RegID.HasValue)
                query = query.Where(l => l.IsVerified == (filterModel.RegID.Value == 1));

            if (filterModel.DegreeID.HasValue)
                query = query.Where(l => l.MaxDegreeID == filterModel.DegreeID.Value);

            int thisYear = DateTime.Today.Year;
            if (filterModel.YearsFrom.HasValue)
                query = query.Where(l => (thisYear - l.StartYear) >= filterModel.YearsFrom.Value);

            if (filterModel.YearsTo.HasValue)
                query = query.Where(l => (thisYear - l.StartYear) <= filterModel.YearsTo.Value);

            newView.ItemType = "IEGen.Models.CTeacherViewModel";
            newView.DataSource = query.ProjectToList<CTeacherViewModel>().AsQueryable();

            newView.DataBind();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("CheckTeachers");
            var totalCols = newView.Rows[0].Cells.Count;
            var totalRows = newView.Rows.Count;
            var headerRow = newView.HeaderRow;

            for (var i = 1; i <= totalCols; i++)
            {
                workSheet.Cells[1, i].Value = HttpUtility.HtmlDecode(headerRow.Cells[i - 1].Text);
                workSheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            using (var range = workSheet.Cells[1, 1, 1, totalCols])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.WhiteSmoke);
                range.Style.ShrinkToFit = false;
            }

            for (var j = 1; j <= totalRows; j++)
            {
                var aRow = newView.Rows[j - 1];
                for (var i = 1; i <= totalCols; i++)
                {
                    workSheet.Cells[j + 1, i].Value = HttpUtility.HtmlDecode(aRow.Cells[i - 1].Text);
                    workSheet.Cells[j + 1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            workSheet.Cells[1, 1, totalRows + 1, totalCols].Style.Border.BorderAround(ExcelBorderStyle.Thick);

            for (var i = 1; i <= totalCols; i++) workSheet.Column(i).AutoFit();

            var ms = new MemoryStream();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            excel.SaveAs(ms);
            ms.Position = 0;

            return File(ms, contentType, fileName);
        }

        public ActionResult _EditCTeacher(int CTeacherID)
        {
            var context = new IEContext();
            var model = new IEContext().CTeacherList.Where(m => m.CTeacherID == CTeacherID).ProjectToFirst<EditCTeacherViewModel>();
            model.LocationList = GetLocationList(context, false);

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteCTeacher(int CTeacherID)
        {
            var context = new IEContext();
            var gp = new CTeacher() { CTeacherID = CTeacherID };
            context.CTeacherList.Attach(gp);
            context.Entry(gp).State = EntityState.Deleted;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }
            return Json(CTeacherID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateCTeacher(EditCTeacherViewModel model)
        {
            var context = new IEContext();

            if (context.CTeacherList.Any(l => l.CTeacherID != model.CTeacherID && l.Email == model.Email))
                return DefaultErrorAlert("Another Teacher with this email already exists!");

            var gp = new CTeacher() { CTeacherID = model.CTeacherID };
            context.CTeacherList.Attach(gp);

            Mapper.Map(model, gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert("Details for " + model.Name + " were updated successfully!");
        }

        public ActionResult _AddCTeacher()
        {
            var context = new IEContext();
            var model = new EditCTeacherViewModel
            {
                LocationList = GetLocationList(context, false)
            };

            return PartialView(model);
        }

        private async Task<EmailMessage> GetAddTeacherMail(ResetPasswordMailModel model, ApplicationUserManager UserManager, IEContext context)
        {
            DateTime now = DateTime.Now;
            string expTime = General.FullTimeString(now.AddHours(2.0));
            var pr = new PasswordReset { UserID = model.UserID, RequestTime = now };
            context.PasswordResetList.Add(pr);
            context.SaveChanges();

            string code = await UserManager.GeneratePasswordResetTokenAsync(model.AspUserId);
            string body = "<p>An account has been created for you on CheckTeachers.com.</p>" +
                          "<p>The Imavate Education Portal (IE Portal) is the central management application for the Imavate Education Program including the CheckTeachers program.</p>" +
                          "<p>Your email is <strong>" + model.Email + "</strong> and your password is <strong>" + model.Password + "</strong>.</p>" +
                          "Use the button below to reset it. <strong>This password reset is only valid for 2 hours till " + expTime + " (UTC).</strong></p>" +
                          "<mb>Reset Password</mb>" +
                          "<p>If you did not request for an account to be created, you can use the button at the bottom of this email to delete this account.</p>";

            string adBtnUrl = Url.Action("Index", "Home", null, Request.Url.Scheme);

            return new EmailMessage
            {
                ButtonUrl = Url.Action("ResetPassword", "Account", new { resetId = pr.ResetID, code = code }, Request.Url.Scheme),
                DeleteAccountUrl = Url.Action("Delete", "Account", new { userId = model.UserID, email = model.Email }, Request.Url.Scheme),
                ToEmail = model.Email,
                Subject = "Account Created",
                Name = model.Name,
                Body = body
            };
        }

        public async Task<ActionResult> CreateCTeacher(EditCTeacherViewModel model)
        {
            var context = new IEContext();

            if (context.CTeacherList.Any(l => l.Email == model.Email))
                return DefaultErrorAlert("A Teacher with this email already exists!");

            var UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationUser user = null;
            bool canLogin = false;
            string pwd = model.Email + "123";

            if (!string.IsNullOrEmpty(model.Email))
            {
                if (context.IEUserList.Any(m => m.Email == model.Email))
                    return DefaultErrorAlert("The Email: " + model.Email + " is in use!");

                user = new ApplicationUser { UserName = model.Email, Email = model.Email };

                var result = await UserManager.CreateAsync(user, pwd);
                if (!result.Succeeded)
                {
                    var alertVM = AlertViewModel.Create("Account creation was not successful. Please refresh your browser and try again.", AlertTypes.Error);
                    foreach (string error in result.Errors)
                    {
                        alertVM.AddAlert(error, AlertTypes.Error);
                        General.AlertElmah(new Exception(error));
                    }
                    Response.StatusCode = 500;
                    return PartialView("_Alert", alertVM);
                }

                canLogin = true;
            }

            var gp = new IEUser();
            Mapper.Map(model, gp);
            gp.AccessGroupID = General.DefAccessGroupID;
            gp.TypeID = (byte)UserType.Teacher;

            context.IEUserList.Add(gp);

            var ct = new CTeacher();
            Mapper.Map(model, ct);
            context.CTeacherList.Add(ct);

            try
            {
                await context.SaveChangesAsync();

                if (canLogin)
                {
                    var pmModel = new ResetPasswordMailModel { Email = gp.Email, UserID = gp.UserID, Name = gp.Name, Password = pwd, AspUserId = user.Id };
                    var msgList = new List<EmailMessage>
                    {
                        await GetAddTeacherMail(pmModel, UserManager, context)
                    };

                    try
                    {
                        context.EmailMessageList.AddRange(msgList);
                        await context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        General.AlertElmah(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                if (canLogin)
                    await UserManager.DeleteAsync(user);

                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert(model.Name + " was added to the database successfully!");
        }

        public ActionResult _ViewCTeacher(int CTeacherID)
        {
            return PartialView(new IEContext().CTeacherList.Where(m => m.CTeacherID == CTeacherID).ProjectToFirst<CTeacherViewModel>());
        }
        #endregion

        #region Students

        public ActionResult Students()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Admin);
            if (!hVM.Has_ManageStudents) return RedirectToAction("Index", "Home");

            var model = new AllStudentPageViewModel
            {
                HeaderViewModel = hVM,

                LocationList = GetLocationList(context, true)
            };

            return View("Students", model);
        }

        public ActionResult GetStudentList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, AllStudentFilterViewModel filterModel)
        {
            var query = new IEContext().StudentList.AsQueryable();
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.School.Name.Contains(search) || p.FirstName.Contains(search) || p.Surname.Contains(search) || p.OtherName.Contains(search) || p.StudentCode.Contains(search));
            }

            if (filterModel.LocationID.HasValue)
                query = query.Where(l => l.School.LocationID == filterModel.LocationID.Value);

            if (filterModel.SchoolTypeID.HasValue)
                query = query.Where(l => l.School.TypeID == filterModel.SchoolTypeID.Value);

            if (filterModel.StateID.HasValue)
                query = query.Where(l => l.School.Location.StateID == filterModel.StateID.Value);

            if (filterModel.SexID.HasValue)
                query = query.Where(l => l.IsMale == (filterModel.SexID.Value == 1));

            int filteredCount = query.Count();

            string sortStr = "";
            foreach (var col in requestModel.Columns.GetSortedColumns())
            {
                sortStr += sortStr != string.Empty ? "," : "";
                if (col.Data == "FirstName" || col.Data == "Surname" || col.Data == "OtherName" || col.Data == "StudentCode")
                {
                    sortStr = sortStr + col.Data + (col.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    continue;
                }

                switch (col.Data)
                {
                    case "Sex": sortStr += "IsMale"; break;

                    case "ClassDesc": sortStr += "Class.Term.Name desc, Class.Arm.Name"; break;

                    case "SchoolDesc": sortStr += "School.Name"; break;
                }

                sortStr += SortDirStr(col.SortDirection);
            }

            var defSort = "School.Name asc, Class.Term.Name desc, Class.Arm.Name asc, Surname asc, FirstName asc";
            var model = query.OrderBy(sortStr == string.Empty ? defSort : sortStr + ", " + defSort)
                             .Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<AllStudentViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _DownloadStudents(AllStudentFilterViewModel filterModel, string Search)
        {
            GridView newView = new GridView
            {
                AutoGenerateColumns = false,
                ShowHeaderWhenEmpty = true
            };

            newView.Columns.Add(new BoundField { HeaderText = "#", DataField = "StudentID" });
            newView.Columns.Add(new BoundField { HeaderText = "Student Code", DataField = "StudentCode" });
            newView.Columns.Add(new BoundField { HeaderText = "First Name", DataField = "FirstName" });
            newView.Columns.Add(new BoundField { HeaderText = "Middle Name", DataField = "OtherName" });
            newView.Columns.Add(new BoundField { HeaderText = "Last Name", DataField = "Surname" });
            newView.Columns.Add(new BoundField { HeaderText = "Sex", DataField = "Sex" });
            newView.Columns.Add(new BoundField { HeaderText = "Birth Date", DataField = "BirthDate" });
            newView.Columns.Add(new BoundField { HeaderText = "Class", DataField = "ClassName" });
            newView.Columns.Add(new BoundField { HeaderText = "School", DataField = "SchoolName" });
            newView.Columns.Add(new BoundField { HeaderText = "School Location", DataField = "LocationDesc" });

            var fileName = "IEPortalStudents.xlsx";

            var context = new IEContext();
            var query = new IEContext().StudentList.AsQueryable();

            if (Search != string.Empty)
            {
                string search = Search.Trim();
                query = query.Where(p => p.FirstName.Contains(search) || p.Surname.Contains(search) || p.OtherName.Contains(search) || p.StudentCode.Contains(search));
            }

            if (filterModel.LocationID.HasValue)
                query = query.Where(l => l.School.LocationID == filterModel.LocationID.Value);

            if (filterModel.SchoolTypeID.HasValue)
                query = query.Where(l => l.School.TypeID == filterModel.SchoolTypeID.Value);

            if (filterModel.StateID.HasValue)
                query = query.Where(l => l.School.Location.StateID == filterModel.StateID.Value);

            if (filterModel.SexID.HasValue)
                query = query.Where(l => l.IsMale == (filterModel.SexID.Value == 1));

            newView.ItemType = "IEGen.Models.AllStudentViewModel";
            newView.DataSource = query.ProjectToList<AllStudentViewModel>().AsQueryable();

            newView.DataBind();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Students");
            var totalCols = newView.Rows[0].Cells.Count;
            var totalRows = newView.Rows.Count;
            var headerRow = newView.HeaderRow;

            for (var i = 1; i <= totalCols; i++)
            {
                workSheet.Cells[1, i].Value = HttpUtility.HtmlDecode(headerRow.Cells[i - 1].Text);
                workSheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            using (var range = workSheet.Cells[1, 1, 1, totalCols])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.WhiteSmoke);
                range.Style.ShrinkToFit = false;
            }

            for (var j = 1; j <= totalRows; j++)
            {
                var aRow = newView.Rows[j - 1];
                for (var i = 1; i <= totalCols; i++)
                {
                    workSheet.Cells[j + 1, i].Value = HttpUtility.HtmlDecode(aRow.Cells[i - 1].Text);
                    workSheet.Cells[j + 1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }

            workSheet.Cells[1, 1, totalRows + 1, totalCols].Style.Border.BorderAround(ExcelBorderStyle.Thick);

            for (var i = 1; i <= totalCols; i++) workSheet.Column(i).AutoFit();

            var ms = new MemoryStream();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            excel.SaveAs(ms);
            ms.Position = 0;

            return File(ms, contentType, fileName);
        }

        public ActionResult _ViewStudent(int StudentID)
        {
            var context = new IEContext();
            var model = context.StudentList.Where(m => m.StudentID == StudentID).ProjectToFirst<AllStudentViewModel>();

            if (!string.IsNullOrEmpty(model.GuidString))
                model.PhotoSrc = "data:image/png;base64," + Convert.ToBase64String(new Student { GuidString = model.GuidString }.DownloadFile());

            return PartialView(model);
        }
        #endregion

        #region Grade Groups

        public ActionResult GradeGroups()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Admin);
            if (!hVM.Has_ManageSchools) return RedirectToAction("Index", "Home");

            var model = new GradeGroupPageViewModel { GradeGroupList = context.GradeGroupList.ProjectToList<GradeGroupViewModel>() };

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _EditGradeGroup(int GradeGroupID)
        {
            var context = new IEContext();
            var model = context.GradeGroupList.Where(m => m.GradeGroupID == GradeGroupID).ProjectToFirst<GradeGroupViewModel>();
            model.IsUsed = context.ScoreEntryList.Any(t => t.Grade.GradeGroupID == GradeGroupID);

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteGradeGroup(int GradeGroupID)
        {
            var context = new IEContext();
            var gp = new GradeGroup() { GradeGroupID = GradeGroupID };
            context.GradeGroupList.Attach(gp);
            context.Entry(gp).State = EntityState.Deleted;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }
            return Json(GradeGroupID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateGradeGroup(GradeGroupViewModel model)
        {
            var context = new IEContext();

            if (!string.IsNullOrEmpty(model.Name) && context.GradeGroupList.Any(l => l.GradeGroupID != model.GradeGroupID && l.Name == model.Name))
                return DefaultErrorAlert("Another Grade Group with this name already exists!");

            var gp = new GradeGroup() { GradeGroupID = model.GradeGroupID };
            context.GradeGroupList.Attach(gp);
            gp.Name = model.Name;

            try
            {
                context.Entry(gp).Property(p => p.IsDisabled).IsModified = true;
                context.Entry(gp).Property(l => l.Name).IsModified = true;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Route("Admin/_AddGradeGroup/{SchoolID:int?}")]
        public ActionResult _AddGradeGroup(int? SchoolID)
        {
            var model = new GradeGroupViewModel { SchoolID = SchoolID };

            if (SchoolID.HasValue)
                model.SchoolName = new IEContext().SchoolList.Where(l => l.SchoolID == SchoolID).Select(l => l.Name).FirstOrDefault();

            return PartialView(model);
        }

        public ActionResult CreateGradeGroup(GradeGroupViewModel model)
        {
            var context = new IEContext();

            if (model.SchoolID.HasValue && context.GradeGroupList.Any(l => l.SchoolID == model.SchoolID && l.Name == model.Name))
                return DefaultErrorAlert("A Grade Group with this name already exists!");

            var gp = new GradeGroup { Name = model.Name, SchoolID = model.SchoolID };
            context.GradeGroupList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            if (model.IsLocal)
                return JSRedirect(Url.Action("GradeSetup", "Setup", new { gp.GradeGroupID }));
            else
                return JSRedirect(Url.Action("GradeSetup", "Admin", new { gp.GradeGroupID }));
        }

        [Route("Admin/GradeSetup/{GradeGroupID:int}")]
        public ActionResult GradeSetup(int GradeGroupID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Admin);
            if (!hVM.Has_ManageSchools) return RedirectToAction("Index", "Home");

            var model = context.GradeGroupList.Where(m => m.GradeGroupID == GradeGroupID).ProjectToFirst<GradeSetupViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _EditGrade(int GradeID)
        {
            var context = new IEContext();
            var model = context.GradeList.Where(m => m.GradeID == GradeID).ProjectToFirst<EditGradeViewModel>();
            model.IsUsed = context.ScoreEntryList.Any(t => t.GradeID == GradeID);

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteGrade(int GradeID)
        {
            var context = new IEContext();
            var gp = new Grade() { GradeID = GradeID };
            context.GradeList.Attach(gp);
            context.Entry(gp).State = EntityState.Deleted;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }
            return Json(GradeID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateGrade(EditGradeViewModel model)
        {
            var context = new IEContext();

            if (context.GradeList.Any(l => l.GradeGroupID == model.GradeGroupID && l.GradeID != model.GradeID && l.Name == model.Name))
                return DefaultErrorAlert("Another Grade with this name already exists!");

            var gp = new Grade() { GradeID = model.GradeID };
            context.GradeList.Attach(gp);
            Mapper.Map(model, gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            var nModel = new GradeViewModel { GradeID = gp.GradeID, Name = gp.Name, UpperBound = gp.UpperBound, LowerBound = gp.LowerBound, SummaryGradeID = gp.SummaryGradeID };
            return Json(nModel, JsonRequestBehavior.AllowGet);
        }

        [Route("Admin/_AddGrade/{GradeGroupID:int}")]
        public ActionResult _AddGrade(int GradeGroupID)
        {
            return PartialView();
        }

        public ActionResult CreateGrade(EditGradeViewModel model)
        {
            var context = new IEContext();

            if (context.GradeList.Any(l => l.GradeGroupID == model.GradeGroupID && l.Name == model.Name))
                return DefaultErrorAlert("A Grade with this name already exists!");

            var gp = new Grade();
            context.GradeList.Add(gp);
            Mapper.Map(model, gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            var nModel = new GradeViewModel { GradeID = gp.GradeID, Name = gp.Name, UpperBound = gp.UpperBound, LowerBound = gp.LowerBound, SummaryGradeID = gp.SummaryGradeID };
            return Json(nModel, JsonRequestBehavior.AllowGet);
        }

        [Route("Admin/GradeCount/{GradeGroupID:int}")]
        public ActionResult GradeCount(int GradeGroupID)
        {
            return Json(new IEContext().GradeList.Count(l => l.GradeGroupID == GradeGroupID), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Skill Groups

        public ActionResult SkillGroups()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Admin);
            if (!hVM.Has_ManageSchools) return RedirectToAction("Index", "Home");

            var model = new SkillGroupPageViewModel { SkillGroupList = context.SkillGroupList.ProjectToList<SkillGroupViewModel>() };

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _EditSkillGroup(int SkillGroupID)
        {
            var context = new IEContext();
            var model = context.SkillGroupList.Where(m => m.SkillGroupID == SkillGroupID).ProjectToFirst<SkillGroupViewModel>();
            //model.IsUsed = context.SkillEntryList.Any(t => t.Grade.SkillGroupID == SkillGroupID);

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteSkillGroup(int SkillGroupID)
        {
            var context = new IEContext();
            var gp = new SkillGroup() { SkillGroupID = SkillGroupID };
            context.SkillGroupList.Attach(gp);
            context.Entry(gp).State = EntityState.Deleted;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }
            return Json(SkillGroupID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateSkillGroup(SkillGroupViewModel model)
        {
            var context = new IEContext();

            if (!string.IsNullOrEmpty(model.Name) && context.SkillGroupList.Any(l => l.SkillGroupID != model.SkillGroupID && l.Name == model.Name))
                return DefaultErrorAlert("Another Skill Group with this name already exists!");

            var gp = new SkillGroup() { SkillGroupID = model.SkillGroupID };
            context.SkillGroupList.Attach(gp);
            Mapper.Map(model, gp);

            try
            {
                context.Entry(gp).Property(p => p.IsDisabled).IsModified = true;
                context.Entry(gp).Property(l => l.Name).IsModified = true;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Route("Admin/_AddSkillGroup/{SchoolID:int?}")]
        public ActionResult _AddSkillGroup(int? SchoolID)
        {
            var model = new SkillGroupViewModel { SchoolID = SchoolID };

            if (SchoolID.HasValue)
                model.SchoolName = new IEContext().SchoolList.Where(l => l.SchoolID == SchoolID).Select(l => l.Name).FirstOrDefault();

            return PartialView(model);
        }

        public ActionResult CreateSkillGroup(SkillGroupViewModel model)
        {
            var context = new IEContext();

            if (model.SchoolID.HasValue && context.SkillGroupList.Any(l => l.SchoolID == model.SchoolID && l.Name == model.Name))
                return DefaultErrorAlert("A Skill Group with this name already exists!");

            var gp = new SkillGroup();
            Mapper.Map(model, gp);
            context.SkillGroupList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            if (model.IsLocal)
                return JSRedirect(Url.Action("SkillSetup", "Setup", new { gp.SkillGroupID }));
            else
                return JSRedirect(Url.Action("SkillSetup", "Admin", new { gp.SkillGroupID }));
        }

        [Route("Admin/SkillSetup/{SkillGroupID:int}")]
        public ActionResult SkillSetup(int SkillGroupID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Admin);
            if (!hVM.Has_ManageSchools) return RedirectToAction("Index", "Home");

            var model = context.SkillGroupList.Where(m => m.SkillGroupID == SkillGroupID).ProjectToFirst<SkillSetupViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _EditSkillGrade(int SkillGradeID)
        {
            var context = new IEContext();
            var model = context.SkillGradeList.Where(m => m.SkillGradeID == SkillGradeID).ProjectToFirst<SkillGradeViewModel>();
            //model.IsUsed = context.SkillEntryList.Any(t => t.SkillGradeID == SkillGradeID);

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteSkillGrade(int SkillGradeID)
        {
            var context = new IEContext();
            var gp = new SkillGrade() { SkillGradeID = SkillGradeID };
            context.SkillGradeList.Attach(gp);
            context.Entry(gp).State = EntityState.Deleted;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }
            return Json(SkillGradeID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateSkillGrade(SkillGradeViewModel model)
        {
            var context = new IEContext();

            if (context.SkillGradeList.Any(l => l.SkillGroupID == model.SkillGroupID && l.SkillGradeID != model.SkillGradeID && l.Name == model.Name))
                return DefaultErrorAlert("Another Skill Grade with this name already exists!");

            var gp = new SkillGrade() { SkillGradeID = model.SkillGradeID };
            context.SkillGradeList.Attach(gp);
            Mapper.Map(model, gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Route("Admin/_AddSkillGrade/{SkillGroupID:int}")]
        public ActionResult _AddSkillGrade(int SkillGroupID)
        {
            return PartialView();
        }

        public ActionResult CreateSkillGrade(SkillGradeViewModel model)
        {
            var context = new IEContext();

            if (context.SkillGradeList.Any(l => l.SkillGroupID == model.SkillGroupID && l.Name == model.Name))
                return DefaultErrorAlert("A Skill Grade with this name already exists!");

            var gp = new SkillGrade();
            context.SkillGradeList.Add(gp);
            Mapper.Map(model, gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            model.SkillGradeID = gp.SkillGradeID;
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Route("Admin/SkillGradeCount/{SkillGroupID:int}")]
        public ActionResult SkillGradeCount(int SkillGroupID)
        {
            return Json(new IEContext().SkillGradeList.Count(l => l.SkillGroupID == SkillGroupID), JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region IPSEDU

        public ActionResult FormerStudents()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Admin);
            if (!hVM.Has_ManageStudents) return RedirectToAction("Index", "Home");

            var gpList = new IPSContext().INSTITUTIONs
                                .Select(t => new { t.INSTITUTION_NAME, t.INSTITUTION_ID }).ToList()
                                .Select(l => new SelectListItem { Value = l.INSTITUTION_ID.ToString(), Text = l.INSTITUTION_NAME }).ToList();

            var model = new FormerStudentPageViewModel
            {
                HeaderViewModel = hVM,

                InstitutionList = gpList
            };

            return View(model);
        }

        public ActionResult GetFormerStudentList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, FormerStudentFilterViewModel filterModel)
        {
            var query = new IPSContext().ED_STUDENT.AsQueryable();
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.DISPLAY_NAME.Contains(search) || p.STUDENT_CODE.Contains(search));
            }

            if (filterModel.InstitutionID.HasValue)
                query = query.Where(l => l.INSTITUTION_ID == filterModel.InstitutionID.Value);

            int filteredCount = query.Count();

            string sortStr = "";
            foreach (var col in requestModel.Columns.GetSortedColumns())
            {
                sortStr += sortStr != string.Empty ? "," : "";
                if (col.Data == "DISPLAY_NAME" || col.Data == "STUDENT_CODE")
                {
                    sortStr = sortStr + col.Data + (col.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    continue;
                }

                sortStr += SortDirStr(col.SortDirection);
            }

            var model = query.OrderBy(sortStr == string.Empty ? "STUDENT_ID asc" : sortStr).Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<FormerStudentViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ViewFormerStudent(int StudentID)
        {
            return PartialView(new IPSContext().ED_STUDENT.Where(m => m.STUDENT_ID == StudentID).ProjectToFirst<FormerStudentDisplayModel>());
        }

        public ActionResult _DownloadIPSResultFile(int StudentID, string StudentName)
        {
            var context = new IPSContext();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Year,TermNumber,Level,SubjectName,Score");

            var resl = context.ED_STUDENT_COURSE.Where(l => l.STUDENT_ID == StudentID)
                              .OrderBy(l => l.ACADEMIC_TERM_ID)
                              .ProjectToList<FormerResultModel>();

            foreach(var res in resl)
            {
                csv.AppendLine(res.CsvRow);
            }

            return File(new System.Text.UTF8Encoding().GetBytes(csv.ToString()), "text/csv", StudentName.Replace(" ", "-") + ".csv");
        }

        #endregion
    }
}