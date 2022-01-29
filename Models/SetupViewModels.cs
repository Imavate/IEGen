using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IEGen.Models
{
    public class SchoolDetailsViewModel : BasePageViewModel
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string Website { get; set; }

        public string Phone { get; set; }

        public string WriteUp { get; set; }

        public bool IsDisabled { get; set; }
        public string Status { get { return IsDisabled ? "Inactive" : "Active"; } }

        public byte TypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(SchoolType), TypeID); } }

        public string LocationName { get; set; }

        public string DefaultTerm { get; set; }

        public List<TermViewModel> Terms { get; set; }
    }

    public class SchoolRequestPageViewModel : BasePageViewModel
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string Website { get; set; }

        public string Phone { get; set; }

        public string WriteUp { get; set; }

        public bool IsDisabled { get; set; }
        public string Status { get { return IsDisabled ? "Inactive" : "Active"; } }

        public byte TypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(SchoolType), TypeID); } }

        public string LocationName { get; set; }

        public List<SchoolRequestViewModel> Requests { get; set; }
    }

    public class SchoolRequestViewModel
    {
        public int RequestID { get; set; }

        public DateTime RequestDate { get; set; }
        public string RequestDateStr { get { return General.DateString(RequestDate); } }

        public string ContactPerson { get; set; }

        public string Notes { get; set; }
    }

    public class SchoolTermMiniModel
    {
        public int TermID { get; set; }

        public string Name { get; set; }

        public string SchoolName { get; set; }
    }

    public class TermMiniModel
    {
        public int TermID { get; set; }

        public string Name { get; set; }
    }

    public class TermViewModel : TermMiniModel
    {
        public short SchoolYear { get; set; }

        public byte TermNumber { get; set; }

        public string TermDef { get { return SchoolYear.ToString() + "-" + TermNumber.ToString(); } }

        public byte DaysOpened { get; set; }

        public DateTime ExamStartDate { get; set; }
        public string ExamStartDateStr { get { return General.FullDateFromSQL(ExamStartDate); } }

        public DateTime VacationDate { get; set; }
        public string VacationDateStr { get { return General.FullDateFromSQL(VacationDate); } }

        public DateTime NextResumptionDate { get; set; }
        public string NRDateStr { get { return General.FullDateFromSQL(NextResumptionDate); } }

        public int ClassCount { get; set; }

        public int? ResultCount { get; set; }
        public int? StudentCount { get; set; }
        public int? ResultOrStudentCount { get { return ResultCount > 0 ? ResultCount : StudentCount; } }

    }

    public class EditTermViewModel
    {
        public int TermID { get; set; }

        public int SchoolID { get; set; }

        [Display(Name = "School Year")]
        public short SchoolYear { get; set; }

        [Display(Name = "Term Number")]
        public byte TermNumber { get; set; }

        [StringLength(128)]
        public string Name { get; set; }

        [Display(Name = "Number of Days School Opened")]
        public byte DaysOpened { get; set; }

        [Display(Name = "Exam Start Date")]
        public DateTime ExamStartDate { get; set; }

        [Display(Name = "Exam End Date")]
        public DateTime ExamEndDate { get; set; }

        [Display(Name = "Score Collection Date")]
        public DateTime ScoreCollectionDate { get; set; }

        [Display(Name = "Vacation Date")]
        public DateTime VacationDate { get; set; }

        [Display(Name = "Next Resumption Date")]
        public DateTime NextResumptionDate { get; set; }

        public int ClassCount { get; set; }

        public bool CanDelete { get { return ClassCount == 0; } }
    }


    public class SchoolSkillsViewModel : BasePageViewModel
    {
        public string Name { get; set; }

        public string LocationName { get; set; }

        public byte TypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(SchoolType), TypeID); } }

        public int? DefSkillGroupID { get; set; }
        public string DefGroupName { get; set; }

        public string DefGroup { get { return DefSkillGroupID.HasValue ? string.IsNullOrEmpty(DefGroupName) ? "Skill Group " + DefSkillGroupID.ToString() : DefGroupName : ""; } }

        public List<SkillGroupViewModel> GroupList { get; set; }
    }

    public class SchoolGradesViewModel : BasePageViewModel
    {
        public string Name { get; set; }

        public string LocationName { get; set; }

        public byte TypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(SchoolType), TypeID); } }

        public int? DefGradeGroupID { get; set; }
        public string DefGroupName { get; set; }

        public string DefGroup { get { return DefGradeGroupID.HasValue ? string.IsNullOrEmpty(DefGroupName) ? "Grade Group " + DefGradeGroupID.ToString() : DefGroupName : ""; } }

        public List<GradeGroupViewModel> GroupList { get; set; }
    }

    public class ViewGradeGroupViewModel : BasePageViewModel
    {
        public int GradeGroupID { get; set; }

        public int? SchoolID { get; set; }

        public string SchoolName { get; set; }

        public bool IsLocal { get { return SchoolID.HasValue; } }

        public string Name { get; set; }   

        public string DisplayName { get { return string.IsNullOrEmpty(Name) ? "Grade Group " + GradeGroupID.ToString() : Name; } }

        public List<GradeViewModel> GradeList { get; set; }

        public int GradeCount { get { return GradeList.Count(); } }

        public bool IsUsed { get; set; }
        public bool CanEdit { get { return !IsUsed && SchoolID.HasValue; } }
    }

    public class ViewSkillGroupViewModel : BasePageViewModel
    {
        public int SkillGroupID { get; set; }

        public string Name { get; set; } 

        public string DisplayName { get { return string.IsNullOrEmpty(Name) ? "Skill Group " + SkillGroupID.ToString() : Name; } }

        public string Skill1 { get; set; }
        public string Skill2 { get; set; }
        public string Skill3 { get; set; }
        public string Skill4 { get; set; }
        public string Skill5 { get; set; }
        public string Skill6 { get; set; }

        public int? SchoolID { get; set; }

        public string SchoolName { get; set; }

        public bool IsLocal { get { return SchoolID.HasValue; } }

        public bool IsDisabled { get; set; }
        public string Status { get { return IsDisabled ? "Disabled" : (IsUsed ? "In Use" : "Available"); } }

        public List<SkillGradeViewModel> GradeList { get; set; }

        public int GradeCount { get { return GradeList.Count(); } }

        public bool IsUsed { get; set; }
        public bool CanEdit { get { return !IsUsed && SchoolID.HasValue; } }
    }


    public class SubjectTemplatePageViewModel : BasePageViewModel
    {
        public List<SubjectTemplateViewModel> TemplateList { get; set; }

        public string SchoolName { get; set; }
    }

    public class SubjectTemplateMiniModel
    {
        public int TemplateID { get; set; }

        public string Name { get; set; }

        public byte ClassLevelID { get; set; }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }
    }

    public class SubjectTemplateViewModel : SubjectTemplateMiniModel
    {
        public byte CategoryID { get; set; }
        public string CategoryName { get { return Eval.GetDisplayName(typeof(SubjectCategory), CategoryID); } }

        public string ResultName { get; set; }

        public bool HasTerm1 { get; set; }
        public bool HasTerm2 { get; set; }
        public bool HasTerm3 { get; set; }

        public byte Order { get; set; }

        public int SubjectID { get; set; }  //for deleting subjects in ClassSetup
    }

    public class EditSubjectTemplateViewModel
    {
        public int TemplateID { get; set; }

        public int SchoolID { get; set; }

        [StringLength(64), Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        [Display(Name = "Class Level")]
        [Required(ErrorMessage = "Class Level is required")]
        public byte ClassLevelID { get; set; }
        public List<System.Web.Mvc.SelectListItem> LevelList { get; set; }

        [Display(Name = "Category")]
        [Required(ErrorMessage = "Category is required")]
        public SubjectCategory? Category { get; set; }
        public byte CategoryID
        {
            get
            {
                return Category.HasValue ? (byte)Category : (byte)0;
            }
            set
            {
                Category = (SubjectCategory)value;
            }
        }

        [Display(Name = "Result Name")]
        [StringLength(16), Required(ErrorMessage = "Result Name is compulsory!")]
        public string ResultName { get; set; }

        public byte Order { get; set; }

        public bool HasTerm1 { get; set; }
        public bool HasTerm2 { get; set; }
        public bool HasTerm3 { get; set; }

        public bool HasSubjects { get; set; }
        public bool CanDelete { get { return !HasSubjects; } }
    }


    public class SchoolPerformanceCommentViewModel : BasePageViewModel
    {
        public string Name { get; set; }

        public string LocationName { get; set; }

        public byte TypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(SchoolType), TypeID); } }

        public string DefGroupName { get; set; }

        public List<PerformanceCommentGroupViewModel> GroupList { get; set; }
    }

    public class PerformanceCommentGroupViewModel
    {
        public int GroupID { get; set; }

        public int SchoolID { get; set; }

        [StringLength(32), Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        public int CommentCount { get; set; }

        public int ClassCount { get; set; }
    }

    public class PerformanceCommentSetupViewModel : BasePageViewModel
    {
        public int GroupID { get; set; }

        public string Name { get; set; }

        public int ClassCount { get; set; }

        public List<PerformanceCommentViewModel> CommentList { get; set; }

        public string SchoolName { get; set; }
    }

    public class PerformanceCommentViewModel
    {
        public int CommentID { get; set; }

        public int GroupID { get; set; }

        [Display(Name = "Lower Bound")]
        public byte LowerBound { get; set; }

        [Display(Name = "Upper Bound")]
        public byte UpperBound { get; set; }

        [StringLength(128)]
        public string Comment { get; set; }
    }


    public class SchoolPromotionCommentViewModel : BasePageViewModel
    {
        public string Name { get; set; }

        public string LocationName { get; set; }

        public byte TypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(SchoolType), TypeID); } }

        public string DefGroupName { get; set; }

        public List<PromotionCommentGroupViewModel> GroupList { get; set; }
    }

    public class PromotionCommentGroupViewModel
    {
        public int GroupID { get; set; }

        public int SchoolID { get; set; }

        [StringLength(32), Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        public int CommentCount { get; set; }

        public int ClassCount { get; set; }
    }

    public class PromotionCommentSetupViewModel : BasePageViewModel
    {
        public int GroupID { get; set; }

        public string Name { get; set; }

        public int ClassCount { get; set; }

        public List<PromotionCommentViewModel> CommentList { get; set; }

        public string SchoolName { get; set; }
    }

    public class PromotionCommentViewModel
    {
        public int CommentID { get; set; }

        public int GroupID { get; set; }

        [Display(Name = "Lower Bound")]
        public byte LowerBound { get; set; }

        [Display(Name = "Upper Bound")]
        public byte UpperBound { get; set; }

        [StringLength(128)]
        public string Comment { get; set; }
    }


    public class SchoolImprovementCommentViewModel : BasePageViewModel
    {
        public string Name { get; set; }

        public string LocationName { get; set; }

        public byte TypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(SchoolType), TypeID); } }

        public string DefGroupName { get; set; }

        public List<ImprovementCommentGroupViewModel> GroupList { get; set; }
    }

    public class ImprovementCommentGroupViewModel
    {
        public int GroupID { get; set; }

        public int SchoolID { get; set; }

        [StringLength(32), Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        public int CommentCount { get; set; }

        public int ClassCount { get; set; }
    }

    public class ImprovementCommentSetupViewModel : BasePageViewModel
    {
        public int GroupID { get; set; }

        public string Name { get; set; }

        public int ClassCount { get; set; }

        public List<ImprovementCommentViewModel> CommentList { get; set; }

        public string SchoolName { get; set; }
    }

    public class ImprovementCommentViewModel
    {
        public int CommentID { get; set; }

        public int GroupID { get; set; }

        [Display(Name = "Min. Failed")]
        public byte MinFailCount { get; set; }

        [Display(Name = "Max. Failed")]
        public byte MaxFailCount { get; set; }

        [StringLength(128)]
        public string Comment { get; set; }
    }


    public class ComplexResultFormatPageViewModel : BasePageViewModel
    {
        public List<ComplexResultFormatViewModel> FormatList { get; set; }

        public string SchoolName { get; set; }
    }

    public class ComplexResultFormatViewModel
    {
        public int FormatID { get; set; }

        public string Name
        {
            get
            {
                var nm = "";

                if (CA1Weight > 0)
                    nm += "CA1-" + CA1Weight.ToString() + ", ";

                if (CA2Weight > 0)
                    nm += "CA2-" + CA2Weight.ToString() + ", ";

                if (CA3Weight > 0)
                    nm += "CA3-" + CA3Weight.ToString() + ", ";

                if (CA4Weight > 0)
                    nm += "CA4-" + CA4Weight.ToString() + ", ";

                if (ExamWeight > 0)
                    nm += "Exam-" + ExamWeight.ToString();

                return nm;
            }
        }

        [Display(Name = "CA1 Weight")]
        [Required(ErrorMessage = "CA1 Weight is required"), Range(0, 100)]
        public byte CA1Weight { get; set; }

        [Display(Name = "CA2 Weight")]
        [Required(ErrorMessage = "CA2 Weight is required"), Range(0, 100)]
        public byte CA2Weight { get; set; }

        [Display(Name = "CA3 Weight")]
        [Required(ErrorMessage = "CA3 Weight is required"), Range(0, 100)]
        public byte CA3Weight { get; set; }

        [Display(Name = "CA4 Weight")]
        [Required(ErrorMessage = "CA4 Weight is required"), Range(0, 100)]
        public byte CA4Weight { get; set; }

        [Display(Name = "Exam Weight")]
        [Required(ErrorMessage = "Exam Weight is required"), Range(0, 100)]
        public byte ExamWeight { get; set; }
    }

    public class EditComplexResultFormatViewModel : ComplexResultFormatViewModel
    {
        public int SchoolID { get; set; }

        public int ClassCount { get; set; }
        public int SubjectCount { get; set; }

        public bool CanEdit { get { return (ClassCount + SubjectCount) == 0; } }
    }


    public class SchoolDefaultsModel
    {
        public string DefGradeGroup { get; set; }

        public string DefSkillGroup { get; set; }

        public string DefPerformanceCommentGroup { get; set; }

        public string DefImprovementCommentGroup { get; set; }
    }

    public class ClassPageViewModel : BasePageViewModel
    {
        public List<ClassViewModel> ClassList { get; set; }

        public int TermID { get; set; }

        public string Name { get; set; }

        public byte TermNumber { get; set; }
        public bool IsFirstTerm { get { return TermNumber == 1; } }

        public short SchoolYear { get; set; }

        public DateTime NextResumptionDate { get; set; }
        public string NRDateStr { get { return General.FullDateString(NextResumptionDate); } }

        public byte DaysOpened { get; set; }

        public int StudentCount { get { return ClassList.Sum(l => l.StudentCount); } }

        public SchoolDefaultsModel SchoolDefaults { get; set; }

        public string DefGradeGroup { get { return SchoolDefaults != null ? SchoolDefaults.DefGradeGroup : ""; } }

        public string DefSkillGroup { get { return SchoolDefaults != null ? SchoolDefaults.DefSkillGroup : ""; } }

        public string DefPerformanceCommentGroup { get { return SchoolDefaults != null ? SchoolDefaults.DefPerformanceCommentGroup : ""; } }

        public string DefImprovementCommentGroup { get { return SchoolDefaults != null ? SchoolDefaults.DefImprovementCommentGroup : ""; } }

        public bool NoDefGradeGroup { get { return SchoolDefaults == null || string.IsNullOrWhiteSpace(SchoolDefaults.DefGradeGroup); } }

        public bool IncompleteArms { get; set; }

        public byte SchoolTypeID { get; set; }
    }

    public class ClassMiniModel
    {
        public int ClassID { get; set; }

        public byte ClassLevelID { get; set; }
        public string Level { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public byte ClassTypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(ClassType), ClassTypeID); } }

        public string Name { get; set; }

        public int StudentCount { get; set; }
    }

    public class ClassViewModel : ClassMiniModel
    {
        public byte DaysOpened { get; set; }

        public byte CAWeight { get; set; }
        public byte ExamWeight { get; set; }

        public int SubjectCount { get; set; }

        public string GradeGroupName { get; set; }
    }

    public class ImportClassesViewModel
    {
        public int TermID { get; set; }

        public short SchoolYear { get; set; }

        public byte TermNumber { get; set; }

        public int SchoolID { get; set; }

        public bool IsCurrentSessionImport { get; set; }

        [Display(Name = "Source Term")]
        [Required(ErrorMessage = "Source Term is required")]
        public int SourceTermID { get; set; }
        public List<System.Web.Mvc.SelectListItem> TermList { get; set; }

        public bool MakeClassesPromotional { get; set; }

        public bool ImportStudents { get; set; }
    }

    public class ImportClassViewModel
    {
        public int ClassID { get; set; }

        public int ArmID { get; set; }

        public bool ShowPosition { get; set; }
        public bool ShowSummaryGrade { get; set; }
        public bool HideClassAverage { get; set; }
        public bool ShowCategoryAnalysis { get; set; }

        public byte RedLine { get; set; }

        // promotional class fields
        public bool IsPromotionalClass { get; set; }
        public bool CommentOnYearResult { get; set; }    //replaces IS_3T_PROMOTION_BASIS
        public bool ShowYearResult { get; set; } // replaces IS_3T_RESULT_BASIS

        public byte CAWeight { get; set; }

        public byte ExamWeight { get; set; }

        public int GradeGroupID { get; set; }

        public int? SkillGroupID { get; set; }

        public int? FormatID { get; set; }

        public int? PerformanceCommentGroupID { get; set; }

        public int? PromotionCommentGroupID { get; set; }

        public int? ImprovementCommentGroupID { get; set; }

        public List<ImportSubjectViewModel> Subjects { get; set; }
    }

    public class ImportSubjectViewModel
    {
        public int TemplateID { get; set; }

        public string ResultName { get; set; }
    }

    public class ImportStudentsPageViewModel : BasePageViewModel
    {
        public int TermID { get; set; }

        public string Name { get; set; }

        public DateTime NextResumptionDate { get; set; }
        public string NRDateStr { get { return General.FullDateString(NextResumptionDate); } }

        public byte DaysOpened { get; set; }

        public int StudentCount { get; set; } 

        public List<TermViewModel> Terms { get; set; }
    }

    public class ImportClassStudentsViewModel
    {
        public int ClassID { get; set; }

        public byte ClassTypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(ClassType), ClassTypeID); } }

        public string Name { get; set; }

        public int StudentCount { get; set; }

        public int? NewClassID { get; set; }  //new class
    }

    public class ImportTermStudentsViewModel
    {
        public int NewTermID { get; set; }

        public string Name { get; set; }

        public string NewTermName { get; set; }

        public bool ImportAllStudents { get; set; }

        public List<ImportClassStudentsViewModel> Classes { get; set; }
        public List<System.Web.Mvc.SelectListItem> NewClassList { get; set; }
    }


    public class EditClassViewModel
    {
        public int ClassID { get; set; }

        public int TermID { get; set; }

        public byte SchoolTypeID { get; set; }

        [Display(Name = "Class Arm")]
        [Required(ErrorMessage = "Class Arm is required")]
        public int ArmID { get; set; }
        public List<System.Web.Mvc.SelectListItem> ArmList { get; set; }

        public string Name { get; set; }

        public bool ShowPosition { get; set; }
        public bool ShowSummaryGrade { get; set; }
        public bool HideClassAverage { get; set; }
        public bool ShowCategoryAnalysis { get; set; }

        [Display(Name = "Red Line")]
        public byte RedLine { get; set; }

        [Display(Name = "Days Opened")]
        public byte DaysOpened { get; set; }

        // promotional class fields
        public bool IsPromotionalClass { get; set; }
        public bool CommentOnYearResult { get; set; }    //replaces IS_3T_PROMOTION_BASIS
        public bool ShowYearResult { get; set; } // replaces IS_3T_RESULT_BASIS

        [Display(Name = "CA Weight (%)")]
        [Required(ErrorMessage = "Required")]
        public byte CAWeight { get; set; }

        [Display(Name = "Exam Weight (%)")]
        [Required(ErrorMessage = "Required")]
        public byte ExamWeight { get; set; }

        [Display(Name = "Grade Group"), Required(ErrorMessage = "Required")]
        public int GradeGroupID { get; set; }
        public List<System.Web.Mvc.SelectListItem> GradeGroupList { get; set; }

        [Display(Name = "Skill Group")]
        public int? SkillGroupID { get; set; }
        public List<System.Web.Mvc.SelectListItem> SkillGroupList { get; set; }

        [Display(Name = "Complex Result Format")]
        public int? FormatID { get; set; }
        public List<System.Web.Mvc.SelectListItem> FormatList { get; set; }

        [Display(Name = "Performance Comments")]
        public int? PerformanceCommentGroupID { get; set; }
        public List<System.Web.Mvc.SelectListItem> PerformanceCommentGroupList { get; set; }

        [Display(Name = "Promotion Comments")]
        public int? PromotionCommentGroupID { get; set; }
        public List<System.Web.Mvc.SelectListItem> PromotionCommentGroupList { get; set; }

        [Display(Name = "Improvement Comments")]
        public int? ImprovementCommentGroupID { get; set; }
        public List<System.Web.Mvc.SelectListItem> ImprovementCommentGroupList { get; set; }

        public int StudentCount { get; set; }
        public int SubjectCount { get; set; }

        public bool HasScoreEntries { get; set; }

        public bool CanDelete { get { return StudentCount == 0 && !HasScoreEntries; } }
    }

    public class ClassSetupViewModel : BasePageViewModel
    {
        public List<SubjectViewModel> SubjectList { get; set; }
        public List<SubjectTemplateViewModel> TemplateList { get; set; }

        public int ClassID { get; set; }

        public byte ClassLevelID { get; set; }
        public string Level { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public byte ClassTypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(ClassType), ClassTypeID); } }

        public string Name { get; set; }

        public string TermName { get; set; }

        public byte TermNumber { get; set; }

        public short SchoolYear { get; set; }

        public int StudentCount { get; set; }

    }

    public class SubjectViewModel
    {
        public int SubjectID { get; set; }

        public int TemplateID { get; set; } //used for adding subjects in ClassSetup

        public byte Order { get; set; }

        public byte ClassLevelID { get; set; }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public byte CategoryID { get; set; }
        public string CategoryName { get { return Eval.GetDisplayName(typeof(SubjectCategory), CategoryID); } }

        public string TeacherName { get; set; }

        public string Name { get; set; }

        public string ResultName { get; set; }
    }

    public class EditSubjectViewModel : SubjectViewModel
    {
        public int ClassID { get; set; }

        public string ClassName { get; set; }

        [StringLength(64), Required(ErrorMessage = "Name is compulsory!")]
        public new string Name { get { return _name; } set { _name = value?.Trim(); } }
        protected string _name;

        [Display(Name = "Result Name")]
        [StringLength(16), Required(ErrorMessage = "Result Name is compulsory!")]
        public new string ResultName { get { return _rname; } set { _rname = value?.Trim(); } }
        protected string _rname;

        [Display(Name = "Teacher")]
        public int? TeacherID { get; set; }
        public List<System.Web.Mvc.SelectListItem> TeacherList { get; set; }

        [Display(Name = "Complex Result Format")]
        public int? FormatID { get; set; }
        public List<System.Web.Mvc.SelectListItem> FormatList { get; set; }

        public bool HasResults { get; set; }
        public bool CanDelete { get { return !HasResults; } }
    }


    public class StudentsPageViewModel : BasePageViewModel
    {
        public int? ClassFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> ClassList { get; set; }

        public int? TermFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> TermList { get; set; }

        public int? LevelFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> LevelList { get; set; }

        public SexValues? SexFilter { get; set; }

        public string SchoolName { get; set; }
        public int? DefClassID { get; set; }
        public int DefTermID { get; set; }

        public string DefTermName { get; set; }
    }

    public class StudentFilterViewModel
    {
        public int SchoolID { get; set; }

        public int? ClassID { get; set; }

        public int? LevelID { get; set; }

        public int? TermID { get; set; }

        public int? SexID { get; set; }
    }

    public class StudentMiniViewModel
    {
        public int StudentID { get; set; }

        public string FirstName { get; set; }

        public string OtherName { get; set; }

        public string Surname { get; set; }

        public string DisplayName { get { return Surname.ToUpper() + " " + FirstName; } }

        public string ClassName { get; set; }

        public string TermName { get; set; }
    }

    public class StudentViewModel : StudentMiniViewModel
    {
        public bool IsMale { get; set; }
        public string Sex { get { return IsMale ? "Male" : "Female"; } }

        public string StudentCode { get; set; }
        public string Code { get { return string.IsNullOrEmpty(StudentCode) ? "ISN" + StudentID.ToString().PadLeft(6, '0') : StudentCode; } }

        public byte ClassLevelID { get; set; }
        public string Level { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public string Button
        {
            get
            {
                return "<button class='btn btn-default btn-xs uDTBtn' data-id=" + StudentID.ToString() + " title='view'><i class='fa fa-search'></i></button>";
            }
        }

        public string EditButton
        {
            get
            {
                return "<button class='btn btn-default btn-xs eDTBtn' data-id=" + StudentID.ToString() + " title='edit'><i class='fa fa-pencil-alt'></i></button>";
            }
        }
    }

    public class EditStudentViewModel
    {
        public int StudentID { get; set; }
        public int? SchoolID { get; set; }

        public int TermID { get; set; }
        public int DefTermID { get; set; }

        [Display(Name = "First Name")]
        [StringLength(128), Required(ErrorMessage = "First Name is compulsory!")]
        public string FirstName { get { return _fname; } set { _fname = value?.Trim(); } }
        protected string _fname;

        [StringLength(128), Display(Name = "Other Name")]
        public string OtherName { get { return _mname; } set { _mname = value?.Trim(); } }
        protected string _mname;

        [Display(Name = "Surname")]
        [StringLength(128), Required(ErrorMessage = "Surname is compulsory!")]
        public string Surname { get { return _lname; } set { _lname = value?.Trim(); } }
        protected string _lname;

        public string DisplayName { get { return _lname.ToUpper() + " " + _fname; } }

        [Display(Name = "Student Code")]
        public string StudentCode { get; set; }

        [Display(Name = "Sex")]
        public SexValues? Sex { get; set; }
        public bool IsMale
        {
            get
            {
                return Sex.HasValue ? Sex == SexValues.Male : false;
            }
            set
            {
                Sex = value ? SexValues.Male : SexValues.Female;
            }
        }

        [Display(Name = "Date Of Birth")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Class"), Required(ErrorMessage = "Required")]
        public int ClassID { get; set; }
        public int OldClassID { get; set; }
        public List<System.Web.Mvc.SelectListItem> ClassList { get; set; }

        public string ClassName { get; set; }
        public string TermName { get; set; }
        public string DefTermName { get; set; }


        [Display(Name = "Location of Residence")]
        public int? LocationID { get; set; }
        public List<System.Web.Mvc.SelectListItem> LocationList { get; set; }

        [Display(Name = "Passport Photo (120px * 120px)")]
        public HttpPostedFile Photo { get; set; }

        public string GuidString { get; set; }
        public string PhotoSrc { get; set; }

        public bool NoPicture { get { return string.IsNullOrEmpty(GuidString); } }

        public bool NotInTerm { get { return DefTermID != TermID; } }

        public bool SaveAnyway { get; set; }

        public bool HasScores { get; set; }
        public bool HasComplexScores { get; set; }

        public bool CanDelete { get { return !HasComplexScores && !HasScores; } }
    }

    public class EditFloatingStudentViewModel
    {
        public int StudentID { get; set; }
        public int? SchoolID { get; set; }

        [Display(Name = "First Name")]
        [StringLength(128), Required(ErrorMessage = "First Name is compulsory!")]
        public string FirstName { get { return _fname; } set { _fname = value?.Trim(); } }
        protected string _fname;

        [StringLength(128), Display(Name = "Other Name")]
        public string OtherName { get { return _mname; } set { _mname = value?.Trim(); } }
        protected string _mname;

        [Display(Name = "Surname")]
        [StringLength(128), Required(ErrorMessage = "Surname is compulsory!")]
        public string Surname { get { return _lname; } set { _lname = value?.Trim(); } }
        protected string _lname;

        public string DisplayName { get { return _lname.ToUpper() + " " + _fname; } }

        [Display(Name = "Student Code")]
        public string StudentCode { get; set; }

        [Display(Name = "Class"), Required(ErrorMessage = "Required")]
        public int? ClassID { get; set; }
        public List<System.Web.Mvc.SelectListItem> ClassList { get; set; }

        public int DefTermID { get; set; }
        public string DefTermName { get; set; }

        [Display(Name = "Sex")]
        public SexValues? Sex { get; set; }
        public bool IsMale
        {
            get
            {
                return Sex.HasValue ? Sex == SexValues.Male : false;
            }
            set
            {
                Sex = value ? SexValues.Male : SexValues.Female;
            }
        }

        [Display(Name = "Date Of Birth")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Location of Residence")]
        public int? LocationID { get; set; }
        public List<System.Web.Mvc.SelectListItem> LocationList { get; set; }

        [Display(Name = "Passport Photo (120px * 120px)")]
        public HttpPostedFile Photo { get; set; }

        public string GuidString { get; set; }
        public string PhotoSrc { get; set; }

        public bool NoPicture { get { return string.IsNullOrEmpty(GuidString); } }

        public bool SaveAnyway { get; set; }

        public bool HasScores { get; set; }
        public bool HasComplexScores { get; set; }

        public bool CanDelete { get { return !HasComplexScores && !HasScores; } }
    }

    public class SimilarStudentViewModel
    {
        public List<StudentMiniViewModel> StudentList { get; set; }

        public int ClassID { get; set; }

        public bool SaveAnyway { get; set; }
    }

    public class StudentPageViewModel : BasePageViewModel
    {
        public int StudentID { get; set; }

        public string FirstName { get; set; }

        public string OtherName { get; set; }

        public string Surname { get; set; }

        public string DisplayName { get { return Surname.ToUpper() + " " + FirstName; } }

        public string ClassName { get; set; }
        public bool HasClass { get; set; }

        public string TermName { get; set; }

        public string DefTermName { get; set; }

        public bool IsMale { get; set; }
        public string Sex { get { return IsMale ? "Male" : "Female"; } }

        public string StudentCode { get; set; }
        public string Code { get { return string.IsNullOrEmpty(StudentCode) ? "ISN" + StudentID.ToString().PadLeft(6, '0') : StudentCode; } }

        public byte ClassLevelID { get; set; }
        public string Level { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public string GuidString { get; set; }
        public string PhotoSrc { get; set; }

        public string LocationName { get; set; }
        public byte? StateID { get; set; }
        public string LocationDesc { get { return StateID.HasValue ? Eval.GetDisplayName(typeof(State), StateID.Value) + " - " + LocationName : ""; } }

        public DateTime? BirthDate { get; set; }

        public string SchoolName { get; set; }

        public int DefTermID { get; set; }

        public List<StudentResultViewModel> Results { get; set; }

        public List<StudentPastResultViewModel> PastResults { get; set; }

    }

    public class StudentPastResultViewModel
    {
        public short Year { get; set; }

        public byte ClassLevelID { get; set; }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public string SubjectName { get; set; }

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

    public class UploadStudentScoreViewModel : StudentMiniViewModel
    {
        public int? SchoolID { get; set; }

        [Display(Name = "Previous Results File (*.csv)")]
        public HttpPostedFile ResultFile { get; set; }
    }

    public class ResultCsvModel
    {
        public short Year { get; set; }

        public byte TermNumber { get; set; }

        public byte Level { get; set; }

        public string SubjectName { get; set; }

        public decimal Score { get; set; }
    }

    public class ResultUploadModel : ResultCsvModel
    {
        [Display(Name = "Subject Template")]
        [Required(ErrorMessage = "Required")]
        public int? TemplateID { get; set; }
        public List<System.Web.Mvc.SelectListItem> TemplateList { get; set; }

        public byte ClassLevelID { get; set; }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public string ScoreStr { get { return Score.ToString("F"); } }

        public bool HasTemplateID { get { return TemplateID.HasValue; } }

        public string Term { get { return Year.ToString() + "-" + TermNumber.ToString(); } }
    }

    public class VerifyStudentScoreUploadViewModel : StudentMiniViewModel
    {
        public List<ResultUploadModel> ResultList { get; set; }
    }

    public class CurrentResultModel
    {
        public short Year { get; set; }

        public byte TermNumber { get; set; }

        public byte ClassLevelID { get; set; }
        public int Level { get { return ClassLevelID > 10 ? ClassLevelID - 10 : ClassLevelID + 15; } }  //back to IPSEDU Level format to maintain consistency

        public string SubjectName { get; set; }

        public decimal? CAScore { get; set; }
        public decimal? ExamScore { get; set; }

        public decimal? Total { get { return (CAScore ?? 0) + ExamScore; } }

        public decimal Score { get { return Total ?? 0; } }

        public string CsvRow { get { return Year + "," + TermNumber.ToString() + "," + Level.ToString() + "," + SubjectName + ", " + Score.ToString("F"); } }
    }

    public class FloatingStudentViewModel
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

        public List<StudentPastResultViewModel> PastResults { get; set; }

        public short MaxYear { get { return PastResults != null && PastResults.Any() ? PastResults.Select(l => l.Year).Max() : (short)0; } }

        public byte MaxLevelID { get { return PastResults != null && PastResults.Any() ? PastResults.Where(l => l.Year == MaxYear).Select(l => l.ClassLevelID).Max() : (byte)0; } }
        public string LevelName { get { return Eval.GetDisplayName(typeof(ClassLevel), MaxLevelID); } }

        public int ResultCount { get { return PastResults != null && PastResults.Any() ? PastResults.Count : 0; } }
    }

    public class FloatingStudentsPageViewModel : BasePageViewModel
    {
        public string SchoolName { get; set; }

        public string LocationName { get; set; }

        public int DefTermID { get; set; }
        public string DefTermName { get; set; }

        public byte SchoolTypeID { get; set; }
        public string SchoolType { get { return Eval.GetDisplayName(typeof(SchoolType), SchoolTypeID); } }

        public List<FloatingStudentViewModel> StudentList { get; set; }
    }


    public class OtherExamTypePageViewModel : BasePageViewModel
    {
        public string SchoolName { get; set; }

        public string LocationName { get; set; }

        public byte SchoolTypeID { get; set; }
        public string SchoolType { get { return Eval.GetDisplayName(typeof(SchoolType), SchoolTypeID); } }

        public List<OtherExamTypeViewModel> TypeList { get; set; }

    }

    public class OtherExamTypeViewModel
    {
        public int TypeID { get; set; }

        public int SchoolID { get; set; }

        [StringLength(64), Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        public bool IsDisabled { get; set; }

        public int ExamCount { get; set; }

        public string Status { get { return IsDisabled ? "Disabled" : (ExamCount == 0 ? "New" : "In Use"); } }
    }


    public class ClassArmPageViewModel : BasePageViewModel
    {
        public string SchoolName { get; set; }

        public string LocationName { get; set; }

        public byte SchoolTypeID { get; set; }
        public string SchoolType { get { return Eval.GetDisplayName(typeof(SchoolType), SchoolTypeID); } }

        public List<ClassArmViewModel> ArmList { get; set; }

    }

    public class ClassArmViewModel
    {
        public int ArmID { get; set; }

        public int? SchoolID { get; set; }

        public string Name { get; set; }

        public byte ClassLevelID { get; set; }
        public string Level { get { return Eval.GetDisplayName(typeof(ClassLevel), ClassLevelID); } }

        public byte ClassTypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(ClassType), ClassTypeID); } }

        public bool IsDisabled { get; set; }

        public bool HasClasses { get; set; }

        public string Status { get { return IsDisabled ? "Disabled" : (HasClasses ? "In Use" : "New"); } }
    }

    public class EditClassArmViewModel
    {
        public int ArmID { get; set; }

        public int? SchoolID { get; set; }

        public byte SchoolTypeID { get; set; }

        [Display(Name = "Class Level")]
        [Required(ErrorMessage = "Class Level is required")]
        public byte ClassLevelID { get; set; }
        public List<System.Web.Mvc.SelectListItem> LevelList { get; set; }

        [Display(Name = "Class Type")]
        [Required(ErrorMessage = "Class Type is required")]
        public ClassType? Type { get; set; }
        public byte ClassTypeID
        {
            get
            {
                return Type.HasValue ? (byte)Type : (byte)0;
            }
            set
            {
                Type = (ClassType)value;
            }
        }

        [StringLength(32), Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get { return _name; } set { _name = value?.Trim(); } }
        protected string _name;

        public bool HasClasses { get; set; }

        public bool IsDisabled { get; set; }
    }
}