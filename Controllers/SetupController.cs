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
using System.Data.Entity.Migrations;
using CsvHelper;

namespace IEGen.Controllers
{
    [Authorize]
    public class SetupController : BaseController
    {
        // GET: Setup
        public ActionResult Index()
        {
            return School();
        }

        #region School and Term
        public ActionResult School()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if(hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.SchoolList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirst<SchoolDetailsViewModel>(); ;
            model.HeaderViewModel = hVM;

            var defName = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => l.Term.Name).FirstOrDefault();

            if (string.IsNullOrEmpty(defName))
            {
                var uDef = new UserDefaults { UserID = hVM.UserID, SchoolID = hVM.SchoolID };
                uDef.TermID = context.TermList.Where(l => l.SchoolID == hVM.SchoolID).Select(l => (int?)l.TermID).DefaultIfEmpty(0).Max();
                context.UserDefaultsList.Add(uDef);

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    General.AlertElmah(ex);
                }

                model.DefaultTerm = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => l.Term.Name).FirstOrDefault();
            }
            else
                model.DefaultTerm = defName;

            return View("School", model);
        }

        public ActionResult SchoolRequests()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.SchoolList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirst<SchoolRequestPageViewModel>(); ;
            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _EditTerm(int TermID)
        {
            return PartialView(new IEContext().TermList.Where(m => m.TermID == TermID).ProjectToFirst<EditTermViewModel>());
        }

        [HttpPost]
        public ActionResult DeleteTerm(int TermID)
        {
            var context = new IEContext();
            var gp = new Term() { TermID = TermID };
            context.TermList.Attach(gp);
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
            return Json(TermID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateTerm(EditTermViewModel model)
        {
            var context = new IEContext();

            if (string.IsNullOrWhiteSpace(model.Name))
                model.Name = General.TermName(model.SchoolYear, model.TermNumber);

            if (context.TermList.Any(l => l.SchoolID == model.SchoolID && l.TermID != model.TermID && (l.Name == model.Name || (l.TermNumber == model.TermNumber && l.SchoolYear == model.SchoolYear))))
                return DefaultErrorAlert("Another Term with this Name or Year and Term Number already exists!");

            var gp = new Term() { TermID = model.TermID };
            context.TermList.Attach(gp);
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

            return Json(context.TermList.Where(m => m.TermID == model.TermID).ProjectToFirst<TermViewModel>(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _AddTerm(int SchoolID)
        {
            return PartialView();
        }

        public ActionResult CreateTerm(EditTermViewModel model)
        {
            var context = new IEContext();

            if (string.IsNullOrWhiteSpace(model.Name))
                model.Name = General.TermName(model.SchoolYear, model.TermNumber);

            if (context.TermList.Any(l => l.SchoolID == model.SchoolID && (l.Name == model.Name || (l.TermNumber == model.TermNumber && l.SchoolYear == model.SchoolYear))))
                return DefaultErrorAlert("A Term with this Name or Year and Term Number already exists!");

            var gp = new Term();
            Mapper.Map(model, gp);

            gp.GuidString = context.SchoolList.Where(l => l.SchoolID == gp.SchoolID).Select(l => l.GuidString).FirstOrDefault();
            context.TermList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            var nModel = new TermViewModel();
            Mapper.Map(gp, nModel);

            return Json(nModel, JsonRequestBehavior.AllowGet);
        }

        [Route("Setup/SetDefTerm/{UserID:int}/{SchoolID:int}/")]
        public ActionResult SetDefTerm(int UserID, int SchoolID, int TermID)
        {
            var context = new IEContext();
            var gp = new UserDefaults() { UserID = UserID, SchoolID = SchoolID };
            context.UserDefaultsList.Attach(gp);
            gp.TermID = TermID;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return JSRedirect(Url.Action("Classes", "Setup"));
        }

        #endregion

        #region Subject Templates

        public ActionResult SubjectTemplates()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new SubjectTemplatePageViewModel { TemplateList = context.SubjectTemplateList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToList<SubjectTemplateViewModel>() };
            model.SchoolName = context.SchoolList.Where(l => l.SchoolID == hVM.SchoolID).Select(l => l.Name).FirstOrDefault();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _EditSubjectTemplate(int TemplateID)
        {
            var context = new IEContext();
            var model = context.SubjectTemplateList.Where(m => m.TemplateID == TemplateID).ProjectToFirst<EditSubjectTemplateViewModel>();
            model.LevelList = GetLevelList(context, model.SchoolID);
            model.HasSubjects = context.SubjectList.Any(l => l.TemplateID == TemplateID);

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteSubjectTemplate(int TemplateID)
        {
            var context = new IEContext();
            var gp = new SubjectTemplate() { TemplateID = TemplateID };
            context.SubjectTemplateList.Attach(gp);
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
            return Json(TemplateID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateSubjectTemplate(EditSubjectTemplateViewModel model)
        {
            var context = new IEContext();

            if (context.SubjectTemplateList.Any(l => l.SchoolID == model.SchoolID && l.ClassLevelID == model.ClassLevelID && l.TemplateID != model.TemplateID && l.Name == model.Name))
                return DefaultErrorAlert("Another Subject with this name already exists!");

            var gp = new SubjectTemplate() { TemplateID = model.TemplateID };
            context.SubjectTemplateList.Attach(gp);
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

            var nModel = new SubjectTemplateViewModel();
            Mapper.Map(gp, nModel);
            return Json(nModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _AddSubjectTemplate(int SchoolID)
        {
            var model = new EditSubjectTemplateViewModel { SchoolID = SchoolID, LevelList = GetLevelList(new IEContext(), SchoolID) };
            return PartialView(model);
        }

        public ActionResult CreateSubjectTemplate(EditSubjectTemplateViewModel model)
        {
            var context = new IEContext();

            if (context.SubjectTemplateList.Any(l => l.SchoolID == model.SchoolID && l.ClassLevelID == model.ClassLevelID && l.Name == model.Name))
                return DefaultErrorAlert("A Subject with this name already exists!");

            var gp = new SubjectTemplate();
            Mapper.Map(model, gp);
            context.SubjectTemplateList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            var nModel = new SubjectTemplateViewModel();
            Mapper.Map(gp, nModel);
            return Json(nModel, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Complex Result (Entry) Formats

        public ActionResult EntryFormats()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new ComplexResultFormatPageViewModel { FormatList = context.ComplexScoreFormatList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToList<ComplexResultFormatViewModel>() };
            model.SchoolName = context.SchoolList.Where(l => l.SchoolID == hVM.SchoolID).Select(l => l.Name).FirstOrDefault();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _EditEntryFormat(int FormatID)
        {
            return PartialView(new IEContext().ComplexScoreFormatList.Where(m => m.FormatID == FormatID).ProjectToFirst<EditComplexResultFormatViewModel>());
        }

        [HttpPost]
        public ActionResult DeleteEntryFormat(int FormatID)
        {
            var context = new IEContext();
            var gp = new ComplexScoreFormat() { FormatID = FormatID };
            context.ComplexScoreFormatList.Attach(gp);
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
            return Json(FormatID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateEntryFormat(EditComplexResultFormatViewModel model)
        {
            var context = new IEContext();

            if (context.ComplexScoreFormatList.Any(l => l.SchoolID == model.SchoolID && l.FormatID != model.FormatID && l.CA1Weight == model.CA1Weight && l.CA2Weight == model.CA2Weight &&
                                                         l.CA3Weight == model.CA3Weight && l.CA4Weight == model.CA4Weight && l.ExamWeight == model.ExamWeight))
                return DefaultErrorAlert("A similar Result Format already exists!");

            var gp = new ComplexScoreFormat() { FormatID = model.FormatID };
            context.ComplexScoreFormatList.Attach(gp);
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

        public ActionResult _AddEntryFormat(int SchoolID)
        {
            return PartialView();
        }

        public ActionResult CreateEntryFormat(EditComplexResultFormatViewModel model)
        {
            var context = new IEContext();

            if (context.ComplexScoreFormatList.Any(l => l.SchoolID == model.SchoolID && l.CA1Weight == model.CA1Weight && l.CA2Weight == model.CA2Weight &&
                                                         l.CA3Weight == model.CA3Weight && l.CA4Weight == model.CA4Weight && l.ExamWeight == model.ExamWeight))
                return DefaultErrorAlert("A similar Result Format already exists!");

            var gp = new ComplexScoreFormat();
            Mapper.Map(model, gp);
            context.ComplexScoreFormatList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            model.FormatID = gp.FormatID;
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Class Arms
        public ActionResult ClassArms()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.SchoolList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirst<ClassArmPageViewModel>();
            model.ArmList = context.ClassArmList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToList<ClassArmViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _EditClassArm(int ArmID)
        {
            var context = new IEContext();
            var model = context.ClassArmList.Where(m => m.ArmID == ArmID).ProjectToFirst<EditClassArmViewModel>();
            model.LevelList = GetLevelList(model.SchoolTypeID);

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteClassArm(int ArmID)
        {
            var context = new IEContext();
            var gp = new ClassArm() { ArmID = ArmID };
            context.ClassArmList.Attach(gp);
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
            return Json(ArmID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateClassArm(EditClassArmViewModel model)
        {
            var context = new IEContext();

            if (context.ClassArmList.Any(l => l.SchoolID == model.SchoolID && l.ArmID != model.ArmID && l.Name == model.Name))
                return DefaultErrorAlert("Another Class Arm with this name already exists!");

            var gp = new ClassArm() { ArmID = model.ArmID };
            context.ClassArmList.Attach(gp);
            Mapper.Map(model, gp);

            context.Entry(gp).Property(l => l.IsDisabled).IsModified = true;

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            var nModel = new ClassArmViewModel();
            Mapper.Map(model, nModel);
            return Json(nModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _AddClassArm(int SchoolID)
        {
            var model = new EditClassArmViewModel { SchoolID = SchoolID };
            model.LevelList = GetLevelList(new IEContext(), SchoolID);

            return PartialView(model);
        }

        public ActionResult CreateClassArm(EditClassArmViewModel model)
        {
            var context = new IEContext();

            if (context.ClassArmList.Any(l => l.SchoolID == model.SchoolID && l.Name == model.Name))
                return DefaultErrorAlert("A Class Arm with this name already exists!");

            var gp = new ClassArm();
            Mapper.Map(model, gp);
            context.ClassArmList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            var nModel = new ClassArmViewModel();
            Mapper.Map(model, nModel);
            nModel.ArmID = gp.ArmID;
            return Json(nModel, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Classes and Subjects
        public ActionResult Classes()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { l.TermID }).FirstOrDefault();
            if (def == null || def.TermID == null) return RedirectToAction("School");

            var model = context.TermList.Where(l => l.TermID == def.TermID).ProjectToFirst<ClassPageViewModel>();

            model.SchoolDefaults = context.SchoolDataList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirstOrDefault<SchoolDefaultsModel>();


            var armLevelList = context.ClassArmList.Where(l => l.SchoolID == hVM.SchoolID && !l.IsDisabled).Select(l => l.ClassLevelID).ToList();

            var tList = Enum.GetValues(typeof(ClassLevel)).Cast<ClassLevel>();

            switch (model.SchoolTypeID)
            {
                case (byte)SchoolType.NurseryPrimary: tList = tList.Where(e => e < ClassLevel.JS1); break;
                case (byte)SchoolType.Secondary: tList = tList.Where(e => e >= ClassLevel.JS1 && e <= ClassLevel.SS3); break;
                default: break;
            }

            foreach (var levelID in tList.Select(l => (byte)l))
                if (!armLevelList.Contains(levelID)) model.IncompleteArms = true;


            model.HeaderViewModel = hVM;

            return View(model);
        }

        private List<SelectListItem> GetClassArmList(IEContext context, int SchoolID)
        {
            var list = new List<SelectListItem>();

            var gpList = context.ClassArmList.Where(l => l.SchoolID == SchoolID && !l.IsDisabled)
                                .Select(l => new SelectListItem { Value = l.ArmID.ToString(), Text = l.Name });

            list.AddRange(gpList);
            return list;
        }

        private List<SelectListItem> GetGradeGroupList(IEContext context, int SchoolID)
        {
            var list = new List<SelectListItem>();

            var gpList = context.GradeGroupList.Where(l => (l.SchoolID == null || l.SchoolID == SchoolID) && !l.IsDisabled).ProjectToList<GradeGroupViewModel>()
                                .Select(l => new SelectListItem { Value = l.GradeGroupID.ToString(), Text = l.DisplayName });

            list.AddRange(gpList);
            return list;
        }

        private List<SelectListItem> GetSkillGroupList(IEContext context, int SchoolID)
        {
            var list = new List<SelectListItem>();

            var gpList = context.SkillGroupList.Where(l => (l.SchoolID == null || l.SchoolID == SchoolID) && !l.IsDisabled).ProjectToList<SkillGroupViewModel>()
                                .Select(l => new SelectListItem { Value = l.SkillGroupID.ToString(), Text = l.DisplayName });

            list.AddRange(gpList);
            return list;
        }

        private List<SelectListItem> GetEntryFormatList(IEContext context, int SchoolID)
        {
            var list = new List<SelectListItem>();
            var gpList = context.ComplexScoreFormatList.Where(l => l.SchoolID == SchoolID).ProjectToList<ComplexResultFormatViewModel>()
                                .Select(l => new SelectListItem { Value = l.FormatID.ToString(), Text = l.Name });

            list.AddRange(gpList);
            return list;
        }

        private List<SelectListItem> GetPromotionCommentGroupList(IEContext context, int SchoolID)
        {
            var list = new List<SelectListItem>();
            var gpList = context.PromotionCommentGroupList.Where(l => l.SchoolID == SchoolID)
                                .Select(l => new SelectListItem { Value = l.GroupID.ToString(), Text = l.Name });

            list.AddRange(gpList);
            return list;
        }

        private List<SelectListItem> GetPerformanceCommentGroupList(IEContext context, int SchoolID)
        {
            var list = new List<SelectListItem>();
            var gpList = context.PerformanceCommentGroupList.Where(l => l.SchoolID == SchoolID)
                                .Select(l => new SelectListItem { Value = l.GroupID.ToString(), Text = l.Name });

            list.AddRange(gpList);
            return list;
        }

        private List<SelectListItem> GetImprovementCommentGroupList(IEContext context, int SchoolID)
        {
            var list = new List<SelectListItem>();
            var gpList = context.ImprovementCommentGroupList.Where(l => l.SchoolID == SchoolID)
                                .Select(l => new SelectListItem { Value = l.GroupID.ToString(), Text = l.Name });

            list.AddRange(gpList);
            return list;
        }

        [Route("Setup/_EditClass/{SchoolID:int}/")]
        public ActionResult _EditClass(int ClassID, int SchoolID)
        {
            var context = new IEContext();
            var model = context.ClassList.Where(m => m.ClassID == ClassID).ProjectToFirst<EditClassViewModel>();
            model.ArmList = GetClassArmList(context, SchoolID);
            model.SkillGroupList = GetSkillGroupList(context, SchoolID);
            model.GradeGroupList = GetGradeGroupList(context, SchoolID);
            model.FormatList = GetEntryFormatList(context, SchoolID);
            model.PerformanceCommentGroupList = GetPerformanceCommentGroupList(context, SchoolID);
            model.PromotionCommentGroupList = GetPromotionCommentGroupList(context, SchoolID);
            model.ImprovementCommentGroupList = GetImprovementCommentGroupList(context, SchoolID);

            model.HasScoreEntries = context.ScoreEntryList.Any(l => l.Subject.ClassID == ClassID);
            model.FormatID = context.ClassScoreFormatList.Where(l => l.ClassID == ClassID).Select(l => (int?)l.FormatID).DefaultIfEmpty(null).FirstOrDefault();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteClass(int ClassID)
        {
            var context = new IEContext();
            var gp = new Class() { ClassID = ClassID };
            context.ClassList.Attach(gp);
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
            return Json(ClassID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateClass(EditClassViewModel model)
        {
            var context = new IEContext();

            if (context.ClassList.Any(l => l.TermID == model.TermID && l.ClassID != model.ClassID && l.ArmID == model.ArmID))
                return DefaultErrorAlert("Another Class with this name already exists!");

            var gp = new Class() { ClassID = model.ClassID };
            context.ClassList.Attach(gp);
            Mapper.Map(model, gp);

            if(model.FormatID.HasValue)
            {
                var cf = new ClassScoreFormat { ClassID = model.ClassID, FormatID = model.FormatID.Value };
                context.ClassScoreFormatList.AddOrUpdate(cf);
            }
            else
            {
                context.ClassScoreFormatList.Where(l => l.ClassID == model.ClassID).Delete();
            }

            try
            {
                context.Entry(gp).Property(p => p.HideClassAverage).IsModified = true;
                context.Entry(gp).Property(p => p.ShowCategoryAnalysis).IsModified = true;
                context.Entry(gp).Property(p => p.ShowPosition).IsModified = true;
                context.Entry(gp).Property(p => p.ShowSummaryGrade).IsModified = true;
                context.Entry(gp).Property(p => p.IsPromotionalClass).IsModified = true;
                context.Entry(gp).Property(p => p.ShowYearResult).IsModified = true;
                context.Entry(gp).Property(p => p.CommentOnYearResult).IsModified = true;
                context.Entry(gp).Property(p => p.PromotionCommentGroupID).IsModified = true;
                context.Entry(gp).Property(p => p.PerformanceCommentGroupID).IsModified = true;
                context.Entry(gp).Property(p => p.ImprovementCommentGroupID).IsModified = true;
                context.Entry(gp).Property(p => p.SkillGroupID).IsModified = true;

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return Json(context.ClassList.Where(m => m.ClassID == model.ClassID).ProjectToFirst<ClassViewModel>(), JsonRequestBehavior.AllowGet);
        }

        [Route("Setup/_AddClass/{TermID:int}/{SchoolID:int}/")]
        public ActionResult _AddClass(int TermID, int SchoolID)
        {
            var context = new IEContext();
            var model = new EditClassViewModel { TermID = TermID };
            var dat = context.SchoolDataList.Where(l => l.SchoolID == SchoolID)
                             .Select(l => new { l.DefGradeGroupID, l.DefSkillGroupID, l.DefPerformanceCommentGroupID, l.DefPromotionCommentGroupID, l.DefImprovementCommentGroupID })
                             .FirstOrDefault();

            if (dat == null)
            {
                model.GradeGroupID = General.DefGradeGroupID;
            }
            else
            {
                model.GradeGroupID = dat.DefGradeGroupID ?? General.DefGradeGroupID; //dat.DefGradeGroupID.HasValue ? dat.DefGradeGroupID.Value : General.DefGradeGroupID;
                model.SkillGroupID = dat.DefSkillGroupID;
                model.PromotionCommentGroupID = dat.DefPromotionCommentGroupID;
                model.PerformanceCommentGroupID = dat.DefPerformanceCommentGroupID;
                model.ImprovementCommentGroupID = dat.DefImprovementCommentGroupID;
            }

            model.SkillGroupList = GetSkillGroupList(context, SchoolID);
            model.GradeGroupList = GetGradeGroupList(context, SchoolID);
            model.FormatList = GetEntryFormatList(context, SchoolID);
            model.PerformanceCommentGroupList = GetPerformanceCommentGroupList(context, SchoolID);
            model.PromotionCommentGroupList = GetPromotionCommentGroupList(context, SchoolID);
            model.ImprovementCommentGroupList = GetImprovementCommentGroupList(context, SchoolID);

            model.ArmList = GetClassArmList(context, SchoolID);

            return PartialView(model);
        }

        public ActionResult CreateClass(EditClassViewModel model)
        {
            var context = new IEContext();

            if (context.ClassList.Any(l => l.TermID == model.TermID && l.ArmID == model.ArmID))
                return DefaultErrorAlert("A Class with this name already exists!");

            var gp = new Class();
            Mapper.Map(model, gp);

            if (!model.IsPromotionalClass)
            {
                gp.PromotionCommentGroupID = null;
                gp.ShowYearResult = false;
                gp.CommentOnYearResult = false;
            }

            context.ClassList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return JSRedirect(Url.Action("ClassSetup", "Setup", new { gp.ClassID }));
        }

        [Route("Setup/_ImportClasses/{TermID:int}/{SchoolID:int}/{SchoolYear:int}/{TermNumber:int}/")]
        public ActionResult _ImportClasses(int TermID, int SchoolID, int SchoolYear, int TermNumber)
        {
            var context = new IEContext();
            var model = new ImportClassesViewModel { TermID = TermID, SchoolID = SchoolID, SchoolYear = (short)SchoolYear, TermNumber = (byte)TermNumber, IsCurrentSessionImport = true };

            var gpList = context.TermList.Where(l => l.SchoolID == SchoolID && l.SchoolYear == SchoolYear && l.TermNumber < TermNumber)
                                .OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber)
                                .Select(t => new { t.Name, t.TermID }).ToList()
                                .Select(l => new SelectListItem { Value = l.TermID.ToString(), Text = l.Name });

            model.TermList = gpList.ToList();

            if (TermNumber == 3) model.MakeClassesPromotional = true;

            return PartialView(model);
        }

        [Route("Setup/_ImportOtherClasses/{TermID:int}/{SchoolID:int}/{SchoolYear:int}/{TermNumber:int}/")]
        public ActionResult _ImportOtherClasses(int TermID, int SchoolID, int SchoolYear, int TermNumber)
        {
            var context = new IEContext();
            var model = new ImportClassesViewModel { TermID = TermID, SchoolID = SchoolID, SchoolYear = (short)SchoolYear, TermNumber = (byte)TermNumber };

            var gpList = context.TermList.Where(l => l.SchoolID == SchoolID && l.TermID != TermID)
                                .OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber)
                                .Select(t => new { t.Name, t.TermID }).ToList()
                                .Select(l => new SelectListItem { Value = l.TermID.ToString(), Text = l.Name });

            model.TermList = gpList.ToList();

            if (TermNumber == 3) model.MakeClassesPromotional = true;

            return PartialView("_ImportClasses", model);
        }

        public ActionResult ImportClasses(ImportClassesViewModel model)
        {
            var context = new IEContext();

            //import the classes that are not present in the destination term
            var classes = context.ClassList.Where(l => l.TermID == model.SourceTermID && !context.ClassList.Any(c => c.TermID == model.TermID && c.ArmID == l.ArmID))
                                 .ProjectToList<ImportClassViewModel>();

            var ncList = new List<Class> { };

            foreach(var cl in classes)
            {
                var newClass = new Class();
                Mapper.Map(cl, newClass);

                newClass.TermID = model.TermID;

                if (model.MakeClassesPromotional)
                    newClass.IsPromotionalClass = true;
                else
                    newClass.IsPromotionalClass = false;

                var nsList = new List<Subject>();
                foreach(var sub in cl.Subjects)
                {
                    var newSubject = new Subject { TemplateID = sub.TemplateID, ResultName = sub.ResultName, SchoolYear = model.SchoolYear, TermNumber = model.TermNumber };
                    nsList.Add(newSubject);
                }

                newClass.Subjects = nsList;

                ncList.Add(newClass);
            }

            context.ClassList.AddRange(ncList);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            if (model.ImportStudents)
            {
                foreach(var cl in classes)
                {
                    var newClassID = ncList.Where(l => l.ArmID == cl.ArmID).Select(l => l.ClassID).First();
                    context.StudentList.Where(l => l.ClassID == cl.ClassID && context.StudentResultList.Any(r => r.StudentID == l.StudentID && r.ClassID == cl.ClassID))
                           .Update(l => new Student { ClassID = newClassID });
                }
            }

            return JSRedirect(Url.Action("Classes", "Setup"));
        }

        public ActionResult ImportStudents()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { l.TermID }).FirstOrDefault();
            if (def == null || def.TermID == null) return RedirectToAction("School");

            var model = context.TermList.Where(l => l.TermID == def.TermID).ProjectToFirst<ImportStudentsPageViewModel>();

            model.Terms = context.TermList.Where(l => l.SchoolID == hVM.SchoolID && l.TermID != def.TermID)
                                 .OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).ProjectToList<TermViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        [Route("Setup/_ImportStudents/{TermID:int}/")]
        public ActionResult _ImportStudents(int TermID, int SourceTermID, string Name)
        {
            var context = new IEContext();
            var model = context.TermList.Where(l => l.TermID == SourceTermID).ProjectToFirst<ImportTermStudentsViewModel>();

            var gpList = context.ClassList.Where(l => l.TermID == TermID)
                                .Select(t => new { t.Arm.Name, t.ClassID }).ToList()
                                .Select(l => new SelectListItem { Value = l.ClassID.ToString(), Text = l.Name });

            model.NewClassList = gpList.ToList();
            model.NewTermID = TermID;
            model.NewTermName = Name;

            return PartialView(model);
        }

        public ActionResult FinishImportStudents(ImportTermStudentsViewModel model)
        {
            var context = new IEContext();

            if (model.ImportAllStudents)
            {
                foreach (var cl in model.Classes)
                {
                    if (cl.NewClassID.HasValue)
                        context.StudentList.Where(l => l.ClassID == cl.ClassID).Update(l => new Student { ClassID = cl.NewClassID.Value });
                }
            }
            else
            {
                foreach (var cl in model.Classes)
                {
                    if (cl.NewClassID.HasValue)
                        context.StudentList.Where(l => l.ClassID == cl.ClassID && context.StudentResultList.Any(r => r.StudentID == l.StudentID && r.ClassID == cl.ClassID))
                               .Update(l => new Student { ClassID = cl.NewClassID.Value });
                }
            }

            return JSRedirect(Url.Action("Classes", "Setup"));
        }



        [Route("Setup/Class/{ClassID:int}/")]
        public ActionResult ClassSetup(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            var model = context.ClassList.Where(l => l.ClassID == ClassID).ProjectToFirst<ClassSetupViewModel>();

            var isTerm1 = model.TermNumber == 1;
            var isTerm2 = model.TermNumber == 2;
            var isTerm3 = model.TermNumber == 3;

            model.TemplateList = context.SubjectTemplateList.Where(l => l.SchoolID == hVM.SchoolID && l.ClassLevelID == model.ClassLevelID &&
                                                                        (isTerm1 ? l.HasTerm1 : isTerm2 ? l.HasTerm2 : isTerm3 ? l.HasTerm3 : false) &&
                                                                        !context.SubjectList.Any(s => s.ClassID == ClassID && s.TemplateID == l.TemplateID))
                                                            .ProjectToList<SubjectTemplateViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _EditSubject(int SubjectID, int SchoolID)
        {
            var context = new IEContext();
            var model = context.SubjectList.Where(m => m.SubjectID == SubjectID).ProjectToFirst<EditSubjectViewModel>();
            model.TeacherList = GetTeacherList(context, SchoolID);
            model.FormatList = GetEntryFormatList(context, SchoolID);

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteSubject(int SubjectID, int TemplateID)
        {
            var context = new IEContext();
            var gp = new Subject() { SubjectID = SubjectID };
            context.SubjectList.Attach(gp);
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

            var model = context.SubjectTemplateList.Where(l => l.TemplateID == TemplateID).ProjectToFirst<SubjectTemplateViewModel>();
            model.SubjectID = SubjectID;
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateSubject(EditSubjectViewModel model)
        {
            var context = new IEContext();

            if (context.SubjectList.Any(l => l.ClassID == model.ClassID && l.SubjectID != model.SubjectID && l.ResultName == model.ResultName))
                return DefaultErrorAlert("Another Subject with this name already exists!");

            var gp = new Subject() { SubjectID = model.SubjectID };
            context.SubjectList.Attach(gp);
            Mapper.Map(model, gp);

            if (model.FormatID != null)
            {
                var cf = new SubjectScoreFormat { SubjectID = model.SubjectID, FormatID = model.FormatID.Value };
                context.SubjectScoreFormatList.AddOrUpdate(cf);
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

            return Json(context.SubjectList.Where(l => l.SubjectID == gp.SubjectID).ProjectToFirst<SubjectViewModel>(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddSubject(int ClassID, int TemplateID)
        {
            var context = new IEContext();

            if (context.SubjectList.Any(l => l.ClassID == ClassID && l.TemplateID == TemplateID))
                return DefaultErrorAlert("A Subject with this name already exists!");

            var gp = new Subject { ClassID = ClassID };
            var tp = context.SubjectTemplateList.Where(l => l.TemplateID == TemplateID).First();

            Mapper.Map(tp, gp);

            var cl = context.ClassList.Where(l => l.ClassID == ClassID).Select(l => new { l.Term.TermNumber, l.Term.SchoolYear }).First();
            gp.TermNumber = cl.TermNumber;
            gp.SchoolYear = cl.SchoolYear;

            context.SubjectList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            var model = new SubjectViewModel { CategoryID = tp.CategoryID, ClassLevelID = tp.ClassLevelID };
            Mapper.Map(gp, model);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddSubjects(int ClassID, string TemplateIDs)
        {
            var context = new IEContext();

            var cl = context.ClassList.Where(l => l.ClassID == ClassID).Select(l => new { l.Term.TermNumber, l.Term.SchoolYear }).First();

            List<int> idList = new List<int>();
            var idArray = TemplateIDs.Split(',');

            foreach (string id in idArray)
            {
                var intID = 0;
                if (!int.TryParse(id, out intID)) continue;

                if (intID > 0) idList.Add(intID);
            }

            var existingIDs = context.SubjectList.Where(l => l.ClassID == ClassID && idList.Contains(l.TemplateID)).Select(l => l.TemplateID).ToList();
            idList.RemoveAll(l => existingIDs.Contains(l));

            if (!idList.Any())
                return DefaultErrorAlert("No subject added!");

            foreach(int TemplateID in idList)
            {
                var gp = new Subject { ClassID = ClassID };
                var tp = context.SubjectTemplateList.Where(l => l.TemplateID == TemplateID).First();

                Mapper.Map(tp, gp);
                gp.TermNumber = cl.TermNumber;
                gp.SchoolYear = cl.SchoolYear;

                context.SubjectList.Add(gp);
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

            return Json("", JsonRequestBehavior.AllowGet);
        }

        [Route("Setup/AddAllSubjects/{ClassID:int}/{SchoolID:int}/{LevelID:int}/{TermNumber:int}/{Year:int}/")]
        public ActionResult AddAllSubjects(int ClassID, int SchoolID, int LevelID, int TermNumber, int Year)
        {
            AddAllClassSubjects(SchoolID, ClassID, (byte)LevelID, (short)Year, (byte)TermNumber);

            return Json("", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Students
        public ActionResult Students()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID)
                             .Select(l => new { l.TermID, l.ClassID, l.School.Name, l.School.TypeID, TermName = l.Term.Name }).FirstOrDefault();

            if (def == null || def.TermID == null) return RedirectToAction("School");

            var model = new StudentsPageViewModel
            {
                HeaderViewModel = hVM,

                SchoolName = def.Name,
                TermFilter = def.TermID,
                DefTermID = def.TermID.Value,
                DefTermName = def.TermName,
                ClassList = GetClassList(context, def.TermID),
                TermList = GetTermList(context, hVM.SchoolID),
                LevelList = GetLevelList(def.TypeID),

                DefClassID = def.ClassID
            };

            return View("Students", model);
        }

        public ActionResult GetStudentList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, StudentFilterViewModel filterModel)
        {
            var query = new IEContext().StudentList.Where(l => l.SchoolID == filterModel.SchoolID);
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.FirstName.Contains(search) || p.Surname.Contains(search) || p.OtherName.Contains(search) || p.StudentCode.Contains(search));
            }

            if (filterModel.ClassID.HasValue)
                query = query.Where(l => l.ClassID == filterModel.ClassID.Value);

            if (filterModel.TermID.HasValue)
                query = query.Where(l => l.Class.TermID == filterModel.TermID.Value);

            if (filterModel.LevelID.HasValue)
                query = query.Where(l => l.Class.Arm.ClassLevelID == filterModel.LevelID.Value);

            if (filterModel.SexID.HasValue)
                query = query.Where(l => l.IsMale == (filterModel.SexID.Value == 1));

            int filteredCount = query.Count();

            string sortStr = "";
            foreach (var col in requestModel.Columns.GetSortedColumns())
            {
                sortStr += sortStr != string.Empty ? "," : "";
                if (col.Data == "FirstName" || col.Data == "Surname" || col.Data == "OtherName")
                {
                    sortStr = sortStr + col.Data + (col.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    continue;
                }

                switch (col.Data)
                {
                    case "Code": sortStr += "StudentID"; break;

                    case "Sex": sortStr += "IsMale"; break;

                    case "ClassName": sortStr += "Class.Arm.Name"; break;

                    case "Level": sortStr += "Class.Arm.ClassLevelID"; break;

                    case "TermName": sortStr += "Class.Term.Name"; break;
                }

                sortStr += SortDirStr(col.SortDirection);
            }

            var defSort = "Class.Term.Name desc, Class.Arm.Name asc, Surname asc, FirstName asc";
            var model = query.OrderBy(sortStr == string.Empty ? defSort : sortStr + ", " + defSort)
                             .Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<StudentViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _DownloadStudents(StudentFilterViewModel filterModel, string Search)
        {
            GridView newView = new GridView
            {
                AutoGenerateColumns = false,
                ShowHeaderWhenEmpty = true
            };

            newView.Columns.Add(new BoundField { HeaderText = "#", DataField = "StudentID" });
            newView.Columns.Add(new BoundField { HeaderText = "Student Code", DataField = "Code" });
            newView.Columns.Add(new BoundField { HeaderText = "First Name", DataField = "FirstName" });
            newView.Columns.Add(new BoundField { HeaderText = "Middle Name", DataField = "OtherName" });
            newView.Columns.Add(new BoundField { HeaderText = "Last Name", DataField = "Surname" });
            newView.Columns.Add(new BoundField { HeaderText = "Sex", DataField = "Sex" });
            newView.Columns.Add(new BoundField { HeaderText = "Class", DataField = "ClassName" });
            newView.Columns.Add(new BoundField { HeaderText = "Level", DataField = "Level" });
            newView.Columns.Add(new BoundField { HeaderText = "Term", DataField = "TermName" });

            var fileName = "IEPortalStudents.xlsx";

            var context = new IEContext();
            var query = new IEContext().StudentList.Where(l => l.SchoolID == filterModel.SchoolID);

            if (Search != string.Empty)
            {
                string search = Search.Trim();
                query = query.Where(p => p.FirstName.Contains(search) || p.Surname.Contains(search) || p.OtherName.Contains(search) || p.StudentCode.Contains(search));
            }

            if (filterModel.ClassID.HasValue)
                query = query.Where(l => l.ClassID == filterModel.ClassID.Value);

            if (filterModel.TermID.HasValue)
                query = query.Where(l => l.Class.TermID == filterModel.TermID.Value);

            if (filterModel.LevelID.HasValue)
                query = query.Where(l => l.Class.Arm.ClassLevelID == filterModel.LevelID.Value);

            if (filterModel.SexID.HasValue)
                query = query.Where(l => l.IsMale == (filterModel.SexID.Value == 1));

            newView.ItemType = "IEGen.Models.StudentViewModel";
            newView.DataSource = query.ProjectToList<StudentViewModel>().AsQueryable();

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

        public ActionResult _EditStudent(int StudentID, int DefTermID, string DefTermName)
        {
            var context = new IEContext();
            var model = context.StudentList.Where(m => m.StudentID == StudentID).ProjectToFirst<EditStudentViewModel>();
            model.DefTermID = DefTermID;
            model.DefTermName = DefTermName;
            
            if(model.NotInTerm)
                model.ClassList = GetClassList(context, DefTermID);
            else
                model.ClassList = GetClassList(context, model.TermID);

            model.LocationList = GetLocationList(context);

            if (!string.IsNullOrEmpty(model.GuidString))
                model.PhotoSrc = "data:image/png;base64," + Convert.ToBase64String(new Student { GuidString = model.GuidString }.DownloadFile());

            return PartialView(model);
        }

        public ActionResult _EditFloatingStudent(int StudentID, int DefTermID, string DefTermName)
        {
            var context = new IEContext();
            var model = context.StudentList.Where(m => m.StudentID == StudentID).ProjectToFirst<EditFloatingStudentViewModel>();
            model.ClassList = GetClassList(context, DefTermID);
            model.LocationList = GetLocationList(context);
            model.DefTermID = DefTermID;
            model.DefTermName = DefTermName;

            if (!string.IsNullOrEmpty(model.GuidString))
                model.PhotoSrc = "data:image/png;base64," + Convert.ToBase64String(new Student { GuidString = model.GuidString }.DownloadFile());

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteStudent(int StudentID, string DisplayName)
        {
            var context = new IEContext();
            var gp = new Student() { StudentID = StudentID };
            context.StudentList.Attach(gp);
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
            return DefaultSuccessAlert(DisplayName + " was DELETED successfully!");
        }

        [HttpPost]
        public ActionResult RemoveStudentClass(int StudentID, string DisplayName)
        {
            var context = new IEContext();
            var gp = new Student() { StudentID = StudentID };
            context.StudentList.Attach(gp);

            gp.ClassID = context.StudentResultList.Where(l => l.StudentID == StudentID).Select(l => (int?)l.ClassID).DefaultIfEmpty(null).Max();

            context.Entry(gp).Property(p => p.ClassID).IsModified = true;

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
            return DefaultSuccessAlert(DisplayName + "'s class was removed successfully!");
        }

        [HttpPost]
        public ActionResult SetStudentClass(int StudentID, int ClassID, string Name)
        {
            var context = new IEContext();
            var gp = new Student() { StudentID = StudentID };
            context.StudentList.Attach(gp);

            gp.ClassID = ClassID;

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
            return DefaultSuccessAlert(Name + "'s class was updated successfully!");
        }

        public ActionResult UpdateStudent(EditStudentViewModel model)
        {
            var lfile = Request.Files["Photo"];
            var lLength = 0;
            if(lfile != null)
            {
                lLength = lfile.ContentLength;
                if (lLength > (General.MaxPictureSizeKB * 1024))
                {
                    return DefaultErrorAlert("Please select a Photo file that is less than " + General.MaxPictureSizeKB.ToString() + " KB");
                }
            }

            var context = new IEContext();

            var gp = new Student() { StudentID = model.StudentID };
            context.StudentList.Attach(gp);
            Mapper.Map(model, gp);
            context.Entry(gp).Property(l => l.OtherName).IsModified = true;
            context.Entry(gp).Property(l => l.IsMale).IsModified = true;
            context.Entry(gp).Property(l => l.StudentCode).IsModified = true;
            context.Entry(gp).Property(l => l.BirthDate).IsModified = true;
            context.Entry(gp).Property(l => l.LocationID).IsModified = true;

            if (lLength > 0)
            {
                if (gp.GuidString != null && !context.StudentResultList.Any(l => l.StudentID == gp.StudentID && l.GuidString == gp.GuidString))
                    gp.DeleteFile();

                MemoryStream lms = new MemoryStream();
                lfile.InputStream.CopyTo(lms);

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

            return DefaultSuccessAlert(model.DisplayName + " was updated successfully!");
        }

        public ActionResult UpdateFloatingStudent(EditFloatingStudentViewModel model)
        {
            var lfile = Request.Files["Photo"];
            var lLength = 0;
            if (lfile != null)
            {
                lLength = lfile.ContentLength;
                if (lLength > (General.MaxPictureSizeKB * 1024))
                {
                    return DefaultErrorAlert("Please select a Photo file that is less than " + General.MaxPictureSizeKB.ToString() + " KB");
                }
            }

            var context = new IEContext();

            var gp = new Student() { StudentID = model.StudentID };
            context.StudentList.Attach(gp);
            Mapper.Map(model, gp);
            context.Entry(gp).Property(l => l.OtherName).IsModified = true;
            context.Entry(gp).Property(l => l.IsMale).IsModified = true;
            context.Entry(gp).Property(l => l.StudentCode).IsModified = true;
            context.Entry(gp).Property(l => l.BirthDate).IsModified = true;
            context.Entry(gp).Property(l => l.LocationID).IsModified = true;

            if (lLength > 0)
            {
                if (!context.StudentResultList.Any(l => l.StudentID == gp.StudentID && l.GuidString == gp.GuidString))
                    gp.DeleteFile();

                MemoryStream lms = new MemoryStream();
                lfile.InputStream.CopyTo(lms);

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

            return DefaultSuccessAlert(model.DisplayName + " was updated successfully!");
        }

        [Route("Setup/_AddStudent/{SchoolID:int}/")]
        public ActionResult _AddStudent(int SchoolID, int TermID, int? ClassID)
        {
            var context = new IEContext();
            var model = new EditStudentViewModel { SchoolID = SchoolID, ClassID = ClassID ?? 0 };
            model.ClassList = GetClassList(context, TermID);
            model.LocationList = GetLocationList(context);

            return PartialView(model);
        }

        public List<StudentMiniViewModel> GetSimilarStudents(Student s, IEContext context)
        {
            var query = context.StudentList.Where(l => l.SchoolID == s.SchoolID);

            var first = s.FirstName.Trim();
            var last = s.Surname.Trim();

            var hasMiddle = !string.IsNullOrEmpty(s.OtherName);  // check for null middle name and remove it from the query
            var mid = "";
            if(hasMiddle)
            {
                mid = s.OtherName.Trim();
                hasMiddle = !string.IsNullOrEmpty(mid);
            }

            query = query.Where(l => l.FirstName.Contains(first) || (hasMiddle && l.FirstName.Contains(mid)) || l.FirstName.Contains(last) ||
                                     l.OtherName.Contains(first) || (hasMiddle && l.OtherName.Contains(mid)) || l.OtherName.Contains(last) ||
                                     l.Surname.Contains(first) || (hasMiddle && l.Surname.Contains(mid)) || l.Surname.Contains(last));

            return query.ProjectToList<StudentMiniViewModel>();
        }

        public ActionResult CreateStudent(EditStudentViewModel model)
        {
            var lfile = Request.Files["Photo"];
            var lLength = 0;
            if (lfile != null)
            {
                lLength = lfile.ContentLength;
                if (lLength > (General.MaxPictureSizeKB * 1024))
                {
                    return DefaultErrorAlert("Please select a Photo file that is less than " + General.MaxPictureSizeKB.ToString() + " KB");
                }
            }

            var context = new IEContext();

            var gp = new Student();
            Mapper.Map(model, gp);

            if (!model.SaveAnyway)
            {
                var similarStudents = GetSimilarStudents(gp, context);

                if (similarStudents.Count > 0)
                {
                    Response.StatusCode = 480;

                    return PartialView("_SimilarStudents", new SimilarStudentViewModel { StudentList = similarStudents, ClassID = gp.ClassID.Value, SaveAnyway = true });
                }
            }

            if (lLength > 0)
            {
                MemoryStream lms = new MemoryStream();
                lfile.InputStream.CopyTo(lms);

                gp.UploadFile(lms.ToArray());
            }

            context.StudentList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert(model.DisplayName + " was added to the database successfully!");
        }

        [Route("Setup/StudentView/{StudentID:int}/")]
        public ActionResult StudentView(int StudentID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { l.School.Name, l.TermID, TermName = l.Term.Name }).FirstOrDefault();
            if (def == null) return RedirectToAction("School");

            var model = context.StudentList.Where(m => m.StudentID == StudentID).ProjectToFirst<StudentPageViewModel>();

            model.PastResults = context.StudentPastResultList.Where(l => l.StudentID == StudentID).ProjectToList<StudentPastResultViewModel>();

            if (!string.IsNullOrEmpty(model.GuidString))
                model.PhotoSrc = "data:image/png;base64," + Convert.ToBase64String(new Student { GuidString = model.GuidString }.DownloadFile());

            model.SchoolName = def.Name;
            model.DefTermID = def.TermID ?? 0;
            model.DefTermName = def.TermName;
            model.HeaderViewModel = hVM;

            return View(model);
        }

        [Route("Setup/_StartResultUpload/{StudentID:int}/")]
        public ActionResult _StartResultUpload(int StudentID)
        {
            return PartialView(new IEContext().StudentList.Where(m => m.StudentID == StudentID).ProjectToFirst<UploadStudentScoreViewModel>());
        }

        public ActionResult UploadStudentResult(UploadStudentScoreViewModel model)
        {
            var lfile = Request.Files["ResultFile"];
            var lLength = 0;
            if (lfile != null)
            {
                lLength = lfile.ContentLength;
                if (lLength > (General.MaxUploadedFileSizeKB * 1024))
                {
                    return DefaultErrorAlert("Please select a Result file that is less than " + General.MaxUploadedFileSizeKB.ToString() + " KB");
                }
            }

            var context = new IEContext();

            var results = new List<ResultUploadModel>();

            if (lLength > 0)
            {
                using (var reader = new StreamReader(lfile.InputStream))
                using (var csv = new CsvReader(reader))
                {
                    var records = csv.GetRecords<ResultCsvModel>();

                    foreach (var rec in records)
                        results.Add(GetUploadModel(rec));
                }
            }
            else
                return DefaultErrorAlert("Result File is Empty!");

            if (!results.Any())
                return DefaultErrorAlert("Result File is Empty!");

            var levelIDs = results.Select(l => l.ClassLevelID).Distinct();

            var templates = context.SubjectTemplateList.Where(l => l.SchoolID == model.SchoolID.Value && levelIDs.Contains(l.ClassLevelID)).ProjectToList<SubjectTemplateMiniModel>();

            foreach(var res in results)
            {
                var subName = res.SubjectName.Trim();
                res.TemplateID = templates.Where(l => l.ClassLevelID == res.ClassLevelID && l.Name.Contains(subName)).Select(l => (int?)l.TemplateID).FirstOrDefault();
                res.TemplateList = GetTemplateList(templates, res.ClassLevelID);
            }

            var verifyModel = new VerifyStudentScoreUploadViewModel();
            Mapper.Map(model, verifyModel);

            verifyModel.ResultList = results.OrderBy(l =>l.HasTemplateID).ThenBy(l => l.ClassLevelID).ToList();

            return PartialView("_VerifyResultUpload", verifyModel);
        }

        public ResultUploadModel GetUploadModel(ResultCsvModel csvModel)
        {
            ResultUploadModel model = new ResultUploadModel();
            Mapper.Map(csvModel, model);

            if (model.Level < 10)
                model.ClassLevelID = (byte)(model.Level + 10);
            else
                model.ClassLevelID = (byte)(model.Level - 15);

            return model;
        }

        private List<SelectListItem> GetTemplateList(List<SubjectTemplateMiniModel> templateList, byte ClassLevelID)
        {
            var list = new List<SelectListItem>();

            var gpList = templateList.Where(l => l.ClassLevelID == ClassLevelID)
                                     .Select(l => new SelectListItem { Value = l.TemplateID.ToString(), Text = l.Name });

            list.AddRange(gpList);
            return list;
        }

        public ActionResult FinishResultUpload(VerifyStudentScoreUploadViewModel model)
        {
            var context = new IEContext();

            var scoreList = new List<StudentPastResult>();

            foreach(var res in model.ResultList)
            {
                var templateID = res.TemplateID.Value;
                if(scoreList.Any(l => l.TemplateID == templateID))
                {
                    var pScore = scoreList.Where(l => l.TemplateID == templateID).First();
                    AttachScore(pScore, res.Score, res.TermNumber);
                }
                else
                {
                    var tScore = new StudentPastResult { StudentID = model.StudentID, TemplateID = templateID, Year = res.Year };
                    AttachScore(tScore, res.Score, res.TermNumber);

                    scoreList.Add(tScore);
                }
            }

            context.StudentPastResultList.AddOrUpdate(scoreList.ToArray());
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert(scoreList.Count().ToString() + " past results were successfully uploaded!");
        }

        public void AttachScore(StudentPastResult result, decimal score, byte termNumber)
        {
            switch (termNumber)
            {
                case 1: result.Term1Score = score; return;
                case 2: result.Term2Score = score; return;
                case 3: result.Term3Score = score; return;
            }
        }

        public ActionResult _DownloadResultCSV(int StudentID, string StudentName)
        {
            var context = new IEContext();

            var maxTerm = context.StudentResultList.Where(l => l.StudentID == StudentID).OrderByDescending(l => l.Class.Term.SchoolYear).ThenByDescending(l => l.Class.Term.TermNumber)
                                 .Select(l => new { l.Class.Term.SchoolYear, l.Class.Term.TermNumber }).FirstOrDefault();

            if (maxTerm == null || maxTerm.SchoolYear == 0)
                new EmptyResult();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Year,TermNumber,Level,SubjectName,Score");

            var resl = context.ScoreEntryList.Where(l => l.StudentID == StudentID && l.ExamScore.HasValue)
                              .Where(l => l.Subject.SchoolYear < maxTerm.SchoolYear || (l.Subject.SchoolYear == maxTerm.SchoolYear && l.Subject.TermNumber <= maxTerm.TermNumber))
                              .ProjectToList<CurrentResultModel>();

            foreach (var res in resl)
            {
                csv.AppendLine(res.CsvRow);
            }

            return File(new System.Text.UTF8Encoding().GetBytes(csv.ToString()), "text/csv", StudentName.Replace(" ", "-") + ".csv");
        }

        public ActionResult UploadedStudents()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { l.TermID, TermName = l.Term.Name }).FirstOrDefault();
            if (def == null) return RedirectToAction("School");

            var model = context.SchoolList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirst<FloatingStudentsPageViewModel>();
            model.StudentList = context.StudentList.Where(l => l.SchoolID == hVM.SchoolID && !l.ClassID.HasValue).ProjectToList<FloatingStudentViewModel>();
            model.DefTermName = def.TermName;
            model.DefTermID = def.TermID ?? 0;

            model.HeaderViewModel = hVM;

            return View(model);
        }

        #endregion

        #region Grade Groups

        public ActionResult SchoolGrades()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.SchoolDataList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirstOrDefault<SchoolGradesViewModel>();
            if(model == null)
            {
                model = context.SchoolList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirstOrDefault<SchoolGradesViewModel>();
            }
            model.GroupList = context.GradeGroupList.Where(l => l.SchoolID == null || l.SchoolID == hVM.SchoolID).ProjectToList<GradeGroupViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _ViewGradeGroup(int GradeGroupID)
        {
            var context = new IEContext();
            var model = context.GradeGroupList.Where(m => m.GradeGroupID == GradeGroupID).ProjectToFirst<ViewGradeGroupViewModel>();

            if (model.SchoolID.HasValue)
                model.SchoolName = context.SchoolList.Where(l => l.SchoolID == model.SchoolID).Select(l => l.Name).First();

            return PartialView(model);
        }

        [Route("Setup/GradeSetup/{GradeGroupID:int}")]
        public ActionResult GradeSetup(int GradeGroupID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            var model = context.GradeGroupList.Where(m => m.GradeGroupID == GradeGroupID).ProjectToFirst<ViewGradeGroupViewModel>();

            if (model.SchoolID.HasValue)
                model.SchoolName = context.SchoolList.Where(l => l.SchoolID == model.SchoolID).Select(l => l.Name).First();
            else
                return RedirectToAction("SchoolGrades", "Setup");

            model.HeaderViewModel = hVM;

            return View(model);
        }

        [Route("Setup/SetDefGradeGroup/{SchoolID:int}/")]
        public ActionResult SetDefGradeGroup(int SchoolID, int GradeGroupID)
        {
            var context = new IEContext();
            var gp = context.SchoolDataList.Where(l => l.SchoolID == SchoolID).FirstOrDefault();

            if (gp != null)
                gp.DefGradeGroupID = GradeGroupID;
            else
            {
                gp = new SchoolData { SchoolID = SchoolID, DefGradeGroupID = GradeGroupID };
                context.SchoolDataList.Add(gp);
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

            return JSRedirect(Url.Action("SchoolGrades", "Setup"));
        }
        #endregion

        #region Skill Groups

        public ActionResult SchoolSkills()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");


            var model = context.SchoolDataList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirstOrDefault<SchoolSkillsViewModel>();
            if (model == null)
            {
                model = context.SchoolList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirstOrDefault<SchoolSkillsViewModel>();
            }
            model.GroupList = context.SkillGroupList.Where(l => l.SchoolID == null || l.SchoolID == hVM.SchoolID).ProjectToList<SkillGroupViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _ViewSkillGroup(int SkillGroupID)
        {
            var context = new IEContext();
            var model = context.SkillGroupList.Where(m => m.SkillGroupID == SkillGroupID).ProjectToFirst<ViewSkillGroupViewModel>();

            if (model.SchoolID.HasValue)
                model.SchoolName = context.SchoolList.Where(l => l.SchoolID == model.SchoolID).Select(l => l.Name).First();

            return PartialView(model);
        }

        [Route("Setup/SkillSetup/{SkillGroupID:int}")]
        public ActionResult SkillSetup(int SkillGroupID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            var model = context.SkillGroupList.Where(m => m.SkillGroupID == SkillGroupID).ProjectToFirst<ViewSkillGroupViewModel>();

            if (model.SchoolID.HasValue)
                model.SchoolName = context.SchoolList.Where(l => l.SchoolID == model.SchoolID).Select(l => l.Name).First();
            else
                return RedirectToAction("SchoolSkills", "Setup");

            model.HeaderViewModel = hVM;

            return View(model);
        }

        [Route("Setup/SetDefSkillGroup/{SchoolID:int}/")]
        public ActionResult SetDefSkillGroup(int SchoolID, int SkillGroupID)
        {
            var context = new IEContext();
            var gp = context.SchoolDataList.Where(l => l.SchoolID == SchoolID).FirstOrDefault();

            if (gp != null)
                gp.DefSkillGroupID = SkillGroupID;
            else
            {
                gp = new SchoolData { SchoolID = SchoolID, DefSkillGroupID = SkillGroupID };
                context.SchoolDataList.Add(gp);
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

            return JSRedirect(Url.Action("SchoolSkills", "Setup"));
        }
        #endregion

        #region Performance Comment Groups

        public ActionResult PerformanceComments()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.SchoolDataList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirstOrDefault<SchoolPerformanceCommentViewModel>();
            if (model == null)
            {
                model = context.SchoolList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirstOrDefault<SchoolPerformanceCommentViewModel>();
            }
            model.GroupList = context.PerformanceCommentGroupList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToList<PerformanceCommentGroupViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _ViewPerformanceCommentGroup(int GroupID)
        {
            var context = new IEContext();
            var model = context.PerformanceCommentGroupList.Where(m => m.GroupID == GroupID).ProjectToFirst<PerformanceCommentSetupViewModel>();

            return PartialView(model);
        }

        [Route("Setup/PerformanceCommentSetup/{GroupID:int}")]
        public ActionResult PerformanceCommentSetup(int GroupID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            var model = context.PerformanceCommentGroupList.Where(m => m.GroupID == GroupID).ProjectToFirst<PerformanceCommentSetupViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        [Route("Setup/SetDefPerformanceCommentGroup/{SchoolID:int}/")]
        public ActionResult SetDefPerformanceCommentGroup(int SchoolID, int GroupID)
        {
            var context = new IEContext();
            var gp = context.SchoolDataList.Where(l => l.SchoolID == SchoolID).FirstOrDefault();

            if (gp != null)
                gp.DefPerformanceCommentGroupID = GroupID;
            else
            {
                gp = new SchoolData { SchoolID = SchoolID, DefPerformanceCommentGroupID = GroupID };
                context.SchoolDataList.Add(gp);
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

            return JSRedirect(Url.Action("PerformanceComments", "Setup"));
        }

        public ActionResult _EditPerformanceCommentGroup(int GroupID)
        {
            var context = new IEContext();
            var model = context.PerformanceCommentGroupList.Where(m => m.GroupID == GroupID).ProjectToFirst<PerformanceCommentGroupViewModel>();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeletePerformanceCommentGroup(int GroupID)
        {
            var context = new IEContext();
            var gp = new PerformanceCommentGroup() { GroupID = GroupID };
            context.PerformanceCommentGroupList.Attach(gp);
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
            return Json(GroupID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePerformanceCommentGroup(PerformanceCommentGroupViewModel model)
        {
            var context = new IEContext();

            if (!string.IsNullOrEmpty(model.Name) && context.PerformanceCommentGroupList.Any(l => l.GroupID != model.GroupID && l.Name == model.Name))
                return DefaultErrorAlert("Another Performance Comment Group with this name already exists!");

            var gp = new PerformanceCommentGroup() { GroupID = model.GroupID };
            context.PerformanceCommentGroupList.Attach(gp);
            gp.Name = model.Name;

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

        [Route("Setup/_AddPerformanceCommentGroup/{SchoolID:int}")]
        public ActionResult _AddPerformanceCommentGroup(int SchoolID)
        {
            return PartialView();
        }

        public ActionResult CreatePerformanceCommentGroup(PerformanceCommentGroupViewModel model)
        {
            var context = new IEContext();

            if (context.PerformanceCommentGroupList.Any(l => l.SchoolID == model.SchoolID && l.Name == model.Name))
                return DefaultErrorAlert("A Performance Comment Group with this name already exists!");

            var gp = new PerformanceCommentGroup { Name = model.Name, SchoolID = model.SchoolID };
            context.PerformanceCommentGroupList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return JSRedirect(Url.Action("PerformanceCommentSetup", "Setup", new { gp.GroupID }));
        }

        public ActionResult _EditPerformanceComment(int CommentID)
        {
            var context = new IEContext();
            var model = context.PerformanceCommentList.Where(m => m.CommentID == CommentID).ProjectToFirst<PerformanceCommentViewModel>();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeletePerformanceComment(int CommentID)
        {
            var context = new IEContext();
            var gp = new PerformanceComment() { CommentID = CommentID };
            context.PerformanceCommentList.Attach(gp);
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
            return Json(CommentID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePerformanceComment(PerformanceCommentViewModel model)
        {
            var context = new IEContext();

            var gp = new PerformanceComment() { CommentID = model.CommentID };
            context.PerformanceCommentList.Attach(gp);
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

        [Route("Setup/_AddPerformanceComment/{GroupID:int}")]
        public ActionResult _AddPerformanceComment(int GroupID)
        {
            return PartialView();
        }

        public ActionResult CreatePerformanceComment(PerformanceCommentViewModel model)
        {
            var context = new IEContext();

            var gp = new PerformanceComment();
            context.PerformanceCommentList.Add(gp);
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

            model.CommentID = gp.CommentID;
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Promotion Comment Groups

        public ActionResult PromotionComments()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.SchoolDataList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirstOrDefault<SchoolPromotionCommentViewModel>();
            if (model == null)
            {
                model = context.SchoolList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirstOrDefault<SchoolPromotionCommentViewModel>();
            }
            model.GroupList = context.PromotionCommentGroupList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToList<PromotionCommentGroupViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _ViewPromotionCommentGroup(int GroupID)
        {
            var context = new IEContext();
            var model = context.PromotionCommentGroupList.Where(m => m.GroupID == GroupID).ProjectToFirst<PromotionCommentSetupViewModel>();

            return PartialView(model);
        }

        [Route("Setup/PromotionCommentSetup/{GroupID:int}")]
        public ActionResult PromotionCommentSetup(int GroupID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            var model = context.PromotionCommentGroupList.Where(m => m.GroupID == GroupID).ProjectToFirst<PromotionCommentSetupViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        [Route("Setup/SetDefPromotionCommentGroup/{SchoolID:int}/")]
        public ActionResult SetDefPromotionCommentGroup(int SchoolID, int GroupID)
        {
            var context = new IEContext();
            var gp = context.SchoolDataList.Where(l => l.SchoolID == SchoolID).FirstOrDefault();

            if (gp != null)
                gp.DefPromotionCommentGroupID = GroupID;
            else
            {
                gp = new SchoolData { SchoolID = SchoolID, DefPromotionCommentGroupID = GroupID };
                context.SchoolDataList.Add(gp);
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

            return JSRedirect(Url.Action("PromotionComments", "Setup"));
        }

        public ActionResult _EditPromotionCommentGroup(int GroupID)
        {
            var context = new IEContext();
            var model = context.PromotionCommentGroupList.Where(m => m.GroupID == GroupID).ProjectToFirst<PromotionCommentGroupViewModel>();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeletePromotionCommentGroup(int GroupID)
        {
            var context = new IEContext();
            var gp = new PromotionCommentGroup() { GroupID = GroupID };
            context.PromotionCommentGroupList.Attach(gp);
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
            return Json(GroupID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePromotionCommentGroup(PromotionCommentGroupViewModel model)
        {
            var context = new IEContext();

            if (!string.IsNullOrEmpty(model.Name) && context.PromotionCommentGroupList.Any(l => l.GroupID != model.GroupID && l.Name == model.Name))
                return DefaultErrorAlert("Another Promotion Comment Group with this name already exists!");

            var gp = new PromotionCommentGroup() { GroupID = model.GroupID };
            context.PromotionCommentGroupList.Attach(gp);
            gp.Name = model.Name;

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

        [Route("Setup/_AddPromotionCommentGroup/{SchoolID:int}")]
        public ActionResult _AddPromotionCommentGroup(int SchoolID)
        {
            return PartialView();
        }

        public ActionResult CreatePromotionCommentGroup(PromotionCommentGroupViewModel model)
        {
            var context = new IEContext();

            if (context.PromotionCommentGroupList.Any(l => l.SchoolID == model.SchoolID && l.Name == model.Name))
                return DefaultErrorAlert("A Promotion Comment Group with this name already exists!");

            var gp = new PromotionCommentGroup { Name = model.Name, SchoolID = model.SchoolID };
            context.PromotionCommentGroupList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return JSRedirect(Url.Action("PromotionCommentSetup", "Setup", new { gp.GroupID }));
        }

        public ActionResult _EditPromotionComment(int CommentID)
        {
            var context = new IEContext();
            var model = context.PromotionCommentList.Where(m => m.CommentID == CommentID).ProjectToFirst<PromotionCommentViewModel>();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeletePromotionComment(int CommentID)
        {
            var context = new IEContext();
            var gp = new PromotionComment() { CommentID = CommentID };
            context.PromotionCommentList.Attach(gp);
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
            return Json(CommentID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePromotionComment(PromotionCommentViewModel model)
        {
            var context = new IEContext();

            var gp = new PromotionComment() { CommentID = model.CommentID };
            context.PromotionCommentList.Attach(gp);
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

        [Route("Setup/_AddPromotionComment/{GroupID:int}")]
        public ActionResult _AddPromotionComment(int GroupID)
        {
            return PartialView();
        }

        public ActionResult CreatePromotionComment(PromotionCommentViewModel model)
        {
            var context = new IEContext();

            var gp = new PromotionComment();
            context.PromotionCommentList.Add(gp);
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

            model.CommentID = gp.CommentID;
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Improvement Comment Groups

        public ActionResult ImprovementComments()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.SchoolDataList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirstOrDefault<SchoolImprovementCommentViewModel>();
            if (model == null)
            {
                model = context.SchoolList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirstOrDefault<SchoolImprovementCommentViewModel>();
            }
            model.GroupList = context.ImprovementCommentGroupList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToList<ImprovementCommentGroupViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _ViewImprovementCommentGroup(int GroupID)
        {
            var context = new IEContext();
            var model = context.ImprovementCommentGroupList.Where(m => m.GroupID == GroupID).ProjectToFirst<ImprovementCommentSetupViewModel>();

            return PartialView(model);
        }

        [Route("Setup/ImprovementCommentSetup/{GroupID:int}")]
        public ActionResult ImprovementCommentSetup(int GroupID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            var model = context.ImprovementCommentGroupList.Where(m => m.GroupID == GroupID).ProjectToFirst<ImprovementCommentSetupViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        [Route("Setup/SetDefImprovementCommentGroup/{SchoolID:int}/")]
        public ActionResult SetDefImprovementCommentGroup(int SchoolID, int GroupID)
        {
            var context = new IEContext();
            var gp = context.SchoolDataList.Where(l => l.SchoolID == SchoolID).FirstOrDefault();

            if (gp != null)
                gp.DefImprovementCommentGroupID = GroupID;
            else
            {
                gp = new SchoolData { SchoolID = SchoolID, DefImprovementCommentGroupID = GroupID };
                context.SchoolDataList.Add(gp);
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

            return JSRedirect(Url.Action("ImprovementComments", "Setup"));
        }

        public ActionResult _EditImprovementCommentGroup(int GroupID)
        {
            var context = new IEContext();
            var model = context.ImprovementCommentGroupList.Where(m => m.GroupID == GroupID).ProjectToFirst<ImprovementCommentGroupViewModel>();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteImprovementCommentGroup(int GroupID)
        {
            var context = new IEContext();
            var gp = new ImprovementCommentGroup() { GroupID = GroupID };
            context.ImprovementCommentGroupList.Attach(gp);
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
            return Json(GroupID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateImprovementCommentGroup(ImprovementCommentGroupViewModel model)
        {
            var context = new IEContext();

            if (!string.IsNullOrEmpty(model.Name) && context.ImprovementCommentGroupList.Any(l => l.GroupID != model.GroupID && l.Name == model.Name))
                return DefaultErrorAlert("Another Improvement Comment Group with this name already exists!");

            var gp = new ImprovementCommentGroup() { GroupID = model.GroupID };
            context.ImprovementCommentGroupList.Attach(gp);
            gp.Name = model.Name;

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

        [Route("Setup/_AddImprovementCommentGroup/{SchoolID:int}")]
        public ActionResult _AddImprovementCommentGroup(int SchoolID)
        {
            return PartialView();
        }

        public ActionResult CreateImprovementCommentGroup(ImprovementCommentGroupViewModel model)
        {
            var context = new IEContext();

            if (context.ImprovementCommentGroupList.Any(l => l.SchoolID == model.SchoolID && l.Name == model.Name))
                return DefaultErrorAlert("A Improvement Comment Group with this name already exists!");

            var gp = new ImprovementCommentGroup { Name = model.Name, SchoolID = model.SchoolID };
            context.ImprovementCommentGroupList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return JSRedirect(Url.Action("ImprovementCommentSetup", "Setup", new { gp.GroupID }));
        }

        public ActionResult _EditImprovementComment(int CommentID)
        {
            var context = new IEContext();
            var model = context.ImprovementCommentList.Where(m => m.CommentID == CommentID).ProjectToFirst<ImprovementCommentViewModel>();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteImprovementComment(int CommentID)
        {
            var context = new IEContext();
            var gp = new ImprovementComment() { CommentID = CommentID };
            context.ImprovementCommentList.Attach(gp);
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
            return Json(CommentID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateImprovementComment(ImprovementCommentViewModel model)
        {
            var context = new IEContext();

            var gp = new ImprovementComment() { CommentID = model.CommentID };
            context.ImprovementCommentList.Attach(gp);
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

        [Route("Setup/_AddImprovementComment/{GroupID:int}")]
        public ActionResult _AddImprovementComment(int GroupID)
        {
            return PartialView();
        }

        public ActionResult CreateImprovementComment(ImprovementCommentViewModel model)
        {
            var context = new IEContext();

            var gp = new ImprovementComment();
            context.ImprovementCommentList.Add(gp);
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

            model.CommentID = gp.CommentID;
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region Other Exam Types
        public ActionResult OtherExamTypes()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.SchoolList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToFirst<OtherExamTypePageViewModel>();
            model.TypeList = context.OtherExamTypeList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToList<OtherExamTypeViewModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult _EditOtherExamType(int TypeID)
        {
            var context = new IEContext();
            var model = context.OtherExamTypeList.Where(m => m.TypeID == TypeID).ProjectToFirst<OtherExamTypeViewModel>();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DeleteOtherExamType(int TypeID)
        {
            var context = new IEContext();
            var gp = new OtherExamType() { TypeID = TypeID };
            context.OtherExamTypeList.Attach(gp);
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
            return Json(TypeID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateOtherExamType(OtherExamTypeViewModel model)
        {
            var context = new IEContext();

            if (context.OtherExamTypeList.Any(l => l.SchoolID == model.SchoolID && l.TypeID != model.TypeID && l.Name == model.Name))
                return DefaultErrorAlert("Another Exam Type with this name already exists!");

            var gp = new OtherExamType() { TypeID = model.TypeID };
            context.OtherExamTypeList.Attach(gp);
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

        public ActionResult _AddOtherExamType(int SchoolID)
        {
            return PartialView();
        }

        public ActionResult CreateOtherExamType(OtherExamTypeViewModel model)
        {
            var context = new IEContext();

            if (context.OtherExamTypeList.Any(l => l.SchoolID == model.SchoolID && l.Name == model.Name))
                return DefaultErrorAlert("An Exam Type with this name already exists!");

            var gp = new OtherExamType();
            Mapper.Map(model, gp);
            context.OtherExamTypeList.Add(gp);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            model.TypeID = gp.TypeID;
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}