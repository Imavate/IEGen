using AutoMapper;
using DataTables.Mvc;
using IEGen.Models;
using PdfSharp;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using Z.EntityFramework.Plus;

namespace IEGen.Controllers
{
    [Authorize]
    public class ReportController : BaseController
    {
        // GET: Report
        public ActionResult Index()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            int termID = 0;
            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { l.TermID }).FirstOrDefault();
                if (def == null || def.TermID == null) return RedirectToAction("School");
                termID = def.TermID.Value;
            }
            else
            {
                var tmID = GetCurrentTermID(context, hVM.SchoolID);
                if (tmID == null) return RedirectToAction("Index", "Home");

                termID = tmID.Value;
            }

            var model = context.TermList.Where(l => l.TermID == termID).ProjectToFirst<ReportIndexPageViewModel>();

            model.HeaderViewModel = hVM;

            var res = context.ClassResultList.Where(l => l.Class.TermID == termID)
                             .OrderBy(l => l.Class.Arm.ClassLevelID).ThenBy(l => l.Class.Arm.Name)
                             .ProjectToList<ClassReportModel>();

            model.Results = new List<ClassReportModel>();

            foreach(var c in model.Classes)
            {
                var rm = res.Where(l => l.ClassID == c.ClassID).FirstOrDefault();
                if (rm == null)
                    rm = new ClassReportModel { ClassID = c.ClassID };

                rm.Name = c.Name;
                rm.StudentCount = c.StudentCount;
                rm.SubjectCount = c.SubjectCount;

                model.Results.Add(rm);
            }

            return View(model);
        }

        public ActionResult _AnalyzeClass(int ClassID)
        {
            return PartialView(new IEContext().ClassList.Where(m => m.ClassID == ClassID).ProjectToFirst<AnalyzeClassViewModel>());
        }

        [HttpPost]
        public ActionResult AnalyzeScores(int ClassID)
        {
            var context = new IEContext();

            //get all score entries...
            var scores = context.ScoreEntryList.Where(l => l.Subject.ClassID == ClassID && l.ExamScore.HasValue).ProjectToList<ScoreEntryRepModel>();

            var stIDs = scores.Select(l => l.StudentID).Distinct();
            var sbIDs = scores.Select(l => l.SubjectID).Distinct();

            decimal maxAvg = 0;
            byte subCnt = 0;
            var bestSubjectID = 0;
            foreach(var id in sbIDs)
            {
                var avg = scores.Where(l => l.SubjectID == id).Select(l => l.Total).Average();
                if (avg == null) continue;

                //var sub = new Subject { SubjectID = id };
                //context.SubjectList.Attach(sub);

                //sub.AverageScore = avg.Value;
                //sub.ResultCount = (byte)scores.Count(l => l.SubjectID == id);

                if (avg > maxAvg)
                {
                    maxAvg = avg.Value;
                    bestSubjectID = id;
                }

                subCnt++;
            }

            if (subCnt == 0)
                return DefaultErrorAlert("There are no subject scores for this class!");

            maxAvg = 0;
            decimal lowAvg = 100;
            byte resCnt = 0;
            var bestStudentID = 0;

            var clres = context.ClassList.Where(l => l.ClassID == ClassID).ProjectToFirst<ClassAnalysisModel>();

            var ycls = context.ClassList.Where(l => l.Term.SchoolID == clres.SchoolID && l.Term.SchoolYear == clres.SchoolYear).Select(l => new { l.ClassID, l.Term.TermNumber }).ToList();
            var thisTermNum = ycls.Where(l => l.ClassID == ClassID).Select(l => l.TermNumber).First();
            var yclIDs = ycls.Where(l => l.TermNumber < thisTermNum).Select(l => l.ClassID).ToList();

            var resList = new List<StudentResult>();
            var stList = context.StudentList.Where(l => stIDs.Contains(l.StudentID))
                                .Select(l => new { l.StudentID, l.GuidString, l.TeacherComment, l.PrincipalComment, l.FirstName, l.IsMale }).ToList();

            foreach (var id in stIDs)
            {
                var low = scores.Where(l => l.StudentID == id).Min(l => l.Total);
                if (low == null) continue;

                var avg = scores.Where(l => l.StudentID == id).Average(l => l.Total);
                var max = scores.Where(l => l.StudentID == id).Max(l => l.Total);

                if (avg < lowAvg) lowAvg = avg.Value;

                if (avg > maxAvg)
                {
                    maxAvg = avg.Value;
                    bestStudentID = id;
                }

                var res = new StudentResult { StudentID = id, ClassID = ClassID, AverageScore = avg.Value, HighestScore = max.Value, LowestScore = low.Value };
                res.BestSubjectID = scores.Where(l => l.StudentID == id).OrderByDescending(l => l.Total).Select(l => l.SubjectID).First();
                res.SubjectCount = (byte)scores.Count(l => l.StudentID == id && l.Total.HasValue);

                res.GradeCountA = (byte)scores.Count(l => l.StudentID == id && l.SummaryGradeID == (byte)SummaryGrade.A);
                res.GradeCountB = (byte)scores.Count(l => l.StudentID == id && l.SummaryGradeID == (byte)SummaryGrade.B);
                res.GradeCountC = (byte)scores.Count(l => l.StudentID == id && l.SummaryGradeID == (byte)SummaryGrade.C);
                res.GradeCountD = (byte)scores.Count(l => l.StudentID == id && l.SummaryGradeID == (byte)SummaryGrade.D);
                res.GradeCountE = (byte)scores.Count(l => l.StudentID == id && l.SummaryGradeID == (byte)SummaryGrade.E);
                res.GradeCountF = (byte)scores.Count(l => l.StudentID == id && l.SummaryGradeID == (byte)SummaryGrade.F);


                var fmrAvg = context.StudentResultList.Where(l => l.StudentID == id && l.Class.Term.NextResumptionDate < clres.NextResumptionDate)
                                    .OrderByDescending(l => l.Class.Term.NextResumptionDate)
                                    .Select(l => (decimal?)l.AverageScore).FirstOrDefault();

                if (fmrAvg.HasValue)
                    res.Improvement = (((res.AverageScore - fmrAvg) / fmrAvg) * 100).Value;

                var st = stList.Where(l => l.StudentID == id).First();
                res.GuidString = st.GuidString;
                res.TeacherComment = st.TeacherComment;

                if (clres.UsePerformanceComments)
                {
                    var perf = avg.Value;

                    if (clres.IsPromotionalClass && clres.CommentOnYearResult)
                    {
                        var avgs = context.StudentResultList.Where(l => l.StudentID == id && yclIDs.Contains(l.ClassID)).Select(l => l.AverageScore).ToList();

                        perf = (avgs.Sum() + perf) / (avgs.Count + 1);
                    }

                    var comment = clres.PerformanceComments
                                       .Where(l => l.LowerBound <= perf && (l.UpperBound + 1) > perf)
                                       .OrderByDescending(l => l.LowerBound)
                                       .Select(l => l.Comment).FirstOrDefault();

                    if (comment != null)
                    {
                        comment = comment.Replace("{:NAME}", st.FirstName);
                        comment = comment.Replace("{:SEX1}", st.IsMale ? "he" : "she");
                        comment = comment.Replace("{:SEX2}", st.IsMale ? "him" : "her");

                        if (clres.UseImprovementComments)
                        {
                            var badCnt = scores.Count(l => l.StudentID == id && l.Total < clres.RedLine);
                            var impCom = clres.ImprovementComments
                                              .Where(l => l.MinFailCount <= badCnt && l.MaxFailCount >= badCnt)
                                              .OrderByDescending(l => l.MinFailCount)
                                              .Select(l => l.Comment).FirstOrDefault();

                            comment += (comment.EndsWith(".") ? " " : ". ") + (impCom ?? "");
                        }

                        if (clres.IsPromotionalClass && clres.UsePromotionComments)
                        {
                            var promCom = clres.PromotionComments
                                               .Where(l => l.LowerBound <= perf && (l.UpperBound + 1) > perf)
                                               .OrderByDescending(l => l.LowerBound)
                                               .Select(l => l.Comment).FirstOrDefault();

                            var nlid = clres.ClassLevelID;
                            promCom = promCom.Replace("{:NEXT_CLASS}", Eval.GetDisplayName(typeof(ClassLevel), ++nlid));

                            comment += (comment.EndsWith(".") ? " " : ". ") + (promCom ?? "");
                        }

                        res.PrincipalComment = General.CapitalizeFirst(comment);
                    }
                }
                else
                {
                    var perf = avg.Value;
                    var comment = st.PrincipalComment;

                    if (clres.IsPromotionalClass && clres.CommentOnYearResult)
                    {
                        var avgs = context.StudentResultList.Where(l => l.StudentID == id && yclIDs.Contains(l.ClassID)).Select(l => l.AverageScore).ToList();

                        perf = (avgs.Sum() + perf) / (avgs.Count + 1);
                    }

                    if (clres.IsPromotionalClass && clres.UsePromotionComments)
                    {
                        var promCom = clres.PromotionComments
                                           .Where(l => l.LowerBound <= perf && (l.UpperBound + 1) > perf)
                                           .OrderByDescending(l => l.LowerBound)
                                           .Select(l => l.Comment).FirstOrDefault();

                        var nlid = clres.ClassLevelID;
                        promCom = promCom.Replace("{:NEXT_CLASS}", Eval.GetDisplayName(typeof(ClassLevel), ++nlid));

                        comment += (comment.EndsWith(".") ? " " : ". ") + (promCom ?? "");
                    }

                    res.PrincipalComment = General.CapitalizeFirst(comment);
                }

                resList.Add(res);

                resCnt++;
            }

            byte pos = 1;
            byte rpos = 1;
            decimal tavg = 0;
            foreach(var r in resList.OrderByDescending(l => l.AverageScore))
            {
                if(tavg != r.AverageScore)
                {
                    tavg = r.AverageScore;
                    pos = rpos;
                }

                r.Position = pos;

                context.StudentResultList.AddOrUpdate(r);
                rpos++;
            }

            var cres = new ClassResult { ClassID = ClassID, ResultCount = resCnt, SubjectCount = subCnt, BestAverage = maxAvg, BestStudentID = bestStudentID  };
            cres.BestSubjectID = bestSubjectID;
            cres.LowestAverage = lowAvg;
            cres.MeanAverage = scores.Average(l => l.Total).Value;
            cres.AnalysisTime = DateTime.Now;

            context.ClassResultList.AddOrUpdate(cres);

            var term = new Term { TermID = clres.TermID };
            context.TermList.Attach(term);

            term.GuidString = clres.LogoGuidString;

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

            return Json("", JsonRequestBehavior.AllowGet);
        }

        #region Class Reports
        [Route("Report/Classes")]
        public ActionResult ClassReports()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new ClassReportPageViewModel();

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

            model.Results = context.ClassResultList.Where(l => l.Class.TermID == model.TermID)
                                   .OrderBy(l => l.Class.Arm.ClassLevelID).ThenBy(l => l.Class.Arm.Name)
                                   .ProjectToList<ClassReportModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public BroadsheetViewModel GetBroadsheetModel(int ClassID, IEContext context)
        {
            var model = context.ClassResultList.Where(l => l.ClassID == ClassID).ProjectToFirst<BroadsheetViewModel>();
            var rows = context.StudentResultList.Where(l => l.ClassID == ClassID).ProjectToList<BroadsheetRowModel>();

            if (model == null || rows == null || !rows.Any()) return null;

            var scores = model.Scores;
            var subjects = model.Subjects;

            byte cnt = 0;
            var subList = new List<SubjectResultModel>();
            foreach (var sub in subjects.OrderBy(s => s.Order).ThenBy(l => l.ResultName))
            {
                var avg = scores.Where(l => l.SubjectID == sub.SubjectID).Average(l => l.Total);

                if (avg == null) continue;

                sub.CN = ++cnt;
                sub.AverageScore = avg.Value;
                subList.Add(sub);
            }

            cnt = 0;
            var stdList = new List<BroadsheetRowModel>();
            foreach(var r in rows.OrderBy(l => l.StudentName))
            {
                var row = new BroadsheetRowModel { SN = ++cnt, AverageScore = r.AverageScore, Position = r.Position, StudentID = r.StudentID, StudentName = r.StudentName };

                var scList = new List<BroadsheetCellModel>();
                foreach (var sub in subList)
                    scList.Add(new BroadsheetCellModel { CN = sub.CN, Total = scores.Where(l => l.StudentID == r.StudentID && l.SubjectID == sub.SubjectID).Select(l => l.Total).FirstOrDefault() });

                row.Scores = scList;

                stdList.Add(row);
            }

            model.Subjects = subList;
            model.Rows = stdList;

            return model;
        }

        public FullBroadsheetViewModel GetFullBroadsheetModel(int ClassID, IEContext context)
        {
            var model = context.ClassResultList.Where(l => l.ClassID == ClassID).ProjectToFirst<FullBroadsheetViewModel>();
            var rows = context.StudentResultList.Where(l => l.ClassID == ClassID).ProjectToList<BroadsheetRowModel>();

            if (model == null || rows == null || !rows.Any()) return null;

            var scores = model.Scores;
            var subjects = model.Subjects;

            byte cnt = 0;
            var subList = new List<SubjectResultModel>();
            foreach (var sub in subjects.OrderBy(s => s.Order).ThenBy(l => l.ResultName))
            {
                var avg = scores.Where(l => l.SubjectID == sub.SubjectID).Average(l => l.Total);

                if (avg == null) continue;

                sub.CN = ++cnt;
                sub.AverageScore = avg.Value;
                subList.Add(sub);
            }

            cnt = 0;
            var stdList = new List<BroadsheetRowModel>();
            foreach (var r in rows.OrderBy(l => l.StudentName))
            {
                var row = new BroadsheetRowModel { SN = ++cnt, AverageScore = r.AverageScore, Position = r.Position, StudentID = r.StudentID, StudentName = r.StudentName };

                var scList = new List<BroadsheetCellModel>();
                foreach (var sub in subList)
                {
                    var sc = scores.Where(l => l.StudentID == r.StudentID && l.SubjectID == sub.SubjectID).Select(l => new { l.Total, l.GradeName, l.SummaryGrade }).FirstOrDefault();

                    if (sc != null)
                        scList.Add(new BroadsheetCellModel { CN = sub.CN, Total = sc.Total, GradeStr = model.ShowSummaryGrade ? sc.SummaryGrade : sc.GradeName });
                    else
                        scList.Add(new BroadsheetCellModel { CN = sub.CN });
                }

                row.CountA = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.A);
                row.CountB = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.B);
                row.CountC = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.C);
                row.CountD = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.D);
                row.CountE = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.E);
                row.CountF = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.F);

                row.Scores = scList;

                stdList.Add(row);
            }

            model.Subjects = subList;
            model.Rows = stdList;

            return model;
        }

        public GradeBroadsheetViewModel GetGradeBroadsheetModel(int ClassID, IEContext context)
        {
            var model = context.ClassResultList.Where(l => l.ClassID == ClassID).ProjectToFirst<GradeBroadsheetViewModel>();
            var rows = context.StudentResultList.Where(l => l.ClassID == ClassID).ProjectToList<BroadsheetRowModel>();

            if (model == null || rows == null || !rows.Any()) return null;

            var scores = model.Scores;
            var subjects = model.Subjects;

            byte cnt = 0;
            var subList = new List<SubjectResultModel>();
            foreach (var sub in subjects.OrderBy(s => s.Order).ThenBy(l => l.ResultName))
            {
                if (!scores.Any(l => l.SubjectID == sub.SubjectID && l.SummaryGradeID.HasValue)) continue;

                sub.CN = ++cnt;
                subList.Add(sub);
            }

            cnt = 0;
            var stdList = new List<BroadsheetRowModel>();
            foreach (var r in rows.OrderBy(l => l.StudentName))
            {
                var row = new BroadsheetRowModel { SN = ++cnt, Position = r.Position, StudentID = r.StudentID, StudentName = r.StudentName };

                var scList = new List<BroadsheetCellModel>();
                foreach (var sub in subList)
                {
                    var sc = scores.Where(l => l.StudentID == r.StudentID && l.SubjectID == sub.SubjectID).Select(l => new { l.GradeName, l.SummaryGrade }).FirstOrDefault();

                    if (sc != null)
                        scList.Add(new BroadsheetCellModel { CN = sub.CN, GradeStr = model.ShowSummaryGrade ? sc.SummaryGrade : sc.GradeName });
                    else
                        scList.Add(new BroadsheetCellModel { CN = sub.CN });
                }

                row.CountA = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.A);
                row.CountB = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.B);
                row.CountC = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.C);
                row.CountD = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.D);
                row.CountE = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.E);
                row.CountF = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.F);

                row.Scores = scList;

                stdList.Add(row);
            }

            model.Subjects = subList;
            model.Rows = stdList;

            return model;
        }

        [Route("Report/BroadsheetP/{ClassID:int}")]
        public ActionResult PrintBroadsheet(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var broadsheet = GetBroadsheetModel(ClassID, context);

            if(hVM.SchoolID != broadsheet.SchoolID) return RedirectToAction("Index", "Home");

            var model = new BroadsheetPageViewModel { Broadsheets = new List<BroadsheetViewModel> { broadsheet }, Name = broadsheet.Name };
            return View("PrintBroadsheets", model);
        }

        [Route("Report/BroadsheetGP/{ClassID:int}")]
        public ActionResult PrintGradeBroadsheet(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var broadsheet = GetGradeBroadsheetModel(ClassID, context);

            if (hVM.SchoolID != broadsheet.SchoolID) return RedirectToAction("Index", "Home");

            var model = new BroadsheetPageViewModel { GradeBroadsheets = new List<GradeBroadsheetViewModel> { broadsheet }, Name = broadsheet.Name };
            return View("PrintBroadsheets", model);
        }

        [Route("Report/BroadsheetFP/{ClassID:int}")]
        public ActionResult PrintFullBroadsheet(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var broadsheet = GetFullBroadsheetModel(ClassID, context);

            if (hVM.SchoolID != broadsheet.SchoolID) return RedirectToAction("Index", "Home");

            var model = new BroadsheetPageViewModel { FullBroadsheets = new List<FullBroadsheetViewModel> { broadsheet }, Name = broadsheet.Name };
            return View("PrintBroadsheets", model);
        }

        [Route("Report/BroadsheetTP/{TermID:int}")]
        public ActionResult PrintTermBroadsheets(int TermID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var cList = context.ClassList.Where(l => l.TermID == TermID && l.Term.SchoolID == hVM.SchoolID && context.ClassResultList.Any(r => r.ClassID == l.ClassID))
                               .OrderBy(l => l.Arm.ClassLevelID).ThenBy(l => l.Arm.Name).Select(l => l.ClassID).ToList();

            var bsList = new List<FullBroadsheetViewModel>();

            foreach(var id in cList)
                bsList.Add(GetFullBroadsheetModel(id, context));

            var model = new BroadsheetPageViewModel { FullBroadsheets = bsList, Name =  bsList.First().TermName };

            if (cList.Any())
                return View("PrintBroadsheets", new BroadsheetPageViewModel { FullBroadsheets = bsList, Name = bsList.First().TermName });
            else
                return View("PrintBroadsheets", new BroadsheetPageViewModel());
        }


        [Route("Report/ClassP/{ClassID:int}")]
        public ActionResult PrintClassReport(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.ClassResultList.Where(l => l.ClassID == ClassID).ProjectToFirst<ClassPerformanceModel>();

            if (hVM.SchoolID != model.SchoolID) return RedirectToAction("Index", "Home");

            if (model == null || model.Scores == null || !model.Scores.Any()) return null;

            var students = model.Students;
            var scores = model.Scores;
            var subjects = model.Subjects;

            byte cnt = 0;
            var subList = new List<SubjectPerformanceModel>();
            foreach (var sub in subjects.OrderBy(s => s.Order).ThenBy(l => l.Name))
            {
                var low = scores.Where(l => l.SubjectID == sub.SubjectID).Min(l => l.Total);

                if (low == null) continue;

                sub.CN = ++cnt;
                sub.LowestScore = low.Value;
                sub.AverageScore = scores.Where(l => l.SubjectID == sub.SubjectID).Average(l => l.Total).Value;

                var bs = scores.Where(l => l.SubjectID == sub.SubjectID && l.Total.HasValue).OrderByDescending(l => l.Total).Select(l => new { l.StudentID, l.Total }).First();
                sub.BestScore = bs.Total.Value;

                sub.BestStudentName = students.Where(l => l.StudentID == bs.StudentID).Select(l => l.DisplayName).First();
                sub.ResultCount = scores.Where(l => l.SubjectID == sub.SubjectID).Count(l => l.Total.HasValue);

                subList.Add(sub);
            }

            var catList = new List<SubjectCategoryModel>();
            foreach (var cat in Enum.GetValues(typeof(SubjectCategory)).Cast<SubjectCategory>())
            {
                var catid = (byte)cat;
                var cm = new SubjectCategoryModel { Name = Eval.GetDisplayName(cat) };
                cm.ResultCount = scores.Where(l => subjects.Any(s => s.SubjectID == l.SubjectID && s.CategoryID == catid) && l.Total.HasValue).Select(l => l.StudentID).Distinct().Count();
                cm.SubjectCount = subjects.Count(s => s.CategoryID == catid && scores.Any(l => l.SubjectID == s.SubjectID && l.Total.HasValue));

                if (cm.ResultCount == 0) continue;

                var avgList = scores.Where(l => subjects.Any(s => s.SubjectID == l.SubjectID && s.CategoryID == catid) && l.Total.HasValue)
                                    .GroupBy(l => l.StudentID).Select(l => new { StudentID = l.Key, Average = l.Average(s => s.Total) });

                cm.LowestAverage = avgList.Min(l => l.Average).Value;
                cm.MeanAverage = avgList.Average(l => l.Average).Value;

                var bs = avgList.OrderByDescending(l => l.Average).First();
                cm.BestAverage = bs.Average.Value;
                cm.BestStudentName = students.Where(l => l.StudentID == bs.StudentID).Select(l => l.DisplayName).First();

                catList.Add(cm);
            }

            model.Subjects = subList;
            model.Categories = catList;

            if (model.IsPromotionalClass)
            {
                var studentIDs = students.Select(l => l.StudentID).ToList();
                var trs = context.StudentResultList.Where(l => studentIDs.Contains(l.StudentID) && l.Class.Term.SchoolYear == model.SchoolYear).ProjectToList<StudentTermResultModel>();
                var symList = new List<StudentYearModel>();
                foreach (var std in students.OrderBy(l => l.DisplayName))
                {
                    var sym = new StudentYearModel { StudentID = std.StudentID, FirstName = std.FirstName, Surname = std.Surname };

                    sym.Term1Score = trs.Where(l => l.StudentID == std.StudentID && l.TermNumber == 1).Select(l => (decimal?)l.AverageScore).FirstOrDefault();
                    sym.Term2Score = trs.Where(l => l.StudentID == std.StudentID && l.TermNumber == 2).Select(l => (decimal?)l.AverageScore).FirstOrDefault();
                    sym.Term3Score = trs.Where(l => l.StudentID == std.StudentID && l.TermNumber == 3).Select(l => (decimal?)l.AverageScore).FirstOrDefault();

                    sym.CountA = trs.Where(l => l.StudentID == std.StudentID).Sum(l => l.GradeCountA);
                    sym.CountB = trs.Where(l => l.StudentID == std.StudentID).Sum(l => l.GradeCountB);
                    sym.CountC = trs.Where(l => l.StudentID == std.StudentID).Sum(l => l.GradeCountC);
                    sym.CountD = trs.Where(l => l.StudentID == std.StudentID).Sum(l => l.GradeCountD);
                    sym.CountE = trs.Where(l => l.StudentID == std.StudentID).Sum(l => l.GradeCountE);
                    sym.CountF = trs.Where(l => l.StudentID == std.StudentID).Sum(l => l.GradeCountF);

                    symList.Add(sym);
                }

                model.YearAverages = symList;
            }

            return View(model);
        }

        #endregion

        #region Student Reports

        public StudentReportModel GetStudentReportModel(StudentReportModel model, int ClassID, IEContext context)
        {
            var subjects = context.SubjectList.Where(l => l.ClassID == ClassID).ProjectToList<StudentSubjectModel>();
            List<int> templateIDs = subjects.Select(l => l.TemplateID).ToList();
            var scores = context.ScoreEntryList.Where(l => templateIDs.Contains(l.Subject.TemplateID) && l.Subject.SchoolYear == model.SchoolYear && 
                                                           l.Subject.TermNumber <= model.TermNumber && l.ExamScore.HasValue)
                                .ProjectToList<ScoreReportModel>();

            List<YearSubjectModel> subList = new List<YearSubjectModel>();
            List<ScoreReportModel> scoreList = new List<ScoreReportModel>();
            List<decimal> lowScores = new List<decimal>();
            List<decimal> avgScores = new List<decimal>();
            List<decimal> maxScores = new List<decimal>();

            foreach (var s in subjects.OrderBy(l => l.Order).ThenBy(l => l.ResultName).ToList())
            {
                var score = scores.Where(l => l.StudentID == model.StudentID && l.SubjectID == s.SubjectID).FirstOrDefault();

                if (score == null) continue;

                score.ResultName = s.ResultName;
                score.NoCA = s.NoCA;
                score.CAWeight = model.CAWeight;
                score.ExamWeight = model.ExamWeight;
                scoreList.Add(score);

                if(model.ShowYearResult)
                {
                    lowScores.Add(scores.Where(l => l.TemplateID == s.TemplateID).Min(l => l.Total).Value);
                    avgScores.Add(scores.Where(l => l.TemplateID == s.TemplateID).Average(l => l.Total).Value);
                    maxScores.Add(scores.Where(l => l.TemplateID == s.TemplateID).Max(l => l.Total).Value);
                }
                else
                {
                    lowScores.Add(scores.Where(l => l.SubjectID == s.SubjectID).Min(l => l.Total).Value);
                    avgScores.Add(scores.Where(l => l.SubjectID == s.SubjectID).Average(l => l.Total).Value);
                    maxScores.Add(scores.Where(l => l.SubjectID == s.SubjectID).Max(l => l.Total).Value);
                }

                var sub = new YearSubjectModel { Name = s.ResultName, Order = s.Order };
                sub.Term1Score = scores.Where(l => l.StudentID == model.StudentID && l.TemplateID == s.TemplateID && l.TermNumber == 1).Select(l => l.Total).FirstOrDefault();

                if(model.TermNumber >= 2)
                {
                    sub.Term2Score = scores.Where(l => l.StudentID == model.StudentID && l.TemplateID == s.TemplateID && l.TermNumber == 2).Select(l => l.Total).FirstOrDefault();

                    if (model.TermNumber > 2)
                        sub.Term3Score = scores.Where(l => l.StudentID == model.StudentID && l.TemplateID == s.TemplateID && l.TermNumber == 3).Select(l => l.Total).FirstOrDefault();
                }

                sub.YearAverage = scores.Where(l => l.StudentID == model.StudentID && l.TemplateID == s.TemplateID).Average(l => l.Total).Value;
                sub.ClassAverage = scores.Where(l => l.TemplateID == s.TemplateID).Average(l => l.Total).Value;

                subList.Add(sub);
            }

            model.Scores = scoreList;

            if(model.ShowCategoryAnalysis)
            {
                var catList = new List<SubjectCategoryModel>();
                foreach (var cat in Enum.GetValues(typeof(SubjectCategory)).Cast<SubjectCategory>())
                {
                    var catid = (byte)cat;
                    var cm = new SubjectCategoryModel { Name = Eval.GetDisplayName(cat) };
                    var catSubList = scores.Where(l => l.StudentID == model.StudentID && subjects.Any(s => s.SubjectID == l.SubjectID && s.CategoryID == catid) && l.Total.HasValue).Select(s => s.Total);

                    if (!catSubList.Any()) continue;

                    cm.LowestAverage = catSubList.Min().Value;
                    cm.MeanAverage = catSubList.Average().Value;
                    cm.BestAverage = catSubList.Max().Value;

                    catList.Add(cm);
                }
                model.Categories = catList;
            }

            var subIDs = subjects.Select(l => l.SubjectID).ToList();
            model.YearSubjects = subList;

            if (model.ShowYearResult)
            {
                model.StudentCount = scores.Select(l => l.StudentID).Distinct().Count();

                model.LowestScore = subList.Min(l => l.YearAverage);
                model.AverageScore = subList.Average(l => l.YearAverage);
                model.HighestScore = subList.Max(l => l.YearAverage);
                model.ScoresJson = General.Json(subList.Select(l => l.YearAverage));

                byte pos = 1;
                foreach(var id in scores.GroupBy(l => l.StudentID).OrderByDescending(l => l.Average(s => s.Total)).Select(l => l.Key))
                {
                    if (id == model.StudentID)
                    {
                        model.Position = pos;
                        break;
                    }

                    pos++;
                }
            }
            else
            {
                model.StudentCount = scores.Where(l => subIDs.Contains(l.SubjectID)).Select(l => l.StudentID).Distinct().Count();
                model.ScoresJson = General.Json(scoreList.Select(l => l.Total));
            }

            model.LowScoresJson = General.Json(lowScores);
            model.AvgScoresJson = General.Json(avgScores);
            model.MaxScoresJson = General.Json(maxScores);

            if(model.HasSkills)
            {
                var skills = context.StudentSkillsList.Where(l => l.StudentID == model.StudentID && l.ClassID == ClassID).ProjectToFirstOrDefault<StudentSkillsReportModel>();

                if (skills != null)
                {
                    if (skills.SkillScore1.HasValue)
                    {
                        var gr = model.SkillGrades.Where(l => l.GradeNumber == skills.SkillScore1).FirstOrDefault();
                        if (gr != null)
                        {
                            model.SkillGrade1 = gr.Name;
                            model.SkillScore1 = gr.NumberScore;
                        }
                    }
                    if (skills.SkillScore2.HasValue)
                    {
                        var gr = model.SkillGrades.Where(l => l.GradeNumber == skills.SkillScore2).FirstOrDefault();
                        if (gr != null)
                        {
                            model.SkillGrade2 = gr.Name;
                            model.SkillScore2 = gr.NumberScore;
                        }
                    }
                    if (skills.SkillScore3.HasValue)
                    {
                        var gr = model.SkillGrades.Where(l => l.GradeNumber == skills.SkillScore3).FirstOrDefault();
                        if (gr != null)
                        {
                            model.SkillGrade3 = gr.Name;
                            model.SkillScore3 = gr.NumberScore;
                        }
                    }
                    if (skills.SkillScore4.HasValue)
                    {
                        var gr = model.SkillGrades.Where(l => l.GradeNumber == skills.SkillScore4).FirstOrDefault();
                        if (gr != null)
                        {
                            model.SkillGrade4 = gr.Name;
                            model.SkillScore4 = gr.NumberScore;
                        }
                    }
                    if (skills.SkillScore5.HasValue)
                    {
                        var gr = model.SkillGrades.Where(l => l.GradeNumber == skills.SkillScore5).FirstOrDefault();
                        if (gr != null)
                        {
                            model.SkillGrade5 = gr.Name;
                            model.SkillScore5 = gr.NumberScore;
                        }
                    }
                    if (skills.SkillScore6.HasValue)
                    {
                        var gr = model.SkillGrades.Where(l => l.GradeNumber == skills.SkillScore6).FirstOrDefault();
                        if (gr != null)
                        {
                            model.SkillGrade6 = gr.Name;
                            model.SkillScore6 = gr.NumberScore;
                        }
                    }
                }
                else
                    model.HasSkills = false;
            }

            if (!string.IsNullOrEmpty(model.GuidString))
                model.PhotoSrc = "data:image/png;base64," + Convert.ToBase64String(new Student { GuidString = model.GuidString }.DownloadFile());

            if (!string.IsNullOrEmpty(model.LogoGuidString))
                model.LogoSrc = "data:image/png;base64," + Convert.ToBase64String(new Term { GuidString = model.LogoGuidString }.DownloadFile());

            return model;
        }

        [Route("Report/StudentAP/{ClassID:int}")]
        public ActionResult PrintResults(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var list = context.StudentResultList.Where(l => l.ClassID == ClassID).ProjectToList<StudentReportModel>();

            if (!list.Any()) return View(new StudentReportPageViewModel { Reports = new List<StudentReportModel>() });

            if (hVM.SchoolID != list.First().SchoolID) return RedirectToAction("Index", "Home");

            var name = list.First().ClassName;

            var reports = new List<StudentReportModel>();
            foreach(var rep in list.OrderBy(l => l.DisplayName))
                reports.Add(GetStudentReportModel(rep, ClassID, context));

            return View(new StudentReportPageViewModel { Reports = reports, Name = name });
        }

        [Route("Report/StudentP/{StudentID:int}")]
        public ActionResult PrintStudentResult(int StudentID, int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var res = context.StudentResultList.Where(l => l.StudentID == StudentID && l.ClassID == ClassID).ProjectToFirstOrDefault<StudentReportModel>();

            if (res == null) return View(new StudentReportPageViewModel { Reports = new List<StudentReportModel>() });

            if (hVM.SchoolID != res.SchoolID) return RedirectToAction("Index", "Home");

            var name = res.TermName.Replace("/","_") + " " + res.ClassName + " " + res.DisplayName;

            return View("PrintResults", new StudentReportPageViewModel { Reports = new List<StudentReportModel> { GetStudentReportModel(res, ClassID, context) }, Name = name });
        }


        [Route("Report/Students")]
        public ActionResult StudentReports()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new StudentsPageViewModel();

            byte typeID = 0;

            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID)
                                 .Select(l => new { l.TermID, l.School.Name, l.School.TypeID }).FirstOrDefault();

                if (def == null || def.TermID == null) return RedirectToAction("School");

                model.TermFilter = def.TermID.Value;
                model.SchoolName = def.Name;
                typeID = def.TypeID;
            }
            else
            {
                var tm = context.TermList.Where(l => l.SchoolID == hVM.SchoolID && context.ClassList.Any(c => c.TermID == l.TermID))
                                .OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).Select(l => new { l.TermID, l.School.Name, l.School.TypeID }).FirstOrDefault();

                if (tm == null) return RedirectToAction("Index", "Home");

                model.TermFilter = tm.TermID;
                model.SchoolName = tm.Name;
                typeID = tm.TypeID;
            }

            model.ClassList = GetClassList(context, model.TermFilter);
            model.TermList = GetTermList(context, hVM.SchoolID);
            model.LevelList = GetLevelList(typeID);

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult GetStudentReportList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, StudentFilterViewModel filterModel)
        {
            var query = new IEContext().StudentResultList.AsQueryable();
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Student.FirstName.Contains(search) || p.Student.Surname.Contains(search));
            }

            if (filterModel.ClassID.HasValue)
                query = query.Where(l => l.ClassID == filterModel.ClassID.Value);

            if (filterModel.TermID.HasValue)
                query = query.Where(l => l.Class.TermID == filterModel.TermID.Value);

            if (filterModel.LevelID.HasValue)
                query = query.Where(l => l.Class.Arm.ClassLevelID == filterModel.LevelID.Value);

            if (filterModel.SexID.HasValue)
                query = query.Where(l => l.Student.IsMale == (filterModel.SexID.Value == 1));

            int filteredCount = query.Count();

            string sortStr = "";
            foreach (var col in requestModel.Columns.GetSortedColumns())
            {
                sortStr += sortStr != string.Empty ? "," : "";
                if (col.Data == "Position" || col.Data == "SubjectCount")
                {
                    sortStr = sortStr + col.Data + (col.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    continue;
                }


                switch (col.Data)
                {
                    case "ImpFull": sortStr += "Improvement"; break;

                    case "AvgStr": sortStr += "AverageScore"; break;

                    case "LowStr": sortStr += "LowestScore"; break;

                    case "HighStr": sortStr += "HighestScore"; break;

                    case "DisplayName": sortStr += "Student.Surname"; break;

                    case "ClassName": sortStr += "Class.Arm.Name"; break;

                    case "Level": sortStr += "Class.ClassLevelID"; break;

                    case "TermName": sortStr += "Class.Term.Name"; break;

                    case "BestSubjectName": sortStr += "BestSubject.Name"; break;
                }

                sortStr += SortDirStr(col.SortDirection);
            }

            var defSort = "Class.Term.Name desc, Class.Arm.Name asc, Student.Surname asc, Student.FirstName asc";
            var model = query.OrderBy(sortStr == string.Empty ? defSort : sortStr + ", " + defSort)
                             .Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<StudentResultViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Subject Reports

        [Route("Report/Subjects")]
        public ActionResult SubjectReports()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new SubjectReportPageModel();

            byte typeID = 0;

            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID)
                                 .Select(l => new { l.TermID, l.School.Name, l.School.TypeID }).FirstOrDefault();

                if (def == null || def.TermID == null) return RedirectToAction("School");

                model.TermFilter = def.TermID.Value;
                model.SchoolName = def.Name;
                typeID = def.TypeID;
            }
            else
            {
                var tm = context.TermList.Where(l => l.SchoolID == hVM.SchoolID && context.ClassList.Any(c => c.TermID == l.TermID))
                                .OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).Select(l => new { l.TermID, l.School.Name, l.School.TypeID }).FirstOrDefault();

                if (tm == null) return RedirectToAction("Index", "Home");

                model.TermFilter = tm.TermID;
                model.SchoolName = tm.Name;
                typeID = tm.TypeID;
            }

            model.ClassList = GetClassList(context, model.TermFilter);
            model.TermList = GetTermList(context, hVM.SchoolID);
            model.LevelList = GetLevelList(typeID);

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public ActionResult GetSubjectReportList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, SubjectReportFilterModel filterModel)
        {
            var query = new IEContext().SubjectList.Where(l => l.Class.TermID == filterModel.TermID);
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Template.Name.Contains(search) || p.ResultName.Contains(search));
            }

            if (filterModel.TermID.HasValue)
                query = query.Where(l => l.Class.TermID == filterModel.TermID.Value);

            if (filterModel.ClassID.HasValue)
                query = query.Where(l => l.ClassID == filterModel.ClassID.Value);

            if (filterModel.LevelID.HasValue)
                query = query.Where(l => l.Class.Arm.ClassLevelID == filterModel.LevelID.Value);

            if (filterModel.CategoryID.HasValue)
                query = query.Where(l => l.Template.CategoryID == filterModel.CategoryID.Value);

            int filteredCount = query.Count();

            string sortStr = "";
            foreach (var col in requestModel.Columns.GetSortedColumns())
            {
                sortStr += sortStr != string.Empty ? "," : "";

                switch (col.Data)
                {
                    case "TimeVerifiedN": sortStr += "TimeVerified"; break;

                    case "TermStr": sortStr += "Class.TermID"; break;

                    case "ClassName": sortStr += "Class.Arm.Name"; break;

                    case "Name": sortStr += "Template.Name"; break;

                    case "CategoryName": sortStr += "Template.CategoryID"; break;

                    case "ResultName": sortStr += "ResultName"; break;
                }

                sortStr += SortDirStr(col.SortDirection);
            }

            var defSort = "Class.Arm.Name asc, Template.Name asc";
            var model = query.OrderBy(sortStr == string.Empty ? defSort : sortStr + ", " + defSort)
                             .Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<SubjectReportViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }

        public ActionResult _ClassFilter(int TermID)
        {
            return PartialView(new SubjectReportPageModel { ClassList = GetClassList(new IEContext(), TermID) });
        }

        [Route("Report/SubjectP/{SubjectID:int}")]
        public ActionResult PrintSubjectReport(int SubjectID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.SubjectList.Where(l => l.SubjectID == SubjectID).ProjectToFirst<SubjectReportModel>();

            if (hVM.SchoolID != model.SchoolID) return RedirectToAction("Index", "Home");

            if (model == null || model.Scores == null || !model.Scores.Any()) return null;

            foreach(var s in model.Scores)
            {
                s.NoCA = model.NoCA;
                s.CAWeight = model.CAWeight;
                s.ExamWeight = model.ExamWeight;
            }

            return View("PrintSubjectReports", new SubjectResultPageViewModel { Reports = new List<SubjectReportModel> { model }, Name = "Subject Report - " + model.ClassName + " - " + model.Name });
        }

        [Route("Report/SubjectsP/{ClassID:int}")]
        public ActionResult PrintSubjectReports(int ClassID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.SubjectList.Where(l => l.ClassID == ClassID && l.Scores.Any(s => s.GradeID.HasValue)).ProjectToList<SubjectReportModel>();

            if (model == null || !model.Any()) return null;

            var sID = model.First().SchoolID;

            var first = model.First();

            if (hVM.SchoolID != first.SchoolID) return RedirectToAction("Index", "Home");

            foreach(var rep in model)
            {
                foreach (var s in rep.Scores)
                {
                    s.NoCA = rep.NoCA;
                    s.CAWeight = rep.CAWeight;
                    s.ExamWeight = rep.ExamWeight;
                }
            }

            return View("PrintSubjectReports", new SubjectResultPageViewModel { Reports = model, Name = "Subject Results - " + first.ClassName + " - " + first.TermName });
        }

        #endregion

        #region Other Exams
        public ActionResult OtherExams()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new OtherExamsPageViewModel();

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

            model.Exams = context.OtherExamList.Where(l => l.Class.TermID == model.TermID).ProjectToList<OtherExamViewModel>();
            model.HeaderViewModel = hVM;

            return View(model);
        }

        [Route("Report/_StartOtherExam/{TermID:int}/{SchoolID:int}/")]
        public ActionResult _StartOtherExam(int TermID, int SchoolID)
        {
            var context = new IEContext();
            return PartialView(new StartOtherExam { ClassList = GetClassList(context, TermID), ExamList = GetExamTypeList(context, SchoolID) });
        }

        public ActionResult _AddOtherExam(StartOtherExam model)
        {
            var context = new IEContext();
            var nModel = context.ClassList.Where(l => l.ClassID == model.ClassID).ProjectToFirst<OtherExamCreateViewModel>();
            nModel.TypeID = model.TypeID.Value;
            nModel.ExamName = context.OtherExamTypeList.Where(l => l.TypeID == model.TypeID).Select(l => l.Name).First();
                       
            return PartialView(nModel);
        }

        public ActionResult CreateOtherExamReports(int ClassID, int TypeID)
        {
            var context = new IEContext();
            if (context.OtherExamList.Any(l => l.ClassID == ClassID && l.TypeID == TypeID))
                return DefaultErrorAlert("This exam already exists!");

            var exam = new OtherExam { ClassID = ClassID, TypeID = TypeID };

            var error = GenerateOtherExamReports(exam, context);

            if (error == "")
                return Json("", JsonRequestBehavior.AllowGet);
            else if (error == "Error")
                return DefaultErrorAlert();
            else
                return DefaultErrorAlert(error);
        }

        public ActionResult _ReanalyzeOtherExam(int ExamID)
        {
            return PartialView(new IEContext().OtherExamList.Where(l => l.OtherExamID == ExamID).ProjectToFirst<OtherExamAnalysisModel>());
        }

        public ActionResult RegenerateOtherExamReports(int ClassID, int ExamID, int TypeID)
        {
            var exam = new OtherExam { ClassID = ClassID, OtherExamID = ExamID, TypeID = TypeID };

            var error = GenerateOtherExamReports(exam, new IEContext());

            if (error == "")
                return Json("", JsonRequestBehavior.AllowGet);
            else if (error == "Error")
                return DefaultErrorAlert();
            else
                return DefaultErrorAlert(error);
        }

        public string GenerateOtherExamReports(OtherExam exam, IEContext context)
        {
            var classID = exam.ClassID.Value;

            //get all score entries...
            var scores = context.ScoreEntryList.Where(l => l.Subject.ClassID == classID && l.ExamScore.HasValue).ProjectToList<ScoreEntryDataModel>();

            var stIDs = scores.Select(l => l.StudentID).Distinct();
            var sbIDs = scores.Select(l => l.SubjectID).Distinct();

            decimal maxAvg = 0;
            byte subCnt = 0;
            var bestSubjectID = 0;
            foreach (var id in sbIDs)
            {
                var avg = scores.Where(l => l.SubjectID == id).Select(l => l.Total).Average();
                if (avg == null) continue;

                if (avg > maxAvg)
                {
                    maxAvg = avg.Value;
                    bestSubjectID = id;
                }

                subCnt++;
            }

            if (subCnt == 0)
                return "There are no subject scores for this class!";

            foreach (var sc in scores)
            {
                var osc = new OtherExamScore();
                Mapper.Map(sc, osc);

                if (exam.OtherExamID == 0)
                    osc.Exam = exam;
                else
                    osc.OtherExamID = exam.OtherExamID;

                context.OtherExamScoreList.AddOrUpdate(osc);
            }

            context.OtherExamScoreList.Where(l => l.OtherExamID == exam.OtherExamID &&
                                                  !context.ScoreEntryList.Any(e => e.SubjectID == l.SubjectID && e.StudentID == l.StudentID && e.ExamScore.HasValue)).Delete();

            maxAvg = 0;
            decimal lowAvg = 100;
            byte resCnt = 0;
            var bestStudentID = 0;

            var clres = context.ClassList.Where(l => l.ClassID == classID).ProjectToFirst<ClassAnalysisModel>();

            var resList = new List<OtherExamResult>();
            var stList = context.StudentList.Where(l => stIDs.Contains(l.StudentID))
                                .Select(l => new { l.StudentID, l.GuidString, l.FirstName, l.IsMale, l.TeacherComment }).ToList();

            foreach (var id in stIDs)
            {
                var low = scores.Where(l => l.StudentID == id).Min(l => l.Total);
                if (low == null) continue;

                var avg = scores.Where(l => l.StudentID == id).Average(l => l.Total);
                var max = scores.Where(l => l.StudentID == id).Max(l => l.Total);

                if (avg < lowAvg) lowAvg = avg.Value;

                if (avg > maxAvg)
                {
                    maxAvg = avg.Value;
                    bestStudentID = id;
                }

                var res = new OtherExamResult { StudentID = id, AverageScore = avg.Value, HighestScore = max.Value, LowestScore = low.Value };
                if (exam.OtherExamID == 0) res.Exam = exam;
                else
                    res.OtherExamID = exam.OtherExamID;

                res.BestSubjectID = scores.Where(l => l.StudentID == id).OrderByDescending(l => l.Total).Select(l => l.SubjectID).First();
                res.SubjectCount = (byte)scores.Count(l => l.StudentID == id && l.Total.HasValue);

                var st = stList.Where(l => l.StudentID == id).First();
                res.GuidString = st.GuidString;

                var perf = avg.Value;

                var comment = clres.PerformanceComments
                                   .Where(l => l.LowerBound <= perf && (l.UpperBound + 1) > perf)
                                   .OrderByDescending(l => l.LowerBound)
                                   .Select(l => l.Comment).FirstOrDefault();

                if(comment != null)
                {
                    comment = comment.Replace("{:NAME}", st.FirstName);
                    comment = comment.Replace("{:SEX1}", st.IsMale ? "he" : "she");
                    comment = comment.Replace("{:SEX2}", st.IsMale ? "him" : "her");

                    if (clres.UseImprovementComments)
                    {
                        var badCnt = scores.Count(l => l.StudentID == id && l.Total < clres.RedLine);
                        var impCom = clres.ImprovementComments
                                          .Where(l => l.MinFailCount <= badCnt && l.MaxFailCount >= badCnt)
                                          .OrderByDescending(l => l.MinFailCount)
                                          .Select(l => l.Comment).FirstOrDefault();

                        comment += ". " + (impCom != null ? impCom : "");
                    }

                    res.PerformanceComment = General.CapitalizeFirst(comment);
                    res.TeacherComment = st.TeacherComment;
                }

                resList.Add(res);

                resCnt++;
            }

            byte pos = 1;
            byte rpos = 1;
            decimal tavg = 0;
            foreach (var r in resList.OrderByDescending(l => l.AverageScore))
            {
                if (tavg != r.AverageScore)
                {
                    tavg = r.AverageScore;
                    pos = rpos;
                }

                r.Position = pos;

                context.OtherExamResultList.AddOrUpdate(r);
                rpos++;
            }

            exam.SubjectCount = subCnt;
            exam.ResultCount = resCnt;
            exam.BestStudentID = bestStudentID;
            exam.BestSubjectID = bestSubjectID;
            exam.LowestAverage = lowAvg;
            exam.MeanAverage = scores.Average(l => l.Total).Value;
            exam.BestAverage = maxAvg;

            context.OtherExamList.AddOrUpdate(exam);

            var term = new Term { TermID = clres.TermID };
            context.TermList.Attach(term);

            term.GuidString = clres.LogoGuidString;

            try
            {
                context.Configuration.ValidateOnSaveEnabled = false;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return "Error";
            }

            return "";
        }

        [Route("Report/OEBroadsheetP/{ExamID:int}")]
        public ActionResult PrintOtherExamBroadsheet(int ExamID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var bsm = context.OtherExamList.Where(l => l.OtherExamID == ExamID).ProjectToFirst<BroadsheetViewModel>();

            if (bsm == null) return null;

            if (hVM.SchoolID != bsm.SchoolID) return RedirectToAction("Index", "Home");

            var rows = context.OtherExamResultList.Where(l => l.OtherExamID == ExamID).ProjectToList<BroadsheetRowModel>();

            if (rows == null || !rows.Any()) return null;

            var scores = bsm.Scores;
            var subjects = bsm.Subjects;

            byte cnt = 0;
            var subList = new List<SubjectResultModel>();
            foreach (var sub in subjects.OrderBy(s => s.Order).ThenBy(l => l.ResultName))
            {
                var avg = scores.Where(l => l.SubjectID == sub.SubjectID).Average(l => l.Total);

                if (avg == null) continue;

                sub.CN = ++cnt;
                sub.AverageScore = avg.Value;
                subList.Add(sub);
            }

            cnt = 0;
            var stdList = new List<BroadsheetRowModel>();
            foreach (var r in rows.OrderBy(l => l.StudentName))
            {
                var row = new BroadsheetRowModel { SN = ++cnt, AverageScore = r.AverageScore, Position = r.Position, StudentID = r.StudentID, StudentName = r.StudentName };

                var scList = new List<BroadsheetCellModel>();
                foreach (var sub in subList)
                    scList.Add(new BroadsheetCellModel { CN = sub.CN, Total = scores.Where(l => l.StudentID == r.StudentID && l.SubjectID == sub.SubjectID).Select(l => l.Total).FirstOrDefault() });

                row.Scores = scList;

                stdList.Add(row);
            }

            bsm.Subjects = subList;
            bsm.Rows = stdList;

            var model = new BroadsheetPageViewModel { Broadsheets = new List<BroadsheetViewModel> { bsm }, Name = bsm.Name };
            return View("PrintBroadsheets", model);
        }

        [Route("Report/OEBroadsheetGP/{ExamID:int}")]
        public ActionResult PrintOtherExamGradeBroadsheet(int ExamID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var bsm = context.OtherExamList.Where(l => l.OtherExamID == ExamID).ProjectToFirst<GradeBroadsheetViewModel>();

            if (bsm == null) return null;

            if (hVM.SchoolID != bsm.SchoolID) return RedirectToAction("Index", "Home");

            var rows = context.OtherExamResultList.Where(l => l.OtherExamID == ExamID).ProjectToList<BroadsheetRowModel>();

            if (rows == null || !rows.Any()) return null;

            var scores = bsm.Scores;
            var subjects = bsm.Subjects;

            byte cnt = 0;
            var subList = new List<SubjectResultModel>();
            foreach (var sub in subjects.OrderBy(s => s.Order).ThenBy(l => l.ResultName))
            {
                if (!scores.Any(l => l.SubjectID == sub.SubjectID && l.SummaryGradeID.HasValue)) continue;

                sub.CN = ++cnt;
                subList.Add(sub);
            }

            cnt = 0;
            var stdList = new List<BroadsheetRowModel>();
            foreach (var r in rows.OrderBy(l => l.StudentName))
            {
                var row = new BroadsheetRowModel { SN = ++cnt, Position = r.Position, StudentID = r.StudentID, StudentName = r.StudentName };

                var scList = new List<BroadsheetCellModel>();
                foreach (var sub in subList)
                {
                    var sc = scores.Where(l => l.StudentID == r.StudentID && l.SubjectID == sub.SubjectID).Select(l => new { l.GradeName, l.SummaryGrade }).FirstOrDefault();

                    if (sc != null)
                        scList.Add(new BroadsheetCellModel { CN = sub.CN, GradeStr = bsm.ShowSummaryGrade ? sc.SummaryGrade : sc.GradeName });
                    else
                        scList.Add(new BroadsheetCellModel { CN = sub.CN });
                }

                row.CountA = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.A);
                row.CountB = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.B);
                row.CountC = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.C);
                row.CountD = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.D);
                row.CountE = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.E);
                row.CountF = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.F);

                row.Scores = scList;

                stdList.Add(row);
            }

            bsm.Subjects = subList;
            bsm.Rows = stdList;

            var model = new BroadsheetPageViewModel { GradeBroadsheets = new List<GradeBroadsheetViewModel> { bsm }, Name = bsm.Name };
            return View("PrintBroadsheets", model);
        }

        [Route("Report/OEBroadsheetFP/{ExamID:int}")]
        public ActionResult PrintOtherExamFullBroadsheet(int ExamID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var bsm = context.OtherExamList.Where(l => l.OtherExamID == ExamID).ProjectToFirst<FullBroadsheetViewModel>();

            if (bsm == null) return null;

            if (hVM.SchoolID != bsm.SchoolID) return RedirectToAction("Index", "Home");

            var rows = context.OtherExamResultList.Where(l => l.OtherExamID == ExamID).ProjectToList<BroadsheetRowModel>();

            if (rows == null || !rows.Any()) return null;

            var scores = bsm.Scores;
            var subjects = bsm.Subjects;

            byte cnt = 0;
            var subList = new List<SubjectResultModel>();
            foreach (var sub in subjects.OrderBy(s => s.Order).ThenBy(l => l.ResultName))
            {
                var avg = scores.Where(l => l.SubjectID == sub.SubjectID).Average(l => l.Total);

                if (avg == null) continue;

                sub.CN = ++cnt;
                sub.AverageScore = avg.Value;
                subList.Add(sub);
            }

            cnt = 0;
            var stdList = new List<BroadsheetRowModel>();
            foreach (var r in rows.OrderBy(l => l.StudentName))
            {
                var row = new BroadsheetRowModel { SN = ++cnt, AverageScore = r.AverageScore, Position = r.Position, StudentID = r.StudentID, StudentName = r.StudentName };

                var scList = new List<BroadsheetCellModel>();
                foreach (var sub in subList)
                {
                    var sc = scores.Where(l => l.StudentID == r.StudentID && l.SubjectID == sub.SubjectID).Select(l => new { l.Total, l.GradeName, l.SummaryGrade }).FirstOrDefault();

                    if (sc != null)
                        scList.Add(new BroadsheetCellModel { CN = sub.CN, Total = sc.Total, GradeStr = bsm.ShowSummaryGrade ? sc.SummaryGrade : sc.GradeName });
                    else
                        scList.Add(new BroadsheetCellModel { CN = sub.CN });
                }

                row.CountA = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.A);
                row.CountB = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.B);
                row.CountC = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.C);
                row.CountD = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.D);
                row.CountE = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.E);
                row.CountF = scores.Where(l => l.StudentID == r.StudentID).Count(l => l.SummaryGradeID == (byte)SummaryGrade.F);

                row.Scores = scList;

                stdList.Add(row);
            }

            bsm.Subjects = subList;
            bsm.Rows = stdList;

            var model = new BroadsheetPageViewModel { FullBroadsheets = new List<FullBroadsheetViewModel> { bsm }, Name = bsm.Name };
            return View("PrintBroadsheets", model);
        }

        public OtherExamReportModel GetOtherExamReportModel(OtherExamReportModel model, int ExamID, IEContext context)
        {
            var subjects = context.SubjectList.Where(l => l.ClassID == model.ClassID).Select(l => new { l.SubjectID, l.ResultName, l.Template.Order }).ToList();
            var scores = context.OtherExamScoreList.Where(l => l.OtherExamID == ExamID).ProjectToList<ExamScoreReportModel>();

            List<ExamScoreReportModel> scoreList = new List<ExamScoreReportModel>();
            List<decimal> lowScores = new List<decimal>();
            List<decimal> avgScores = new List<decimal>();
            List<decimal> maxScores = new List<decimal>();

            foreach (var s in subjects.OrderBy(l => l.Order).ThenBy(l => l.ResultName).ToList())
            {
                var score = scores.Where(l => l.StudentID == model.StudentID && l.SubjectID == s.SubjectID).FirstOrDefault();

                if (score == null) continue;

                var avg = scores.Where(l => l.SubjectID == s.SubjectID).Average(l => l.Total).Value;
                score.ClassAverage = avg;
                score.ResultName = s.ResultName;

                scoreList.Add(score);

                lowScores.Add(scores.Where(l => l.SubjectID == s.SubjectID).Min(l => l.Total).Value);
                avgScores.Add(avg);
                maxScores.Add(scores.Where(l => l.SubjectID == s.SubjectID).Max(l => l.Total).Value);
            }

            model.Scores = scoreList;

            model.StudentCount = scores.Select(l => l.StudentID).Distinct().Count();
            model.ScoresJson = General.Json(scoreList.Select(l => l.Total));

            model.LowScoresJson = General.Json(lowScores);
            model.AvgScoresJson = General.Json(avgScores);
            model.MaxScoresJson = General.Json(maxScores);

            var guid = model.GuidString;

            if (model.ClassID != model.StudentClassID)
                guid = context.StudentResultList.Where(l => l.ClassID == model.ClassID && l.StudentID == model.StudentID).Select(l => l.GuidString).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(guid)) guid = model.GuidString;

            if (!string.IsNullOrEmpty(guid))
                model.PhotoSrc = "data:image/png;base64," + Convert.ToBase64String(new Student { GuidString = guid }.DownloadFile());

            if (!string.IsNullOrEmpty(model.LogoGuidString))
                model.LogoSrc = "data:image/png;base64," + Convert.ToBase64String(new Term { GuidString = model.LogoGuidString }.DownloadFile());

            return model;
        }

        [Route("Report/OEStudentAP/{ExamID:int}")]
        public ActionResult PrintOtherExamResults(int ExamID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var list = context.OtherExamResultList.Where(l => l.OtherExamID == ExamID).ProjectToList<OtherExamReportModel>();

            if (!list.Any()) return View(new OtherExamReportPageViewModel { Reports = new List<OtherExamReportModel>() });

            if (hVM.SchoolID != list.First().SchoolID) return RedirectToAction("Index", "Home");

            var type = context.OtherExamList.Where(l => l.OtherExamID == ExamID).Select(l => l.Type.Name).First();

            var name = type + " " + list.First().ClassName;

            var reports = new List<OtherExamReportModel>();
            foreach (var rep in list.OrderBy(l => l.DisplayName))
                reports.Add(GetOtherExamReportModel(rep, ExamID, context));

            return View(new OtherExamReportPageViewModel { Reports = reports, Name = name });
        }

        [Route("Report/OEStudentAPZ/{ExamID:int}")]
        public ActionResult PrintOtherExamResultsZip(int ExamID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var list = context.OtherExamResultList.Where(l => l.OtherExamID == ExamID).ProjectToList<OtherExamReportModel>();

            if (!list.Any()) return View(new OtherExamReportPageViewModel { Reports = new List<OtherExamReportModel>() });

            if (hVM.SchoolID != list.First().SchoolID) return RedirectToAction("Index", "Home");

            var type = context.OtherExamList.Where(l => l.OtherExamID == ExamID).Select(l => l.Type.Name).First();

            var name = type + " " + list.First().ClassName;

            var validPrefix = General.MakeValidFileName(name);

            using (var ams = new System.IO.MemoryStream())
            {
                using (var archive = new ZipArchive(ams, ZipArchiveMode.Create, true))
                {
                    foreach (var rep in list.OrderBy(l => l.DisplayName))
                    {
                        
                        var mod = GetOtherExamReportModel(rep, ExamID, context);
                        var pgMod = new OtherExamReportPageViewModel { Reports = new List<OtherExamReportModel>() { mod }, Name = name };
                        string HtmlContent = RenderRazorViewToString(this, "PrintOtherExamResults", pgMod);

                        var fileName = validPrefix + " - " + General.MakeValidFileName(mod.DisplayName) + ".pdf";

                        var zipEntry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                        using (var zipStream = zipEntry.Open())
                        {
                            PdfGenerateConfig cfg = new PdfGenerateConfig
                            {
                                MarginBottom = 5,
                                MarginLeft = 5,
                                MarginRight = 5,
                                MarginTop = 18,
                                PageOrientation = PageOrientation.Portrait,
                                PageSize = PageSize.A4
                            };

                            PdfDocument pdf = PdfGenerator.GeneratePdf(HtmlContent, cfg);
                            pdf.Save(zipStream);
                        }
                    }
                }
                return File(ams.ToArray(), "application/zip", validPrefix + ".zip");
            }
        }

        [Route("Report/OEStudentP/{StudentID:int}")]
        public ActionResult PrintOtherExamResult(int StudentID, int ExamID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var res = context.OtherExamResultList.Where(l => l.StudentID == StudentID && l.OtherExamID == ExamID).ProjectToFirstOrDefault<OtherExamReportModel>();

            if (res == null) return View(new OtherExamReportPageViewModel { Reports = new List<OtherExamReportModel>() });

            if (hVM.SchoolID != res.SchoolID) return RedirectToAction("Index", "Home");

            var type = context.OtherExamList.Where(l => l.OtherExamID == ExamID).Select(l => l.Type.Name).First();

            var name = type + " " + res.TermName.Replace("/", "_") + " " + res.ClassName + " " + res.DisplayName;

            return View("PrintOtherExamResults", new OtherExamReportPageViewModel { Reports = new List<OtherExamReportModel> { GetOtherExamReportModel(res, ExamID, context) }, Name = name });
        }


        [Route("Report/ExamResults")]
        public ActionResult OtherExamResults()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new OtherExamResultPageViewModel();

            byte typeID = 0;

            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID)
                                 .Select(l => new { l.TermID, l.School.Name, l.School.TypeID }).FirstOrDefault();

                if (def == null || def.TermID == null) return RedirectToAction("School");

                model.SchoolName = def.Name;
                typeID = def.TypeID;
                model.TermID = def.TermID.Value;
            }
            else
            {
                var tm = context.TermList.Where(l => l.SchoolID == hVM.SchoolID && context.ClassList.Any(c => c.TermID == l.TermID))
                                .OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).Select(l => new { l.TermID, l.School.Name, l.School.TypeID }).FirstOrDefault();

                if (tm == null) return RedirectToAction("Index", "Home");

                model.SchoolName = tm.Name;
                typeID = tm.TypeID;
                model.TermID = tm.TermID;
            }

            model.ClassList = GetClassList(context, model.TermID);
            model.TypeList = GetOtherExamTypeList(context, hVM.SchoolID);
            model.LevelList = GetLevelList(typeID);

            model.HeaderViewModel = hVM;

            return View(model);
        }

        public List<SelectListItem> GetOtherExamTypeList(IEContext context, int SchoolID)
        {
            var list = new List<SelectListItem>();

            var gpList = context.OtherExamTypeList.Where(l => l.SchoolID == SchoolID)
                                .Select(t => new { t.Name, t.TypeID }).ToList()
                                .Select(l => new SelectListItem { Value = l.TypeID.ToString(), Text = l.Name });

            list.AddRange(gpList);
            return list;
        }

        public ActionResult GetOtherExamResultList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, OtherExamResultFilterModel filterModel)
        {
            var query = new IEContext().OtherExamResultList.AsQueryable();
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.Student.FirstName.Contains(search) || p.Student.Surname.Contains(search));
            }

            if (filterModel.ClassID.HasValue)
                query = query.Where(l => l.Exam.ClassID == filterModel.ClassID.Value);
            else
                query = query.Where(l => l.Exam.Class.TermID == filterModel.TermID);

            if (filterModel.TypeID.HasValue)
                query = query.Where(l => l.Exam.TypeID == filterModel.TypeID.Value);

            if (filterModel.LevelID.HasValue)
                query = query.Where(l => l.Exam.Class.Arm.ClassLevelID == filterModel.LevelID.Value);

            if (filterModel.SexID.HasValue)
                query = query.Where(l => l.Student.IsMale == (filterModel.SexID.Value == 1));

            int filteredCount = query.Count();

            string sortStr = "";
            foreach (var col in requestModel.Columns.GetSortedColumns())
            {
                sortStr += sortStr != string.Empty ? "," : "";
                if (col.Data == "Position" || col.Data == "SubjectCount")
                {
                    sortStr = sortStr + col.Data + (col.SortDirection == Column.OrderDirection.Ascendant ? " asc" : " desc");
                    continue;
                }


                switch (col.Data)
                {
                    case "ImpFull": sortStr += "Improvement"; break;

                    case "AvgStr": sortStr += "AverageScore"; break;

                    case "LowStr": sortStr += "LowestScore"; break;

                    case "HighStr": sortStr += "HighestScore"; break;

                    case "DisplayName": sortStr += "Student.Surname"; break;

                    case "ClassName": sortStr += "Exam.Class.Arm.Name"; break;

                    case "Level": sortStr += "Exam.Class.ClassLevelID"; break;

                    case "ExamName": sortStr += "Exam.Type.Name"; break;

                    case "BestSubjectName": sortStr += "BestSubject.Name"; break;
                }

                sortStr += SortDirStr(col.SortDirection);
            }

            var defSort = "Exam.Type.Name, Exam.Class.Arm.Name asc, Student.Surname asc, Student.FirstName asc";
            var model = query.OrderBy(sortStr == string.Empty ? defSort : sortStr + ", " + defSort)
                             .Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<OtherExamResultViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Term Analysis and Reports

        [Route("Report/Term")]
        public ActionResult TermReports()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            int termID = 0;
            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { l.TermID }).FirstOrDefault();
                if (def == null || def.TermID == null) return RedirectToAction("School");
                termID = def.TermID.Value;
            }
            else
            {
                var tm = GetCurrentTermID(context, hVM.SchoolID);
                if (tm == null) return RedirectToAction("Index", "Home");

                termID = tm.Value;
            }

            var model = context.TermResultList.Where(l => l.TermID == termID).ProjectToFirstOrDefault<TermReportPageViewModel>();

            if (model == null)
            {
                model = context.TermList.Where(l => l.TermID == termID).ProjectToFirst<TermReportPageViewModel>();
                model.ShouldAnalyze = context.ClassResultList.Any(l => l.Class.TermID == termID);
            }
            else
            {
                model.CategoryStats = context.TermSubjectCategoryStatsList.Where(l => l.TermID == termID).ProjectToList<SubjectCategoryStatsViewModel>();
                model.ShouldAnalyze = context.ClassResultList.Any(l => l.Class.TermID == termID && l.AnalysisTime >= model.AnalysisTime);

                var bsclID = context.ClassResultList.Where(l => l.Class.TermID == termID && l.BestStudentID == model.BestStudentID).Select(l => (int?)l.ClassID).FirstOrDefault();
                if (bsclID.HasValue) model.BestStudentClassID = bsclID.Value;
            }

            model.HeaderViewModel = hVM;

            return View(model);
        }

        [Route("Report/Term/Analyze/{TermID:int}")]
        public ActionResult AnalyzeTerm(int TermID)
        {
            var context = new IEContext();

            var cres = context.ClassResultList.Where(l => l.Class.TermID == TermID)
                              .Select(l => new { l.ClassID, l.Class.Arm.Name, l.LowestAverage, l.MeanAverage, l.BestAverage, l.BestStudentID, l.ResultCount, l.SubjectCount }).ToList();

            var clIDs = cres.Select(l => l.ClassID).ToList();
            var subs = context.SubjectList.Where(l => clIDs.Contains(l.ClassID) && l.VerifiedByID.HasValue).ProjectToList<SubjectReportMiniModel>();
            var levelIDs = subs.Select(l => l.ClassLevelID).Distinct();
            var sbIDs = subs.Select(l => l.SubjectID).ToList();

            var scores = context.ScoreEntryList.Where(l => sbIDs.Contains(l.SubjectID) && l.ExamScore.HasValue).ProjectToList<ScoreEntryRepModel>();
            foreach (var cat in Eval.EnumToList<SubjectCategory>())
            {
                var catid = (byte)cat;
                var catRes = new TermSubjectCategoryStats { TermID = TermID, CategoryID = catid };
                var catSbIDs = subs.Where(l => l.CategoryID == catid).Select(l => l.SubjectID).ToList();

                if (!catSbIDs.Any()) continue;

                var catScores = scores.Where(l => catSbIDs.Contains(l.SubjectID));

                catRes.LowestScore = catScores.Min(l => l.Total).Value;
                catRes.AverageScore = catScores.Average(l => l.Total).Value;
                catRes.HighestScore = catScores.Max(l => l.Total).Value;

                catRes.ResultCount = (short)catScores.Count();
                catRes.SubjectCount = (byte)catSbIDs.Count;

                var bestStd = catScores.GroupBy(l => l.StudentID).Select(g => new { StudentID = g.Key, Average = g.Average(l => l.Total) }).OrderByDescending(l => l.Average).First();
                catRes.BestStudentID = bestStd.StudentID;
                catRes.BestStudentAverage = bestStd.Average.Value;

                var bestCSub = catScores.GroupBy(l => l.SubjectID).Select(g => new { SubjectID = g.Key, Average = g.Average(l => l.Total) }).OrderByDescending(l => l.Average).First();
                catRes.BestSubjectID = bestCSub.SubjectID;
                catRes.BestSubjectAverage = bestCSub.Average.Value;

                catRes.ACount = (short)catScores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.A);
                catRes.BCount = (short)catScores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.B);
                catRes.CCount = (short)catScores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.C);
                catRes.DCount = (short)catScores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.D);
                catRes.ECount = (short)catScores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.E);
                catRes.FCount = (short)catScores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.F);

                context.TermSubjectCategoryStatsList.AddOrUpdate(catRes);

                foreach(var levelID in levelIDs)
                {
                    var levelRes = new TermSubjectCategoryLevelStats { TermID = TermID, CategoryID = catid, LevelID = levelID };
                    var levelSbIDs = subs.Where(l => l.CategoryID == catid && l.ClassLevelID == levelID).Select(l => l.SubjectID).ToList();

                    if (!levelSbIDs.Any()) continue;

                    var levelScores = scores.Where(l => levelSbIDs.Contains(l.SubjectID));

                    levelRes.LowestScore = levelScores.Min(l => l.Total).Value;
                    levelRes.AverageScore = levelScores.Average(l => l.Total).Value;
                    levelRes.HighestScore = levelScores.Max(l => l.Total).Value;

                    levelRes.ResultCount = (short)levelScores.Count();
                    levelRes.SubjectCount = (byte)levelSbIDs.Count;

                    var bestLevelStd = levelScores.GroupBy(l => l.StudentID).Select(g => new { StudentID = g.Key, Average = g.Average(l => l.Total) }).OrderByDescending(l => l.Average).First();
                    levelRes.BestStudentID = bestLevelStd.StudentID;
                    levelRes.BestAverage = bestLevelStd.Average.Value;

                    context.TermSubjectCategoryLevelStatsList.AddOrUpdate(levelRes);
                }
            }

            var tempIDs = subs.Select(l => l.TemplateID).Distinct();
            foreach(var id in subs.Select(l => l.TemplateID).Distinct())
            {
                var tRes = new TermSubjectStats { TermID = TermID, TemplateID = id };
                var tSbIDs = subs.Where(l => l.TemplateID == id).Select(l => l.SubjectID).ToList();
                var tScores = scores.Where(l => tSbIDs.Contains(l.SubjectID));

                tRes.LowestScore = tScores.Min(l => l.Total).Value;
                tRes.AverageScore = tScores.Average(l => l.Total).Value;
                tRes.HighestScore = tScores.Max(l => l.Total).Value;

                tRes.ResultCount = (short)tScores.Count();
                tRes.SubjectCount = (byte)tSbIDs.Count();

                var bestStd = tScores.GroupBy(l => l.StudentID).Select(g => new { StudentID = g.Key, Average = g.Average(l => l.Total) }).OrderByDescending(l => l.Average).First();
                tRes.BestStudentID = bestStd.StudentID;

                context.TermSubjectStatsList.AddOrUpdate(tRes);
            }

            var tmRes = new TermResult { TermID = TermID };
            tmRes.LowestAverage = cres.Min(l => l.LowestAverage);
            tmRes.MeanAverage = cres.Sum(l => l.MeanAverage * l.ResultCount) / cres.Sum(l => l.ResultCount);
            tmRes.BestAverage = cres.Max(l => l.BestAverage);

            tmRes.ResultCount = (short)cres.Sum(l => l.ResultCount);
            tmRes.SubjectCount = (byte)cres.Sum(l => l.SubjectCount);

            tmRes.BestStudentID = cres.OrderByDescending(l => l.BestAverage).Select(l => l.BestStudentID).First();
            tmRes.BestClassID = cres.OrderByDescending(l => l.MeanAverage).Select(l => l.ClassID).First();

            var bestSub = scores.GroupBy(l => l.SubjectID).Select(g => new { SubjectID = g.Key, Average = g.Average(l => l.Total) }).OrderByDescending(l => l.Average).First();
            tmRes.BestSubjectID = bestSub.SubjectID;

            tmRes.AnalysisTime = DateTime.Now;

            context.TermResultList.AddOrUpdate(tmRes);

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

        [Route("Report/TermP/{TermID:int}")]
        public ActionResult PrintTermReport(int TermID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = context.TermResultList.Where(l => l.TermID == TermID).ProjectToFirst<TermPerformanceModel>();

            model.CategoryStats = context.TermSubjectCategoryStatsList.Where(l => l.TermID == TermID).ProjectToList<SubjectCategoryStatsViewModel>();
            model.Classes = context.ClassResultList.Where(l => l.Class.TermID == TermID).OrderBy(l => l.Class.Arm.ClassLevelID).ThenBy(l => l.Class.Arm.Name).ProjectToList<ClassReportModel>();

            return View(model);
        }

        [Route("Report/CategoryP/{SchoolID:int}/{CategoryID:int}")]
        public ActionResult PrintCategoryReport(int SchoolID, int CategoryID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var terms = context.TermList.Where(l => l.SchoolID == SchoolID).Select(l => new { l.TermID, l.Name, l.SchoolYear, l.TermNumber }).ToList();
            var tIDs = terms.Select(l => l.TermID);

            var model = new CategoryPerformanceModel { CategoryID = (byte)CategoryID };

            var stats = context.TermSubjectCategoryStatsList.Where(l => tIDs.Contains(l.TermID) && l.CategoryID == CategoryID).ProjectToList<TermCategoryStatsModel>();

            if (stats == null || !stats.Any())
                return null;

            var curTerm = terms.Where(l => stats.Any(s => s.TermID == l.TermID)).OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).First();
            model.CurrentTermName = curTerm.Name;
            model.CurrentAverage = stats.Where(l => l.TermID == curTerm.TermID).Select(l => l.AverageScore).First();

            model.FirstTermName = terms.Where(l => stats.Any(s => s.TermID == l.TermID)).OrderBy(l => l.SchoolYear).ThenBy(l => l.TermNumber).Select(l => l.Name).First();

            model.SubjectCount = stats.Sum(l => l.SubjectCount);
            model.ResultCount = stats.Sum(l => l.ResultCount);
            model.LowestAverage = stats.Min(l => l.AverageScore);
            model.MeanAverage = stats.Average(l => l.AverageScore);
            model.BestAverage = stats.Max(l => l.AverageScore);

            foreach(var md in stats)
                md.TermName = terms.Where(l => l.TermID == md.TermID).Select(l => l.Name).First();

            model.TermStats = stats;

            var defGradeGroupID = context.SchoolDataList.Where(l => l.SchoolID == SchoolID).Select(l => l.DefGradeGroupID).FirstOrDefault();

            if (defGradeGroupID == null)
                defGradeGroupID = context.ClassList.Where(l => l.Term.SchoolID == SchoolID).Select(l => l.GradeGroupID).First();

            model.PieLegend = new GradePieLegend { Grades = context.GradeList.Where(l => l.GradeGroupID == defGradeGroupID).ProjectToList<GradeViewModel>() };

            return View(model);
        }

        [Route("Report/CategoryTP/{TermID:int}")]
        public ActionResult PrintTermCategoryReports(int TermID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var tm = context.TermList.Where(l => l.TermID == TermID).Select(l => new { l.Name, l.SchoolID, SchoolName = l.School.Name }).FirstOrDefault();

            if(tm.SchoolID != hVM.SchoolID) return RedirectToAction("Schools", "Admin");

            if (tm == null) return null;

            var defGradeGroupID = context.SchoolDataList.Where(l => l.SchoolID == tm.SchoolID).Select(l => l.DefGradeGroupID).FirstOrDefault();

            if (defGradeGroupID == null)
                defGradeGroupID = context.ClassList.Where(l => l.TermID == TermID).Select(l => l.GradeGroupID).First();

            var levels = context.TermSubjectCategoryLevelStatsList.Where(l => l.TermID == TermID).ProjectToList<LevelCategoryStatsModel>();
            var categories = context.TermSubjectCategoryStatsList.Where(l => l.TermID == TermID).ProjectToList<TermCategoryPerformanceModel>();

            foreach (var cat in categories)
                cat.LevelStats = levels.Where(l => l.CategoryID == cat.CategoryID).ToList();

            var legend = new GradePieLegend { Grades = context.GradeList.Where(l => l.GradeGroupID == defGradeGroupID).ProjectToList<GradeViewModel>() };

            return View(new TermSubjectCategoriesModel { TermName = tm.Name, SchoolName = tm.SchoolName, Categories = categories, PieLegend = legend });
        }

        [Route("Report/LevelTP/{TermID:int}")]
        public ActionResult PrintTermLevelReports(int TermID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var tm = context.TermList.Where(l => l.TermID == TermID).Select(l => new { l.Name, l.SchoolID, SchoolName = l.School.Name }).FirstOrDefault();

            if (tm.SchoolID != hVM.SchoolID) return RedirectToAction("Schools", "Admin");

            if (tm == null) return null;

            var stats = context.TermSubjectStatsList.Where(l => l.TermID == TermID).ProjectToList<SubjectStatsModel>();

            var levelStats = new List<LevelSubjectStatsModel>();

            foreach (var lev in Eval.EnumToList<ClassLevel>())
            {
                var levid = (byte)lev;

                var st = stats.Where(l => l.LevelID == levid);

                if (st.Any())
                    levelStats.Add(new LevelSubjectStatsModel { LevelID = levid, Stats = st.ToList() });
            }

            return View(new TermSubjectPerformanceModel { TermName = tm.Name, SchoolName = tm.SchoolName, LevelStats = levelStats });
        }

        [Route("Report/TermsP/{SchoolID:int}")]
        public ActionResult PrintAllTermsReport(int SchoolID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            if (hVM.SchoolID != SchoolID) return null;

            var model = new AllTermsReportModel { TermStats = context.TermResultList.Where(l => l.Term.SchoolID == SchoolID).ProjectToList<TermResultModel>() };

            return View(model);
        }

        #endregion


        #region School Reports

        [Route("Report/Templates")]
        public ActionResult TemplateReports()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new TemplateReportPageViewModel();

            byte typeID = 0;
            int termID = 0;

            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID)
                                 .Select(l => new { l.TermID, l.School.Name, l.School.TypeID }).FirstOrDefault();

                if (def == null || def.TermID == null) return RedirectToAction("School");

                termID = def.TermID.Value;
                model.SchoolName = def.Name;
                typeID = def.TypeID;
            }
            else
            {
                var tm = context.TermList.Where(l => l.SchoolID == hVM.SchoolID && context.ClassList.Any(c => c.TermID == l.TermID))
                                .OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).Select(l => new { l.TermID, l.School.Name, l.School.TypeID }).FirstOrDefault();

                if (tm == null) return RedirectToAction("Index", "Home");

                termID = tm.TermID;
                model.SchoolName = tm.Name;
                typeID = tm.TypeID;
            }

            model.Levels = GetLevelModelList(typeID);

            model.Subjects = context.TermSubjectStatsList.Where(l => l.TermID == termID).ProjectToList<SubjectStatsRowModel>();

            model.HeaderViewModel = hVM;

            return View(model);
        }

        [Route("Report/TemplateP/{SchoolID:int}/{TemplateID:int}")]
        public ActionResult PrintTemplateReport(int SchoolID, int TemplateID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            if (SchoolID != hVM.SchoolID) return null;

            var schoolName = context.SchoolList.Where(l => l.SchoolID == SchoolID).Select(l => l.Name).First();

            var model = context.SubjectTemplateList.Where(l => l.TemplateID == TemplateID).ProjectToFirst<TemplateReportModel>();

            return View("PrintTemplateReports", new TemplateReportsPrintModel { SchoolName = schoolName, LevelID = model.ClassLevelID, Reports = new List<TemplateReportModel> { model } });
        }

        [Route("Report/TemplatesP/{SchoolID:int}/{LevelID:int}")]
        public ActionResult PrintTemplateReports(int SchoolID, int LevelID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            if (SchoolID != hVM.SchoolID) return null;

            var schoolName = context.SchoolList.Where(l => l.SchoolID == SchoolID).Select(l => l.Name).First();

            var reports = context.SubjectTemplateList.Where(l => l.SchoolID == SchoolID && l.ClassLevelID == LevelID && l.TermStats.Any())
                                 .OrderBy(l => l.Name)
                                 .ProjectToList<TemplateReportModel>();

            if (!reports.Any()) return null;

            return View(new TemplateReportsPrintModel { SchoolName = schoolName, LevelID = (byte)LevelID, Reports = reports });
        }


        [Route("Report/School")]
        public ActionResult SchoolReports()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_DataEntry) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new SchoolReportPageViewModel();

            var termID = 0;

            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID)
                                 .Select(l => new { l.TermID, l.School.Name, l.School.TypeID }).FirstOrDefault();

                if (def == null || def.TermID == null) return RedirectToAction("School");

                model.SchoolName = def.Name;
                model.TypeID = def.TypeID;
                termID = def.TermID.Value;
            }
            else
            {
                var tm = context.TermList.Where(l => l.SchoolID == hVM.SchoolID && context.ClassList.Any(c => c.TermID == l.TermID))
                                .OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).Select(l => new { l.TermID, l.School.Name, l.School.TypeID }).FirstOrDefault();

                if (tm == null) return RedirectToAction("Index", "Home");

                model.SchoolName = tm.Name;
                model.TypeID = tm.TypeID;
                termID = tm.TermID;
            }

            var aTime = context.SchoolDataList.Where(l => l.SchoolID == hVM.SchoolID).Select(l => l.AnalysisTime).First();
            var tTime = context.TermResultList.Where(l => l.TermID == termID).Select(l => l.AnalysisTime).DefaultIfEmpty(DateTime.MinValue).First();

            if (tTime > aTime) model.ShouldAnalyze = true;

            model.Stats = context.ClassTypeStatsList.Where(l => l.SchoolID == hVM.SchoolID).ProjectToList<ClassTypeStatsModel>();
            model.HeaderViewModel = hVM;

            return View(model);
        }

        [Route("Report/School/Analyze/{SchoolID:int}")]
        public ActionResult AnalyzeSchool(int SchoolID)
        {
            var context = new IEContext();

            var results = context.ClassResultList.Where(l => l.Class.Term.SchoolID == SchoolID).ProjectToList<ClassTypeAnalysisModel>();

            if (!results.Any()) return Json("", JsonRequestBehavior.AllowGet);

            foreach (var lev in Enum.GetValues(typeof(ClassLevel)).Cast<ClassLevel>())
            {
                var res = results.Where(l => l.ClassLevelID == (byte)lev);
                if (res.Any())
                {
                    var typeids = res.Select(l => l.ClassTypeID).Distinct();

                    foreach (var typeid in typeids)
                    {
                        var tres = res.Where(l => l.ClassTypeID == typeid);
                        var rep = new ClassTypeStats { SchoolID = SchoolID, ClassTypeID = typeid, ClassLevelID = (byte)lev };

                        rep.ResultCount = tres.Sum(l => l.ResultCount);
                        rep.ClassCount = (short)tres.Count();
                        rep.CurrentAverage = tres.OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).Select(l => l.MeanAverage).First();
                        rep.LowestAverage = tres.Min(l => l.MeanAverage);
                        rep.MeanAverage = tres.Average(l => l.MeanAverage);
                        rep.BestAverage = tres.Max(l => l.MeanAverage);

                        context.ClassTypeStatsList.AddOrUpdate(rep);
                    }
                }
            }

            var sdata = new SchoolData { SchoolID = SchoolID };
            context.SchoolDataList.Attach(sdata);

            sdata.AnalysisTime = DateTime.Now;

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

            return Json("", JsonRequestBehavior.AllowGet);
        }

        [Route("Report/ClassTypeP/{SchoolID:int}")]
        public ActionResult PrintClassTypeReport(int SchoolID, int LevelID, int ClassTypeID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            if (SchoolID != hVM.SchoolID) return null;

            var schoolName = context.SchoolList.Where(l => l.SchoolID == SchoolID).Select(l => l.Name).First();

            var model = new ClassTypeReportModel { ClassLevelID = (byte)LevelID, ClassTypeID = (byte)ClassTypeID };
                 
            model.Classes = context.ClassResultList.Where(l => l.Class.Arm.ClassLevelID == LevelID && l.Class.Arm.ClassTypeID == ClassTypeID && l.Class.Term.SchoolID == SchoolID)
                                   .OrderBy(l => l.Class.Arm.Name)
                                   .ProjectToList<ClassReportMaxModel>();

            return View("PrintClassTypeReports", new ClassTypeReportsPrintModel { SchoolName = schoolName, Reports = new List<ClassTypeReportModel> { model } });
        }

        [Route("Report/ClassTypesP/{SchoolID:int}")]
        public ActionResult PrintClassTypeReports(int SchoolID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            if (SchoolID != hVM.SchoolID) return null;

            var school = context.SchoolList.Where(l => l.SchoolID == SchoolID).Select(l => new { l.Name, l.TypeID }).First();

            var results = context.ClassResultList.Where(l => l.Class.Term.SchoolID == SchoolID)
                                 .OrderBy(l => l.Class.Arm.ClassLevelID).ThenBy(l => l.Class.Arm.Name)
                                 .ProjectToList<ClassReportMaxiModel>();

            if (!results.Any()) return null;

            var reports = new List<ClassTypeReportModel>();
            foreach(var lev in GetLevelModelList(school.TypeID))
            {
                var res = results.Where(l => l.ClassLevelID == lev.LevelID);
                if (res.Any())
                {
                    var typeids = res.Select(l => l.ClassTypeID).Distinct();

                    foreach(var typeid in typeids)
                    {
                        var rep = new ClassTypeReportModel { ClassTypeID = typeid, ClassLevelID = lev.LevelID };
                        rep.Classes = res.Where(l => l.ClassTypeID == typeid).AsQueryable().ProjectToList<ClassReportMaxModel>();
                        reports.Add(rep);
                    }
                }
            }

            return View(new ClassTypeReportsPrintModel { SchoolName = school.Name, Reports = reports });
        }

        #endregion

        #region Result Labels

        [Route("Report/Labels")]
        public ActionResult ResultLabels()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var model = new ResultLabelsPageViewModel();

            if (hVM.Has_ViewSetup)
            {
                var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID)
                                 .Select(l => new { l.Term.SchoolYear, l.Term.TermNumber, l.Term.Name }).FirstOrDefault();
                if (def == null) return RedirectToAction("School");

                model.TermName = def.Name;
                model.Terms = context.TermList.Where(l => l.SchoolYear == def.SchoolYear && l.TermNumber == def.TermNumber).ProjectToList<SchoolTermViewModel>();
            }
            else
            {
                var tm = context.TermList.Where(l => l.SchoolID == hVM.SchoolID && context.ClassList.Any(c => c.TermID == l.TermID))
                                .OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).ProjectToFirstOrDefault<SchoolTermViewModel>();
                if (tm == null) return RedirectToAction("Index", "Home");

                model.TermName = tm.Name;
                model.Terms = new List<SchoolTermViewModel> { tm };
            }

            model.HeaderViewModel = hVM;

            return View(model);
        }

        [Route("Report/LabelsP/{TermIDs}/")]
        public ActionResult PrintResultLabels(string TermIDs)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            List<int> idList = new List<int>();

            var termName = "";

            if (hVM.Has_ViewSetup)
            {
                var idArray = TermIDs.Split(',');

                var termID = 0;
                foreach (string id in idArray)
                {
                    if (!int.TryParse(id, out termID)) continue;

                    if (termID > 0) idList.Add(termID);
                }

                termName = context.TermList.Where(l => l.TermID == termID).Select(l => l.Name).FirstOrDefault();
            }
            else
            {
                var tm = GetCurrentTerm(context, hVM.SchoolID);

                if (tm == null) return RedirectToAction("Index", "Home");

                idList.Add(tm.TermID);
                termName = tm.Name;
            }

            var labels = context.StudentList.Where(l => idList.Contains(l.Class.TermID))
                                .OrderBy(l => l.Class.TermID).ThenBy(l => l.Class.Arm.ClassLevelID).ThenBy(l => l.Class.Arm.Name).ThenBy(l => l.Surname).ThenBy(l => l.FirstName)
                                .ProjectToList<ResultLabelModel>();

            var model = new List<ResultLabelRowModel>();
            var row = new ResultLabelRowModel();

            int cnt = 0;
            foreach(var lbl in labels)
            {
                cnt++;
                var pos = cnt % 5;

                lbl.TermName = termName;
                switch (pos)
                {
                    case 1: row.Label1 = lbl; break;
                    case 2: row.Label2 = lbl; break;
                    case 3: row.Label3 = lbl; break;
                    case 4: row.Label4 = lbl; break;
                    default:
                        row.Label5 = lbl;
                        model.Add(row);
                        row = new ResultLabelRowModel();
                        break;
                }
            }

            if(row.Label1 != null)
            {
                if (row.Label2 == null) row.Label2 = new ResultLabelModel();
                if (row.Label3 == null) row.Label3 = new ResultLabelModel();
                if (row.Label4 == null) row.Label4 = new ResultLabelModel();
                if (row.Label5 == null) row.Label5 = new ResultLabelModel();

                model.Add(row);
            }

            return View(model);
        }

        #endregion


        #region Transcripts
        public ActionResult Transcripts()
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Setup);
            if (!hVM.Has_ViewSetup) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");

            var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID)
                             .Select(l => new { l.School.Name, l.School.TypeID }).FirstOrDefault();

            if (def == null || def.TypeID == 0) return RedirectToAction("School");

            var model = new TranscriptPageViewModel
            {
                HeaderViewModel = hVM,

                SchoolName = def.Name,
                LevelList = GetLevelList(def.TypeID),
            };

            return View(model);
        }

        public ActionResult GetTranscriptList([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, TranscriptFilterViewModel filterModel)
        {
            var query = new IEContext().StudentList.Where(l => l.SchoolID == filterModel.SchoolID);
            int total = query.Count();

            if (requestModel.Search.Value != string.Empty)
            {
                string search = requestModel.Search.Value.Trim();
                query = query.Where(p => p.FirstName.Contains(search) || p.Surname.Contains(search) || p.OtherName.Contains(search) || p.StudentCode.Contains(search));
            }

            if (filterModel.YearFrom.HasValue)
                query = query.Where(l => !l.ClassID.HasValue || l.Class.Term.SchoolYear >= filterModel.YearFrom.Value);

            if (filterModel.YearTo.HasValue)
                query = query.Where(l => !l.ClassID.HasValue || l.Class.Term.SchoolYear <= filterModel.YearTo.Value);

            if (filterModel.LevelID.HasValue)
                query = query.Where(l => !l.ClassID.HasValue || l.Class.Arm.ClassLevelID == filterModel.LevelID.Value);

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

                    case "ClassName": sortStr += "Class.Arm.Name"; break;

                    case "Level": sortStr += "Class.ClassLevelID"; break;

                    case "TermName": sortStr += "Class.Term.Name"; break;
                }

                sortStr += SortDirStr(col.SortDirection);
            }

            var defSort = "Surname asc, FirstName asc";
            var model = query.OrderBy(sortStr == string.Empty ? defSort : sortStr + ", " + defSort)
                             .Skip(requestModel.Start).Take(requestModel.Length).ProjectToList<TranscriptViewModel>();

            return Json(new DataTablesResponse(requestModel.Draw, model, filteredCount, total), JsonRequestBehavior.AllowGet);
        }

        [Route("Report/TranscriptP/{StudentID:int}")]
        public ActionResult PrintTranscript(int StudentID)
        {
            var context = new IEContext();
            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Report);
            if (!hVM.Has_ViewReports) return RedirectToAction("Index", "Home");

            if (hVM.SchoolID == 0) return RedirectToAction("Schools", "Admin");



            return View();
        }

        #endregion
    }
}