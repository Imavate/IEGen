using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IEGen.Models
{
    public class ReportIndexPageViewModel : BasePageViewModel
    {
        public List<ClassStatsModel> Classes { get; set; }
        public List<ClassReportModel> Results { get; set; }

        public int TermID { get; set; }

        public string Name { get; set; }

        public DateTime NextResumptionDate { get; set; }
        public string NRDateStr { get { return General.FullDateString(NextResumptionDate); } }

        public byte DaysOpened { get; set; }

        public int StudentCount { get { return Results.Sum(l => l.StudentCount); } }
    }

    public class ClassStatsModel
    {
        public int ClassID { get; set; }

        public string Name { get; set; }

        public int StudentCount { get; set; }

        public byte SubjectCount { get; set; }
    }

    public class ClassReportModel : ClassStatsModel
    {
        public byte ResultCount { get; set; }

        public string BestStudentName { get; set; }

        public string BestSubjectName { get; set; }

        public decimal LowestAverage { get; set; }
        public decimal MeanAverage { get; set; }
        public decimal BestAverage { get; set; }

        public string LowestAvg { get { return HasResult ? LowestAverage.ToString("F") + " %" : ""; } }
        public string MeanAvg { get { return HasResult ? MeanAverage.ToString("F") + " %" : ""; } }
        public string BestAvg { get { return HasResult ? BestAverage.ToString("F") + " %" : ""; } }

        public bool HasResult { get { return BestAverage > 0; } }
    }

    public class AnalyzeClassViewModel
    {
        public int ClassID { get; set; }

        public byte ClassLevelID { get; set; }
        public string Level { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public string Name { get; set; }

        public byte CAWeight { get; set; }
        public byte ExamWeight { get; set; }

        // promotional class fields
        public bool IsPromotionalClass { get; set; }
        public bool CommentOnYearResult { get; set; }   
        public bool ShowYearResult { get; set; } 
        public string PromotionComments { get; set; }

        public string PerformanceComments { get; set; }
        public string ImprovementComments { get; set; }

        public List<SubjectMiniModel> Subjects { get; set; }
    }

    public class ClassAnalysisModel
    {
        public short SchoolYear { get; set; }

        public byte ClassLevelID { get; set; }

        public bool IsPromotionalClass { get; set; }
        public bool CommentOnYearResult { get; set; }
        public byte RedLine { get; set; }

        public bool UsePerformanceComments { get; set; }
        public bool UsePromotionComments { get; set; }
        public bool UseImprovementComments { get; set; }

        public int TermID { get; set; }
        public DateTime NextResumptionDate { get; set; }
        public string LogoGuidString { get; set; }

        public int SchoolID { get; set; }

        public List<PerformanceCommentViewModel> PerformanceComments { get; set; }
        public List<PromotionCommentViewModel> PromotionComments { get; set; }
        public List<ImprovementCommentViewModel> ImprovementComments { get; set; }
    }

    public class SubjectMiniModel
    {
        public string Name { get; set; }

        public string ResultName { get; set; }

        public int? VerifiedByID { get; set; }
        public bool IsVerified { get { return VerifiedByID.HasValue; } }
    }

    public class ScoreEntryModel
    {
        public int StudentID { get; set; }
        public int SubjectID { get; set; }

        public decimal? CAScore { get; set; }
        public decimal? ExamScore { get; set; }

        public decimal? Total { get { return (CAScore.HasValue ? CAScore.Value : 0) + ExamScore; } }
    }

    public class ScoreEntryRepModel : ScoreEntryModel
    {
        public byte? SummaryGradeID { get; set; }
        public string SummaryGrade { get { return SummaryGradeID.HasValue ? Eval.GetDisplayName(typeof(SummaryGrade), SummaryGradeID.Value) : ""; } }
    }

    public class ScoreSummaryGradeModel
    {
        public int StudentID { get; set; }
        public byte? SummaryGradeID { get; set; }
    }

    public class ScoreEntryGradeModel
    {
        public int StudentID { get; set; }
        public int SubjectID { get; set; }

        public string GradeName { get; set; }
        public byte? SummaryGradeID { get; set; }
        public string SummaryGrade { get { return SummaryGradeID.HasValue ? Eval.GetDisplayName(typeof(SummaryGrade), SummaryGradeID.Value) : ""; } }
    }

    public class ScoreEntryFullModel : ScoreEntryGradeModel
    {
        public decimal? CAScore { get; set; }
        public decimal? ExamScore { get; set; }

        public decimal? Total { get { return (CAScore.HasValue ? CAScore.Value : 0) + ExamScore; } }
    }


    public class ClassResultMiniModel
    {
        public int ClassID { get; set; }

        public string Name { get; set; }

        public string TermName { get; set; }

        public int SchoolID { get; set; }
        public string SchoolName { get; set; }

        public decimal MeanAverage { get; set; }
        public string MeanAvgStr { get { return MeanAverage.ToString("F"); } }

        public string BestStudent { get; set; }

        public string BestSubject { get; set; }

        public byte ResultCount { get; set; }

        public byte SubjectCount { get; set; }

    }

    public class BroadsheetViewModel : ClassResultMiniModel
    {
        public List<BroadsheetRowModel> Rows { get; set; }

        public List<SubjectResultModel> Subjects { get; set; }

        public List<ScoreEntryModel> Scores { get; set; }

        public byte RedLine { get; set; }
    }

    public class GradeBroadsheetViewModel : BroadsheetViewModel
    {
        public bool ShowSummaryGrade { get; set; }

        public new List<ScoreEntryGradeModel> Scores { get; set; }
    }

    public class FullBroadsheetViewModel : GradeBroadsheetViewModel
    {
        public new List<ScoreEntryFullModel> Scores { get; set; }
    }

    public class SubjectResultModel
    {
        public byte CN { get; set; }

        public int SubjectID { get; set; }

        public byte Order { get; set; }

        public string ResultName { get; set; }

        public decimal AverageScore { get; set; }
        public string AvgStr { get { return AverageScore.ToString("F"); } }
    }

    public class BroadsheetCellModel
    {
        public byte CN { get; set; }

        public decimal? Total { get; set; }
        public string TotalStr { get { return Total.HasValue ? Total.Value.ToString("F") : ""; } }

        public string GradeStr { get; set; }
    }

    public class BroadsheetRowModel
    {
        public byte SN { get; set; }

        public int StudentID { get; set; }

        public string StudentName { get; set; }

        public decimal AverageScore { get; set; }
        public string AvgStr { get { return AverageScore.ToString("F"); } }

        public byte Position { get; set; }

        public int CountA { get; set; }
        public int CountB { get; set; }
        public int CountC { get; set; }
        public int CountD { get; set; }
        public int CountE { get; set; }
        public int CountF { get; set; }

        public byte GradeCountA { get; set; }
        public byte GradeCountB { get; set; }
        public byte GradeCountC { get; set; }
        public byte GradeCountD { get; set; }
        public byte GradeCountE { get; set; }
        public byte GradeCountF { get; set; }

        public List<BroadsheetCellModel> Scores { get; set; }
    }


    public class ClassReportPageViewModel : BasePageViewModel
    {
        public int TermID { get; set; }

        public string TermName { get; set; }

        public List<ClassReportModel> Results { get; set; }
    }

    public class BroadsheetPageViewModel : BasePageViewModel
    {
        public BroadsheetPageViewModel()
        {
            Broadsheets = new List<BroadsheetViewModel>();
            GradeBroadsheets = new List<GradeBroadsheetViewModel>();
            FullBroadsheets = new List<FullBroadsheetViewModel>();
        }

        public string Name { get; set; }

        public List<BroadsheetViewModel> Broadsheets { get; set; }
        public List<GradeBroadsheetViewModel> GradeBroadsheets { get; set; }
        public List<FullBroadsheetViewModel> FullBroadsheets { get; set; }
    }


    public class SubjectPerformanceModel : SubjectResultModel
    {
        public string Name { get; set; }

        public byte CategoryID { get; set; }
        public string CategoryName { get { return Eval.GetDisplayName(typeof(SubjectCategory), CategoryID); } }

        public int ResultCount { get; set; }

        public decimal LowestScore { get; set; }
        public string LowStr { get { return LowestScore.ToString("F"); } }

        public decimal BestScore { get; set; }
        public string BestStr { get { return BestScore.ToString("F"); } }

        public string BestStudentName { get; set; }
    }

    public class SubjectCategoryModel
    {
        public string Name { get; set; }

        public decimal LowestAverage { get; set; }
        public string LowAvgStr { get { return LowestAverage.ToString("F"); } }

        public decimal MeanAverage { get; set; }
        public string MeanAvgStr { get { return MeanAverage.ToString("F"); } }

        public decimal BestAverage { get; set; }
        public string BestAvgStr { get { return BestAverage.ToString("F"); } }

        public int SubjectCount { get; set; }
        public int ResultCount { get; set; }

        public string BestStudentName { get; set; }
    }

    public class StudentMiniModel
    {
        public int StudentID { get; set; }

        public string FirstName { get; set; }

        public string Surname { get; set; }

        public string DisplayName { get { return Surname.ToUpper() + " " + FirstName; } }
    }

    public class GradePieLegend
    {
        public List<GradeViewModel> Grades { get; set; }

        public bool ShowSummaryGrade { get; set; }

        string GradeLegend(IEnumerable<GradeViewModel> Grades)
        {
            if (!Grades.Any())
                return "";

            var low = Grades.Min(l => l.LowerBound);
            var high = Grades.Max(l => l.UpperBound);
            return "[" + low + "-" + high + "] " + (ShowSummaryGrade ? "" : "(" + Grades.Select(l => l.Name).Aggregate((a, b) => (a + "," + b)) + ")");
        }

        public string LegendA { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.A)); } }
        public string LegendB { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.B)); } }
        public string LegendC { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.C)); } }
        public string LegendD { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.D)); } }
        public string LegendE { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.E)); } }
        public string LegendF { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.F)); } }

        public bool HasLegendA { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.A); } }
        public bool HasLegendB { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.B); } }
        public bool HasLegendC { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.C); } }
        public bool HasLegendD { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.D); } }
        public bool HasLegendE { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.E); } }
        public bool HasLegendF { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.F); } }
    }

    public class StudentTermResultModel
    {
        public int StudentID { get; set; }

        public byte TermNumber { get; set; }

        public decimal AverageScore { get; set; }

        public byte GradeCountA { get; set; }
        public byte GradeCountB { get; set; }
        public byte GradeCountC { get; set; }
        public byte GradeCountD { get; set; }
        public byte GradeCountE { get; set; }
        public byte GradeCountF { get; set; }
    }

    public class StudentYearModel
    {
        public int StudentID { get; set; }

        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string DisplayName { get { return Surname.ToUpper() + " " + FirstName; } }

        public decimal? Term1Score { get; set; }
        public string Term1Str { get { return Term1Score.HasValue ? Term1Score.Value.ToString("F") : ""; } }

        public decimal? Term2Score { get; set; }
        public string Term2Str { get { return Term2Score.HasValue ? Term2Score.Value.ToString("F") : ""; } }

        public decimal? Term3Score { get; set; }
        public string Term3Str { get { return Term3Score.HasValue ? Term3Score.Value.ToString("F") : ""; } }

        public decimal YearAverage { get { return new decimal?[] { Term1Score, Term2Score, Term3Score }.Average() ?? 0; } }
        public string YearAvgStr { get { return YearAverage.ToString("F"); } }

        public int CountA { get; set; }
        public int CountB { get; set; }
        public int CountC { get; set; }
        public int CountD { get; set; }
        public int CountE { get; set; }
        public int CountF { get; set; }

    }

    public class ClassPerformanceModel : ClassResultMiniModel
    {
        public List<StudentMiniModel> Students { get; set; }

        public List<SubjectPerformanceModel> Subjects { get; set; }

        public List<ScoreEntryRepModel> Scores { get; set; }

        public List<SubjectCategoryModel> Categories { get; set; }

        public List<GradeViewModel> Grades { get; set; }

        public bool ShowSummaryGrade { get; set; }

        string GradeLegend(IEnumerable<GradeViewModel> Grades)
        {
            if (!Grades.Any())
                return "";

            var low = Grades.Min(l => l.LowerBound);
            var high = Grades.Max(l => l.UpperBound);
            return "[" + low + "-" + high + "] " + (ShowSummaryGrade ? "" : "(" + Grades.Select(l => l.Name).Aggregate((a, b) => (a + "," + b)) + ")");
        }

        public string LegendA { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.A)); } }
        public string LegendB { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.B)); } }
        public string LegendC { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.C)); } }
        public string LegendD { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.D)); } }
        public string LegendE { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.E)); } }
        public string LegendF { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.F)); } }

        public bool HasLegendA { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.A); } }
        public bool HasLegendB { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.B); } }
        public bool HasLegendC { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.C); } }
        public bool HasLegendD { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.D); } }
        public bool HasLegendE { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.E); } }
        public bool HasLegendF { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.F); } }

        public int GradeCountA { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.A); } }
        public int GradeCountB { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.B); } }
        public int GradeCountC { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.C); } }
        public int GradeCountD { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.D); } }
        public int GradeCountE { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.E); } }
        public int GradeCountF { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.F); } }

        public string SubjectNames { get { return General.Json(Subjects.Select(l => l.ResultName)); } }
        public string LowestScores { get { return General.Json(Subjects.Select(l => l.LowestScore)); } }
        public string AverageScores { get { return General.Json(Subjects.Select(l => decimal.Round(l.AverageScore))); } }
        public string BestScores { get { return General.Json(Subjects.Select(l => l.BestScore)); } }

        public string CategoryNames { get { return General.Json(Categories.Select(l => l.Name)); } }
        public string CatLowAverages { get { return General.Json(Categories.Select(l => decimal.Round(l.LowestAverage))); } }
        public string CatMeanAverages { get { return General.Json(Categories.Select(l => decimal.Round(l.MeanAverage))); } }
        public string CatBestAverages { get { return General.Json(Categories.Select(l => decimal.Round(l.BestAverage))); } }

        public decimal LowestAverage { get; set; }
        public string LowAvgStr { get { return LowestAverage.ToString("F"); } }

        public decimal BestAverage { get; set; }
        public string BestAvgStr { get { return BestAverage.ToString("F"); } }


        public bool IsPromotionalClass { get; set; }
        public short SchoolYear { get; set; }
        public string YearName { get { return General.YearName(SchoolYear); } }
        public List<StudentYearModel> YearAverages { get; set; }

        public decimal MeanYearAverage { get { return YearAverages.Average(l => l.YearAverage); } }
        public string MeanYearAvgStr { get { return MeanYearAverage.ToString("F"); } }

        public decimal BestYearAverage { get { return YearAverages.Max(l => l.YearAverage); } }
        public string BestYearAvgStr { get { return BestYearAverage.ToString("F"); } }

        public string BestYearStudent { get { return YearAverages.OrderByDescending(l => l.YearAverage).Select(l => l.DisplayName).FirstOrDefault(); } }

        public int YearCountA { get { return YearAverages.Sum(l => l.CountA); } }
        public int YearCountB { get { return YearAverages.Sum(l => l.CountB); } }
        public int YearCountC { get { return YearAverages.Sum(l => l.CountC); } }
        public int YearCountD { get { return YearAverages.Sum(l => l.CountD); } }
        public int YearCountE { get { return YearAverages.Sum(l => l.CountE); } }
        public int YearCountF { get { return YearAverages.Sum(l => l.CountF); } }

    }


    public class StudentReportModel
    {
        public int StudentID { get; set; }
        public string ChartID { get { return "scoreChart" + StudentID.ToString(); } }
        public string CategoryChartID { get { return "categoryChart" + StudentID.ToString(); } }

        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string DisplayName { get { return Surname.ToUpper() + " " + FirstName; } }

        public string ClassName { get; set; }

        public bool ShowCode { get { return (Surname.Length * 1.5) + (FirstName + ClassName).Length < 35; } }

        public string TermName { get; set; }
        public byte TermNumber { get; set; }
        public DateTime NextResumptionDate { get; set; }
        public string NRDateStr { get { return General.FullDateString(NextResumptionDate); } }

        public byte CAWeight { get; set; }
        public byte ExamWeight { get; set; }
        public bool ShowSummaryGrade { get; set; }
        public bool ShowYearResult { get; set; }
        public bool ShowPosition { get; set; }
        public bool ShowCategoryAnalysis { get; set; }
        public bool HideClassAverage { get; set; }

        public byte RedLine { get; set; }

        public byte CARedLine { get { return (byte)(RedLine * CAWeight * 0.01); } }
        public byte ExamRedLine { get { return (byte)(RedLine * ExamWeight * 0.01); } }

        public int SchoolID { get; set; }
        public string SchoolName { get; set; }
        public byte SchoolTypeID { get; set; }
        public string PrincipalCommentText { get { return SchoolTypeID == (byte)SchoolType.NurseryPrimary ? "Head Teacher's Comment:" : "Principal's Comment:"; } }

        public short SchoolYear { get; set; }
        public string YearStr { get { return SchoolYear.ToString() + "/" + (SchoolYear + 1).ToString(); } }

        public string StudentCode { get; set; }
        public string Code { get { return string.IsNullOrEmpty(StudentCode) ? "ISN" + StudentID.ToString().PadLeft(4, '0') : StudentCode; } }

        public string GuidString { get; set; }
        public string PhotoSrc { get; set; }

        public string LogoGuidString { get; set; }
        public string LogoSrc { get; set; }

        public decimal LowestScore { get; set; }
        public string LowStr { get { return LowestScore.ToString("F") + "%"; } }

        public decimal AverageScore { get; set; }
        public string AvgStr { get { return AverageScore.ToString("F") + "%"; } }

        public decimal HighestScore { get; set; }
        public string HighStr { get { return HighestScore.ToString("F") + "%"; } }

        public decimal Improvement { get; set; }  // points improvement from last term
        public string ImpFull { get { return General.ImprovementDisplay(Improvement); } }

        public byte Position { get; set; }
        public int StudentCount { get; set; }
        public string PosFull { get { return General.PositionDisplay(Position, StudentCount) + (Improvement == 0 ? "" :" (" + ImpFull + ")"); } }

        public string BestSubjectName { get; set; }

        public string TeacherComment { get; set; }
        public string PrincipalComment { get; set; }

        public bool HasTeacherComment { get { return !string.IsNullOrWhiteSpace(TeacherComment); } }

        public List<GradeViewModel> Grades { get; set; }

        public List<GradeViewModel> SGrades
        {
            get
            {
                var AGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.A);
                var BGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.B);
                var CGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.C);
                var DGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.D);
                var EGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.E);
                var FGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.F);

                var list = new List<GradeViewModel>();

                if (AGrades.Any()) list.Add(new GradeViewModel { LowerBound = AGrades.Min(l => l.LowerBound), UpperBound = AGrades.Max(l => l.UpperBound), Name = "A" });
                if (BGrades.Any()) list.Add(new GradeViewModel { LowerBound = BGrades.Min(l => l.LowerBound), UpperBound = BGrades.Max(l => l.UpperBound), Name = "B" });
                if (CGrades.Any()) list.Add(new GradeViewModel { LowerBound = CGrades.Min(l => l.LowerBound), UpperBound = CGrades.Max(l => l.UpperBound), Name = "C" });
                if (DGrades.Any()) list.Add(new GradeViewModel { LowerBound = DGrades.Min(l => l.LowerBound), UpperBound = DGrades.Max(l => l.UpperBound), Name = "D" });
                if (EGrades.Any()) list.Add(new GradeViewModel { LowerBound = EGrades.Min(l => l.LowerBound), UpperBound = EGrades.Max(l => l.UpperBound), Name = "E" });
                if (FGrades.Any()) list.Add(new GradeViewModel { LowerBound = FGrades.Min(l => l.LowerBound), UpperBound = FGrades.Max(l => l.UpperBound), Name = "F" });

                return list;
            }
        }

        public List<GradeViewModel> GradeRows { get { return ShowSummaryGrade ? SGrades : Grades; } }

        public List<ScoreReportModel> Scores { get; set; }

        public string SubjectNames { get { return General.Json(Scores.Select(l => l.ResultName)); } }
        public string FailureLine { get { return General.Json(Scores.Select(l => RedLine)); } }
        public string ScoresJson { get; set; }
        public string LowScoresJson { get; set; }
        public string AvgScoresJson { get; set; }
        public string MaxScoresJson { get; set; }

        public List<YearSubjectModel> YearSubjects { get; set; }
        public string LowestAverage { get { return YearSubjects.Min(l => l.YearAverage).ToString("F") + "%"; } }
        public string MeanAverage { get { return YearSubjects.Average(l => l.YearAverage).ToString("F") + "%"; } }
        public string BestAverage { get { return YearSubjects.Max(l => l.YearAverage).ToString("F") + "%"; } }

        public List<SubjectCategoryModel> Categories { get; set; }
        public string CategoryNames { get { return Categories == null ? General.Json("-") : General.Json(Categories.Select(l => l.Name)); } }
        public string CatLowAverages { get { return Categories == null ? "0" : General.Json(Categories.Select(l => decimal.Round(l.LowestAverage))); } }
        public string CatMeanAverages { get { return Categories == null ? "0" : General.Json(Categories.Select(l => decimal.Round(l.MeanAverage))); } }
        public string CatBestAverages { get { return Categories == null ? "0" : General.Json(Categories.Select(l => decimal.Round(l.BestAverage))); } }

        public int ScoreCount { get { return Scores.Count; } }

        public bool HasSkills { get; set; }

        public string Skill1 { get; set; }
        public string Skill2 { get; set; }
        public string Skill3 { get; set; }
        public string Skill4 { get; set; }
        public string Skill5 { get; set; }
        public string Skill6 { get; set; }

        public List<SkillGradeResultModel> SkillGrades { get; set; }

        public string SkillScore1 { get; set; }
        public string SkillScore2 { get; set; }
        public string SkillScore3 { get; set; }
        public string SkillScore4 { get; set; }
        public string SkillScore5 { get; set; }
        public string SkillScore6 { get; set; }

        public string SkillGrade1 { get; set; }
        public string SkillGrade2 { get; set; }
        public string SkillGrade3 { get; set; }
        public string SkillGrade4 { get; set; }
        public string SkillGrade5 { get; set; }
        public string SkillGrade6 { get; set; }
    }

    public class SkillGradeResultModel : SkillGradeMiniModel
    {
        public byte GradeScore { get; set; }

        public string NumberScore { get { return GradeNumber.ToString() + " (" + GradeScore.ToString() + "%)"; } }
    }

    public class StudentSkillsReportModel
    {
        public byte? SkillScore1 { get; set; }
        public byte? SkillScore2 { get; set; }
        public byte? SkillScore3 { get; set; }
        public byte? SkillScore4 { get; set; }
        public byte? SkillScore5 { get; set; }
        public byte? SkillScore6 { get; set; }
    }

    public class StudentSubjectModel : SubjectResultModel
    {
        public int TemplateID { get; set; }
        public byte TermNumber { get; set; }

        public byte CategoryID { get; set; }

        public bool NoCA { get; set; }

        public int ClassID { get; set; }
    }

    public class ScoreReportModel : ScoreEntryFullModel
    {
        public int TemplateID { get; set; }
        public byte TermNumber { get; set; }

        public bool NoCA { get; set; }
        public byte CAWeight { get; set; }
        public byte ExamWeight { get; set; }

        public decimal? DisplayCA { get { return NoCA ? (ExamScore * CAWeight * (decimal)0.01) : CAScore; } }
        public decimal? DisplayExam { get { return NoCA ? (ExamScore * ExamWeight * (decimal)0.01) : ExamScore; } }

        public string ResultName { get; set; }

        public string CAStr { get { return DisplayCA.HasValue ? DisplayCA.Value.ToString("F") : ""; } }
        public string ExamStr { get { return ExamScore.HasValue ? DisplayExam.Value.ToString("F") : ""; } }
        public string TotalStr { get { return Total.Value.ToString("F"); } }
    }

    public class YearSubjectModel
    {
        public byte Order { get; set; }

        public string Name { get; set; }

        public decimal? Term1Score { get; set; }
        public string Term1Str { get { return Term1Score.HasValue ? Term1Score.Value.ToString("F") : ""; } }

        public decimal? Term2Score { get; set; }
        public string Term2Str { get { return Term2Score.HasValue ? Term2Score.Value.ToString("F") : ""; } }

        public decimal? Term3Score { get; set; }
        public string Term3Str { get { return Term3Score.HasValue ? Term3Score.Value.ToString("F") : ""; } }

        public decimal Improvement
        {
            get
            {
                if(Term3Score.HasValue)
                {
                    if (Term2Score.HasValue)
                        return ((Term3Score - Term2Score) / Term2Score).Value * 100;
                    else if (Term1Score.HasValue)
                        return ((Term3Score - Term1Score) / Term1Score).Value * 100;
                    else return 0;
                }

                if(Term2Score.HasValue)
                {
                    if (Term1Score.HasValue)
                        return ((Term2Score - Term1Score) / Term1Score).Value * 100;
                    else return 0;
                }

                return 0;
            }
        }
        public string ImpStr { get { return Improvement == 0 ? "-" : Improvement.ToString("F") + "%"; } }
        public bool Improved { get { return Improvement > 0; } }
        public bool Declined { get { return Improvement < 0; } }

        public decimal YearAverage { get; set; }
        public string YearAvgStr { get { return YearAverage.ToString("F"); } }

        public decimal ClassAverage { get; set; }
        public string ClassAvgStr { get { return ClassAverage.ToString("F"); } }

    }

    public class StudentReportPageViewModel : BasePageViewModel
    {
        public string Name { get; set; }

        public List<StudentReportModel> Reports { get; set; }
    }

    public class StudentResultViewModel
    {
        public int StudentID { get; set; }

        public int ClassID { get; set; }

        public string DisplayName { get; set; }

        public string ClassName { get; set; }

        public string TermName { get; set; }

        public decimal LowestScore { get; set; }
        public string LowStr { get { return LowestScore.ToString("F") + "%"; } }

        public decimal AverageScore { get; set; }
        public string AvgStr { get { return AverageScore.ToString("F") + "%"; } }

        public decimal HighestScore { get; set; }
        public string HighStr { get { return HighestScore.ToString("F") + "%"; } }

        public decimal Improvement { get; set; }  // points improvement from last term
        public string ImpFull { get { return General.ImprovementDisplay(Improvement); } }

        public byte Position { get; set; }

        public byte SubjectCount { get; set; }

        public string BestSubjectName { get; set; }

        public string Button
        {
            get
            {
                return "<button class='btn btn-default btn-xs pDTBtn' data-id=" + StudentID.ToString() + " data-cid=" + ClassID.ToString() + " title='print'><i class='fa fa-print'></i></button>";
            }
        }
    }


    public class OtherExamsPageViewModel : BasePageViewModel
    {
        public int TermID { get; set; }

        public string TermName { get; set; }

        public List<OtherExamViewModel> Exams { get; set; }
    }

    public class OtherExamViewModel
    {
        public int OtherExamID { get; set; }

        public string Name { get; set; }

        public string ClassName { get; set; }

        public byte SubjectCount { get; set; }

        public byte ResultCount { get; set; }

        public decimal LowestAverage { get; set; }
        public string LowAvgStr { get { return LowestAverage.ToString("F"); } }

        public decimal MeanAverage { get; set; }
        public string MeanAvgStr { get { return MeanAverage.ToString("F"); } }

        public decimal BestAverage { get; set; }
        public string BestAvgStr { get { return BestAverage.ToString("F"); } }

        public string BestStudentName { get; set; }

        public string BestSubjectName { get; set; }
    }

    public class StartOtherExam
    {
        [Display(Name = "Exam")]
        public int? TypeID { get; set; }
        public List<System.Web.Mvc.SelectListItem> ExamList { get; set; }

        [Display(Name = "Class")]
        public int? ClassID { get; set; }
        public List<System.Web.Mvc.SelectListItem> ClassList { get; set; }

    }

    public class OtherExamCreateViewModel
    {
        public int TypeID { get; set; }

        public int ClassID { get; set; }

        public string ExamName { get; set; }

        public string ClassName { get; set; }

        public List<SubjectMiniModel> Subjects { get; set; }
    }

    public class OtherExamAnalysisModel : OtherExamCreateViewModel
    {
        public int OtherExamID { get; set; }
    }

    public class ScoreEntryDataModel
    {
        public int StudentID { get; set; }
        public int SubjectID { get; set; }

        public decimal? CAScore { get; set; }
        public decimal? ExamScore { get; set; }

        public decimal? Total { get { return (CAScore.HasValue ? CAScore.Value : 0) + ExamScore; } }
        public int? GradeID { get; set; }
    }

    public class OtherExamReportModel
    {
        public int ClassID { get; set; }
        public int StudentID { get; set; }
        public int StudentClassID { get; set; }
        public string ChartID { get { return "scoreChart" + StudentID.ToString(); } }
        public string GradeChartID { get { return "gradeChart" + StudentID.ToString(); } }

        public string ExamName { get; set; }

        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string DisplayName { get { return Surname.ToUpper() + " " + FirstName; } }

        public string ClassName { get; set; }

        public bool ShowCode { get { return (Surname.Length * 1.5) + (FirstName + ClassName).Length < 35; } }

        public string TermName { get; set; }
        public byte TermNumber { get; set; }
        public DateTime NextResumptionDate { get; set; }
        public string NRDateStr { get { return General.FullDateString(NextResumptionDate); } }

        public bool ShowSummaryGrade { get; set; }
        public bool ShowPosition { get; set; }

        public byte RedLine { get; set; }

        public int SchoolID { get; set; }
        public string SchoolName { get; set; }
        public byte SchoolTypeID { get; set; }
        public string PrincipalCommentText { get { return SchoolTypeID == (byte)SchoolType.NurseryPrimary ? "Head Teacher's Comment:" : "Principal's Comment:"; } }

        public string StudentCode { get; set; }
        public string Code { get { return string.IsNullOrEmpty(StudentCode) ? "ISN" + StudentID.ToString().PadLeft(4, '0') : StudentCode; } }

        public string GuidString { get; set; }
        public string PhotoSrc { get; set; }

        public string LogoGuidString { get; set; }
        public string LogoSrc { get; set; }

        public decimal LowestScore { get; set; }
        public string LowStr { get { return LowestScore.ToString("F") + "%"; } }

        public decimal AverageScore { get; set; }
        public string AvgStr { get { return AverageScore.ToString("F") + "%"; } }

        public decimal HighestScore { get; set; }
        public string HighStr { get { return HighestScore.ToString("F") + "%"; } }

        public byte Position { get; set; }
        public int StudentCount { get; set; }
        public string PosStr
        {
            get
            {
                var postfix = "th";
                var rem = Position % 10;
                if (rem == 1) postfix = "st";
                else if (rem == 2) postfix = "nd";
                else if (rem == 3) postfix = "rd";

                return Position.ToString() + postfix + " of " + StudentCount;
            }
        }

        public string BestSubjectName { get; set; }

        public string TeacherComment { get; set; }

        public string PerformanceComment { get; set; }

        public List<GradeViewModel> Grades { get; set; }

        public List<GradeViewModel> SGrades
        {
            get
            {
                var AGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.A);
                var BGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.B);
                var CGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.C);
                var DGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.D);
                var EGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.E);
                var FGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.F);

                var list = new List<GradeViewModel>();

                if (AGrades.Any()) list.Add(new GradeViewModel { LowerBound = AGrades.Min(l => l.LowerBound), UpperBound = AGrades.Max(l => l.UpperBound), Name = "A" });
                if (BGrades.Any()) list.Add(new GradeViewModel { LowerBound = BGrades.Min(l => l.LowerBound), UpperBound = BGrades.Max(l => l.UpperBound), Name = "B" });
                if (CGrades.Any()) list.Add(new GradeViewModel { LowerBound = CGrades.Min(l => l.LowerBound), UpperBound = CGrades.Max(l => l.UpperBound), Name = "C" });
                if (DGrades.Any()) list.Add(new GradeViewModel { LowerBound = DGrades.Min(l => l.LowerBound), UpperBound = DGrades.Max(l => l.UpperBound), Name = "D" });
                if (EGrades.Any()) list.Add(new GradeViewModel { LowerBound = EGrades.Min(l => l.LowerBound), UpperBound = EGrades.Max(l => l.UpperBound), Name = "E" });
                if (FGrades.Any()) list.Add(new GradeViewModel { LowerBound = FGrades.Min(l => l.LowerBound), UpperBound = FGrades.Max(l => l.UpperBound), Name = "F" });

                return list;
            }
        }

        public List<GradeViewModel> GradeRows { get { return ShowSummaryGrade ? SGrades : Grades; } }

        string GradeLegend(IEnumerable<GradeViewModel> Grades)
        {
            if (!Grades.Any())
                return "";

            var low = Grades.Min(l => l.LowerBound);
            var high = Grades.Max(l => l.UpperBound);
            return "[" + low + "-" + high + "] " + (ShowSummaryGrade ? "" : "(" + Grades.Select(l => l.Name).Aggregate((a, b) => (a + "," + b)) + ")");
        }

        public string LegendA { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.A)); } }
        public string LegendB { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.B)); } }
        public string LegendC { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.C)); } }
        public string LegendD { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.D)); } }
        public string LegendE { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.E)); } }
        public string LegendF { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.F)); } }

        public bool HasLegendA { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.A); } }
        public bool HasLegendB { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.B); } }
        public bool HasLegendC { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.C); } }
        public bool HasLegendD { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.D); } }
        public bool HasLegendE { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.E); } }
        public bool HasLegendF { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.F); } }

        public int GradeCountA { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.A); } }
        public int GradeCountB { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.B); } }
        public int GradeCountC { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.C); } }
        public int GradeCountD { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.D); } }
        public int GradeCountE { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.E); } }
        public int GradeCountF { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.F); } }

        public string GradeCountAStr { get { return GradeCountA == 0 ? "-" : GradeCountA.ToString(); } }
        public string GradeCountBStr { get { return GradeCountB == 0 ? "-" : GradeCountB.ToString(); } }
        public string GradeCountCStr { get { return GradeCountC == 0 ? "-" : GradeCountC.ToString(); } }
        public string GradeCountDStr { get { return GradeCountD == 0 ? "-" : GradeCountD.ToString(); } }
        public string GradeCountEStr { get { return GradeCountE == 0 ? "-" : GradeCountE.ToString(); } }
        public string GradeCountFStr { get { return GradeCountF == 0 ? "-" : GradeCountF.ToString(); } }

        public List<ExamScoreReportModel> Scores { get; set; }

        public string SubjectNames { get { return General.Json(Scores.Select(l => l.ResultName)); } }
        public string FailureLine { get { return General.Json(Scores.Select(l => RedLine)); } }
        public string ScoresJson { get; set; }
        public string LowScoresJson { get; set; }
        public string AvgScoresJson { get; set; }
        public string MaxScoresJson { get; set; }

        public int ScoreCount { get { return Scores.Count; } }
    }

    public class OtherExamReportPageViewModel : BasePageViewModel
    {
        public string Name { get; set; }

        public List<OtherExamReportModel> Reports { get; set; }
    }

    public class ExamScoreReportModel : ScoreEntryGradeModel
    {
        public string ResultName { get; set; }

        public decimal ClassAverage { get; set; }
        public string ClassAvgStr { get { return ClassAverage.ToString("F"); } }

        public decimal? Total { get; set; }
        public string TotalStr { get { return Total.Value.ToString("F"); } }
    }

    public class OtherExamResultPageViewModel : BasePageViewModel
    {
        public int? ClassFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> ClassList { get; set; }

        public int? TypeFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> TypeList { get; set; }

        public int? LevelFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> LevelList { get; set; }

        public SexValues? SexFilter { get; set; }

        public string SchoolName { get; set; }
        public int TermID { get; set; }
    }

    public class OtherExamResultFilterModel
    {
        public int TermID { get; set; }

        public int? ClassID { get; set; }

        public int? LevelID { get; set; }

        public int? TypeID { get; set; }

        public int? SexID { get; set; }
    }

    public class OtherExamResultViewModel
    {
        public int StudentID { get; set; }

        public int OtherExamID { get; set; }

        public string DisplayName { get; set; }

        public string ClassName { get; set; }

        public string ExamName { get; set; }

        public decimal LowestScore { get; set; }
        public string LowStr { get { return LowestScore.ToString("F") + "%"; } }

        public decimal AverageScore { get; set; }
        public string AvgStr { get { return AverageScore.ToString("F") + "%"; } }

        public decimal HighestScore { get; set; }
        public string HighStr { get { return HighestScore.ToString("F") + "%"; } }

        public byte Position { get; set; }

        public byte SubjectCount { get; set; }

        public string BestSubjectName { get; set; }

        public string Button
        {
            get
            {
                return "<button class='btn btn-default btn-xs pDTBtn' data-id=" + StudentID.ToString() + " data-cid=" + OtherExamID.ToString() + " title='print'><i class='fa fa-print'></i></button>";
            }
        }
    }


    public class SubjectReportFilterModel
    {
        public int SchoolID { get; set; }

        public int? TermID { get; set; }

        public int? ClassID { get; set; }

        public int? LevelID { get; set; }

        public int? CategoryID { get; set; }
    }

    public class SubjectReportViewModel
    {
        public int SubjectID { get; set; }

        public short SchoolYear { get; set; }
        public byte TermNumber { get; set; }

        public string TermStr { get { return SchoolYear.ToString() + "/" + (SchoolYear + 1).ToString() + " - " + TermNumber.ToString(); } }

        public string ClassName { get; set; }

        public string Name { get; set; }

        public string ResultName { get; set; }

        public byte CategoryID { get; set; }
        public string CategoryName { get { return Eval.GetDisplayName(typeof(SubjectCategory), CategoryID); } }

        public DateTime TimeVerified { get; set; }
        public string TimeVerifiedN { get { return General.ShortTimeString(TimeVerified); } }

        public List<ScoreEntryModel> Scores { get; set; }

        public int ScoreCount { get { return Scores.Count; } }

        public string LowStr { get { return Scores.Any(l => l.Total.HasValue) ? Scores.Min(l => l.Total).Value.ToString("F") : "-"; } }

        public string AvgStr { get { return Scores.Any(l => l.Total.HasValue) ? Scores.Average(l => l.Total).Value.ToString("F") : "-"; } }

        public string HighStr { get { return Scores.Any(l => l.Total.HasValue) ? Scores.Max(l => l.Total).Value.ToString("F") : "-"; } }


        public string Button
        {
            get
            {
                return "<button class='btn btn-default btn-xs pDTBtn' data-id=" + SubjectID.ToString() + " title='print'><i class='fa fa-print'></i></button>";
            }
        }
    }

    public class SubjectReportPageModel : BasePageViewModel
    {
        public int? ClassFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> ClassList { get; set; }

        public int? TermFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> TermList { get; set; }

        public int? LevelFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> LevelList { get; set; }

        public SubjectCategory? CategoryFilter { get; set; }

        public int SchoolID { get; set; }

        public string SchoolName { get; set; }
    }

    public class SubjectReportModel
    {
        public int SubjectID { get; set; }
        public string ChartID { get { return "gradeChart" + SubjectID.ToString(); } }

        public string ClassName { get; set; }

        public string TermName { get; set; }

        public int SchoolID { get; set; }

        public string SchoolName { get; set; }

        public string Name { get; set; }

        public string ResultName { get; set; }

        public string TeacherName { get; set; }

        public byte CategoryID { get; set; }
        public string CategoryName { get { return Eval.GetDisplayName(typeof(SubjectCategory), CategoryID); } }

        public byte CAWeight { get; set; }
        public byte ExamWeight { get; set; }

        public bool NoCA { get; set; }

        public byte RedLine { get; set; }

        public byte CARedLine { get { return (byte)(RedLine * CAWeight * 0.01); } }
        public byte ExamRedLine { get { return (byte)(RedLine * ExamWeight * 0.01); } }

        public List<SubjectScoreFullModel> Scores { get; set; }

        public List<SubjectScoreFullModel> DisplayScores { get { return Scores.Where(l => l.ExamScore.HasValue).ToList(); } }

        public int ScoreCount { get { return Scores.Count(l => l.ExamScore.HasValue); } }

        public string LowStr { get { return Scores.Any(l => l.Total.HasValue) ? Scores.Min(l => l.Total).Value.ToString("F") : "-"; } }

        public string AvgStr { get { return Scores.Any(l => l.Total.HasValue) ? Scores.Average(l => l.Total).Value.ToString("F") : "-"; } }

        public string HighStr { get { return Scores.Any(l => l.Total.HasValue) ? Scores.Max(l => l.Total).Value.ToString("F") : "-"; } }

        public string BestStudentName { get { return Scores.Any(l => l.Total.HasValue) ? Scores.OrderByDescending(l => l.Total).FirstOrDefault().DisplayName : "-"; } }

        public List<GradeViewModel> Grades { get; set; }

        public bool ShowSummaryGrade { get; set; }

        public List<GradeViewModel> SGrades
        {
            get
            {
                var AGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.A);
                var BGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.B);
                var CGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.C);
                var DGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.D);
                var EGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.E);
                var FGrades = Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.F);

                var list = new List<GradeViewModel>();

                if (AGrades.Any()) list.Add(new GradeViewModel { LowerBound = AGrades.Min(l => l.LowerBound), UpperBound = AGrades.Max(l => l.UpperBound), Name = "A" });
                if (BGrades.Any()) list.Add(new GradeViewModel { LowerBound = BGrades.Min(l => l.LowerBound), UpperBound = BGrades.Max(l => l.UpperBound), Name = "B" });
                if (CGrades.Any()) list.Add(new GradeViewModel { LowerBound = CGrades.Min(l => l.LowerBound), UpperBound = CGrades.Max(l => l.UpperBound), Name = "C" });
                if (DGrades.Any()) list.Add(new GradeViewModel { LowerBound = DGrades.Min(l => l.LowerBound), UpperBound = DGrades.Max(l => l.UpperBound), Name = "D" });
                if (EGrades.Any()) list.Add(new GradeViewModel { LowerBound = EGrades.Min(l => l.LowerBound), UpperBound = EGrades.Max(l => l.UpperBound), Name = "E" });
                if (FGrades.Any()) list.Add(new GradeViewModel { LowerBound = FGrades.Min(l => l.LowerBound), UpperBound = FGrades.Max(l => l.UpperBound), Name = "F" });

                return list;
            }
        }

        public List<GradeViewModel> GradeRows { get { return ShowSummaryGrade ? SGrades : Grades; } }

        string GradeLegend(IEnumerable<GradeViewModel> Grades)
        {
            if (!Grades.Any())
                return "";

            var low = Grades.Min(l => l.LowerBound);
            var high = Grades.Max(l => l.UpperBound);
            return "[" + low + "-" + high + "] " + (ShowSummaryGrade ? "" : "(" + Grades.Select(l => l.Name).Aggregate((a, b) => (a + "," + b)) + ")");
        }

        public string LegendA { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.A)); } }
        public string LegendB { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.B)); } }
        public string LegendC { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.C)); } }
        public string LegendD { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.D)); } }
        public string LegendE { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.E)); } }
        public string LegendF { get { return GradeLegend(Grades.Where(l => l.SummaryGradeID == (byte)SummaryGrade.F)); } }

        public bool HasLegendA { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.A); } }
        public bool HasLegendB { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.B); } }
        public bool HasLegendC { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.C); } }
        public bool HasLegendD { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.D); } }
        public bool HasLegendE { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.E); } }
        public bool HasLegendF { get { return Grades.Any(l => l.SummaryGradeID == (byte)SummaryGrade.F); } }

        public int GradeCountA { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.A); } }
        public int GradeCountB { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.B); } }
        public int GradeCountC { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.C); } }
        public int GradeCountD { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.D); } }
        public int GradeCountE { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.E); } }
        public int GradeCountF { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.F); } }

        public string GradeCountAStr { get { return GradeCountA == 0 ? "-" : GradeCountA.ToString(); } }
        public string GradeCountBStr { get { return GradeCountB == 0 ? "-" : GradeCountB.ToString(); } }
        public string GradeCountCStr { get { return GradeCountC == 0 ? "-" : GradeCountC.ToString(); } }
        public string GradeCountDStr { get { return GradeCountD == 0 ? "-" : GradeCountD.ToString(); } }
        public string GradeCountEStr { get { return GradeCountE == 0 ? "-" : GradeCountE.ToString(); } }
        public string GradeCountFStr { get { return GradeCountF == 0 ? "-" : GradeCountF.ToString(); } }
    }

    public class SubjectStudentModel : StudentMiniModel
    {
        public bool IsMale { get; set; }

        public string StudentCode { get; set; }
    }

    public class SubjectScoreFullModel : ScoreEntryFullModel
    {
        public string FirstName { get; set; }

        public string Surname { get; set; }

        public string DisplayName { get { return Surname.ToUpper() + " " + FirstName; } }

        public bool IsMale { get; set; }
        public string Sex { get { return IsMale ? "Male" : "Female"; } }

        public string StudentCode { get; set; }
        public string Code { get { return string.IsNullOrEmpty(StudentCode) ? "ISN" + StudentID.ToString().PadLeft(6, '0') : StudentCode; } }

        public bool NoCA { get; set; }
        public byte CAWeight { get; set; }
        public byte ExamWeight { get; set; }

        public decimal? DisplayCA { get { return NoCA ? (ExamScore * CAWeight * (decimal)0.01) : CAScore; } }
        public decimal? DisplayExam { get { return NoCA ? (ExamScore * ExamWeight * (decimal)0.01) : ExamScore; } }

        public string CAStr { get { return DisplayCA.HasValue ? DisplayCA.Value.ToString("F") : "-"; } }
        public string ExamStr { get { return ExamScore.HasValue ? DisplayExam.Value.ToString("F") : "-"; } }

        public string TotalStr { get { return Total.HasValue ? Total.Value.ToString("F") : "-"; ; } }

    }

    public class SubjectResultPageViewModel : BasePageViewModel
    {
        public string Name { get; set; }

        public List<SubjectReportModel> Reports { get; set; }
    }


    public class TermReportPageViewModel : BasePageViewModel
    {
        public int TermID { get; set; }

        public string TermName { get; set; }

        public int ClassCount { get; set; }
        public int StudentCount { get; set; }

        public int DefSubjectCount { get; set; }

        public DateTime NextResumptionDate { get; set; }
        public string NRDateStr { get { return General.FullDateString(NextResumptionDate); } }

        public DateTime AnalysisTime { get; set; }

        public bool ShouldAnalyze { get; set; }

        //result fields...
        public short ResultCount { get; set; }
        public short SubjectCount { get; set; }

        public decimal LowestAverage { get; set; }
        public decimal MeanAverage { get; set; }
        public decimal BestAverage { get; set; }

        public string LowAvgStr { get { return LowestAverage.ToString("F"); } }
        public string MeanAvgStr { get { return MeanAverage.ToString("F"); } }
        public string BestAvgStr { get { return BestAverage.ToString("F"); } }

        public int BestStudentID { get; set; }
        public int BestStudentClassID { get; set; }
        public int BestSubjectID { get; set; }

        public string BestStudentName { get; set; }
        public string BestSubjectName { get; set; }
        public string BestClassName { get; set; }

        public decimal BestClassAverage { get; set; }
        public string BestClassAvgStr { get { return BestClassAverage.ToString("F") + "%"; } }
        public int BestClassSize { get; set; }

        public bool HasResults { get { return ResultCount > 0; } }

        public List<SubjectCategoryStatsViewModel> CategoryStats { get; set; }
    }

    public class SubjectReportMiniModel
    {
        public int SubjectID { get; set; }

        public int TemplateID { get; set; } 

        public byte ClassLevelID { get; set; }

        public string Name { get; set; }

        public byte CategoryID { get; set; }
    }

    public class StatsViewModel
    {
        public decimal LowestScore { get; set; }
        public decimal AverageScore { get; set; }
        public decimal HighestScore { get; set; }

        public string LowStr { get { return LowestScore.ToString("F") + "%"; } }
        public string AvgStr { get { return AverageScore.ToString("F") + "%"; } }
        public string HighStr { get { return HighestScore.ToString("F") + "%"; } }

        public short ResultCount { get; set; }
        public byte SubjectCount { get; set; }

        public string BestStudentName { get; set; }
    }

    public class SubjectCategoryStatsViewModel : StatsViewModel
    {
        public byte CategoryID { get; set; }
        public string CategoryName { get { return Eval.GetDisplayName(typeof(SubjectCategory), CategoryID); } }

        public decimal BestStudentAverage { get; set; }
        public decimal BestSubjectAverage { get; set; }

        public string BestAvgStr { get { return BestStudentAverage.ToString("F"); } }
        public string BestSubAvgStr { get { return BestSubjectAverage.ToString("F"); } }

        public string BestSubjectName { get; set; }

    }

    public class TermPerformanceModel
    {
        public int TermID { get; set; }
        public int SchoolID { get; set; }

        public string TermName { get; set; }
        public string SchoolName { get; set; }

        public short ResultCount { get; set; }
        public short SubjectCount { get; set; }

        public decimal LowestAverage { get; set; }
        public decimal MeanAverage { get; set; }
        public decimal BestAverage { get; set; }

        public string LowAvgStr { get { return LowestAverage.ToString("F"); } }
        public string MeanAvgStr { get { return MeanAverage.ToString("F"); } }
        public string BestAvgStr { get { return BestAverage.ToString("F"); } }

        public string BestStudentName { get; set; }

        public string BestSubjectName { get; set; }

        public string BestClassName { get; set; }

        public decimal BestClassAverage { get; set; }
        public string BestClassAvgStr { get { return BestClassAverage.ToString("F") + "%"; } }
        public int BestClassSize { get; set; }

        public List<SubjectCategoryStatsViewModel> CategoryStats { get; set; }

        public List<ClassReportModel> Classes { get; set; }
        public int ClassCount { get { return Classes == null ? 0 : Classes.Count(); } }

        public string ClassNames { get { return General.Json(Classes.Select(l => l.Name)); } }
        public string LowestAverages { get { return General.Json(Classes.Select(l => decimal.Round(l.LowestAverage))); } }
        public string MeanAverages { get { return General.Json(Classes.Select(l => decimal.Round(l.MeanAverage))); } }
        public string BestAverages { get { return General.Json(Classes.Select(l => decimal.Round(l.BestAverage))); } }

        public string CategoryNames { get { return General.Json(CategoryStats.Select(l => l.CategoryName)); } }
        public string CatLowScores { get { return General.Json(CategoryStats.Select(l => decimal.Round(l.LowestScore))); } }
        public string CatAvgScores { get { return General.Json(CategoryStats.Select(l => decimal.Round(l.AverageScore))); } }
        public string CatHighScores { get { return General.Json(CategoryStats.Select(l => decimal.Round(l.HighestScore))); } }
        public string CatBestAverages { get { return General.Json(CategoryStats.Select(l => decimal.Round(l.BestStudentAverage))); } }
    }

    public class TermCategoryStatsModel : SubjectCategoryStatsViewModel
    {
        public int TermID { get; set; }

        public string TermName { get; set; }

        public short ACount { get; set; }
        public short BCount { get; set; }
        public short CCount { get; set; }
        public short DCount { get; set; }
        public short ECount { get; set; }
        public short FCount { get; set; }
    }

    public class LevelCategoryStatsModel : SubjectCategoryStatsViewModel
    {
        public byte LevelID { get; set; }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), LevelID); } }
    }

    public class CategoryPerformanceModel
    {
        public byte CategoryID { get; set; }
        public string CategoryName { get { return Eval.GetDisplayName(typeof(SubjectCategory), CategoryID); } }

        public string FirstTermName { get; set; }
        public string CurrentTermName { get; set; }
        public string TermRange { get { return FirstTermName + " - " + CurrentTermName; } }

        public int ResultCount { get; set; }
        public int SubjectCount { get; set; }

        public GradePieLegend PieLegend { get; set; }

        public decimal LowestAverage { get; set; }
        public decimal MeanAverage { get; set; }
        public decimal BestAverage { get; set; }
        public decimal CurrentAverage { get; set; }

        public string LowAvgStr { get { return LowestAverage.ToString("F"); } }
        public string MeanAvgStr { get { return MeanAverage.ToString("F"); } }
        public string BestAvgStr { get { return BestAverage.ToString("F"); } }
        public string CurrentAvgStr { get { return CurrentAverage.ToString("F"); } }

        public List<TermCategoryStatsModel> TermStats { get; set; }

        public short ACount { get { return (short)TermStats.Sum(l => l.ACount); } }
        public short BCount { get { return (short)TermStats.Sum(l => l.BCount); } }
        public short CCount { get { return (short)TermStats.Sum(l => l.CCount); } }
        public short DCount { get { return (short)TermStats.Sum(l => l.DCount); } }
        public short ECount { get { return (short)TermStats.Sum(l => l.ECount); } }
        public short FCount { get { return (short)TermStats.Sum(l => l.FCount); } }

        public string TermNames { get { return General.Json(TermStats.Select(l => l.TermName)); } }
        public string CatLowScores { get { return General.Json(TermStats.Select(l => decimal.Round(l.LowestScore))); } }
        public string CatAvgScores { get { return General.Json(TermStats.Select(l => decimal.Round(l.AverageScore))); } }
        public string CatHighScores { get { return General.Json(TermStats.Select(l => decimal.Round(l.HighestScore))); } }
    }

    public class TermSubjectCategoriesModel
    {
        public string TermName { get; set; }
        public string SchoolName { get; set; }

        public GradePieLegend PieLegend { get; set; }

        public List<TermCategoryPerformanceModel> Categories { get; set; }
    }

    public class TermCategoryPerformanceModel : StatsViewModel
    {
        public byte CategoryID { get; set; }
        public string CategoryName { get { return Eval.GetDisplayName(typeof(SubjectCategory), CategoryID); } }

        public string PieChartID { get { return "pieChart" + CategoryID.ToString(); } }
        public string BarChartID { get { return "barChart" + CategoryID.ToString(); } }

        public string BestSubjectName { get; set; }

        public short ACount { get; set; }
        public short BCount { get; set; }
        public short CCount { get; set; }
        public short DCount { get; set; }
        public short ECount { get; set; }
        public short FCount { get; set; }

        public List<LevelCategoryStatsModel> LevelStats { get; set; }

        public string LevelNames { get { return General.Json(LevelStats.Select(l => l.LevelName)); } }
        public string LowScores { get { return General.Json(LevelStats.Select(l => decimal.Round(l.LowestScore))); } }
        public string AvgScores { get { return General.Json(LevelStats.Select(l => decimal.Round(l.AverageScore))); } }
        public string HighScores { get { return General.Json(LevelStats.Select(l => decimal.Round(l.HighestScore))); } }
    }

    public class SubjectStatsModel : StatsViewModel
    {
        public string Name { get; set; }

        public string ResultName { get; set; }

        public byte LevelID { get; set; }
    }

    public class LevelSubjectStatsModel
    {
        public byte LevelID { get; set; }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), LevelID); } }
        public string BarChartID { get { return "barChart" + LevelID.ToString(); } }

        public List<SubjectStatsModel> Stats { get; set; }

        public short ResultCount { get { return (short)Stats.Sum(l => l.ResultCount); } }
        public byte SubjectCount { get { return (byte)Stats.Sum(l => l.SubjectCount); } }

        public decimal LowestAverage { get { return Stats.Min(l => l.AverageScore); } }
        public decimal MeanAverage { get { return Stats.Average(l => l.AverageScore); } }
        public decimal BestAverage { get { return Stats.Max(l => l.AverageScore); } }

        public string LowAvgStr { get { return LowestAverage.ToString("F"); } }
        public string MeanAvgStr { get { return MeanAverage.ToString("F"); } }
        public string BestAvgStr { get { return BestAverage.ToString("F"); } }

        public string BestSubjectName { get { return Stats.OrderByDescending(l => l.AverageScore).Select(l => l.ResultName).First(); } }

        public string Subjects { get { return General.Json(Stats.Select(l => l.ResultName)); } }
        public string LowScores { get { return General.Json(Stats.Select(l => decimal.Round(l.LowestScore))); } }
        public string AvgScores { get { return General.Json(Stats.Select(l => decimal.Round(l.AverageScore))); } }
        public string HighScores { get { return General.Json(Stats.Select(l => decimal.Round(l.HighestScore))); } }
    }

    public class TermSubjectPerformanceModel
    {
        public string TermName { get; set; }
        public string SchoolName { get; set; }

        public List<LevelSubjectStatsModel> LevelStats { get; set; }
    }

    public class TermResultModel
    {
        public string TermName { get; set; }

        public short SchoolYear { get; set; }
        public byte TermNumber { get; set; }

        public int ClassCount { get; set; }
        public short ResultCount { get; set; }
        public short SubjectCount { get; set; }

        public decimal LowestAverage { get; set; }
        public decimal MeanAverage { get; set; }
        public decimal BestAverage { get; set; }

        public string LowAvgStr { get { return LowestAverage.ToString("F"); } }
        public string MeanAvgStr { get { return MeanAverage.ToString("F"); } }
        public string BestAvgStr { get { return BestAverage.ToString("F"); } }

        public string BestStudentName { get; set; }

        public string BestSubjectName { get; set; }

        public string BestClassName { get; set; }
    }

    public class AllTermsReportModel
    {
        public List<TermResultModel> TermStats { get; set; }

        public string FirstTermName { get { return TermStats.OrderBy(l => l.SchoolYear).ThenBy(l => l.TermNumber).Select(l => l.TermName).First(); } }
        public string CurrentTermName { get { return TermStats.OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).Select(l => l.TermName).First(); } }
        public string TermRange { get { return FirstTermName + " - " + CurrentTermName; } }

        public byte TermCount { get { return (byte)TermStats.Count(); } }
        public int ResultCount { get { return TermStats.Sum(l => l.ResultCount); } }

        public decimal CurrentAverage { get { return TermStats.OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).Select(l => l.MeanAverage).First(); } }
        public decimal LowestAverage { get { return TermStats.Min(l => l.MeanAverage); } }
        public decimal MeanAverage { get { return TermStats.Average(l => l.MeanAverage); } }
        public decimal BestAverage { get { return TermStats.Max(l => l.MeanAverage); } }

        public string CurAvgStr { get { return CurrentAverage.ToString("F"); } }
        public string LowAvgStr { get { return LowestAverage.ToString("F"); } }
        public string MeanAvgStr { get { return MeanAverage.ToString("F"); } }
        public string BestAvgStr { get { return BestAverage.ToString("F"); } }

        public string TermNames { get { return General.Json(TermStats.Select(l => l.TermName)); } }
        public string LowScores { get { return General.Json(TermStats.Select(l => decimal.Round(l.LowestAverage))); } }
        public string AvgScores { get { return General.Json(TermStats.Select(l => decimal.Round(l.MeanAverage))); } }
        public string HighScores { get { return General.Json(TermStats.Select(l => decimal.Round(l.BestAverage))); } }
    }

    public class SubjectStatsRowModel : SubjectStatsModel
    {
        public int TemplateID { get; set; }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), LevelID); } }

        public byte Order { get; set; }
    }

    public class LevelModel
    {
        public byte LevelID { get; set; }

        public string LevelName { get; set; }
    }

    public class TemplateReportPageViewModel : BasePageViewModel
    {
        public string SchoolName { get; set; }

        public List<LevelModel> Levels { get; set; }

        public List<SubjectStatsRowModel> Subjects { get; set; }
    }

    public class TermSubjectStatsModel : StatsViewModel
    {
        public string TermName { get; set; }

        public short SchoolYear { get; set; }
        public byte TermNumber { get; set; }
    }

    public class TemplateReportModel
    {
        public int TemplateID { get; set; }
        public string Name { get; set; }
        public string ResultName { get; set; }

        public byte CategoryID { get; set; }
        public string CategoryName { get { return Eval.GetDisplayName(typeof(SubjectCategory), CategoryID); } }

        public byte ClassLevelID { get; set; }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public string TemplateName { get { return LevelName + " " + Name; } }
        public string BarChartID { get { return "barChart" + TemplateID.ToString(); } }

        public List<TermSubjectStatsModel> TermStats { get; set; }

        public string FirstTermName { get { return TermStats.OrderBy(l => l.SchoolYear).ThenBy(l => l.TermNumber).Select(l => l.TermName).FirstOrDefault(); } }
        public string CurrentTermName { get { return TermStats.OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).Select(l => l.TermName).FirstOrDefault(); } }
        public string TermRange { get { return FirstTermName + " - " + CurrentTermName; } }

        public byte TermCount { get { return (byte)TermStats.Count(); } }
        public int ResultCount { get { return TermStats.Sum(l => l.ResultCount); } }

        public decimal CurrentAverage { get { return TermStats.OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).Select(l => l.AverageScore).FirstOrDefault(); } }
        public decimal LowestAverage { get { return TermStats.Any() ? TermStats.Min(l => l.AverageScore) : 0; } }
        public decimal MeanAverage { get { return TermStats.Any() ? TermStats.Average(l => l.AverageScore) : 0; } }
        public decimal BestAverage { get { return TermStats.Any() ? TermStats.Max(l => l.AverageScore) : 0; } }

        public string CurAvgStr { get { return CurrentAverage.ToString("F"); } }
        public string LowAvgStr { get { return LowestAverage.ToString("F"); } }
        public string MeanAvgStr { get { return MeanAverage.ToString("F"); } }
        public string BestAvgStr { get { return BestAverage.ToString("F"); } }

        public string TermNames { get { return General.Json(TermStats.Select(l => l.TermName)); } }
        public string LowScores { get { return General.Json(TermStats.Select(l => decimal.Round(l.LowestScore))); } }
        public string AvgScores { get { return General.Json(TermStats.Select(l => decimal.Round(l.AverageScore))); } }
        public string HighScores { get { return General.Json(TermStats.Select(l => decimal.Round(l.HighestScore))); } }
    }

    public class TemplateReportsPrintModel
    {
        public byte LevelID { get; set; }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), LevelID); } }

        public string SchoolName { get; set; }

        public List<TemplateReportModel> Reports { get; set; }
    }



    public class SchoolReportPageViewModel : BasePageViewModel
    {
        public string SchoolName { get; set; }

        public byte TypeID { get; set; }
        public string TypeName { get { return Eval.GetDisplayName(typeof(SchoolType), TypeID); } }

        public List<ClassTypeStatsModel> Stats { get; set; }

        public string MeanAvgStr { get { return Stats.Any() ? Stats.Average(l => l.MeanAverage).ToString("F") : ""; } }

        public string ResultCount { get { return Stats.Any() ? Stats.Sum(l => l.ResultCount).ToString() : ""; } }

        public bool ShouldAnalyze { get; set; }

        public bool HasResults { get { return Stats.Any(); } }
    }

    public class ClassReportMaxModel : ClassReportModel
    {
        public short SchoolYear { get; set; }

        public byte TermNumber { get; set; }

        public string ClassDesc { get { return SchoolYear.ToString() + "-" + TermNumber.ToString() + " " + Name; } }

        public string TermName { get; set; }

    }

    public class ClassReportMaxiModel : ClassReportMaxModel
    {
        public byte ClassLevelID { get; set; }

        public byte ClassTypeID { get; set; }
    }

    public class ClassTypeStatsModel
    {
        public byte ClassLevelID { get; set; }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public byte ClassTypeID { get; set; }
        public string TypeName { get { return Eval.GetDisplayName(typeof(ClassType), ClassTypeID); } }

        public short ClassCount { get; set; }
        public int ResultCount { get; set; }

        public decimal CurrentAverage { get; set; }
        public decimal LowestAverage { get; set; }
        public decimal MeanAverage { get; set; }
        public decimal BestAverage { get; set; }

        public string CurAvgStr { get { return CurrentAverage.ToString("F"); } }
        public string LowAvgStr { get { return LowestAverage.ToString("F"); } }
        public string MeanAvgStr { get { return MeanAverage.ToString("F"); } }
        public string BestAvgStr { get { return BestAverage.ToString("F"); } }
    }

    public class ClassTypeReportModel
    {
        public byte ClassLevelID { get; set; }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public byte ClassTypeID { get; set; }
        public string TypeName { get { return Eval.GetDisplayName(typeof(ClassType), ClassTypeID); } }

        public string ClassTypeName { get { return LevelName + (ClassTypeID == (byte)ClassType.General ? "" : " " + TypeName); } }
        public string BarChartID { get { return "barChart" + ClassLevelID.ToString() + "-" + ClassTypeID.ToString(); } }

        public List<ClassReportMaxModel> Classes { get; set; }

        public string FirstTermName { get { return Classes.OrderBy(l => l.SchoolYear).ThenBy(l => l.TermNumber).Select(l => l.TermName).FirstOrDefault(); } }
        public string CurrentTermName { get { return Classes.OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).Select(l => l.TermName).FirstOrDefault(); } }
        public string TermRange { get { return FirstTermName + " - " + CurrentTermName; } }

        public int ClassCount { get { return Classes.Count; } }
        public int ResultCount { get { return Classes.Sum(l => l.ResultCount); } }

        public string ClassNames { get { return General.Json(Classes.Select(l => l.ClassDesc)); } }
        public string LowestAverages { get { return General.Json(Classes.Select(l => decimal.Round(l.LowestAverage))); } }
        public string MeanAverages { get { return General.Json(Classes.Select(l => decimal.Round(l.MeanAverage))); } }
        public string BestAverages { get { return General.Json(Classes.Select(l => decimal.Round(l.BestAverage))); } }

        public decimal CurrentAverage { get { return Classes.OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).Select(l => l.MeanAverage).First(); } }
        public decimal LowestAverage { get { return Classes.Min(l => l.MeanAverage); } }
        public decimal MeanAverage { get { return Classes.Average(l => l.MeanAverage); } }
        public decimal BestAverage { get { return Classes.Max(l => l.MeanAverage); } }

        public string CurAvgStr { get { return CurrentAverage.ToString("F"); } }
        public string LowAvgStr { get { return LowestAverage.ToString("F"); } }
        public string MeanAvgStr { get { return MeanAverage.ToString("F"); } }
        public string BestAvgStr { get { return BestAverage.ToString("F"); } }
    }

    public class ClassTypeReportsPrintModel
    {
        public string SchoolName { get; set; }

        public List<ClassTypeReportModel> Reports { get; set; }
    }

    public class ClassTypeAnalysisModel
    {
        public byte ClassLevelID { get; set; }

        public byte ClassTypeID { get; set; }

        public byte SchoolYear { get; set; }

        public byte TermNumber { get; set; }

        public byte SubjectCount { get; set; }

        public byte ResultCount { get; set; }

        public decimal LowestAverage { get; set; }
        public decimal MeanAverage { get; set; }
        public decimal BestAverage { get; set; }
    }


    public class ResultLabelsPageViewModel : BasePageViewModel
    {
        public string TermName { get; set; }

        public List<SchoolTermViewModel> Terms { get; set; }
    }

    public class SchoolTermViewModel
    {
        public int TermID { get; set; }

        public string Name { get; set; }

        public string SchoolName { get; set; }

        public int ClassCount { get; set; }
        public int StudentCount { get; set; }
        public int SubjectCount { get; set; }
        public int ResultCount { get; set; }
    }

    public class ResultLabelRowModel
    {
        public ResultLabelModel Label1 { get; set; }
        public ResultLabelModel Label2 { get; set; }
        public ResultLabelModel Label3 { get; set; }
        public ResultLabelModel Label4 { get; set; }
        public ResultLabelModel Label5 { get; set; }
    }

    public class ResultLabelModel
    {
        public string TermName { get; set; }

        public string ClassName { get; set; }

        public string Surname { get; set; }

        public string SurnameU { get { return string.IsNullOrWhiteSpace(Surname) ? "" : Surname.ToUpper(); } }

        public string FirstName { get; set; }
    }





    public class TranscriptViewModel
    {
        public int StudentID { get; set; }

        public string FirstName { get; set; }

        public string OtherName { get; set; }

        public string Surname { get; set; }

        public string DisplayName { get { return Surname.ToUpper() + " " + FirstName; } }

        public bool IsMale { get; set; }
        public string Sex { get { return IsMale ? "Male" : "Female"; } }

        public string StudentCode { get; set; }
        public string Code { get { return string.IsNullOrEmpty(StudentCode) ? "ISN" + StudentID.ToString().PadLeft(6, '0') : StudentCode; } }

        public short? CYear { get; set; }

        public byte? CLevelID { get; set; }

        public List<StudentReportMiniModel> Results { get; set; }

        public int CResultCount { get { return Results != null && Results.Any() ? Results.Count() : 0; } }

        public decimal CResultAverage { get { return Results != null && Results.Any() ? Results.Average(l => l.AverageScore) : 0; } }

        public int CYResultCount { get { return Results != null && Results.Any() ? Results.Select(l => l.Year).Distinct().Count() : 0; } }

        public List<StudentPastResultViewModel> PastResults { get; set; }

        public short PMaxYear { get { return PastResults != null && PastResults.Any() ? PastResults.Select(l => l.Year).Max() : (short)0; } }

        public byte PMaxLevelID { get { return PastResults != null && PastResults.Any() ? PastResults.Where(l => l.Year == PMaxYear).Select(l => l.ClassLevelID).Max() : (byte)0; } }

        public int PResultCount { get { return PastResults != null && PastResults.Any() ? PastResults.Select(l => l.Year).Distinct().Count() : 0; } }

        public decimal PResultAverage { get { return PastResults != null && PastResults.Any() ? PastResults.Average(l => l.Average) ?? 0 : 0; } }


        public short MaxYear { get { return CYear.HasValue ? CYear.Value : PMaxYear; } }

        public byte MaxLevelID { get { return CLevelID.HasValue ? CLevelID.Value : PMaxLevelID; } }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), MaxLevelID); } }

        public int ResultCount { get { return CYResultCount + PResultCount; } } //number of sessions

        public decimal MeanAverage { get { return ResultCount > 0 ? (CResultAverage * CResultCount + PResultCount * PResultAverage) / (CResultCount + PResultCount) : 0; } }
        public string MeanAvgStr { get { return ResultCount > 0 ? MeanAverage.ToString("F") + "%" : "-"; } }

        public string Button { get { return "<button class='btn btn-default btn-xs pDTBtn' data-id=" + StudentID.ToString() + " title='print'><i class='fa fa-print'></i></button>"; } }
    }

    public class StudentReportMiniModel
    {
        public int StudentID { get; set; }

        public short Year { get; set; }

        public byte TermNumber { get; set; }

        public decimal AverageScore { get; set; }
        public string AvgStr { get { return AverageScore.ToString("F") + "%"; } }
    }

    public class TranscriptPageViewModel : BasePageViewModel
    {
        public int? YearFromFilter { get; set; }
        public int? YearToFilter { get; set; }

        public int? LevelFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> LevelList { get; set; }

        public SexValues? SexFilter { get; set; }

        public string SchoolName { get; set; }

    }

    public class TranscriptFilterViewModel
    {
        public int SchoolID { get; set; }

        public int? YearFrom { get; set; }

        public int? YearTo { get; set; }

        public int? LevelID { get; set; }

        public int? SexID { get; set; }
    }

    public class TranscriptScoreModel
    {
        public int TemplateID { get; set; }

        public byte TermNumber { get; set; }

        public decimal? CAScore { get; set; }
        public decimal? ExamScore { get; set; }

        public decimal? Total { get { return (CAScore ?? 0) + ExamScore; } }
    }

    public class TranscriptPrintModel
    {
        public int StudentID { get; set; }

        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string DisplayName { get { return Surname.ToUpper() + " " + FirstName; } }

        public int SchoolID { get; set; }
        public string SchoolName { get; set; }
        public byte SchoolTypeID { get; set; }
        public string PrincipalCommentText { get { return SchoolTypeID == (byte)SchoolType.NurseryPrimary ? "Head Teacher's Comment:" : "Principal's Comment:"; } }

        public string StudentCode { get; set; }
        public string Code { get { return string.IsNullOrEmpty(StudentCode) ? "ISN" + StudentID.ToString().PadLeft(4, '0') : StudentCode; } }

        public string GuidString { get; set; }
        public string PhotoSrc { get; set; }

        public string LogoGuidString { get; set; }
        public string LogoSrc { get; set; }

        public string PrincipalComment { get; set; }

        public List<TranscriptScoreModel> Scores { get; set; }

        public List<StudentPastResultViewModel> PastResults { get; set; }

        public List<GradeViewModel> Grades { get; set; }

        public List<StudentYearReportModel> Reports { get; set; }

        public short MaxYear { get { return Reports != null && Reports.Any() ? Reports.Select(l => l.Year).Max() : (short)0; } }
        public string MaxYearStr { get { return MaxYear.ToString() + "/" + (MaxYear + 1).ToString(); } }
    }

    public class StudentYearReportModel
    {
        public short Year { get; set; }
        public string YearStr { get { return Year.ToString() + "/" + (Year + 1).ToString(); } }

        public byte ClassLevelID { get; set; }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public List<StudentYearSubjectModel> Subjects { get; set; }

        // do this in the controller
        //public int GradeCountA { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.A); } }
        //public int GradeCountB { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.B); } }
        //public int GradeCountC { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.C); } }
        //public int GradeCountD { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.D); } }
        //public int GradeCountE { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.E); } }
        //public int GradeCountF { get { return Scores.Count(l => l.SummaryGradeID == (byte)SummaryGrade.F); } }

    }

    public class StudentYearSubjectModel
    {
        public string SubjectName { get; set; }

        public byte Order { get; set; }

        public byte CategoryID { get; set; }
        public string CategoryName { get { return Eval.GetDisplayName(typeof(SubjectCategory), CategoryID); } }

        public decimal? Term1Score { get; set; }
        public string Term1ScoreStr { get { return Term1Score.HasValue ? Term1Score.Value.ToString("F") : ""; } }

        public decimal? Term2Score { get; set; }
        public string Term2ScoreStr { get { return Term2Score.HasValue ? Term2Score.Value.ToString("F") : ""; } }

        public decimal? Term3Score { get; set; }
        public string Term3ScoreStr { get { return Term3Score.HasValue ? Term3Score.Value.ToString("F") : ""; } }

        public decimal? Average { get { return (new decimal?[] { Term1Score, Term2Score, Term3Score }).Average(); } }
        public string AverageStr { get { return (Term1Score.HasValue || Term2Score.HasValue || Term3Score.HasValue) ? Average.Value.ToString("F") : ""; } }
    }
}