using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IEGen.Models
{
    public class AccessGroupPageViewModel : BasePageViewModel
    {
        public List<AccessGroupViewModel> AccessGroupList { get; set; }
    }

    public class AccessGroupViewModel
    {
        public short AccessGroupID { get; set; }

        public bool CanDelete { get { return !UserCount.HasValue || UserCount.Value == 0; } }

        [StringLength(128)]
        [Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        public int? UserCount { get; set; }

        public int? RoleCount { get; set; }

        public string ChangedByName { get; set; }

        public DateTime TimeChanged { get; set; }

        public string TimeChangedStr { get { return General.FullTimeString(TimeChanged); } }

        public bool IsDefault { get; set; }
    }

    public class GroupRoleViewModel
    {
        public short AccessGroupID { get; set; }

        public byte RoleID { get; set; }

        public string Name { get { return Eval.GetDisplayName(typeof(UserRoles), RoleID); } }
    }

    public class EditGroupRolesViewModel
    {
        public short AccessGroupID { get; set; }

        public bool CanDelete { get { return !UserCount.HasValue || UserCount.Value == 0; } }

        [StringLength(128)]
        [Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        public int? UserCount { get; set; }

        public List<GroupRoleViewModel> AssignedRoles { get; set; }

        public List<GroupRoleViewModel> AvailableRoles { get; set; }
    }

    public class UserPageViewModel : BasePageViewModel
    {
        public int? AccessGroupFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> AccessGroupList { get; set; }

        public UserType? TypeFilter { get; set; }
    }

    public class UserFilterViewModel
    {
        public int? AccessGroupID { get; set; }

        public int? TypeID { get; set; }
    }

    public class UserDetailViewModel
    {
        public int UserID { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public byte TypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(UserType), TypeID); } }
    }

    public class UserViewModel : UserDetailViewModel
    {
        public string AccessGroupName { get; set; }

        public string Button
        {
            get
            {
                return "<button class='btn btn-default btn-xs uDTBtn' data-id=" + UserID.ToString() + " title='edit'><i class='fa fa-pencil-alt'></i></button>";
            }
        }
    }

    public class UserMiniViewModel : UserDetailViewModel
    {
        public string AddButton
        {
            get
            {
                return "<button class='btn btn-success btn-xs addBtn' data-id=" + UserID.ToString() + " title='Add User'><i class='fa fa-plus'></i></button>";
            }
        }
    }

    public class EditUserViewModel
    {
        public int UserID { get; set; }

        [StringLength(128)]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [StringLength(128)]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        public string OldEmail { get; set; }

        [StringLength(128), Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Access Group")]
        public short AccessGroupID { get; set; }
        public List<System.Web.Mvc.SelectListItem> AccessGroupList { get; set; }

        [Display(Name = "User Type")]
        [Required(ErrorMessage = "User Type is required")]
        public UserType? UserType { get; set; }
        public byte TypeID
        {
            get
            {
                return UserType.HasValue ? (byte)UserType : (byte)0;
            }
            set
            {
                UserType = (UserType)value;
            }
        }

        public int? MergeUserID { get; set; }

        public string MergeEmail { get; set; }

        public string MergeName { get; set; }

        public bool CanMerge { get { return MergeUserID.HasValue && MergeUserID.Value > 0; } }
    }

    public class UpdateUserMailModel : BaseMail
    {
        public string SchoolName { get; set; }

        public string NewEmail { get; set; }

        public string Username { get; set; }
    }

    public enum StatusValues : byte
    {
        Active = 0,
        Inactive = 1
    }

    public class SchoolPageViewModel : BasePageViewModel
    {
        public List<SchoolViewModel> SchoolList { get; set; }

        public string DefaultSchool { get; set; }

        public int? LocationFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> LocationList { get; set; }

        public State? StateFilter { get; set; }
        public SchoolType? TypeFilter { get; set; }
        public StatusValues? StatusFilter { get; set; }
    }

    public class SchoolFilterViewModel
    {
        public int? LocationID { get; set; }

        public int? StateID { get; set; }

        public int? TypeID { get; set; }

        public int? StatusID { get; set; }
    }

    public class SchoolViewModel
    {
        public int SchoolID { get; set; }

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

        public byte StateID { get; set; }

        public string LocationDesc { get { return Eval.GetDisplayName(typeof(State), StateID) + " - " + LocationName; } }

        public int? TermCount { get; set; }

        public string Button
        {
            get
            {
                return "<button class='btn btn-default btn-xs uDTBtn' data-id=" + SchoolID.ToString() + " title='edit'><i class='fa fa-pencil-alt'></i></button>";
            }
        }

        public string DefButton
        {
            get
            {
                return "<button class='btn btn-success btn-xs defBtn' data-id=" + SchoolID.ToString() + " title='set as default'><i class='fa fa-play'></i></button>";
            }
        }
    }

    public class EditSchoolViewModel
    {
        public int SchoolID { get; set; }

        public int SchoolRegID { get; set; }

        public bool HasTerms { get; set; }
        public int? TermCount { get; set; }

        public bool CanDelete { get { return !HasTerms; } }

        [StringLength(128)]
        [Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        [StringLength(256)]
        public string Address { get; set; }

        [StringLength(128), EmailAddress]
        public string Email { get; set; }

        [StringLength(128), Url]
        public string Website { get; set; }

        [StringLength(128)]
        public string Phone { get; set; }

        [StringLength(512), Display(Name = "Write Up")]
        public string WriteUp { get; set; }

        [Display(Name = "School Type")]
        [Required(ErrorMessage = "School Type is required")]
        public SchoolType? SchoolType { get; set; }
        public byte TypeID
        {
            get
            {
                return SchoolType.HasValue ? (byte)SchoolType : (byte)0;
            }
            set
            {
                SchoolType = (SchoolType)value;
            }
        }

        [Display(Name = "Status")]
        public StatusValues? Status { get; set; }
        public bool IsDisabled
        {
            get
            {
                return Status.HasValue ? Status == StatusValues.Inactive : false;
            }
            set
            {
                Status = value ? StatusValues.Inactive : StatusValues.Active;
            }
        }

        [Display(Name = "Location")]
        public int? LocationID { get; set; }
        public List<System.Web.Mvc.SelectListItem> LocationList { get; set; }

        public string GuidString { get; set; }
        public string LogoSrc { get; set; }

        public bool NoLogo { get { return string.IsNullOrEmpty(GuidString); } }

        [Display(Name = "Logo (360px * 360px)")]
        public HttpPostedFile Logo { get; set; }
    }

    public class UpdateSchoolLogoViewModel
    {
        public int SchoolID { get; set; }

        public string Name { get; set; }

        public string LocationName { get; set; }

        [Display(Name = "New Logo File (88px * 88px)")]
        [Required(ErrorMessage = "Required!")]
        public HttpPostedFile LogoFile { get; set; }
    }

    public class LocationPageViewModel : BasePageViewModel
    {
        public List<LocationViewModel> LocationList { get; set; }
    }

    public class LocationViewModel
    {
        public int LocationID { get; set; }

        [Required(ErrorMessage = "Required!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "State is required")]
        public State? State { get; set; }
        public byte StateID
        {
            get
            {
                return State.HasValue ? (byte)State : (byte)0;
            }
            set
            {
                State = (State)value;
            }
        }
        public string StateName { get { return Eval.GetDisplayName(typeof(State), StateID); } }

        public int SchoolCount { get; set; }

        public bool CanEdit { get { return SchoolCount == 0; } }
    }

    public class GradeGroupPageViewModel : BasePageViewModel
    {
        public List<GradeGroupViewModel> GradeGroupList { get; set; }
    }

    public class GradeGroupViewModel
    {
        public int GradeGroupID { get; set; }

        public int? SchoolID { get; set; }

        public string SchoolName { get; set; }

        public bool IsLocal { get { return SchoolID.HasValue; } }

        [StringLength(128)]
        public string Name { get; set; }   // Default Name is "Grade Group 1" where "1" is the Grade Group ID

        public string DisplayName { get { return string.IsNullOrEmpty(Name) ? "Grade Group " + GradeGroupID.ToString() : Name; } }

        public int? GradeCount { get; set; }

        public int? SchoolCount { get; set; }

        public bool IsDisabled { get; set; }
        public string Status { get { return IsDisabled ? "Disabled" : (IsUsed ? "In Use" : "Available"); } }

        public bool IsUsed { get; set; }
        public bool CanEdit { get { return !IsUsed; } }
    }

    public class GradeViewModel
    {
        public int GradeID { get; set; }

        public int GradeGroupID { get; set; }

        [StringLength(128)]
        [Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        [Display(Name = "Lower Bound")]
        public byte LowerBound { get; set; }

        [Display(Name = "Upper Bound")]
        public byte UpperBound { get; set; }

        public byte SummaryGradeID { get; set; }
        public string SummaryGradeName { get { return Eval.GetDisplayName(typeof(SummaryGrade), SummaryGradeID); } }

        public bool IsUsed { get; set; }
    }

    public class EditGradeViewModel : GradeViewModel
    {
        public bool CanEdit { get { return !IsUsed; } } 

        [Display(Name = "Summary Grade")]
        [Required(ErrorMessage = "User Type is required")]
        public SummaryGrade? SummaryGrade { get; set; }
        public new byte SummaryGradeID
        {
            get
            {
                return SummaryGrade.HasValue ? (byte)SummaryGrade : (byte)0;
            }
            set
            {
                SummaryGrade = (SummaryGrade)value;
            }
        }
    }

    public class GradeSetupViewModel : BasePageViewModel
    {
        public int GradeGroupID { get; set; }

        public string Name { get; set; }

        public string DisplayName { get { return string.IsNullOrEmpty(Name) ? "Grade Group " + GradeGroupID.ToString() : Name; } }

        public int SchoolCount { get; set; }

        public List<GradeViewModel> GradeList { get; set; }

        public string SchoolName { get; set; }

        public int GradeCount { get { return GradeList.Count(); } }
    }

    public class SkillGroupPageViewModel : BasePageViewModel
    {
        public List<SkillGroupViewModel> SkillGroupList { get; set; }
    }

    public class SkillGroupViewModel
    {
        public int SkillGroupID { get; set; }

        public bool CanDelete { get { return !GradeCount.HasValue || GradeCount.Value == 0; } }

        [StringLength(64)]
        public string Name { get; set; } // Default Name is "Skill Group 1" where "1" is the Skill Group ID

        public string DisplayName { get { return string.IsNullOrEmpty(Name) ? "Skill Group " + SkillGroupID.ToString() : Name; } }

        public int? GradeCount { get; set; }

        public int? SchoolCount { get; set; }

        [StringLength(32), Display(Name = "Skill 1")]
        public string Skill1 { get; set; }

        [StringLength(32), Display(Name = "Skill 2")]
        public string Skill2 { get; set; }

        [StringLength(32), Display(Name = "Skill 3")]
        public string Skill3 { get; set; }

        [StringLength(32), Display(Name = "Skill 4")]
        public string Skill4 { get; set; }

        [StringLength(32), Display(Name = "Skill 5")]
        public string Skill5 { get; set; }

        [StringLength(32), Display(Name = "Skill 6")]
        public string Skill6 { get; set; }

        public int? SchoolID { get; set; }

        public string SchoolName { get; set; }

        public bool IsLocal { get { return SchoolID.HasValue; } }

        public bool IsDisabled { get; set; }
        public string Status { get { return IsDisabled ? "Disabled" : (IsUsed ? "In Use" : "Available"); } }

        public bool IsUsed { get; set; }
        public bool CanEdit { get { return !IsUsed; } }
    }

    public class SkillGradeViewModel
    {
        public int SkillGradeID { get; set; }
        public int SkillGroupID { get; set; }

        [StringLength(128)]
        [Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        [Display(Name = "Grade Number")]
        public byte GradeNumber { get; set; }

        [Display(Name = "Grade Score")]
        public byte GradeScore { get; set; }
        public string FormattedScore { get { return GradeScore.ToString() + "%"; } }

        public bool IsUsed { get; set; }
        public bool CanEdit { get { return !IsUsed; } }
    }

    public class SkillSetupViewModel : BasePageViewModel
    {
        public int SkillGroupID { get; set; }

        public string Name { get; set; }

        public string DisplayName { get { return string.IsNullOrEmpty(Name) ? "Skill Group " + SkillGroupID.ToString() : Name; } }

        public int SchoolCount { get; set; }

        public string Skill1 { get; set; }

        public string Skill2 { get; set; }

        public string Skill3 { get; set; }

        public string Skill4 { get; set; }

        public string Skill5 { get; set; }

        public string Skill6 { get; set; }

        public List<SkillGradeViewModel> GradeList { get; set; }

        public string SchoolName { get; set; }

        public int GradeCount { get { return GradeList.Count(); } }
    }

    public enum SexValues : byte
    {
        Female = 0,
        Male = 1
    }

    public class TeacherPageViewModel : BasePageViewModel
    {
        public int? LocationFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> LocationList { get; set; }

        public State? StateFilter { get; set; }
        public SchoolType? SchoolTypeFilter { get; set; }
        public SexValues? SexFilter { get; set; }
    }

    public class TeacherFilterViewModel
    {
        public int? LocationID { get; set; }

        public int? StateID { get; set; }

        public int? SchoolTypeID { get; set; }

        public int? SexID { get; set; }
    }

    public class TeacherViewModel
    {
        public int TeacherID { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public bool IsMale { get; set; }
        public string Sex { get { return IsMale ? "Male" : "Female"; } }

        public string SchoolName { get; set; }

        public string LocationName { get; set; }

        public byte StateID { get; set; }

        public string LocationDesc { get { return Eval.GetDisplayName(typeof(State), StateID) + " - " + LocationName; } }

        public int? CTeacherID { get; set; }
        public string IETNumber { get { return CTeacherID.HasValue ? "IET" + CTeacherID.ToString() : ""; } }

        public string Qualifications { get; set; }

        public short? StartYear { get; set; } //year the teacher started teaching

        public string Button
        {
            get
            {
                return "<button class='btn btn-default btn-xs uDTBtn' data-id=" + TeacherID.ToString() + " title='view'><i class='fa fa-search'></i></button>";
            }
        }
    }

    public class AllStudentPageViewModel : BasePageViewModel
    {
        public int? LocationFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> LocationList { get; set; }

        public State? StateFilter { get; set; }
        public SchoolType? SchoolTypeFilter { get; set; }
        public SexValues? SexFilter { get; set; }
    }

    public class AllStudentFilterViewModel
    {
        public int? LocationID { get; set; }

        public int? StateID { get; set; }

        public int? SchoolTypeID { get; set; }

        public int? SexID { get; set; }
    }

    public class AllStudentViewModel
    {
        public int StudentID { get; set; }

        public string FirstName { get; set; }

        public string OtherName { get; set; }

        public string Surname { get; set; }

        public string DisplayName { get { return FirstName + " " + Surname; } }

        public bool IsMale { get; set; }
        public string Sex { get { return IsMale ? "Male" : "Female"; } }

        public string StudentCode { get; set; }

        public string ClassName { get; set; }

        public string TermName { get; set; }

        public string ClassDesc { get { return ClassName + " - " + TermName; } }

        public string SchoolName { get; set; }

        public string SchoolLocation { get; set; }

        public byte? SchoolStateID { get; set; }

        public string SchoolDesc { get { return SchoolName + (SchoolStateID.HasValue ? " | " + Eval.GetDisplayName(typeof(State), SchoolStateID.Value) + " - " + SchoolLocation : ""); } }

        public DateTime? BirthDate { get; set; }

        public string LocationName { get; set; }

        public byte? StateID { get; set; }

        public string LocationDesc { get { return StateID.HasValue ? Eval.GetDisplayName(typeof(State), StateID.Value) + " - " + LocationName : ""; } }

        public string PhotoSrc { get; set; }

        public string GuidString { get; set; }

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

    public class CTeacherFilterViewModel
    {
        public int? LocationID { get; set; }

        public int? StateID { get; set; }

        public int? SchoolTypeID { get; set; }

        public int? SexID { get; set; }

        public int? YearsFrom { get; set; }

        public int? YearsTo { get; set; }

        public int? DegreeID { get; set; }

        public int? RegID { get; set; }
    }

    public enum Degrees : byte
    {
        [Display(Name = "N.C.E.")]
        NCE = 1,

        [Display(Name = "Bachelors Degree")]
        Bachelors,

        [Display(Name = "Masters Degree")]
        Masters,

        [Display(Name = "Ph.D [Doctoral]")]
        PhD
    }

    public enum RegValues : byte
    {
        Unverified = 0,
        Verified = 1
    }

    public class CTeacherPageViewModel : BasePageViewModel
    {
        public int? LocationFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> LocationList { get; set; }

        public State? StateFilter { get; set; }
        public SchoolType? SchoolTypeFilter { get; set; }
        public SexValues? SexFilter { get; set; }

        public int? YearFromFilter { get; set; }
        public int? YearToFilter { get; set; }
        public RegValues? RegFilter { get; set; }
        public Degrees? DegreeFilter { get; set; }
    }

    public class CTeacherViewModel
    {
        public int CTeacherID { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public bool IsMale { get; set; }
        public string Sex { get { return IsMale ? "Male" : "Female"; } }

        public byte MaxDegreeID { get; set; }
        public string MaxDegreeName { get { return Eval.GetDisplayName(typeof(Degrees), MaxDegreeID); } }

        public byte SchoolTypeID { get; set; }
        public string SchoolType { get { return Eval.GetDisplayName(typeof(SchoolType), SchoolTypeID); } }

        public string SchoolName { get; set; }
        public string SchoolAddress { get; set; }
        public string Designation { get; set; } 

        public string LocationName { get; set; }

        public byte? StateID { get; set; }

        public string LocationDesc { get { return StateID.HasValue ? Eval.GetDisplayName(typeof(State), StateID.Value) + " - " + LocationName : ""; } }

        public string IETNumber { get { return "IET" + CTeacherID.ToString(); } }

        public string Subjects { get; set; } // e.g. Mathematics (SS3), Further Mathematics (SS1-2)

        public string Qualifications { get; set; }

        public short? StartYear { get; set; } //year the teacher started teaching

        public bool IsVerified { get; set; }
        public string RegStatus { get { return IsVerified ? "Verified" : "Unverified"; } }

        public string Button
        {
            get
            {
                return "<button class='btn btn-default btn-xs uDTBtn' data-id=" + CTeacherID.ToString() + " title='edit'><i class='fa fa-pencil-alt'></i></button>";
            }
        }
    }

    public class EditCTeacherViewModel
    {
        public int CTeacherID { get; set; }

        public string IETNumber { get { return "IET" + CTeacherID.ToString().PadLeft(4, '0'); } }

        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(128), Phone]
        public string Phone { get; set; }

        [StringLength(128), EmailAddress]
        public string Email { get; set; }
        public string OldEmail { get; set; }

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

        [Display(Name = "School Type")]
        [Required(ErrorMessage = "School Type is required")]
        public SchoolType? SchoolType { get; set; }
        public byte SchoolTypeID
        {
            get
            {
                return SchoolType.HasValue ? (byte)SchoolType : (byte)0;
            }
            set
            {
                SchoolType = (SchoolType)value;
            }
        }

        [StringLength(128)]
        [Display(Name = "School Name"), Required(ErrorMessage = "School Type is required")]
        public string SchoolName { get; set; }

        [StringLength(128)]
        [Display(Name = "School Address"), Required(ErrorMessage = "School Address is required")]
        public string SchoolAddress { get; set; }

        [StringLength(128)]
        public string Designation { get; set; } //e.g. Class Teacher (JS 2 Blue) or H.O.D. English

        [StringLength(256)]
        public string Subjects { get; set; } // e.g. Mathematics (SS3), Further Mathematics (SS1-2)

        [StringLength(128)]
        public string Qualifications { get; set; }

        [Display(Name = "Start Year"), Required(ErrorMessage = "Start Year is required"), Range(General.MinYear, General.MaxYear)]
        public short StartYear { get; set; } //year the teacher started teaching

        [Display(Name = "Maximum Qualification")]
        [Required(ErrorMessage = "Maximum Qualification is required")]
        public Degrees? MaxDegree { get; set; }
        public byte MaxDegreeID
        {
            get
            {
                return MaxDegree.HasValue ? (byte)MaxDegree : (byte)0;
            }
            set
            {
                MaxDegree = (Degrees)value;
            }
        }

        [Display(Name = "Registration Status")]
        public RegValues? RegStatus { get; set; }
        public bool IsVerified
        {
            get
            {
                return RegStatus.HasValue ? RegStatus == RegValues.Verified : false;
            }
            set
            {
                RegStatus = value ? RegValues.Verified : RegValues.Unverified;
            }
        }

        [Display(Name = "Location")]
        public int? LocationID { get; set; }
        public List<System.Web.Mvc.SelectListItem> LocationList { get; set; }
    }


    public class SchoolRegViewModel
    {
        public int SchoolRegID { get; set; }

        public string Name { get; set; }

        public DateTime RegDate { get; set; }
        public string RegDateStr { get { return General.DateString(RegDate); } }

        public string ContactPerson { get; set; }

        public byte TypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(SchoolType), TypeID); } }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Notes { get; set; }
    }

    public class AddSchoolRegViewModel
    {
        public int SchoolRegID { get; set; }

        [Display(Name = "School Name")]
        [StringLength(128), Required(ErrorMessage = "School Name is compulsory!")]
        public string Name { get; set; }

        [Display(Name = "Contact Person")]
        [StringLength(128), Required(ErrorMessage = "Contact Person is compulsory!")]
        public string ContactPerson { get; set; }

        [Display(Name = "School Type")]
        [Required(ErrorMessage = "School Type is required")]
        public SchoolType? SchoolType { get; set; }
        public byte TypeID
        {
            get
            {
                return SchoolType.HasValue ? (byte)SchoolType : (byte)0;
            }
            set
            {
                SchoolType = (SchoolType)value;
            }
        }

        [StringLength(128), EmailAddress]
        public string Email { get; set; }

        [StringLength(128)]
        public string Phone { get; set; }

        [StringLength(256)]
        public string Address { get; set; }

        [StringLength(512)]
        public string Notes { get; set; }
    }

    public class ChooseSchoolViewModel
    {
        public int SchoolID { get; set; }

        public string Name { get; set; }

        public byte TypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(SchoolType), TypeID); } }

        public string LocationName { get; set; }

        public byte StateID { get; set; }

        public string LocationDesc { get { return Eval.GetDisplayName(typeof(State), StateID) + " - " + LocationName; } }
    }

    public class NewRequestViewModel
    {
        public int SchoolRegID { get; set; }

        public DateTime RequestDate { get; set; }
        public string RequestDateStr { get { return General.DateString(RequestDate); } }

        public string Name { get; set; }

        public string ContactPerson { get; set; }

        public byte TypeID { get; set; }
        public string Type { get { return Eval.GetDisplayName(typeof(SchoolType), TypeID); } }

        public string Email { get; set; }

        public string Phone { get; set; }
    }

    public class NewRequestPageViewModel : BasePageViewModel
    {
        public List<NewRequestViewModel> RequestList { get; set; }
    }



    public class FormerStudentPageViewModel : BasePageViewModel
    {
        public int? InstitutionFilter { get; set; }
        public List<System.Web.Mvc.SelectListItem> InstitutionList { get; set; }
    }

    public class FormerStudentFilterViewModel
    {
        public int? InstitutionID { get; set; }
    }

    public class FormerStudentMiniModel
    {
        public int STUDENT_ID { get; set; }

        public string DISPLAY_NAME { get; set; }

        public string STUDENT_CODE { get; set; }

        public string SchoolName { get; set; }

        public string ClassName { get; set; }

        public string TermName { get; set; }

        public string ClassDesc { get { return ClassName + " - " + TermName; } }
    }

    public class FormerStudentViewModel : FormerStudentMiniModel
    {
        public string Button
        {
            get
            {
                return "<button class='btn btn-default btn-xs uDTBtn' data-id=" + STUDENT_ID.ToString() + " title='view'><i class='fa fa-search'></i></button>";
            }
        }
    }

    public class FormerStudentDisplayModel : FormerStudentMiniModel
    {
        public byte[] Picture { get; set; }

        public string PhotoSrc { get { return Picture == null ? "" : "data:image/png;base64," + Convert.ToBase64String(Picture); } }

        public string PictureName { get { return DISPLAY_NAME.Replace(" ", "-") + ".png"; } }
    }

    public class FormerResultModel
    {
        public string TermName { get; set; }


        public string Year { get { return TermName.Substring(0, 4); } }

        public byte TermNumber { get; set; }

        public short Level { get; set; }
        public string LevelName { get { return Eval.GetDisplayName(typeof(IPSLevel), (byte)Level); } }

        public string CourseName { get; set; }
        //public string SubjectName { get { return CourseName.Substring(LevelName.Length); } }

        public string DefinitionName { get; set; }
        public string SubjectName { get { return DefinitionName.Substring(6); } }

        public decimal Score { get; set; }

        public string CsvRow { get { return Year + "," + TermNumber.ToString() + "," + Level.ToString() + "," + SubjectName + ", " + (Score * 100).ToString("F"); } }
    }
}