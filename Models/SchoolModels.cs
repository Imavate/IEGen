using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace IEGen.Models
{
    public enum SchoolType : byte
    {
        [Display(Name = "Nursery/Primary")]
        NurseryPrimary = 1,

        [Display(Name = "Secondary School")]
        Secondary,

        [Display(Name = "Special Education")]
        Special,

        [Display(Name = "Technical College")]
        Technical,

        [Display(Name = "Hand Crafts/Home Econs. School")]
        HandCraft,

        [Display(Name = "Computer Centre")]
        Computer,

        [Display(Name = "Vocational School")]
        Vocational,

        [Display(Name = "Remedial School")]
        Remedial
    }

    public enum SchoolSize : byte
    {
        [Display(Name = "1 - 100 Students")]
        Size100 = 1,

        [Display(Name = "101 - 150 Students")]
        Size150,

        [Display(Name = "151 - 200 Students")]
        Size200,

        [Display(Name = "201 - 250 Students")]
        Size250,

        [Display(Name = "251 - 300 Students")]
        Size300,

        [Display(Name = "301 - 350 Students")]
        Size350,

        [Display(Name = "351 - 400 Students")]
        Size400,

        [Display(Name = "Above 400 Students")]
        SizeBig,
    }

    public enum PlanType : byte
    {
        [Display(Name = "Premium")]
        Premium = 1,

        [Display(Name = "Basic")]
        Basic,

        [Display(Name = "Basic Lite")]
        BasicLite,

        [Display(Name = "Custom")]
        Custom,

    }

    public enum IEProduct : byte
    {
        [Display(Name = "Previous Term Entry")]
        PreviousTerm = 1,

        [Display(Name = "Score Sheet Print")]
        ScoreSheet,

        [Display(Name = "Manual Score Entry")]
        ManualEntry,

        [Display(Name = "Printed Student Result")]
        ResultPrint,

        [Display(Name = "Packaged Student Result")]
        ResultPackage,

        [Display(Name = "Printed Class Broadsheets")]
        BroadsheetPrint,

        [Display(Name = "Printed Subject Analysis")]
        SubjectAnalysis,

        [Display(Name = "Analysis Booklet & CD")]
        AnalysisBooklet,

        [Display(Name = "Analysis Booklet in 30 days")]
        AnalysisBooklet30,

        [Display(Name = "Analysis CD only")]
        AnalysisCD,

        [Display(Name = "Electronic Results")]
        Online,

        [Display(Name = "Electronic Results in 30 days")]
        Online30,

    }

    public class SchoolReg
    {
        public SchoolReg() { }

        [Key]
        public int SchoolRegID { get; set; }

        public DateTime RegDate { get; set; }

        [StringLength(128), Required(ErrorMessage = "School Name is compulsory!")]
        public string Name { get; set; }

        [StringLength(128), Required(ErrorMessage = "Contact Person is compulsory!")]
        public string ContactPerson { get; set; }

        public byte TypeID { get; set; }

        [StringLength(128), EmailAddress]
        public string Email { get; set; }

        [StringLength(128)]
        public string Phone { get; set; }

        [StringLength(256)]
        public string Address { get; set; }

        [StringLength(512)]
        public string Notes { get; set; }
    }

    public class School : UploadedFile
    {
        public School() { }

        [Key]
        public int SchoolID { get; set; }

        [StringLength(128), Required(ErrorMessage = "School Name is compulsory!")]
        [Index(IsUnique = true)]
        public string Name { get; set; }

        [StringLength(256)]
        public string Address { get; set; }

        [StringLength(128), EmailAddress]
        public string Email { get; set; }

        [StringLength(128), Url]
        public string Website { get; set; }

        [StringLength(128)]
        public string Phone { get; set; }

        [StringLength(512)]
        public string WriteUp { get; set; }

        public byte TypeID { get; set; }

        public bool IsDisabled { get; set; }

        public int? LocationID { get; set; }

        [NotMapped]
        public new string FileName { get; set; }        // created to suppress the inherited property...

        [NotMapped]
        public new string ContentType { get; set; }

        [NotMapped]
        public new string Description { get; set; }

        [NotMapped]
        public new int FileSize { get; set; }

        [NotMapped]
        public new DateTime UploadTime { get; set; }

        public override FileType FileType { get { return FileType.Logo; } }

        [ForeignKey("LocationID")]
        public Location Location { get; set; }

        [InverseProperty("School")]
        public List<Term> Terms { get; set; }

        [InverseProperty("School")]
        public List<SchoolRequest> Requests { get; set; }
    }

    /// <summary>
    /// Store the school registration details after they have been used to create a school object or are added to an existing school
    /// Other details in the school registration object can be added to the notes if they differ from the details the school already has.
    /// </summary>
    public class SchoolRequest
    {
        public SchoolRequest() { }

        [Key]
        public int RequestID { get; set; }

        public int SchoolID { get; set; }

        public DateTime RequestDate { get; set; }

        [StringLength(128), Required(ErrorMessage = "Contact Person is compulsory!")]
        public string ContactPerson { get; set; }

        [StringLength(512)]
        public string Notes { get; set; }

        [ForeignKey("SchoolID")]
        public School School { get; set; }
    }

    public class SchoolData
    {
        public SchoolData() { }

        [Key]
        public int SchoolID { get; set; }

        public int? DefSkillGroupID { get; set; }

        public int? DefGradeGroupID { get; set; }

        public int? DefPerformanceCommentGroupID { get; set; }

        public int? DefImprovementCommentGroupID { get; set; }

        public int? DefPromotionCommentGroupID { get; set; }

        public DateTime AnalysisTime { get; set; }


        [ForeignKey("DefPerformanceCommentGroupID")]
        public PerformanceCommentGroup DefPerformanceCommentGroup { get; set; }

        [ForeignKey("DefImprovementCommentGroupID")]
        public ImprovementCommentGroup DefImprovementCommentGroup { get; set; }

        [ForeignKey("DefPromotionCommentGroupID")]
        public PromotionCommentGroup DefPromotionCommentGroup { get; set; }

        [ForeignKey("DefSkillGroupID")]
        public SkillGroup DefaultSkillGroup { get; set; }

        [ForeignKey("DefGradeGroupID")]
        public GradeGroup DefaultGradeGroup { get; set; }

        [ForeignKey("SchoolID")]
        public School School { get; set; }
    }

    public class Location
    {
        public Location() { }

        [Key]
        public int LocationID { get; set; }

        [StringLength(64), Required(ErrorMessage = "Location Name is compulsory!")]
        public string Name { get; set; }

        public byte StateID { get; set; }

        [InverseProperty("Location")]
        public List<School> Schools { get; set; }
    }

    public enum State : byte
    {
        Abia = 1,
        Adamawa,

        [Display(Name = "Akwa Ibom")]
        AkwaIbom,

        Anambra,
        Bauchi,
        Bayelsa,
        Benue,
        Borno,

        [Display(Name = "Cross River")]
        CrossRiver,

        Delta,
        Ebonyi,
        Enugu,
        Edo,
        Ekiti,
        Gombe,
        Imo,
        Jigawa,
        Kaduna,
        Kano,
        Katsina,
        Kebbi,
        Kogi,
        Kwara,
        Lagos,
        Nasarawa,
        Niger,
        Ogun,
        Ondo,
        Osun,
        Oyo,
        Plateau,
        Rivers,
        Sokoto,
        Taraba,
        Yobe,
        Zamfara,

        [Display(Name = "Federal Capital Territory (FCT)")]
        AbujaFCT
    }

    public enum ClassLevel : byte
    {
        Playgroup = 1,
        Reception,

        [Display(Name ="Nursery 1")]
        Nursery1,

        [Display(Name = "Nursery 2")]
        Nursery2,

        [Display(Name = "Basic 1")]
        Basic1,

        [Display(Name = "Basic 2")]
        Basic2,

        [Display(Name = "Basic 3")]
        Basic3,

        [Display(Name = "Basic 4")]
        Basic4,

        [Display(Name = "Basic 5")]
        Basic5,

        [Display(Name = "Basic 6")]
        Basic6,

        JS1,
        JS2,
        JS3,
        SS1,
        SS2,
        SS3
    }

    /// <summary>
    /// Replaces Arm in IPSEDU. Arm name will only be at the Class Name level. 
    /// It is global in scope not specific to schools
    /// The following will be seeded: Default, Arts, Commercial, Arts & Commercial, Science
    /// Classes with the same class type and class level will have homogenous subjects 
    /// e.g. JS 2 Blue has a Default class type while SS 2 Blue is a Science Class and SS 2 Yellow is Arts & Commercial
    /// </summary>
    public enum ClassType : byte
    {
        General = 1,
        Arts,
        Commercial,
        Science,

        [Display(Name = "Arts and Commercial")]
        NonScience //Arts and Commercial
    }

    /// <summary>
    /// The term carries the school logo in case the logo changes for the next term...
    /// </summary>
    public class Term : UploadedFile
    {
        public Term() { }

        [Key]
        public int TermID { get; set; }

        [Index("IDX_Term_Main", IsUnique = true, Order = 1)]
        public int SchoolID { get; set; }
        [Index("IDX_Term_Main", IsUnique = true, Order = 2)]
        public short SchoolYear { get; set; }
        [Index("IDX_Term_Main", IsUnique = true, Order = 3)]
        public byte TermNumber { get; set; }

        [StringLength(32), Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        public byte DaysOpened { get; set; }

        public DateTime ExamStartDate { get; set; }
        public DateTime ExamEndDate { get; set; }
        public DateTime ScoreCollectionDate { get; set; }
        public DateTime VacationDate { get; set; }
        public DateTime NextResumptionDate { get; set; }

        [NotMapped]
        public new string FileName { get; set; }        // created to suppress the inherited property...

        [NotMapped]
        public new string ContentType { get; set; }

        [NotMapped]
        public new string Description { get; set; }

        [NotMapped]
        public new int FileSize { get; set; }

        [NotMapped]
        public new DateTime UploadTime { get; set; }

        public override FileType FileType { get { return FileType.Logo; } }

        [ForeignKey("SchoolID")]
        public School School { get; set; }

        [InverseProperty("Term")]
        public List<Class> Classes { get; set; }
    }

    public class TermProduct
    {
        public TermProduct() { }

        [Key, Column(Order = 1)]
        public int TermID { get; set; }

        [Key, Column(Order = 2)]
        public byte ProductID { get; set; }

        public decimal Price { get; set; }

        public string Comment { get; set; }

        [ForeignKey("TermID")]
        public Term Term { get; set; }
    }

    public class Discount
    {
        public Discount() { }

        [Key]
        public int DiscountID { get; set; }

        public int CreatedByID { get; set; }

        public DateTime DateCreated { get; set; }

        public int? SchoolID { get; set; }

        public decimal LowerBound { get; set; }

        public decimal Amount { get; set; }

        public bool IsEarlyBird { get; set; }

        public bool IsDisabled { get; set; }

        [ForeignKey("SchoolID")]
        public School School { get; set; }
    }

    public class Transaction
    {
        public Transaction() { }

        [Key]
        public int TransactionID { get; set; }

        public DateTime DateEntered { get; set; }

        public int TermID { get; set; }

        public decimal Amount { get; set; } //invoices are -ve, payments are positive

        public string Comments { get; set; }

        [ForeignKey("TermID")]
        public Term Term { get; set; }
    }

    /// <summary>
    /// Creates a distinct group of grades... will be generally available for all schools...
    /// A school can create a new one if the existing one does not fit...
    /// </summary>
    public class GradeGroup
    {
        public GradeGroup() { }

        [Key]
        public int GradeGroupID { get; set; }

        public int? SchoolID { get; set; }

        [StringLength(64)]
        public string Name { get; set; }

        public bool IsDisabled { get; set; }

        [InverseProperty("DefaultGradeGroup")]
        public List<SchoolData> Schools { get; set; }

        [InverseProperty("GradeGroup")]
        public List<Grade> Grades { get; set; }

        [InverseProperty("GradeGroup")]
        public List<Class> Classes { get; set; }
    }

    public class Grade
    {
        [Key]
        public int GradeID { get; set; }
        public int GradeGroupID { get; set; }
        public byte LowerBound { get; set; }
        public byte UpperBound { get; set; }
        public byte SummaryGradeID { get; set; }

        [StringLength(16), Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        [ForeignKey("GradeGroupID")]
        public GradeGroup GradeGroup { get; set; }
    }

    public enum SummaryGrade : byte { A = 1, B, C, D, E, F }

    /// <summary>
    /// Qualitative Skill Group
    /// </summary>
    public class SkillGroup
    {
        public SkillGroup() { }

        [Key]
        public int SkillGroupID { get; set; }

        public int? SchoolID { get; set; }

        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(32)]
        public string Skill1 { get; set; }
        [StringLength(32)]
        public string Skill2 { get; set; }
        [StringLength(32)]
        public string Skill3 { get; set; }
        [StringLength(32)]
        public string Skill4 { get; set; }
        [StringLength(32)]
        public string Skill5 { get; set; }
        [StringLength(32)]
        public string Skill6 { get; set; }

        public bool IsDisabled { get; set; }

        [InverseProperty("DefaultSkillGroup")]
        public List<SchoolData> Schools { get; set; }

        [InverseProperty("SkillGroup")]
        public List<SkillGrade> Grades { get; set; }

        [InverseProperty("SkillGroup")]
        public List<Class> Classes { get; set; }
    }

    public class SkillGrade
    {
        public SkillGrade() { }

        [Key]
        public int SkillGradeID { get; set; }
        public int SkillGroupID { get; set; }
        public byte GradeNumber { get; set; }
        public byte GradeScore { get; set; }

        [StringLength(16), Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        [ForeignKey("SkillGroupID")]
        public SkillGroup SkillGroup { get; set; }
    }

    /// <summary>
    /// For Normalization Purposes only! 
    /// 
    /// </summary>
    public class ClassArm
    {
        [Key]
        public int ArmID { get; set; }

        public int? SchoolID { get; set; }  //set to nullable so as to avoid multiple cascade paths in the Classes table if a school is deleted

        public byte ClassLevelID { get; set; }
        public byte ClassTypeID { get; set; }

        [StringLength(32)]
        public string Name { get; set; }

        public bool IsDisabled { get; set; }

        [ForeignKey("SchoolID")]
        public School School { get; set; }

        [InverseProperty("Arm")]
        public List<Class> Classes { get; set; }
    }

    public class Class
    {
        public Class() { }

        [Key]
        public int ClassID { get; set; }
        public int TermID { get; set; }
        public int ArmID { get; set; }

        public bool ShowPosition { get; set; }
        public bool ShowSummaryGrade { get; set; }
        public bool HideClassAverage { get; set; }
        public bool ShowCategoryAnalysis { get; set; }

        public byte RedLine { get; set; }
        public byte DaysOpened { get; set; }

        // promotional class fields
        public bool IsPromotionalClass { get; set; }
        public bool CommentOnYearResult { get; set; }    //replaces IS_3T_PROMOTION_BASIS
        public bool ShowYearResult { get; set; } // replaces IS_3T_RESULT_BASIS

        public bool VerifySkills { get; set; }

        public byte CAWeight { get; set; }
        public byte ExamWeight { get; set; }

        public int? ClassTeacherID { get; set; }

        public int? SkillGroupID { get; set; }
        public int GradeGroupID { get; set; }

        public int? PerformanceCommentGroupID { get; set; }

        public int? ImprovementCommentGroupID { get; set; }

        public int? PromotionCommentGroupID { get; set; }


        [ForeignKey("PerformanceCommentGroupID")]
        public PerformanceCommentGroup PerformanceCommentGroup { get; set; }

        [ForeignKey("ImprovementCommentGroupID")]
        public ImprovementCommentGroup ImprovementCommentGroup { get; set; }

        [ForeignKey("PromotionCommentGroupID")]
        public PromotionCommentGroup PromotionCommentGroup { get; set; }

        [ForeignKey("SkillGroupID")]
        public SkillGroup SkillGroup { get; set; }

        [ForeignKey("GradeGroupID")]
        public GradeGroup GradeGroup { get; set; }

        [ForeignKey("ArmID")]
        public ClassArm Arm { get; set; }

        [ForeignKey("TermID")]
        public Term Term { get; set; }

        [ForeignKey("ClassTeacherID")]
        public Teacher ClassTeacher { get; set; }

        [InverseProperty("Class")]
        public List<Student> Students { get; set; }

        [InverseProperty("Class")]
        public List<Subject> Subjects { get; set; }

        [InverseProperty("Class")]
        public List<StudentResult> Results { get; set; }

        [InverseProperty("Class")]
        public List<StudentSkills> SkillEntries { get; set; }
    }

    public class Student : UploadedFile
    {
        public Student() { }

        [Key]
        public int StudentID { get; set; }
        public int? SchoolID { get; set; }

        [StringLength(128), Required(ErrorMessage = "First Name is compulsory!")]
        public string FirstName { get; set; }

        [StringLength(128)]
        public string Surname { get; set; }

        [StringLength(128)]
        public string OtherName { get; set; }

        public bool IsMale { get; set; }

        [StringLength(128)]
        public string StudentCode { get; set; }

        public DateTime? BirthDate { get; set; }

        public int? LocationID { get; set; }  // we may want to preserve this in the student term information

        public int? ClassID { get; set; }

        [StringLength(512)]
        public string TeacherComment { get; set; } //Class teacher's comments

        [StringLength(512)]
        public string PrincipalComment { get; set; }   //Principal's Comments

        [NotMapped]
        public new string FileName { get; set; }        // created to suppress the inherited property...

        [NotMapped]
        public new string ContentType { get; set; }

        [NotMapped]
        public new string Description { get; set; }

        [NotMapped]
        public new int FileSize { get; set; }

        [NotMapped]
        public new int UploadTime { get; set; }

        public override FileType FileType { get { return FileType.Picture; } }

        [ForeignKey("ClassID")]
        public Class Class { get; set; }

        [ForeignKey("SchoolID")]
        public School School { get; set; }

        [ForeignKey("LocationID")]
        public Location Location { get; set; }

        [InverseProperty("Student")]
        public List<ScoreEntry> Scores { get; set; }

        [InverseProperty("Student")]
        public List<ComplexScoreEntry> ComplexScores { get; set; }

        [InverseProperty("Student")]
        public List<StudentResult> Results { get; set; }

        [InverseProperty("Student")]
        public List<StudentPastResult> PastResults { get; set; }
    }

    public enum SubjectCategory : byte
    {
        Arts = 1,

        [Display(Name = "Business Studies")]
        BusinessStudies,

        English,

        [Display(Name = "Foreign Languages")]
        ForeignLanguages,

        [Display(Name = "Local Languages")]
        LocalLanguages,

        Mathematics,
        Sciences,

        [Display(Name = "Social Sciences")]
        SocialSciences
    }

    /// <summary>
    /// Defines a Subject. Replaces Year Course and Course Definition in IPSEDU
    /// </summary>
    public class SubjectTemplate
    {
        public SubjectTemplate() { }

        [Key]
        public int TemplateID { get; set; }

        [Index("IDX_SubjectTemplate_Main", Order = 1)]
        public int SchoolID { get; set; }

        [Index("IDX_SubjectTemplate_Main", Order = 2)]
        public byte ClassLevelID { get; set; }

        public byte CategoryID { get; set; }

        [StringLength(64), Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        [StringLength(16), Required(ErrorMessage = "Result Name is compulsory!")]
        public string ResultName { get; set; }

        public bool HasTerm1 { get; set; }
        public bool HasTerm2 { get; set; }
        public bool HasTerm3 { get; set; }

        /// <summary>
        /// Relative ordering to other subjects. Default is 10. 
        /// 1 is at the top.
        /// e.g. English and Maths should be of order 1
        /// </summary>
        public byte Order { get; set; }

        //[ForeignKey("SchoolID")]
        //public School School { get; set; }

        [InverseProperty("Template")]
        public List<TermSubjectStats> TermStats { get; set; }
    }

    /// <summary>
    /// Subject for a particular class... Delete the subjects that do not have any results as soon as students are transferred out of the class...
    /// </summary>
    public class Subject
    {
        public Subject() { }

        [Key]
        public int SubjectID { get; set; }

        public int ClassID { get; set; }
        public int TemplateID { get; set; }
        public short SchoolYear { get; set; }
        public byte TermNumber { get; set; }

        public int? TeacherID { get; set; }

        //[StringLength(64), Required(ErrorMessage = "Name is compulsory!")]
        //public string Name { get; set; }

        [StringLength(16), Required(ErrorMessage = "Result Name is compulsory!")]
        public string ResultName { get; set; }

        public int? EnteredByID { get; set; }
        public int? VerifiedByID { get; set; }
        public DateTime TimeEntered { get; set; }
        public DateTime TimeVerified { get; set; }
        public byte PercentCorrected { get; set; }

        //public decimal AverageScore { get; set; }
        //public byte ResultCount { get; set; }

        public bool NoCA { get; set; }  //if this is set the MaxExamScore = 100 and the MaxCAScore = 0

        // compute from ResultEntry where ExamScore is not null
        //public byte StudentCount { get; set; }
        //public decimal? LowestScore { get; set; }
        //public decimal? AverageScore { get; set; }
        //public decimal? HighestScore { get; set; }

        //public int? BestStudentID { get; set; }

        //[ForeignKey("BestStudentID")]
        //public Student BestStudent { get; set; }

        [ForeignKey("EnteredByID")]
        public IEUser EnteredBy { get; set; }

        [ForeignKey("VerifiedByID")]
        public IEUser VerifiedBy { get; set; }

        [ForeignKey("ClassID")]
        public Class Class { get; set; }

        [ForeignKey("TemplateID")]
        public SubjectTemplate Template { get; set; }

        [ForeignKey("TeacherID")]
        public Teacher Teacher { get; set; }

        [InverseProperty("Subject")]
        public List<ScoreEntry> Scores { get; set; }

        [InverseProperty("Subject")]
        public List<OtherExamScore> OtherExamScores { get; set; }

    }

    /// <summary>
    /// Random tests given by a teacher... it is not part of a larger exam structure like what exists in the Other Exam Object
    /// </summary>
    public class Assessment
    {
        public Assessment() { }

        [Key]
        public int AssessmentID { get; set; }

        public int SubjectID { get; set; }

        public byte TotalMarks { get; set; }

        [StringLength(32), Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }
    }

    public class OtherExamType
    {
        [Key]
        public int TypeID { get; set; }

        public int SchoolID { get; set; }

        [StringLength(64), Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        public bool IsDisabled { get; set; }

        [InverseProperty("Type")]
        public List<OtherExam> Exams { get; set; }
    }

    /// <summary>
    /// Any other kind of examination aside from the terminal examinations e.g. Mid-Term Examinations. 
    /// Will be computed from Student Result
    /// </summary>
    public class OtherExam
    {
        [Key]
        public int OtherExamID { get; set; }
        public int TypeID { get; set; }

        public int? ClassID { get; set; }

        //Computed from the final assessment of the term
        public byte ResultCount { get; set; }
        public byte SubjectCount { get; set; }

        public decimal? LowestAverage { get; set; }
        public decimal? MeanAverage { get; set; }
        public decimal? BestAverage { get; set; }

        public int? BestStudentID { get; set; }
        public int? BestSubjectID { get; set; }

        [ForeignKey("BestStudentID")]
        public Student BestStudent { get; set; }

        [ForeignKey("BestSubjectID")]
        public Subject BestSubject { get; set; }

        [ForeignKey("ClassID")]
        public Class Class { get; set; }

        [ForeignKey("TypeID")]
        public OtherExamType Type { get; set; }

        //[InverseProperty("Exam")]
        //public List<OtherExamSubject> Subjects { get; set; }

        [InverseProperty("Exam")]
        public List<OtherExamResult> Results { get; set; }
    }

    //public class OtherExamSubject
    //{
    //    [Key, Column(Order = 1)]
    //    public int OtherExamID { get; set; }

    //    [Key, Column(Order = 2)]
    //    public int SubjectID { get; set; }

    //    [ForeignKey("OtherExamID")]
    //    public OtherExam Exam { get; set; }

    //    [ForeignKey("SubjectID")]
    //    public Subject Subject { get; set; }
    //}

    /// <summary>
    /// If a teacher can log into the system then the teacher should have a Check Teacher reference...
    /// Login is tied to the CTeacher reference
    /// Teachers are unique to schools... The teacher can link his existing IET profile 
    /// </summary>
    public class Teacher : UploadedFile
    {
        public Teacher() { }

        [Key]
        public int TeacherID { get; set; }

        public int SchoolID { get; set; }

        public int? CTeacherID { get; set; }

        [StringLength(64), Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        [StringLength(128), Phone]
        public string Phone { get; set; }

        [StringLength(128), EmailAddress]
        public string Email { get; set; }

        public bool IsMale { get; set; }

        public bool HasLeft { get; set; }

        [NotMapped]
        public new string FileName { get; set; }        // created to suppress the inherited property...

        [NotMapped]
        public new string ContentType { get; set; }

        [NotMapped]
        public new string Description { get; set; }

        [NotMapped]
        public new int FileSize { get; set; }

        [NotMapped]
        public new DateTime UploadTime { get; set; }

        public override FileType FileType { get { return FileType.Picture; } }

        [ForeignKey("CTeacherID")]
        public CTeacher CTeacher { get; set; }

        [ForeignKey("SchoolID")]
        public School School { get; set; }

        [InverseProperty("Teacher")]
        public List<TeacherRole> Roles { get; set; }

        [InverseProperty("Teacher")]
        public List<Subject> Subjects { get; set; }
    }

    public class TeacherRole
    {
        public TeacherRole() { }

        [Key]
        public int TeacherRoleID { get; set; }

        public int TeacherID { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public short DurationMonths { get; set; }

        [ForeignKey("TeacherID")]
        public Teacher Teacher { get; set; }
    }

    /// <summary>
    /// All Check Teachers can log into CheckTeachers.com which is actually an alias for app.imavate-edu.com or portal.imavate-edu.com
    /// If this teacher is verified then the teacher should have a Teacher reference... we will use that to bring up the teacher's other information...
    /// </summary>
    public class CTeacher
    {
        public CTeacher() { }

        [Key]
        public int CTeacherID { get; set; }

        public int? UserID { get; set; }

        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(128), Phone]
        public string Phone { get; set; }

        [StringLength(128), EmailAddress]
        public string Email { get; set; }

        public bool IsMale { get; set; }

        public byte MaxDegreeID { get; set; }

        public int? LocationID { get; set; }

        public byte SchoolTypeID { get; set; }

        [StringLength(128)]
        public string SchoolName { get; set; }

        [StringLength(128)]
        public string SchoolAddress { get; set; }

        [StringLength(128)]
        public string Designation { get; set; } //e.g. Class Teacher (JS 2 Blue) or H.O.D. English

        [StringLength(256)]
        public string Subjects { get; set; } // e.g. Mathematics (SS3), Further Mathematics (SS1-2)

        [StringLength(128)]
        public string Qualifications { get; set; }

        public short StartYear { get; set; } //year the teacher started teaching

        public bool IsVerified { get; set; }

        [ForeignKey("UserID")]
        public IEUser User { get; set; }

        [ForeignKey("LocationID")]
        public Location Location { get; set; }

    }
}