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
using System.IO.Compression;
using DataTables.Mvc;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;
using Microsoft.AspNet.Identity.Owin;
using System.Drawing;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;

namespace IEGen.Controllers
{
    [Authorize]
    public class DataController : BaseController
    {
        // GET: Data
        public ActionResult Index()
        {
            return Subjects();
        }

        #region Subjects
        public ActionResult Subjects()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new SubjectPageViewModel();

            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { l.TermID, l.Term.Name }).FirstOrDefault();
                if (def == null || def.TermID == null) return RedirectToAction("School");
                model.TermID = def.TermID.Value;
                model.TermName = def.Name;
            }
            else
            {
                var tm = GetCurrentTerm(context, hVM.SchoolID);
                if(tm == null) return RedirectToAction("Index", "Home");

                model.TermID = tm.TermID;
                model.TermName = tm.Name;
            }

            model.HeaderViewModel = hVM;

            model.ClassList = GetClassList(context, model.TermID);
            model.TeacherList = GetTeacherList(context, hVM.SchoolID);
            model.LevelList = GetLevelList(context, hVM.SchoolID);

            return View("Subjects", model);
        }

        public ActionResult GetSubjectList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, SubjectFilterViewModel filterModel)
        {
            var query = new IEContext().SubjectList.Where(l => l.Class.TermID == filterModel.TermID);
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Template.Name.Contains(search) || p.ResultName.Contains(search));
            }

            if (filterModel.ClassID.HasValue)
                query = query.Where(l => l.ClassID == filterModel.ClassID.Value);

            if (filterModel.LevelID.HasValue)
                query = query.Where(l => l.Class.Arm.ClassLevelID == filterModel.LevelID.Value);

            if (filterModel.TeacherID.HasValue)
                query = query.Where(l => l.TeacherID == filterModel.TeacherID.Value);

            if (filterModel.StatusID.HasValue)
            {
                switch ((EntryStatus)filterModel.StatusID.Value)
                {
                    case EntryStatus.New: query = query.Where(l => !l.EnteredByID.HasValue && !l.VerifiedByID.HasValue); break;
                    case EntryStatus.Entered: query = query.Where(l => l.EnteredByID.HasValue && !l.VerifiedByID.HasValue); break;
                    case EntryStatus.Verified: query = query.Where(l => l.VerifiedByID.HasValue); break;
                }
            }

            int filteredCount = query.Count();

            string sortStr = "";
            foreach (var col in requestModel.Columns.GetSortedColumns())
            {
                sortStr += sortStr != string.Empty ? "," : "";
                //if (col.Data == "Name" || col.Data == "ResultName")
                //{
                //    sortStr = sortStr + col.Data + (col.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                //    continue;
                //}

                switch (col.Data)
                {
                    case "TimeEnteredN": sortStr += "TimeEntered"; break;

                    case "TimeVerifiedN": sortStr += "TimeVerified"; break;

                    case "TeacherName": sortStr += "Teacher.Name"; break;

                    case "ClassName": sortStr += "Class.Arm.Name"; break;
                         
                    case "Name": sortStr += "Template.Name"; break;

                    case "ResultName": sortStr += "ResultName"; break;

                    case "PercentCorrected": sortStr += "PercentCorrected"; break;
                }

                sortStr += SortDirStr(col.SortDirection);
            }

            var defSort = "Class.Arm.Name asc, Template.Name asc";
            var model = query.OrderBy(sortStr == string.Empty ? defSort : sortStr + ", " + defSort)
                             .Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<SubjectEntryViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _DownloadSubjects(SubjectFilterViewModel filterModel, string Search)
        {
            GridView newView = new GridView();
            newView.AutoGenerateColumns = false;
            newView.ShowHeaderWhenEmpty = true;

            newView.Columns.Add(new BoundField { HeaderText = "#", DataField = "SubjectID" });
            newView.Columns.Add(new BoundField { HeaderText = "Level", DataField = "LevelName" });
            newView.Columns.Add(new BoundField { HeaderText = "Class", DataField = "ClassName" });
            newView.Columns.Add(new BoundField { HeaderText = "Name", DataField = "Name" });
            newView.Columns.Add(new BoundField { HeaderText = "Result Name", DataField = "ResultName" });
            newView.Columns.Add(new BoundField { HeaderText = "Category", DataField = "CategoryName" });
            newView.Columns.Add(new BoundField { HeaderText = "Teacher", DataField = "TeacherName" });
            newView.Columns.Add(new BoundField { HeaderText = "Entered By", DataField = "EnteredByName" });
            newView.Columns.Add(new BoundField { HeaderText = "Time Entered", DataField = "TimeEnteredN" });
            newView.Columns.Add(new BoundField { HeaderText = "Verified By", DataField = "VerifiedByName" });
            newView.Columns.Add(new BoundField { HeaderText = "Time Verified", DataField = "TimeVerifiedN" });
            newView.Columns.Add(new BoundField { HeaderText = "%C", DataField = "PercentCorrected" });

            var fileName = "IEPortalSubjects.xlsx";

            var context = new IEContext();
            var query = new IEContext().SubjectList.AsQueryable();

            if (Search != string.Empty)
            {
                string search = Search.Trim();
                query = query.Where(p => p.Template.Name.Contains(search) || p.ResultName.Contains(search));
            }

            if (filterModel.ClassID.HasValue)
                query = query.Where(l => l.ClassID == filterModel.ClassID.Value);

            if (filterModel.LevelID.HasValue)
                query = query.Where(l => l.Class.Arm.ClassLevelID == filterModel.LevelID.Value);

            if (filterModel.TeacherID.HasValue)
                query = query.Where(l => l.TeacherID == filterModel.TeacherID.Value);

            if (filterModel.StatusID.HasValue)
            {
                switch ((EntryStatus)filterModel.StatusID.Value)
                {
                    case EntryStatus.New: query = query.Where(l => !l.EnteredByID.HasValue && !l.VerifiedByID.HasValue); break;
                    case EntryStatus.Entered: query = query.Where(l => l.EnteredByID.HasValue && !l.VerifiedByID.HasValue); break;
                    case EntryStatus.Verified: query = query.Where(l => l.VerifiedByID.HasValue); break;
                }
            }

            newView.ItemType = "IEGen.Models.SubjectDownloadModel";
            newView.DataSource = query.ProjectToList<SubjectDownloadModel>().AsQueryable();

            newView.DataBind();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Subjects");
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

        [Route("Data/_ScoreEntry/{UserID:int}/")]
        public ActionResult _ScoreEntry(int SubjectID, int UserID)
        {
            var context = new IEContext();

            var model = new ScorePageViewModel { SubjectID = SubjectID, UserID = UserID };
            var sub = context.SubjectList.Where(m => m.SubjectID == SubjectID).Select(l => new { l.Template.Name, ClassName = l.Class.Arm.Name }).FirstOrDefault();
            if (sub == null)
                return null;

            model.Name = sub.Name;
            model.ClassName = sub.ClassName;

            return PartialView(model);
        }

        public ActionResult _ScoreEntryBody(int SubjectID, int UserID)
        {
            var context = new IEContext();

            var model = context.SubjectList.Where(m => m.SubjectID == SubjectID).ProjectToFirst<ScorePageViewModel>();
            model.UserID = UserID;
            if (model.NoCA)
            {
                model.MaxCAScore = 0;
                model.MaxExamScore = 100;
            }

            var nsList = context.StudentList.Where(l => l.ClassID == model.ClassID && !context.ScoreEntryList.Any(e => e.SubjectID == SubjectID && e.StudentID == l.StudentID))
                                .Select(l => new { l.StudentID, l.Surname, l.FirstName }).ToList();

            foreach(var sc in model.Scores)
            {
                sc.MaxCAScore = model.MaxCAScore;
                sc.MaxExamScore = model.MaxExamScore;
            }

            byte sn = model.Scores.Select(l => l.SerialNumber).DefaultIfEmpty((byte)0).Max();

            if (nsList != null && nsList.Count > 0)
            {
                foreach (var std in nsList.OrderBy(l => l.Surname).ThenBy(l => l.FirstName))
                {
                    var ne = new ScoreEntry { SubjectID = SubjectID, StudentID = std.StudentID, SerialNumber = ++sn };
                    var nem = new ScoreEntryViewModel { FirstName = std.FirstName, Surname = std.Surname, StudentID = ne.StudentID, SerialNumber = ne.SerialNumber };
                    nem.MaxCAScore = model.MaxCAScore;
                    nem.MaxExamScore = model.MaxExamScore;

                    model.Scores.Add(nem);
                    context.ScoreEntryList.Add(ne);
                }

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    General.AlertElmah(ex);
                }
            }

            return PartialView("_ScoreEntryBody", model);
        }

        public async Task<ActionResult> AllowCAEntry(int SubjectID, int UserID)
        {
            var context = new IEContext();

            var sub = new Subject { SubjectID = SubjectID };
            context.SubjectList.Attach(sub);

            context.Entry(sub).Property(l => l.NoCA).IsModified = true;
            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
            }

            return _ScoreEntryBody(SubjectID, UserID);
        }

        public async Task<ActionResult> SetTotalEntry(int SubjectID, int UserID)
        {
            var context = new IEContext();

            var sub = new Subject { SubjectID = SubjectID };
            context.SubjectList.Attach(sub);

            sub.NoCA = true;

            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
            }

            return _ScoreEntryBody(SubjectID, UserID);
        }

        public async Task<ActionResult> EnterScores(ScorePageViewModel model)
        {
            var context = new IEContext();

            bool hasScores = false;

            foreach(var se in model.Scores)
            {
                var gp = new ScoreEntry() { StudentID = se.StudentID, SubjectID = model.SubjectID };
                context.ScoreEntryList.Attach(gp);
                Mapper.Map(se, gp);

                context.Entry(gp).Property(l => l.CAScore).IsModified = true;
                context.Entry(gp).Property(l => l.ExamScore).IsModified = true;

                if (gp.CAScore.HasValue || gp.ExamScore.HasValue)
                    hasScores = true;
            }

            var sb = new Subject { SubjectID = model.SubjectID };
            context.SubjectList.Attach(sb);

            if(hasScores)
            {
                sb.EnteredByID = model.UserID;
                sb.TimeEntered = DateTime.Now;
            }
            else
            {
                context.Entry(sb).Property(l => l.EnteredByID).IsModified = true;
                context.Entry(sb).Property(l => l.TimeEntered).IsModified = true;
            }

            context.Entry(sb).Property(l => l.VerifiedByID).IsModified = true;

            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
            }

            return DefaultSuccessAlert("Score entry for " + model.DisplayName + " was completed successfully!");
        }

        [Route("Data/_ScoreVerify/{UserID:int}/")]
        public ActionResult _ScoreVerify(int SubjectID, int UserID)
        {
            var context = new IEContext();

            var model = context.SubjectList.Where(m => m.SubjectID == SubjectID).ProjectToFirst<VerifyPageViewModel>();
            model.UserID = UserID;
            if (model.NoCA)
            {
                model.MaxCAScore = 0;
                model.MaxExamScore = 100;
            }

            var nsList = context.StudentList.Where(l => l.ClassID == model.ClassID && !context.ScoreEntryList.Any(e => e.SubjectID == SubjectID && e.StudentID == l.StudentID))
                                .Select(l => new { l.StudentID, l.Surname, l.FirstName }).ToList();

            if(model.VerifiedPreviously)
            {
                foreach (var sc in model.Scores)
                {
                    sc.MaxCAScore = model.MaxCAScore;
                    sc.MaxExamScore = model.MaxExamScore;
                    sc.CAScoreV = sc.CAScore;
                    sc.ExamScoreV = sc.ExamScore;
                }
            }
            else
            {
                foreach (var sc in model.Scores)
                {
                    sc.MaxCAScore = model.MaxCAScore;
                    sc.MaxExamScore = model.MaxExamScore;
                }
            }

            byte sn = model.Scores.Select(l => l.SerialNumber).DefaultIfEmpty((byte)0).Max();

            if (nsList.Count > 0)
            {
                foreach (var std in nsList.OrderBy(l => l.Surname).ThenBy(l => l.FirstName))
                {
                    var ne = new ScoreEntry { SubjectID = SubjectID, StudentID = std.StudentID, SerialNumber = ++sn };
                    var nem = new ScoreVerifyViewModel { FirstName = std.FirstName, Surname = std.Surname, StudentID = ne.StudentID, SerialNumber = ne.SerialNumber };
                    nem.MaxCAScore = model.MaxCAScore;
                    nem.MaxExamScore = model.MaxExamScore;

                    model.Scores.Add(nem);
                    context.ScoreEntryList.Add(ne);
                }

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    General.AlertElmah(ex);
                }
            }

            return PartialView(model);
        }

        public async Task<ActionResult> VerifyScores(VerifyPageViewModel model)
        {
            var context = new IEContext();

            var mModel = new ModifyPageViewModel { Scores = new List<ScoreModifyViewModel>() };

            foreach (var sc in model.Scores)
            {
                if (sc.CAScore != sc.CAScoreV || sc.ExamScore != sc.ExamScoreV)
                {
                    var msc = new ScoreModifyViewModel();
                    Mapper.Map(sc, msc);

                    mModel.Scores.Add(msc);
                }
            }

            if(mModel.Scores.Count > 0)
            {
                Mapper.Map(model, mModel);
                mModel.ScoreCount = model.Scores.Count;
                Response.StatusCode = 480;

                ModelState.Clear();
                return PartialView("_ScoreModify", mModel);
            }

            var sb = new Subject { SubjectID = model.SubjectID };
            context.SubjectList.Attach(sb);

            sb.VerifiedByID = model.UserID;
            sb.TimeVerified = DateTime.Now;
            //sb.AverageScore = model.Scores.Average(l => l.Total).Value;
            //sb.BestStudentID = model.Scores.OrderByDescending(l => l.Total).Select(l => l.StudentID).First();

            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();

                AssignScoreGrades(model.SubjectID, model.GradeGroupID);
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert("Score validation for " + model.DisplayName + " was completed successfully!");
        }

        public async Task<ActionResult> ModifyScores(ModifyPageViewModel model)
        {
            var context = new IEContext();

            var cor = model.Corrected;

            var mModel = new ModifyPageViewModel { Scores = new List<ScoreModifyViewModel>() };
            Mapper.Map(model, mModel);

            foreach (var sc in model.Scores)
            {
                if (sc.CAScore == sc.CAScoreM && sc.ExamScore == sc.ExamScoreM)
                    continue;

                if (sc.CAScoreV == sc.CAScoreM && sc.ExamScoreV == sc.ExamScoreM)
                {
                    cor++;

                    var nsc = new ScoreEntry { StudentID = sc.StudentID, SubjectID = model.SubjectID };
                    context.ScoreEntryList.Attach(nsc);

                    nsc.CAScore = sc.CAScoreM;
                    nsc.ExamScore = sc.ExamScoreM;

                    context.Entry(nsc).Property(l => l.CAScore).IsModified = true;
                    context.Entry(nsc).Property(l => l.ExamScore).IsModified = true;

                    continue;
                }

                var msc = new ScoreModifyViewModel();
                Mapper.Map(sc, msc);

                msc.CAScoreV = sc.CAScoreM;
                msc.ExamScoreV = sc.ExamScoreM;
                msc.CAScoreM = 0;
                msc.ExamScoreM = 0;

                mModel.Scores.Add(msc);
            }


            mModel.Corrected = cor;
            
            if (mModel.Scores.Count > 0)
            {
                try
                {
                    context.Configuration.ValidateOnSaveEnabled = false;
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    General.AlertElmah(ex);
                    return DefaultErrorAlert();
                }

                Response.StatusCode = 480;

                ModelState.Clear();
                return PartialView("_ScoreModify", mModel);
            }

            var sb = new Subject { SubjectID = model.SubjectID };
            context.SubjectList.Attach(sb);

            sb.VerifiedByID = model.UserID;
            sb.TimeVerified = DateTime.Now;
            sb.PercentCorrected = (byte)((cor * 100) / model.ScoreCount);

            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();

                AssignScoreGrades(model.SubjectID, model.GradeGroupID);
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert("Score validation for " + model.DisplayName + " was completed successfully!");
        }

        [Route("Data/_DeleteScores/{TermID:int}/")]
        public ActionResult _DeleteScores(int TermID)
        {
            return PartialView(new DeleteScoresViewModel { Classes = new IEContext().ClassList.Where(l => l.TermID == TermID).ProjectToList<ClassDataModel>() });
        }


        [HttpPost]
        public ActionResult DeleteScores(int TermID)
        {
            DeleteTermScores(TermID);

            return Json("", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult DeleteRearrangeScores(int TermID)
        {
            DeleteRearrangeTermScores(TermID);

            return Json("", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Faculty Comments

        public ActionResult TeacherComments()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new CommentsPageViewModel();

            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { l.TermID, l.Term.Name }).FirstOrDefault();
                if (def == null || def.TermID == null) return RedirectToAction("School");
                model.TermID = def.TermID.Value;
                model.TermName = def.Name;
            }
            else
            {
                var tm = GetCurrentTerm(context, hVM.SchoolID);
                if (tm == null) return RedirectToAction("Index", "Home");

                model.TermID = tm.TermID;
                model.TermName = tm.Name;
            }

            model.HeaderViewModel = hVM;

            model.HasScores = context.ScoreEntryList.Any(l => l.Subject.Class.TermID == model.TermID && l.ExamScore.HasValue);
            model.CommentTypeID = (byte)CommentType.ClassTeacher;

            model.ClassList = GetClassList(context, model.TermID);
            model.LevelList = GetLevelList(context, hVM.SchoolID);

            return View(model);
        }

        public ActionResult PrincipalComments()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new CommentsPageViewModel();

            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { l.TermID, l.Term.Name, l.School.TypeID }).FirstOrDefault();
                if (def == null || def.TermID == null) return RedirectToAction("School");
                model.TermID = def.TermID.Value;
                model.TermName = def.Name;

                if (def.TypeID == (byte)SchoolType.NurseryPrimary)
                    model.CommentTypeID = (byte)CommentType.HeadTeacher;
                else
                    model.CommentTypeID = (byte)CommentType.Principal;
            }
            else
            {
                var tm = GetCurrentTerm(context, hVM.SchoolID);
                if (tm == null) return RedirectToAction("Index", "Home");

                model.TermID = tm.TermID;
                model.TermName = tm.Name;
                model.CommentTypeID = (byte)CommentType.Principal;
            }

            model.HeaderViewModel = hVM;

            model.HasScores = context.ScoreEntryList.Any(l => l.Subject.Class.TermID == model.TermID && l.ExamScore.HasValue);

            model.ClassList = GetClassList(context, model.TermID);
            model.LevelList = GetLevelList(context, hVM.SchoolID);

            return View(model);
        }

        public ActionResult GetCTCommentList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, CommentsFilterViewModel filterModel)
        {
            var query = new IEContext().StudentList.Where(l => l.Class.TermID == filterModel.TermID);
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.FirstName.Contains(search) || p.OtherName.Contains(search) || p.Surname.Contains(search));
            }

            if (filterModel.ClassID.HasValue)
                query = query.Where(l => l.ClassID == filterModel.ClassID.Value);

            if (filterModel.LevelID.HasValue)
                query = query.Where(l => l.Class.Arm.ClassLevelID == filterModel.LevelID.Value);

            int filteredCount = query.Count();

            string sortStr = "";
            foreach (var col in requestModel.Columns.GetSortedColumns())
            {
                sortStr += sortStr != string.Empty ? "," : "";
                if (col.Data == "TeacherComment" || col.Data == "PrincipalComment")
                {
                    sortStr = sortStr + col.Data + (col.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    continue;
                }

                switch (col.Data)
                {
                    case "DisplayName": sortStr += "Surname"; break;

                    case "ClassName": sortStr += "Class.Arm.Name"; break;
                }

                sortStr += SortDirStr(col.SortDirection);
            }

            var defSort = "Class.Arm.Name asc, Surname asc";
            var model = query.OrderBy(sortStr == string.Empty ? defSort : sortStr + ", " + defSort)
                             .Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<CTCommentViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPCommentList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, CommentsFilterViewModel filterModel)
        {
            var query = new IEContext().StudentList.Where(l => l.Class.TermID == filterModel.TermID);
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.FirstName.Contains(search) || p.OtherName.Contains(search) || p.Surname.Contains(search));
            }

            if (filterModel.ClassID.HasValue)
                query = query.Where(l => l.ClassID == filterModel.ClassID.Value);

            if (filterModel.LevelID.HasValue)
                query = query.Where(l => l.Class.Arm.ClassLevelID == filterModel.LevelID.Value);

            int filteredCount = query.Count();

            string sortStr = "";
            foreach (var col in requestModel.Columns.GetSortedColumns())
            {
                sortStr += sortStr != string.Empty ? "," : "";
                if (col.Data == "TeacherComment" || col.Data == "PrincipalComment")
                {
                    sortStr = sortStr + col.Data + (col.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    continue;
                }

                switch (col.Data)
                {
                    case "DisplayName": sortStr += "Surname"; break;

                    case "ClassName": sortStr += "Class.Arm.Name"; break;
                }

                sortStr += SortDirStr(col.SortDirection);
            }

            var defSort = "Class.Arm.Name asc, Surname asc";
            var model = query.OrderBy(sortStr == string.Empty ? defSort : sortStr + ", " + defSort)
                             .Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<PCommentViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _DownloadComments(SubjectFilterViewModel filterModel, string Search)
        {
            GridView newView = new GridView();
            newView.AutoGenerateColumns = false;
            newView.ShowHeaderWhenEmpty = true;

            newView.Columns.Add(new BoundField { HeaderText = "#", DataField = "StudentID" });
            newView.Columns.Add(new BoundField { HeaderText = "Class", DataField = "ClassName" });
            newView.Columns.Add(new BoundField { HeaderText = "Student", DataField = "DisplayName" });
            newView.Columns.Add(new BoundField { HeaderText = "Sex", DataField = "Sex" });
            newView.Columns.Add(new BoundField { HeaderText = "Class Teacher's Comment", DataField = "TeacherComment" });
            newView.Columns.Add(new BoundField { HeaderText = "Principal's Comment", DataField = "PrincipalComment" });

            var fileName = "IEPortalComments.xlsx";

            var context = new IEContext();
            var query = new IEContext().StudentList.AsQueryable();

            if (Search != string.Empty)
            {
                string search = Search.Trim();
                query = query.Where(p => p.FirstName.Contains(search) || p.OtherName.Contains(search) || p.Surname.Contains(search));
            }

            if (filterModel.ClassID.HasValue)
                query = query.Where(l => l.ClassID == filterModel.ClassID.Value);

            if (filterModel.LevelID.HasValue)
                query = query.Where(l => l.Class.Arm.ClassLevelID == filterModel.LevelID.Value);

            newView.ItemType = "IEGen.Models.CommentsMiniModel";
            newView.DataSource = query.ProjectToList<CommentsMiniModel>().AsQueryable();

            newView.DataBind();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Faculty Comments");
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

        public ActionResult _EditComments(int StudentID)
        {
            var context = new IEContext();
            var model = context.StudentList.Where(m => m.StudentID == StudentID).ProjectToFirst<CommentsViewModel>();

            var res = context.StudentResultList.Where(l => l.StudentID == StudentID && l.ClassID == model.ClassID)
                             .Select(l => new { l.TeacherComment, l.PrincipalComment }).FirstOrDefault();

            if(res != null)
            {
                model.HasResult = true;
                model.CTComment = res.TeacherComment;
                model.PComment = res.PrincipalComment;
            }

            if (!string.IsNullOrEmpty(model.GuidString))
                model.PhotoSrc = "data:image/png;base64," + Convert.ToBase64String(new Student { GuidString = model.GuidString }.DownloadFile());

            return PartialView(model);
        }

        public ActionResult UpdateFacultyComments(CommentsViewModel model)
        {
            var context = new IEContext();

            var gp = new Student() { StudentID = model.StudentID };
            context.StudentList.Attach(gp);
            Mapper.Map(model, gp);

            if(model.UpdateResult)
            {
                var sr = new StudentResult { StudentID = model.StudentID, ClassID = model.ClassID };
                context.StudentResultList.Attach(sr);
                sr.TeacherComment = model.TeacherComment;
                if (!string.IsNullOrWhiteSpace(model.PrincipalComment)) sr.PrincipalComment = model.PrincipalComment;
            }

            try
            {
                context.Entry(gp).Property(l => l.TeacherComment).IsModified = true;
                context.Entry(gp).Property(l => l.PrincipalComment).IsModified = true;

                context.Configuration.ValidateOnSaveEnabled = false;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert("Faculty Comments for " + model.DisplayName + " were updated successfully!");
        }

        [Route("Data/ClassCommentsCT/{ClassID:int}/")]
        public ActionResult ClassCommentsCT(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.ClassList.Where(l => l.ClassID == ClassID).ProjectToFirst<ClassCommentsPageViewModel>();

            if (hVM.SchoolID != model.SchoolID) return RedirectToAction("Index", "Home");

            model.HeaderViewModel = hVM;

            //PrepareNewClassComments(ClassID);

            model.CTComments = context.StudentList.Where(l => l.ClassID == ClassID).ProjectToList<CTCommentViewModel>().OrderBy(l => l.DisplayName).ToList();

            return View(model);
        }

        public async Task<ActionResult> SaveClassCommentsCT(ClassCommentsPageViewModel model)
        {
            var context = new IEContext();

            foreach (var se in model.CTComments)
            {
                var gp = new Student() { StudentID = se.StudentID };
                context.StudentList.Attach(gp);
                gp.TeacherComment = se.TeacherComment;

                context.Entry(gp).Property(p => p.TeacherComment).IsModified = true;
            }

            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
            }

            return DefaultSuccessAlert("Class Teacher comments for " + model.Name + " were saved successfully!");
        }

        [Route("Data/ClassCommentsP/{ClassID:int}/")]
        public ActionResult ClassCommentsP(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.ClassList.Where(l => l.ClassID == ClassID).ProjectToFirst<ClassCommentsPageViewModel>();

            if (hVM.SchoolID != model.SchoolID) return RedirectToAction("Index", "Home");

            model.HeaderViewModel = hVM;

            //PrepareNewClassComments(ClassID);

            model.PComments = context.StudentList.Where(l => l.ClassID == ClassID).ProjectToList<PCommentViewModel>().OrderBy(l => l.DisplayName).ToList();

            return View(model);
        }

        public async Task<ActionResult> SaveClassCommentsP(ClassCommentsPageViewModel model)
        {
            var context = new IEContext();

            foreach (var se in model.PComments)
            {
                var gp = new Student() { StudentID = se.StudentID };
                context.StudentList.Attach(gp);
                gp.PrincipalComment = se.PrincipalComment;

                context.Entry(gp).Property(p => p.PrincipalComment).IsModified = true;
            }

            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
            }

            return DefaultSuccessAlert("Principal comments for " + model.Name + " were saved successfully!");
        }

        [Route("Data/CommentSheetsP/{TermID:int}/{TypeID:int}/")]
        public ActionResult CommentSheetsP(int TermID, int TypeID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var term = context.TermList.Where(l => l.TermID == TermID).Select(l => new { l.Name, l.SchoolID, SchoolName = l.School.Name }).First();
            if (hVM.SchoolID != term.SchoolID) return null;

            var model = new CommentSheetsPageViewModel { TermName = term.Name, SchoolName = term.SchoolName, CommentTypeID = (byte)TypeID };

            var classes = context.ClassList.Where(l => l.TermID == TermID).Select(l => l.ClassID).ToList();
            model.Sheets = new List<ClassCommentSheetsViewModel>();

            foreach (var classid in classes)
            {
                model.Sheets.Add(context.ClassList.Where(l => l.ClassID == classid).ProjectToFirst<ClassCommentSheetsViewModel>());
            }


            model.DisplaySheets = new List<ClassCommentSheetsViewModel>();
            var brk = 20;
            var blankStart = 0;
            foreach (var s in model.Sheets)
            {
                byte cnt = 0;
                foreach (var e in s.Entries.OrderBy(l => l.StudentName))
                    e.SerialNumber = ++cnt;

                var ns = new ClassCommentSheetsViewModel() { Page = 1, Entries = s.Entries.Where(l => l.SerialNumber <= brk).OrderBy(l => l.SerialNumber).ToList() };
                Mapper.Map(s, ns);
                var scnt = s.Entries.Count;
                if (scnt == 0) continue;
                if (scnt > brk)
                {
                    var ecnt = brk;
                    while (ecnt < scnt)
                    {
                        ns.Page = (ecnt / brk);
                        model.DisplaySheets.Add(ns);

                        ns = new ClassCommentSheetsViewModel
                        {
                            Name = s.Name,
                            Entries = s.Entries.Where(l => l.SerialNumber > ecnt && l.SerialNumber <= ecnt + brk).OrderBy(l => l.SerialNumber).ToList()
                        };

                        ecnt += brk;
                    }

                    ns.Page = (ecnt / brk);
                }

                blankStart = ns.Entries.Max(l => l.SerialNumber) + 1;

                for (int i = blankStart; i <= brk * ns.Page; i++) ns.Entries.Add(new ScoreSheetEntryViewModel { SerialNumber = (byte)i });

                model.DisplaySheets.Add(ns);
            }

            return View(model);
        }

        [Route("Data/CommentSheetsE/{TermID:int}/{TypeID:int}/")]
        public ActionResult CommentSheetsE(int TermID, int TypeID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.TermList.Where(l => l.TermID == TermID).ProjectToFirst<CommentSheetsExcel>();
            model.CommentTypeID = (byte)TypeID;

            if (hVM.SchoolID != model.SchoolID) return RedirectToAction("Index", "Home");

            ExcelPackage excel = new ExcelPackage();

            foreach (var cl in model.Sheets.OrderBy(l => l.ClassLevelID).ThenBy(l => l.Name))
            {
                var workSheet = excel.Workbook.Worksheets.Add(cl.Name);

                workSheet.Cells[1, 1].Value = "## Please DO NOT modify the ID column. DO NOT DELETE or REPLACE any existing student with a new student.";
                workSheet.Cells[1, 1, 1, 13].Merge = true;
                workSheet.Cells[2, 1].Value = "## For new students, enter only the surname, first name and Comments ON A NEW ROW. Do not leave any blank rows between students.";
                workSheet.Cells[2, 1, 2, 13].Merge = true;
                workSheet.Cells[3, 1].Value = "## ";
                workSheet.Cells[3, 1, 3, 13].Merge = true;

                workSheet.Cells[5, 1].Value = "CID";
                workSheet.Cells[5, 1].Style.Font.Bold = true;
                workSheet.Cells[5, 2].Value = cl.ClassID;

                workSheet.Cells[5, 5].Value = "Complete?";
                workSheet.Cells[5, 5].Style.Font.Bold = true;
                workSheet.Cells[5, 6].Value = "No";

                workSheet.Cells[6, 1].Value = "Class";
                workSheet.Cells[6, 1].Style.Font.Bold = true;
                workSheet.Cells[6, 2].Value = cl.Name;
                workSheet.Cells[6, 2, 6, 6].Merge = true;

                workSheet.Cells[8, 1].Value = "Level";
                workSheet.Cells[8, 1].Style.Font.Bold = true;
                workSheet.Cells[8, 2].Value = cl.Level;
                workSheet.Cells[8, 2, 8, 6].Merge = true;

                workSheet.Cells[10, 1].Value = "ID";
                workSheet.Cells[10, 2].Value = "Surname";
                workSheet.Cells[10, 3].Value = "First Name";
                workSheet.Cells[10, 4].Value = model.CommentTypeName;
                workSheet.Cells[10, 4, 10, 13].Merge = true;


                using (var range = workSheet.Cells[10, 1, 10, 13])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.WhiteSmoke);
                    range.Style.ShrinkToFit = false;
                }

                int i = 11;
                foreach (var e in cl.Entries.OrderBy(l => l.Surname).ThenBy(l => l.FirstName))
                {
                    workSheet.Cells[i, 1].Value = e.StudentID;
                    workSheet.Cells[i, 2].Value = e.Surname;
                    workSheet.Cells[i, 3].Value = e.FirstName;
                    workSheet.Cells[i, 4, i, 13].Merge = true;

                    i++;
                }

                for (var c = 1; c <= 13; c++) workSheet.Cells[10, c].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                workSheet.Cells.AutoFitColumns();
            }

            var ms = new MemoryStream();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            excel.SaveAs(ms);
            ms.Position = 0;

            string fileName = model.CommentTypeName + " Sheet " + TermID.ToString() + " - " +
                              model.SchoolYear.ToString() + "-" + (model.SchoolYear + 1).ToString() + "-" + model.TermNumber.ToString() + ".xlsx";

            return File(ms, contentType, fileName);
        }

        [Route("Data/_StartUploadComments/{TermID:int}/{TypeID:int}/")]
        public ActionResult _StartUploadComments(int TermID, int TypeID)
        {
            var model = new IEContext().TermList.Where(m => m.TermID == TermID).ProjectToFirst<UploadCommentsViewModel>();
            model.CommentTypeID = (byte)TypeID;

            return PartialView(model);
        }

        public ActionResult UploadComments(UploadCommentsViewModel model)
        {
            var lfile = Request.Files["CommentFile"];
            var lLength = 0;
            if (lfile != null)
            {
                lLength = lfile.ContentLength;
                if (lLength == 0)
                    return DefaultErrorAlert("The Comment File is Invalid!");
            }
            else
                return DefaultErrorAlert("The Comment File is Invalid!");

            var context = new IEContext();
            var initContext = new IEContext();

            var classIDList = context.ClassList.Where(l => l.TermID == model.TermID).Select(l => l.ClassID).ToList();

            var loadedClassIDs = new List<int>();

            MemoryStream lms = new MemoryStream();
            lfile.InputStream.CopyTo(lms);

            var uploads = 0;

            try
            {
                using (var excel = new ExcelPackage(lms))
                {
                    foreach (var sheet in excel.Workbook.Worksheets)
                    {
                        if (sheet.Cells[5, 1].Value.ToString() != "CID")
                            return DefaultErrorAlert("The Comment File is Invalid! Current Sheet: " + sheet.Name);

                        int classID = 0;
                        var classObj = sheet.Cells[5, 2].Value;
                        if (!(classObj != null && int.TryParse(classObj.ToString(), out classID)))
                            return DefaultErrorAlert("The Comment File is Invalid! Invalid Class ID in Current Sheet: " + sheet.Name);

                        if (!classIDList.Contains(classID)) continue;

                        if (loadedClassIDs.Contains(classID))
                            return DefaultErrorAlert("The Comment File is Invalid! Duplicate Subject ID in Current Sheet: " + sheet.Name);

                        var rowID = 11;
                        var surnameObj = sheet.Cells[rowID, 2].Value;
                        if (surnameObj == null)
                            return DefaultErrorAlert("The Comment File is Invalid! No students in Current Sheet: " + sheet.Name);

                        var surname = surnameObj.ToString();

                        bool hasValidScores = false;
                        while (!string.IsNullOrWhiteSpace(surname))
                        {
                            int studentID = 0;
                            var studentObj = sheet.Cells[rowID, 1].Value;
                            if (!(studentObj != null && int.TryParse(studentObj.ToString(), out studentID)))
                                return DefaultErrorAlert("There are new Students. Please create them and input their IDs! Current Sheet: " + sheet.Name + "; Surname: " + surname);

                            //get a list of students in the class in order to detect foreign student IDs
                            var studentIDList = context.StudentList.Where(l => l.ClassID == classID).Select(l => l.StudentID).ToList();

                            if (studentIDList.Contains(studentID))
                            {
                                var se = new Student() { StudentID = studentID };
                                context.StudentList.Attach(se);

                                var commentObj = sheet.Cells[rowID, 4].Value;

                                if (commentObj != null)
                                {
                                    var comment = commentObj.ToString();

                                    if (model.CommentTypeID == (byte)CommentType.ClassTeacher)
                                    {
                                        se.TeacherComment = comment;
                                        context.Entry(se).Property(l => l.TeacherComment).IsModified = true;
                                    }
                                    else
                                    {
                                        se.PrincipalComment = comment;
                                        context.Entry(se).Property(l => l.PrincipalComment).IsModified = true;
                                    }

                                    if (!string.IsNullOrWhiteSpace(comment)) uploads++;
                                }

                            }

                            rowID++;

                            surnameObj = sheet.Cells[rowID, 2].Value;
                            if (surnameObj == null) break;

                            surname = surnameObj.ToString();
                        }

                        if (hasValidScores)
                        {
                            loadedClassIDs.Add(classID);
                        }
                    }
                }

                context.Configuration.ValidateOnSaveEnabled = false;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert(model.CommentTypeName + " for " + uploads.ToString() + " students were updated successfully!");
        }

        #endregion

        #region Qualitative Skills

        public ActionResult Skills()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new SkillPageViewModel();

            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { l.TermID, l.Term.Name }).FirstOrDefault();
                if (def == null || def.TermID == null) return RedirectToAction("School");
                model.TermID = def.TermID.Value;
                model.TermName = def.Name;
            }
            else
            {
                var tm = GetCurrentTerm(context, hVM.SchoolID);
                if (tm == null) return RedirectToAction("Index", "Home");

                model.TermID = tm.TermID;
                model.TermName = tm.Name;
            }

            model.HeaderViewModel = hVM;

            model.Classes = context.ClassList.Where(l => l.TermID == model.TermID).ProjectToList<ClassSkillViewModel>();

            return View(model);
        }

        [Route("Data/SkillEntry/{ClassID:int}/")]
        public ActionResult SkillEntry(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.ClassList.Where(l => l.ClassID == ClassID).ProjectToFirst<ClassSkillPageViewModel>();
            model.MinScore = model.Grades.Min(l => l.GradeNumber);
            model.MaxScore = model.Grades.Max(l => l.GradeNumber);

            if (hVM.SchoolID != model.SchoolID) return RedirectToAction("Index", "Home");

            model.HeaderViewModel = hVM;

            // sort the skill entries after verifying that user is in the relevant school
            PrepareNewClassSkills(ClassID);

            model.Entries = context.StudentSkillsList.Where(l => l.ClassID == ClassID).OrderBy(l => l.SerialNumber).ProjectToList<SkillEntryViewModel>();
            model.Entries.ForEach(l => l.MaxScore = model.MaxScore);

            return View(model);
        }

        public async Task<ActionResult> SaveSkillEntry(ClassSkillPageViewModel model)
        {
            var context = new IEContext();

            foreach (var se in model.Entries)
            {
                var gp = new StudentSkills() { StudentID = se.StudentID, ClassID = model.ClassID };
                context.StudentSkillsList.Attach(gp);
                Mapper.Map(se, gp);

                context.Entry(gp).Property(p => p.SkillScore1).IsModified = true;
                context.Entry(gp).Property(p => p.SkillScore2).IsModified = true;
                context.Entry(gp).Property(p => p.SkillScore3).IsModified = true;
                context.Entry(gp).Property(p => p.SkillScore4).IsModified = true;
                context.Entry(gp).Property(p => p.SkillScore5).IsModified = true;
                context.Entry(gp).Property(p => p.SkillScore6).IsModified = true;
            }

            var cl = new Class { ClassID = model.ClassID };
            context.ClassList.Attach(cl);
            cl.VerifySkills = true;

            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
            }

            return JSRedirect(Url.Action("SkillVerify", "Data", new { model.ClassID }));
            //return DefaultSuccessAlert("Qualitative Skill entries for " + model.Name + " were saved successfully!");
        }

        [Route("Data/SkillVerify/{ClassID:int}/")]
        public ActionResult SkillVerify(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.ClassList.Where(l => l.ClassID == ClassID).ProjectToFirst<ClassSkillVPageViewModel>();
            model.MinScore = model.Grades.Min(l => l.GradeNumber);
            model.MaxScore = model.Grades.Max(l => l.GradeNumber);

            if (hVM.SchoolID != model.SchoolID) return RedirectToAction("Index", "Home");

            model.HeaderViewModel = hVM;

            PrepareNewClassSkills(ClassID);

            model.Entries = context.StudentSkillsList.Where(l => l.ClassID == ClassID).OrderBy(l => l.SerialNumber).ProjectToList<SkillVerifyViewModel>();
            model.Entries.ForEach(l => l.MaxScore = model.MaxScore);

            return View(model);
        }

        public async Task<ActionResult> VerifySkillEntry(ClassSkillVPageViewModel model)
        {
            var context = new IEContext();


            var bEntries = new List<SkillModifyViewModel>();
            foreach (var sc in model.Entries)
            {
                if (sc.SkillScore1 != sc.SkillScore1V || sc.SkillScore2 != sc.SkillScore2V || sc.SkillScore3 != sc.SkillScore3V || sc.SkillScore4 != sc.SkillScore4V || 
                    sc.SkillScore5 != sc.SkillScore5V || sc.SkillScore6 != sc.SkillScore6V)
                {
                    var msc = new SkillModifyViewModel();
                    Mapper.Map(sc, msc);

                    bEntries.Add(msc);
                }
            }

            if (bEntries.Count > 0)
            {
                var mModel = context.ClassList.Where(l => l.ClassID == model.ClassID).ProjectToFirst<ClassSkillMViewModel>();

                mModel.Entries = bEntries;
                mModel.MinScore = model.MinScore;
                mModel.MaxScore = model.MaxScore;
                Response.StatusCode = 480;

                ModelState.Clear();
                return PartialView("_SkillModify", mModel);
            }

            var cl = new Class { ClassID = model.ClassID };
            context.ClassList.Attach(cl);
            context.Entry(cl).Property(l => l.VerifySkills).IsModified = true;

            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
            }

            return JSRedirect(Url.Action("Skills", "Data", new { model.ClassID }));
        }

        public async Task<ActionResult> ModifySkillEntry(ClassSkillMViewModel model)
        {
            var context = new IEContext();


            var bEntries = new List<SkillModifyViewModel>();
            foreach (var sc in model.Entries)
            {
                if (sc.SkillScore1 == sc.SkillScore1M && sc.SkillScore2 == sc.SkillScore2M && sc.SkillScore3 == sc.SkillScore3M && sc.SkillScore4 == sc.SkillScore4M &&
                    sc.SkillScore5 == sc.SkillScore5M && sc.SkillScore6 == sc.SkillScore6M)
                {
                    continue;
                }

                if (sc.SkillScore1V == sc.SkillScore1M && sc.SkillScore2V == sc.SkillScore2M && sc.SkillScore3V == sc.SkillScore3M && sc.SkillScore4V == sc.SkillScore4M &&
                    sc.SkillScore5V == sc.SkillScore5M && sc.SkillScore6V == sc.SkillScore6M)
                {
                    var nsc = new StudentSkills { StudentID = sc.StudentID, ClassID = model.ClassID };
                    context.StudentSkillsList.Attach(nsc);
                    Mapper.Map(sc, nsc);

                    context.Entry(nsc).Property(l => l.SkillScore1).IsModified = true;
                    context.Entry(nsc).Property(l => l.SkillScore2).IsModified = true;
                    context.Entry(nsc).Property(l => l.SkillScore3).IsModified = true;
                    context.Entry(nsc).Property(l => l.SkillScore4).IsModified = true;
                    context.Entry(nsc).Property(l => l.SkillScore5).IsModified = true;
                    context.Entry(nsc).Property(l => l.SkillScore6).IsModified = true;

                    continue;
                }

                var msc = new SkillModifyViewModel();
                Mapper.Map(sc, msc);

                bEntries.Add(msc);
            }

            if (bEntries.Count > 0)
            {
                try
                {
                    context.Configuration.ValidateOnSaveEnabled = false;
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    General.AlertElmah(ex);
                    return DefaultErrorAlert();
                }

                var mModel = context.ClassList.Where(l => l.ClassID == model.ClassID).ProjectToFirst<ClassSkillMViewModel>();

                mModel.Entries = bEntries;
                mModel.MinScore = model.MinScore;
                mModel.MaxScore = model.MaxScore;
                Response.StatusCode = 480;

                ModelState.Clear();
                return PartialView("_SkillModify", mModel);
            }

            var cl = new Class { ClassID = model.ClassID };
            context.ClassList.Attach(cl);
            context.Entry(cl).Property(l => l.VerifySkills).IsModified = true;

            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
            }

            return JSRedirect(Url.Action("Skills", "Data", new { model.ClassID }));
        }

        [Route("Data/SkillSheetsP/{TermID:int}/")]
        public ActionResult SkillSheetsP(int TermID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var term = context.TermList.Where(l => l.TermID == TermID).Select(l => new { l.Name, l.SchoolID, SchoolName = l.School.Name }).First();
            if (hVM.SchoolID != term.SchoolID) return null;

            var model = new SkillSheetsPageViewModel { TermName = term.Name, SchoolName = term.SchoolName };

            var classes = context.ClassList.Where(l => l.TermID == TermID).Select(l => l.ClassID).ToList();
            model.Sheets = new List<ClassSkillSheetsViewModel>();

            foreach (var classid in classes)
            {
                PrepareNewClassSkills(classid);

                model.Sheets.Add(context.ClassList.Where(l => l.ClassID == classid).ProjectToFirst<ClassSkillSheetsViewModel>());
            }


            model.DisplaySheets = new List<ClassSkillSheetsViewModel>();
            var brk = 26;
            var blankStart = 0;
            foreach (var s in model.Sheets)
            {
                var ns = new ClassSkillSheetsViewModel() { Page = 1, Entries = s.Entries.Where(l => l.SerialNumber <= brk).OrderBy(l => l.SerialNumber).ToList() };
                Mapper.Map(s, ns);
                var scnt = s.Entries.Count;
                if (scnt == 0) continue;
                if (scnt > brk)
                {
                    var ecnt = brk;
                    while (ecnt < scnt)
                    {
                        ns.Page = (ecnt / brk);
                        model.DisplaySheets.Add(ns);

                        ns = new ClassSkillSheetsViewModel { Entries = s.Entries.Where(l => l.SerialNumber > ecnt && l.SerialNumber <= ecnt + brk).OrderBy(l => l.SerialNumber).ToList() };
                        Mapper.Map(s, ns);

                        ecnt += brk;
                    }

                    ns.Page = (ecnt / brk);
                }

                blankStart = ns.Entries.Max(l => l.SerialNumber) + 1;

                for (int i = blankStart; i <= brk * ns.Page; i++) ns.Entries.Add(new ScoreSheetEntryViewModel { SerialNumber = (byte)i });

                model.DisplaySheets.Add(ns);
            }

            return View(model);
        }

        #endregion

        #region Score Sheets

        public ActionResult ScoreSheets()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new ScoreSheetsPageViewModel();

            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { l.TermID, l.Term.Name }).FirstOrDefault();
                if (def == null || def.TermID == null) return RedirectToAction("School");
                model.TermID = def.TermID.Value;
                model.TermName = def.Name;
            }
            else
            {
                var tm = GetCurrentTerm(context, hVM.SchoolID);
                if (tm == null) return RedirectToAction("Index", "Home");

                model.TermID = tm.TermID;
                model.TermName = tm.Name;
            }

            model.HeaderViewModel = hVM;

            model.HasScores = context.ScoreEntryList.Any(l => l.Subject.Class.TermID == model.TermID && l.ExamScore.HasValue);

            model.Classes = context.ClassList.Where(l => l.TermID == model.TermID).ProjectToList<ClassViewModel>();

            return View(model);
        }

        [Route("Data/ScoreSheetsP/{ClassID:int}/")]
        public ActionResult ScoreSheetsP(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            PrepareScoreSheets(ClassID);

            var model = context.ClassList.Where(l => l.ClassID == ClassID).ProjectToFirst<ClassScoreSheetsPageViewModel>();

            if (hVM.SchoolID != model.SchoolID) return RedirectToAction("Index", "Home");

            model.DisplaySheets = new List<SubjectScoreSheetViewModel>();
            var brk = 27;
            var blankStart = 0;
            foreach (var s in model.Sheets.OrderBy(l => l.Order).ThenBy(l => l.Name))
            {
                var ns = new SubjectScoreSheetViewModel { Page = 1, Name = s.Name, Entries = s.Entries.Where(l => l.SerialNumber <= brk).OrderBy(l => l.SerialNumber).ToList() };
                if (model.StudentCount > brk)
                {
                    var ecnt = brk;
                    while (ecnt < model.StudentCount)
                    {
                        ns.Page = (ecnt / brk);
                        model.DisplaySheets.Add(ns);

                        ns = new SubjectScoreSheetViewModel
                        {
                            Name = s.Name,
                            Entries = s.Entries.Where(l => l.SerialNumber > ecnt && l.SerialNumber <= ecnt + brk).OrderBy(l => l.SerialNumber).ToList()
                        };

                        ecnt += brk;
                    }

                    ns.Page = (ecnt / brk);
                }

                blankStart = ns.Entries.Max(l => l.SerialNumber) + 1;

                for (int i = blankStart; i <= brk * ns.Page; i++) ns.Entries.Add(new ScoreSheetEntryViewModel { SerialNumber = (byte)i });

                model.DisplaySheets.Add(ns);
            }



            return View(model);
        }

        [Route("Data/ScoreSheetsE/{ClassID:int}/")]
        public ActionResult ScoreSheetsE(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            PrepareScoreSheets(ClassID);

            var model = context.ClassList.Where(l => l.ClassID == ClassID).ProjectToFirst<ClassScoreSheetsExcel>();

            if (hVM.SchoolID != model.SchoolID) return RedirectToAction("Index", "Home");

            ExcelPackage excel = new ExcelPackage();

            foreach(var sub in model.Sheets.OrderBy(l => l.Order))
            {
                var workSheet = excel.Workbook.Worksheets.Add(sub.ResultName);

                workSheet.Cells[1, 1].Value = "## Please DO NOT modify the ID column. DO NOT DELETE or REPLACE any existing student with a new student.";
                workSheet.Cells[1, 1, 1, 13].Merge = true;
                workSheet.Cells[2, 1].Value = "## For new students, enter only the surname, first name and scores ON A NEW ROW. Do not leave any blank rows between students.";
                workSheet.Cells[2, 1, 2, 13].Merge = true;
                workSheet.Cells[3, 1].Value = "## Please enter the Total (100%) ONLY if you do not have the individual CA and Exam Scores. Otherwise leave the Total column blank.";
                workSheet.Cells[3, 1, 3, 13].Merge = true;

                workSheet.Cells[5, 1].Value = "CID";
                workSheet.Cells[5, 1].Style.Font.Bold = true;
                workSheet.Cells[5, 2].Value = ClassID;

                workSheet.Cells[5, 5].Value = "Complete?";
                workSheet.Cells[5, 5].Style.Font.Bold = true;
                workSheet.Cells[5, 6].Value = "No";

                workSheet.Cells[6, 1].Value = "Class";
                workSheet.Cells[6, 1].Style.Font.Bold = true;
                workSheet.Cells[6, 2].Value = model.Name;
                workSheet.Cells[6, 2, 6, 6].Merge = true;

                workSheet.Cells[7, 1].Value = "SID";
                workSheet.Cells[7, 1].Style.Font.Bold = true;
                workSheet.Cells[7, 2].Value = sub.SubjectID;

                workSheet.Cells[8, 1].Value = "Subject";
                workSheet.Cells[8, 1].Style.Font.Bold = true;
                workSheet.Cells[8, 2].Value = sub.Name;
                workSheet.Cells[8, 2, 8, 6].Merge = true;

                workSheet.Cells[10, 1].Value = "ID";
                workSheet.Cells[10, 2].Value = "Surname";
                workSheet.Cells[10, 3].Value = "First Name";
                workSheet.Cells[10, 4].Value = "CA [" + model.CAWeight.ToString() + "]";
                workSheet.Cells[10, 5].Value = "Exam [" + model.ExamWeight.ToString() + "]";
                workSheet.Cells[10, 6].Value = "Total";


                using (var range = workSheet.Cells[10, 1, 10, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.WhiteSmoke);
                    range.Style.ShrinkToFit = false;
                }

                int i = 11;
                foreach(var e in sub.Entries.OrderBy(l => l.Surname).ThenBy(l => l.FirstName))
                {
                    workSheet.Cells[i, 1].Value = e.StudentID;
                    workSheet.Cells[i, 2].Value = e.Surname;
                    workSheet.Cells[i, 3].Value = e.FirstName;

                    i++;
                }

                for (var c = 1; c <= 6; c++) workSheet.Cells[10, c].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                workSheet.Cells.AutoFitColumns();
            }

            var ms = new MemoryStream();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            excel.SaveAs(ms);
            ms.Position = 0;

            string fileName = "Score Sheet " + ClassID.ToString() + " - " + model.Name + " - " +
                              model.SchoolYear.ToString() + "-" + (model.SchoolYear + 1).ToString() + "-" + model.TermNumber.ToString() + ".xlsx";
                              
            return File(ms, contentType, fileName);
        }

        [Route("Data/ScoreSheetsZip/{ClassID:int}/")]
        public ActionResult ScoreSheetsZip(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            PrepareScoreSheets(ClassID);

            var model = context.ClassList.Where(l => l.ClassID == ClassID).ProjectToFirst<ClassScoreSheetsExcel>();

            if (hVM.SchoolID != model.SchoolID) return RedirectToAction("Index", "Home");

            var fileSuffix =  model.SchoolYear.ToString() + "-" + (model.SchoolYear + 1).ToString() + "-" + model.TermNumber.ToString() + " Score Sheet " + model.Name;

            var validSuffix = General.MakeValidFileName(fileSuffix);

            using (var ams = new MemoryStream())
            {
                using (var archive = new ZipArchive(ams, ZipArchiveMode.Create, true))
                {
                    foreach (var sub in model.Sheets.OrderBy(l => l.Order))
                    {
                        var fileName = model.Name + " - " + General.MakeValidFileName(sub.ResultName) + " - " + validSuffix + ".xlsx";

                        var zipEntry = archive.CreateEntry(fileName, System.IO.Compression.CompressionLevel.Fastest);
                        using (var zipStream = zipEntry.Open())
                        {
                            var excelBytes = GetSubjectExcelFile(sub, ClassID, model.Name, model.CAWeight, model.ExamWeight);
                            zipStream.Write(excelBytes, 0, excelBytes.Length);
                        }
                    }
                }
                return File(ams.ToArray(), "application/zip", validSuffix + ".zip");
            }
        }

        private byte[] GetSubjectExcelFile(SubjectScoreSheetExcel sub, int ClassID, string ClassName, byte CAWeight, byte ExamWeight)
        {
            var excel = new ExcelPackage();

            var workSheet = excel.Workbook.Worksheets.Add(sub.ResultName);

            workSheet.Cells[1, 1].Value = "## Please DO NOT modify the ID column. DO NOT DELETE or REPLACE any existing student with a new student.";
            workSheet.Cells[1, 1, 1, 13].Merge = true;
            workSheet.Cells[2, 1].Value = "## For new students, enter only the surname, first name and scores ON A NEW ROW. Do not leave any blank rows between students.";
            workSheet.Cells[2, 1, 2, 13].Merge = true;
            workSheet.Cells[3, 1].Value = "## Please enter the Total (100%) ONLY if you do not have the individual CA and Exam Scores. Otherwise leave the Total column blank.";
            workSheet.Cells[3, 1, 3, 13].Merge = true;

            workSheet.Cells[5, 1].Value = "CID";
            workSheet.Cells[5, 1].Style.Font.Bold = true;
            workSheet.Cells[5, 2].Value = ClassID;

            workSheet.Cells[5, 5].Value = "Complete?";
            workSheet.Cells[5, 5].Style.Font.Bold = true;
            workSheet.Cells[5, 6].Value = "No";

            workSheet.Cells[6, 1].Value = "Class";
            workSheet.Cells[6, 1].Style.Font.Bold = true;
            workSheet.Cells[6, 2].Value = ClassName;
            workSheet.Cells[6, 2, 6, 6].Merge = true;

            workSheet.Cells[7, 1].Value = "SID";
            workSheet.Cells[7, 1].Style.Font.Bold = true;
            workSheet.Cells[7, 2].Value = sub.SubjectID;

            workSheet.Cells[8, 1].Value = "Subject";
            workSheet.Cells[8, 1].Style.Font.Bold = true;
            workSheet.Cells[8, 2].Value = sub.Name;
            workSheet.Cells[8, 2, 8, 6].Merge = true;

            workSheet.Cells[10, 1].Value = "ID";
            workSheet.Cells[10, 2].Value = "Surname";
            workSheet.Cells[10, 3].Value = "First Name";
            workSheet.Cells[10, 4].Value = "CA [" + CAWeight.ToString() + "]";
            workSheet.Cells[10, 5].Value = "Exam [" + ExamWeight.ToString() + "]";
            workSheet.Cells[10, 6].Value = "Total";


            using (var range = workSheet.Cells[10, 1, 10, 6])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.WhiteSmoke);
                range.Style.ShrinkToFit = false;
            }

            int i = 11;
            foreach (var e in sub.Entries.OrderBy(l => l.Surname).ThenBy(l => l.FirstName))
            {
                workSheet.Cells[i, 1].Value = e.StudentID;
                workSheet.Cells[i, 2].Value = e.Surname;
                workSheet.Cells[i, 3].Value = e.FirstName;

                i++;
            }

            for (var c = 1; c <= 6; c++) workSheet.Cells[10, c].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            workSheet.Cells.AutoFitColumns();

            using (var ms = new MemoryStream())
            {
                excel.SaveAs(ms);
                ms.Position = 0;

                return ms.ToArray();
            }
        }

        #endregion

        #region Class Scores

        public ActionResult Classes()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new ClassScoreEntryPageViewModel();

            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { l.TermID, l.Term.Name }).FirstOrDefault();
                if (def == null || def.TermID == null) return RedirectToAction("School");
                model.TermID = def.TermID.Value;
                model.TermName = def.Name;
            }
            else
            {
                var tm = GetCurrentTerm(context, hVM.SchoolID);
                if (tm == null) return RedirectToAction("Index", "Home");

                model.TermID = tm.TermID;
                model.TermName = tm.Name;
            }

            model.HeaderViewModel = hVM;

            model.Classes = context.ClassList.Where(l => l.TermID == model.TermID).ProjectToList<ClassEntryModel>();

            return View(model);
        }

        public ActionResult _UploadClassScores(int ClassID, int UserID)
        {
            var model = new IEContext().ClassList.Where(m => m.ClassID == ClassID).ProjectToFirst<UploadClassScoreViewModel>();
            model.UserID = UserID;

            return PartialView(model);
        }

        public void InitializeNewStudents(int ClassID, int SubjectID, IEContext context)
        {
            var nsList = context.StudentList.Where(l => l.ClassID == ClassID && !context.ScoreEntryList.Any(e => e.SubjectID == SubjectID && e.StudentID == l.StudentID))
                                .Select(l => new { l.StudentID, l.Surname, l.FirstName }).ToList();

            if (nsList != null && nsList.Count > 0)
            {
                byte sn = context.ScoreEntryList.Where(l => l.SubjectID == SubjectID).Select(l => l.SerialNumber).DefaultIfEmpty((byte)0).Max();

                foreach (var std in nsList.OrderBy(l => l.Surname).ThenBy(l => l.FirstName))
                {
                    var ne = new ScoreEntry { SubjectID = SubjectID, StudentID = std.StudentID, SerialNumber = ++sn };
                    context.ScoreEntryList.Add(ne);
                }

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    General.AlertElmah(ex);
                }
            }

        }

        public ActionResult UploadClassScores(UploadClassScoreViewModel model)
        {
            var fileCount = Request.Files.Count;

            if(fileCount == 0)
                return DefaultErrorAlert("No files were uploaded!");

            var context = new IEContext();
            var initContext = new IEContext();

            //get a list of students in the class in order to detect foreign student IDs
            var studentIDList = context.StudentList.Where(l => l.ClassID == model.ClassID).Select(l => l.StudentID).ToList();
            var subjectIDList = context.SubjectList.Where(l => l.ClassID == model.ClassID).Select(l => l.SubjectID).ToList();

            var loadedSubjectIDs = new List<int>();
            var loadedStudentIDs = new List<int>();
            
            try
            {
                for (int i = 0; i < fileCount; i++)
                {
                    var sFile = Request.Files[i];

                    var sLength = 0;
                    if (sFile != null)
                    {
                        sLength = sFile.ContentLength;
                        if (sLength == 0)
                            return DefaultErrorAlert("The Score Entry File - " + sFile.FileName + " is Invalid!");
                    }
                    else
                        return DefaultErrorAlert("The Score Entry File is Invalid!");

                    using (var sms = new MemoryStream())
                    {
                        sFile.InputStream.CopyTo(sms);

                        using (var excel = new ExcelPackage(sms))
                        {
                            foreach (var sheet in excel.Workbook.Worksheets)
                            {
                                if (sheet.Cells[5, 1].Value == null || sheet.Cells[5, 1].Value.ToString() != "CID")
                                    return DefaultErrorAlert("The Score Entry File is Invalid! Current Sheet: " + sheet.Name);

                                int classID = 0;
                                var classObj = sheet.Cells[5, 2].Value;
                                if (!(classObj != null && int.TryParse(classObj.ToString(), out classID)))
                                    return DefaultErrorAlert("The Score Entry File is Invalid! Invalid Class ID in Current Sheet: " + sheet.Name);

                                if (classID != model.ClassID)
                                    return DefaultErrorAlert("The Score Entry File is Invalid! Wrong Class ID in Current Sheet: " + sheet.Name);

                                int subjectID = 0;
                                var subjectObj = sheet.Cells[7, 2].Value;
                                if (!(subjectObj != null && int.TryParse(subjectObj.ToString(), out subjectID)))
                                    return DefaultErrorAlert("The Score Entry File is Invalid! Blank Subject ID in Current Sheet: " + sheet.Name);

                                if (!subjectIDList.Contains(subjectID)) continue;

                                if (loadedSubjectIDs.Contains(subjectID))
                                    return DefaultErrorAlert("The Score Entry File is Invalid! Duplicate Subject ID in Current Sheet: " + sheet.Name);

                                //create score entries for new students...
                                InitializeNewStudents(classID, subjectID, initContext);  // separate context used to prevent conflicts whe attaching students...

                                var rowID = 11;
                                var surnameObj = sheet.Cells[rowID, 2].Value;
                                if (surnameObj == null)
                                    return DefaultErrorAlert("The Score Entry File is Invalid! No students in Current Sheet: " + sheet.Name);

                                var surname = surnameObj.ToString();
                                loadedStudentIDs = new List<int>();

                                bool hasValidScores = false;
                                while (!string.IsNullOrWhiteSpace(surname))
                                {
                                    int studentID = 0;
                                    var studentObj = sheet.Cells[rowID, 1].Value;
                                    if (!(studentObj != null && int.TryParse(studentObj.ToString(), out studentID)))
                                        return DefaultErrorAlert("There are new Students. Please create them and input their IDs! Current Sheet: " + sheet.Name + "; Surname: " + surname);

                                    if (loadedStudentIDs.Contains(studentID))
                                        return DefaultErrorAlert("There are duplicate Student IDs in the current sheet: " + sheet.Name + "; ID: " + studentID.ToString() + "; Surname: " + surname);

                                    if (studentIDList.Contains(studentID))
                                    {
                                        var se = new ScoreEntry() { StudentID = studentID, SubjectID = subjectID };
                                        context.ScoreEntryList.Attach(se);

                                        decimal caScore = 0, examScore = 0;
                                        var caObj = sheet.Cells[rowID, 4].Value;
                                        var examObj = sheet.Cells[rowID, 5].Value;
                                        var hasCA = caObj != null && decimal.TryParse(caObj.ToString(), out caScore);
                                        var hasExam = examObj != null && decimal.TryParse(examObj.ToString(), out examScore);

                                        if (hasExam)
                                        {
                                            if (caScore < 0 || caScore > model.CAWeight)
                                                return DefaultErrorAlert("CA Score is Invalid! Current Sheet: " + sheet.Name + "; Surname: " + surname);


                                            if (examScore < 0 || examScore > model.ExamWeight)
                                                return DefaultErrorAlert("Exam Score is Invalid! Current Sheet: " + sheet.Name + "; Surname: " + surname);

                                            if (hasCA) se.CAScore = caScore;

                                            se.ExamScore = examScore;
                                            hasValidScores = true;
                                        }
                                        else
                                        {
                                            if (!hasCA)
                                            {
                                                // compute CA and exam from total score if it exists
                                                decimal total = 0;
                                                var totalObj = sheet.Cells[rowID, 6].Value;
                                                if (totalObj != null && decimal.TryParse(totalObj.ToString(), out total))
                                                {
                                                    if (total > 0)
                                                    {
                                                        se.CAScore = decimal.Floor((total * model.CAWeight / 100));
                                                        se.ExamScore = decimal.Ceiling((total * model.ExamWeight / 100));

                                                        hasValidScores = true;
                                                    }
                                                }
                                            }
                                        }

                                        context.Entry(se).Property(l => l.CAScore).IsModified = true;
                                        context.Entry(se).Property(l => l.ExamScore).IsModified = true;

                                        loadedStudentIDs.Add(studentID);
                                    }

                                    rowID++;

                                    surnameObj = sheet.Cells[rowID, 2].Value;
                                    if (surnameObj == null) break;

                                    surname = surnameObj.ToString();
                                }

                                if (hasValidScores)
                                {
                                    loadedSubjectIDs.Add(subjectID);

                                    var sb = new Subject { SubjectID = subjectID };
                                    context.SubjectList.Attach(sb);

                                    sb.VerifiedByID = model.UserID;
                                    sb.TimeVerified = DateTime.Now;
                                }
                            }
                        }
                    }

                }

                context.Configuration.ValidateOnSaveEnabled = false;
                context.SaveChanges();

                AssignClassGrades(model.ClassID, model.GradeGroupID);
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return Json(context.ClassList.Where(m => m.ClassID == model.ClassID).ProjectToFirst<ClassEntryModel>(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult NoPictures()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Data);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new NoPicturePageViewModel();

            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { l.TermID, l.Term.Name }).FirstOrDefault();
                if (def == null || def.TermID == null) return RedirectToAction("School");
                model.TermID = def.TermID.Value;
                model.TermName = def.Name;
            }
            else
            {
                var tm = GetCurrentTerm(context, hVM.SchoolID);
                if (tm == null) return RedirectToAction("Index", "Home");

                model.TermID = tm.TermID;
                model.TermName = tm.Name;
            }

            model.HeaderViewModel = hVM;

            model.Students = context.StudentList.Where(l => l.Class.TermID == model.TermID && (l.GuidString == null || l.GuidString == string.Empty)).ProjectToList<StudentViewModel>();

            return View(model);
        }

    }
}