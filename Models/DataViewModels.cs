using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IEGen.Models
{
    public class SubjectPageViewModel : BasePageViewModel
    {
        public int? ClassFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> ClassList { get; set; }

        public int? TeacherFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> TeacherList { get; set; }

        public int? LevelFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> LevelList { get; set; }

        public EntryStatus? StatusFilter { get; set; }

        public int TermID { get; set; }

        public string TermName { get; set; }
    }

    public class SubjectFilterViewModel
    {
        public int TermID { get; set; }

        public int? ClassID { get; set; }

        public int? LevelID { get; set; }

        public int? TeacherID { get; set; }

        public int? StatusID { get; set; }
    }

    public enum EntryStatus : byte
    {
        New = 1,
        Entered,
        Verified
    }

    public class SubjectDownloadModel : SubjectViewModel
    {
        public string ClassName { get; set; }

        public string EnteredByName { get; set; }
        public string VerifiedByName { get; set; }

        public DateTime TimeEntered { get; set; }
        public string TimeEnteredN { get { return General.ShortTimeString(TimeEntered); } }

        public DateTime TimeVerified { get; set; }
        public string TimeVerifiedN { get { return General.ShortTimeString(TimeVerified); } }

        public byte PercentCorrected { get; set; }
    }

    public class SubjectEntryViewModel
    {
        public int SubjectID { get; set; }

        public string TeacherName { get; set; }

        public string Name { get; set; }

        public string ClassName { get; set; }

        public string ResultName { get; set; }

        public string EnteredByName { get; set; }
        public string VerifiedByName { get; set; }

        public DateTime TimeEntered { get; set; }
        public string TimeEnteredN { get { return General.ShortTimeString(TimeEntered); } }

        public DateTime TimeVerified { get; set; }
        public string TimeVerifiedN { get { return General.ShortTimeString(TimeVerified); } }

        public byte PercentCorrected { get; set; }

        public string Button
        {
            get
            {
                var enter = "<button class='btn btn-primary btn-xs sDTBtn' data-id=" + SubjectID.ToString() + " title='enter scores'><i class='fa fa-file-alt'></i></button>";
                var verify = "<button class='btn btn-warning btn-xs svDTBtn' data-id=" + SubjectID.ToString() + " title='verify scores'><i class='fa fa-file-alt'></i></button>";
                var modify = "<button class='btn btn-default btn-xs sDTBtn' data-id=" + SubjectID.ToString() + " title='modify scores'><i class='fa fa-file-alt'></i></button>";

                if (TimeVerified > TimeEntered)
                    return modify;
                
                if (TimeEntered == DateTime.MinValue)
                    return enter;

                if (TimeVerified < TimeEntered)
                    return verify;

                return modify;
            }
        }

        public string EditButton
        {
            get
            {
                return "<button class='btn btn-default btn-xs eDTBtn' data-id=" + SubjectID.ToString() + " title='edit'><i class='fa fa-pencil-alt'></i></button>";
            }
        }
    }

    public class ScoreEntryMiniViewModel
    {
        public int StudentID { get; set; }

        public string FirstName { get; set; }
        public string Surname { get; set; }

        public string DisplayName { get { return Surname.ToUpper() + " " + FirstName; } }

        public byte SerialNumber { get; set; }

        public byte MaxCAScore { get; set; }
        public byte MaxExamScore { get; set; }
    }

    public class ScoreEntryViewModel : ScoreEntryMiniViewModel
    {
        [Range(0, 100, ErrorMessage = "Bad!")]
        [CompareNumbers("MaxCAScore", ErrorMessage = "Bad!")]
        public decimal? CAScore { get; set; }

        [Range(0, 100, ErrorMessage = "Bad!")]
        [CompareNumbers("MaxExamScore", ErrorMessage = "Bad!")]
        public decimal? ExamScore { get; set; } // will hold the total score if there is no CA
    }

    public class ScoreVerifyViewModel : ScoreEntryMiniViewModel
    {
        public decimal? CAScore { get; set; }
        public decimal? ExamScore { get; set; }

        [Range(0, 100, ErrorMessage = "Bad!")]
        [CompareNumbers("MaxCAScore", ErrorMessage = "Bad!")]
        public decimal? CAScoreV { get; set; }

        [Range(0, 100, ErrorMessage = "Bad!")]
        [CompareNumbers("MaxExamScore", ErrorMessage = "Bad!")]
        public decimal? ExamScoreV { get; set; } // will hold the total score if there is no CA

        public decimal? Total { get { return (CAScore.HasValue ? CAScore.Value : 0) + ExamScore; } }
    }

    public class ScoreModifyViewModel : ScoreEntryMiniViewModel
    {
        public decimal? CAScore { get; set; }
        public decimal? ExamScore { get; set; }
        public decimal? CAScoreV { get; set; }
        public decimal? ExamScoreV { get; set; }

        [Range(0, 100, ErrorMessage = "Bad!")]
        [CompareNumbers("MaxCAScore", ErrorMessage = "Bad!")]
        public decimal? CAScoreM { get; set; }

        [Range(0, 100, ErrorMessage = "Bad!")]
        [CompareNumbers("MaxExamScore", ErrorMessage = "Bad!")]
        public decimal? ExamScoreM { get; set; } // will hold the total score if there is no CA
    }

    public class ScorePageViewModel
    {
        public int SubjectID { get; set; }
        public string Name { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public string TeacherName { get; set; }
        public int UserID { get; set; }

        public byte MaxCAScore { get; set; }
        public byte MaxExamScore { get; set; }

        public bool NoCA { get; set; }

        public string DisplayName { get { return ClassName + " - " + Name; } }

        public List<ScoreEntryViewModel> Scores { get; set; }
    }

    public class VerifyPageViewModel : ScorePageViewModel
    {
        public new List<ScoreVerifyViewModel> Scores { get; set; }

        public int GradeGroupID { get; set; }

        public DateTime TimeVerified { get; set; }
        public bool VerifiedPreviously { get { return TimeVerified > DateTime.MinValue; } }
    }

    public class ModifyPageViewModel : VerifyPageViewModel
    {
        public new List<ScoreModifyViewModel> Scores { get; set; }

        public int ScoreCount { get; set; }

        public byte Corrected { get; set; }
    }


    public class CommentsPageViewModel : BasePageViewModel
    {
        public int? ClassFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> ClassList { get; set; }

        public int? LevelFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> LevelList { get; set; }

        public int TermID { get; set; }

        public string TermName { get; set; }

        public byte CommentTypeID { get; set; }

        public bool HasScores { get; set; }
    }

    public class CommentsFilterViewModel
    {
        public int TermID { get; set; }

        public int? ClassID { get; set; }

        public int? LevelID { get; set; }
    }


    public class CommentsMiniModel
    {
        public int StudentID { get; set; }

        public string ClassName { get; set; }

        public string DisplayName { get; set; }

        public bool IsMale { get; set; }
        public string Sex { get { return IsMale ? "Male" : "Female"; } }

        [StringLength(512), Display(Name = "Class Teacher's Comment")]
        public string TeacherComment { get; set; }

        [StringLength(512), Display(Name = "Principal's Comment")]
        public string PrincipalComment { get; set; }
    }

    public class CommentsViewModel : CommentsMiniModel
    {
        public int ClassID { get; set; }

        public string GuidString { get; set; }
        public string PhotoSrc { get; set; }

        public bool HasResult { get; set; }
        public string CTComment { get; set; }
        public string PComment { get; set; }

        public bool UpdateResult { get; set; }
    }

    public class CommentTableModel
    {
        public int StudentID { get; set; }

        public int ClassID { get; set; }

        public string ClassName { get; set; }

        public string DisplayName { get; set; }

        public bool IsMale { get; set; }
        public string Sex { get { return IsMale ? "Male" : "Female"; } }

        public string Button
        {
            get
            {
                return "<button class='btn btn-info btn-xs cDTBtn' data-id=" + ClassID.ToString() + " title='enter comments for " + ClassName + "'><i class='fa fa-file-alt'></i></button>";
            }
        }

        public string EditButton
        {
            get
            {
                return "<button class='btn btn-default btn-xs eDTBtn' data-id=" + ClassID.ToString() + " data-sid=" + StudentID.ToString() + " title='edit'><i class='fa fa-pencil-alt'></i></button>";
            }
        }
    }

    public class CTCommentViewModel : CommentTableModel
    {
        public string TeacherComment { get; set; }
    }

    public class PCommentViewModel : CommentTableModel
    {
        public string PrincipalComment { get; set; }
    }

    public class ClassCommentsPageViewModel : BasePageViewModel
    {
        public int ClassID { get; set; }

        public int SchoolID { get; set; }

        public string Name { get; set; }

        public string TermName { get; set; }

        public List<CTCommentViewModel> CTComments { get; set; }
        public List<PCommentViewModel> PComments { get; set; }
    }


    public class ClassSkillViewModel : ClassMiniModel
    {
        public int? SkillGroupID { get; set; }
        public string SkillGroupName { get; set; }
        public string SkillGroup { get { return SkillGroupID.HasValue ? string.IsNullOrEmpty(SkillGroupName) ? "Skill Group " + SkillGroupID.ToString() : SkillGroupName : ""; } }

        public bool HasSkill { get { return SkillGroupID.HasValue; } }

        public bool VerifySkills { get; set; }
        public bool HasEntries { get; set; }

        public string Status { get { return HasEntries ? (VerifySkills ? "Pending Verification" : "Verified") : "No Entries"; } }
    }

    public class SkillPageViewModel : BasePageViewModel
    {
        public int TermID { get; set; }

        public string TermName { get; set; }

        public List<ClassSkillViewModel> Classes { get; set; }
    }

    public class SkillEntryMiniModel
    {
        public int StudentID { get; set; }

        public byte SerialNumber { get; set; }

        public string DisplayName { get; set; }

        public byte MinScore { get; set; }
        public byte MaxScore { get; set; }
    }

    public class SkillEntryViewModel : SkillEntryMiniModel
    {
        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore1 { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore2 { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore3 { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore4 { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore5 { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore6 { get; set; }
    }

    public class SkillVerifyViewModel : SkillEntryViewModel
    {
        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore1V { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore2V { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore3V { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore4V { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore5V { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore6V { get; set; }
    }

    public class SkillModifyViewModel : SkillVerifyViewModel
    {
        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore1M { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore2M { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore3M { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore4M { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore5M { get; set; }

        [CustomRange("MinScore", "MaxScore", ErrorMessage = "Bad!")]
        public byte? SkillScore6M { get; set; }
    }

    public class SkillGradeMiniModel
    {
        public string Name { get; set; }

        public byte GradeNumber { get; set; }
    }

    public class ClassSkillPageViewModel : BasePageViewModel
    {
        public int ClassID { get; set; }

        public int SchoolID { get; set; }

        public string Name { get; set; }

        public string TermName { get; set; }

        public bool VerifySkills { get; set; }

        public string Skill1 { get; set; }
        public string Skill2 { get; set; }
        public string Skill3 { get; set; }
        public string Skill4 { get; set; }
        public string Skill5 { get; set; }
        public string Skill6 { get; set; }

        public bool HasSkill1 { get { return !string.IsNullOrWhiteSpace(Skill1); } }
        public bool HasSkill2 { get { return !string.IsNullOrWhiteSpace(Skill2); } }
        public bool HasSkill3 { get { return !string.IsNullOrWhiteSpace(Skill3); } }
        public bool HasSkill4 { get { return !string.IsNullOrWhiteSpace(Skill4); } }
        public bool HasSkill5 { get { return !string.IsNullOrWhiteSpace(Skill5); } }
        public bool HasSkill6 { get { return !string.IsNullOrWhiteSpace(Skill6); } }

        public List<SkillGradeMiniModel> Grades { get; set; }
        public string GradeGuide { get { return string.Join(", ", Grades.OrderBy(l => l.GradeNumber).Select(l => l.GradeNumber.ToString() + "-" + l.Name)); } }

        public byte MinScore { get; set; }
        public byte MaxScore { get; set; }

        public List<SkillEntryViewModel> Entries { get; set; }
    }

    public class ClassSkillVPageViewModel : ClassSkillPageViewModel
    {
        public new List<SkillVerifyViewModel> Entries { get; set; }
    }

    public class ClassSkillMViewModel : ClassSkillPageViewModel
    {
        public new List<SkillModifyViewModel> Entries { get; set; }
    }

    public class SkillSheetsPageViewModel
    {
        public string SchoolName { get; set; }

        public string TermName { get; set; }

        public List<ClassSkillSheetsViewModel> Sheets { get; set; }
        public List<ClassSkillSheetsViewModel> DisplaySheets { get; set; }
    }

    public class ClassSkillSheetsViewModel
    {
        public string Name { get; set; }

        public string Skill1 { get; set; }
        public string Skill2 { get; set; }
        public string Skill3 { get; set; }
        public string Skill4 { get; set; }
        public string Skill5 { get; set; }
        public string Skill6 { get; set; }

        public bool HasSkills { get; set; }

        public List<SkillGradeMiniModel> Grades { get; set; }
        public string GradeGuide { get { return Grades == null ? "" : (Grades.Any() ? string.Join(", ", Grades.OrderBy(l => l.GradeNumber).Select(l => l.GradeNumber.ToString() + "-" + l.Name)) : ""); } }

        public List<ScoreSheetEntryViewModel> Entries { get; set; }

        public byte MaxNameLength { get { return (byte)Entries.Max(l => string.IsNullOrEmpty(l.StudentName) ? 0 : l.StudentName.Length); } }

        public int Page { get; set; }
    }


    public class ScoreSheetsPageViewModel : BasePageViewModel
    {
        public int TermID { get; set; }

        public string TermName { get; set; }

        public bool HasScores { get; set; }

        public List<ClassViewModel> Classes { get; set; }
    }

    public class ScoreSheetEntryViewModel
    {
        public byte SerialNumber { get; set; }

        public string StudentName { get; set; }
    }

    public class SubjectScoreSheetViewModel
    {
        public byte Order { get; set; }

        public string Name { get; set; }

        public string TeacherName { get; set; }

        public List<ScoreSheetEntryViewModel> Entries { get; set; }

        public int Page { get; set; }
    }

    public class ClassScoreSheetsPageViewModel : BasePageViewModel
    {
        public int ClassID { get; set; }

        public int SchoolID { get; set; }

        public string Name { get; set; }

        public string TermName { get; set; }

        public string SchoolName { get; set; }

        public byte CAWeight { get; set; }
        public byte ExamWeight { get; set; }

        public int StudentCount { get; set; }

        public List<SubjectScoreSheetViewModel> Sheets { get; set; }
        public List<SubjectScoreSheetViewModel> DisplaySheets { get; set; }
    }


    public class ScoreEntryExcel
    {
        public int StudentID { get; set; }

        public string Surname { get; set; }

        public string FirstName { get; set; }
    }

    public class SubjectScoreSheetExcel
    {
        public int SubjectID { get; set; }

        public byte Order { get; set; }

        public string Name { get; set; }

        public string ResultName { get; set; }

        public List<ScoreEntryExcel> Entries { get; set; }
    }

    public class ClassScoreSheetsExcel
    {
        public int SchoolID { get; set; }

        public string Name { get; set; }

        public short SchoolYear { get; set; }

        public byte TermNumber { get; set; }

        public byte CAWeight { get; set; }
        public byte ExamWeight { get; set; }

        public List<SubjectScoreSheetExcel> Sheets { get; set; }
    }



    public class ClassScoreEntryPageViewModel : BasePageViewModel
    {
        public int TermID { get; set; }

        public string TermName { get; set; }

        public List<ClassEntryModel> Classes { get; set; }
    }

    public class ClassEntryModel : ClassMiniModel
    {
        public byte CAWeight { get; set; }
        public byte ExamWeight { get; set; }

        public int SubjectCount { get; set; }

        public int VerifiedCount { get; set; }

        public DateTime LastTimeVerified { get; set; }
        public string TimeVerifiedN { get { return General.ShortTimeString(LastTimeVerified); } }
    }

    public class UploadClassScoreViewModel
    {
        public int ClassID { get; set; }

        public int UserID { get; set; }

        public string Name { get; set; }

        public byte CAWeight { get; set; }
        public byte ExamWeight { get; set; }

        public int GradeGroupID { get; set; }

        public int SubjectCount { get; set; }

        public int VerifiedCount { get; set; }

        public DateTime LastTimeVerified { get; set; }
        public string TimeVerifiedN { get { return General.ShortTimeString(LastTimeVerified); } }

        [Display(Name = "Score Files (Excel)"), Required(ErrorMessage = "Required!")]
        public HttpPostedFile ScoreFile { get; set; }
    }


    public class NoPicturePageViewModel : BasePageViewModel
    {
        public int TermID { get; set; }

        public string TermName { get; set; }

        public List<StudentViewModel> Students { get; set; }
    }

    public class ClassDataModel
    {
        public string Name { get; set; }

        public int StudentCount { get; set; }

        public int SubjectCount { get; set; }
    }

    public class DeleteScoresViewModel
    {
        public bool CanDelete { get; set; }

        public List<ClassDataModel> Classes { get; set; }
    }


    public class CommentSheetsPageViewModel
    {
        public string SchoolName { get; set; }

        public string TermName { get; set; }

        public byte CommentTypeID { get; set; }
        public string CommentTypeName { get { return Eval.GetDisplayName(typeof(CommentType), CommentTypeID); } }

        public List<ClassCommentSheetsViewModel> Sheets { get; set; }
        public List<ClassCommentSheetsViewModel> DisplaySheets { get; set; }
    }

    public class ClassCommentSheetsViewModel
    {
        public string Name { get; set; }

        public List<ScoreSheetEntryViewModel> Entries { get; set; }

        public byte MaxNameLength { get { return (byte)Entries.Max(l => string.IsNullOrEmpty(l.StudentName) ? 0 : l.StudentName.Length); } }

        public int Page { get; set; }
    }

    public class ClassCommentSheetExcel
    {
        public int ClassID { get; set; }

        public byte ClassLevelID { get; set; }
        public string Level { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public string Name { get; set; }

        public List<ScoreEntryExcel> Entries { get; set; }
    }

    public enum CommentType : byte
    {
        [Display(Name = "Class Teacher's Comments")]
        ClassTeacher = 1,

        [Display(Name = "Head Teacher's Comments")]
        HeadTeacher,

        [Display(Name = "Principal's Comments")]
        Principal
    }

    public class CommentSheetsExcel
    {
        public int SchoolID { get; set; }

        public short SchoolYear { get; set; }

        public byte TermNumber { get; set; }

        public byte CommentTypeID { get; set; }
        public string CommentTypeName { get { return Eval.GetDisplayName(typeof(CommentType), CommentTypeID); } }

        public List<ClassCommentSheetExcel> Sheets { get; set; }
    }

    public class UploadCommentsViewModel
    {
        public int TermID { get; set; }

        public string Name { get; set; }

        public int ClassCount { get; set; }

        public int StudentCount { get; set; }

        public byte CommentTypeID { get; set; }
        public string CommentTypeName { get { return Eval.GetDisplayName(typeof(CommentType), CommentTypeID); } }

        [Display(Name = "Comment File (Excel)"), Required(ErrorMessage = "Required!")]
        public HttpPostedFile CommentFile { get; set; }
    }
}