using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace IEGen.Models
{
    /// <summary>
    /// Contains the subject results for a particular student in a particular subject 
    /// </summary>
    public class ScoreEntry
    {
        public ScoreEntry() { }

        [Key, Column(Order = 1)]
        public int SubjectID { get; set; }

        [Key, Column(Order = 2)]
        public int StudentID { get; set; }

        public byte SerialNumber { get; set; }

        public decimal? CAScore { get; set; }
        public decimal? ExamScore { get; set; } // will hold the total score if there is no CA

        public int? GradeID { get; set; }

        [ForeignKey("SubjectID")]
        public Subject Subject { get; set; }

        [ForeignKey("StudentID")]
        public Student Student { get; set; }

        [ForeignKey("GradeID")]
        public Grade Grade { get; set; }
    }

    /// <summary>
    /// On create of Complex Result Format check if the format conforms to an existing format THAT HAS BEEN USED and just reference it.
    /// </summary>
    public class ComplexScoreFormat
    {
        public ComplexScoreFormat() { }

        [Key]
        public int FormatID { get; set; }

        public int SchoolID { get; set; }

        public byte CA1Weight { get; set; }
        public byte CA2Weight { get; set; }
        public byte CA3Weight { get; set; }
        public byte CA4Weight { get; set; }
        public byte ExamWeight { get; set; }        

        [ForeignKey("SchoolID")]
        public School School { get; set; }

        [InverseProperty("Format")]
        public List<ClassScoreFormat> Classes { get; set; }

        [InverseProperty("Format")]
        public List<SubjectScoreFormat> Subjects { get; set; }

    }

    /// <summary>
    /// If a class has this reference then all subjects will reference it by default though Subject Result Formats..
    /// </summary>
    public class ClassScoreFormat
    {
        public ClassScoreFormat() { }

        [Key]
        public int ClassID { get; set; }

        public int FormatID { get; set; }

        [ForeignKey("FormatID")]
        public ComplexScoreFormat Format { get; set; }

        [ForeignKey("ClassID")]
        public Class Class { get; set; }
    }

    public class SubjectScoreFormat
    {
        public SubjectScoreFormat() { }

        [Key]
        public int SubjectID { get; set; }

        public int FormatID { get; set; }

        [ForeignKey("FormatID")]
        public ComplexScoreFormat Format { get; set; }

        [ForeignKey("SubjectID")]
        public Subject Subject { get; set; }
    }

    public class ComplexScoreEntry
    {
        public ComplexScoreEntry() { }

        [Key, Column(Order = 1)]
        public int SubjectID { get; set; }

        [Key, Column(Order = 2)]
        public int StudentID { get; set; }

        public byte SerialNumber { get; set; }

        public decimal? CA1Score { get; set; }
        public decimal? CA2Score { get; set; }
        public decimal? CA3Score { get; set; }
        public decimal? CA4Score { get; set; }
        public decimal? ExamScore { get; set; }

        [ForeignKey("StudentID")]
        public Student Student { get; set; }
    }

    /// <summary>
    /// Attendance Object
    /// </summary>
    public class Attendance
    {
        public Attendance() { }

        [Key, Column(Order = 1)]
        public int ClassID { get; set; }

        [Key, Column(Order = 2)]
        public int StudentID { get; set; }

        public byte SerialNumber { get; set; }

        public byte Opened { get; set; }
        public byte Present { get; set; }
        public byte Absent { get; set; }
        public byte Punctual { get; set; }

        [ForeignKey("StudentID")]
        public Student Student { get; set; }

        [ForeignKey("ClassID")]
        public Class Class { get; set; }
    }

    public class StudentSkills
    {
        public StudentSkills() { }

        [Key, Column(Order = 1)]
        public int ClassID { get; set; }

        [Key, Column(Order = 2)]
        public int StudentID { get; set; }

        public byte SerialNumber { get; set; }

        public byte? SkillScore1 { get; set; }
        public byte? SkillScore2 { get; set; }
        public byte? SkillScore3 { get; set; }
        public byte? SkillScore4 { get; set; }
        public byte? SkillScore5 { get; set; }
        public byte? SkillScore6 { get; set; }

        [ForeignKey("ClassID")]
        public Class Class { get; set; }

        [ForeignKey("StudentID")]
        public Student Student { get; set; }
    }

    public class FacultyComments
    {
        public FacultyComments() { }

        [Key, Column(Order = 1)]
        public int ClassID { get; set; }

        [Key, Column(Order = 2)]
        public int StudentID { get; set; }

        [StringLength(512)]
        public string CTComment { get; set; } //Class teacher's comments

        [StringLength(512)]
        public string PComment { get; set; }   //Principal's Comments

        [ForeignKey("ClassID")]
        public Class Class { get; set; }

        [ForeignKey("StudentID")]
        public Student Student { get; set; }
    }

    public class ImprovementCommentGroup
    {
        public ImprovementCommentGroup() { }

        [Key]
        public int GroupID { get; set; }

        public int SchoolID { get; set; }

        [StringLength(32), Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        [ForeignKey("SchoolID")]
        public School School { get; set; }

        [InverseProperty("Group")]
        public List<ImprovementComment> Comments { get; set; }

        [InverseProperty("ImprovementCommentGroup")]
        public List<Class> Classes { get; set; }
    }

    public class ImprovementComment
    {
        public ImprovementComment() { }

        [Key]
        public int CommentID { get; set; }

        public int GroupID { get; set; }

        public byte MinFailCount { get; set; }
        public byte MaxFailCount { get; set; }

        [StringLength(128)]
        public string Comment { get; set; }

        [ForeignKey("GroupID")]
        public ImprovementCommentGroup Group { get; set; }
    }

    public class PerformanceCommentGroup
    {
        public PerformanceCommentGroup() { }

        [Key]
        public int GroupID { get; set; }

        public int SchoolID { get; set; }

        [StringLength(32), Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        [ForeignKey("SchoolID")]
        public School School { get; set; }

        [InverseProperty("Group")]
        public List<PerformanceComment> Comments { get; set; }

        [InverseProperty("PerformanceCommentGroup")]
        public List<Class> Classes { get; set; }
    }

    public class PerformanceComment
    {
        public PerformanceComment() { }

        [Key]
        public int CommentID { get; set; }

        public int GroupID { get; set; }

        public byte LowerBound { get; set; }
        public byte UpperBound { get; set; }

        [StringLength(128)]
        public string Comment { get; set; }

        [ForeignKey("GroupID")]
        public PerformanceCommentGroup Group { get; set; }
    }

    public class PromotionCommentGroup
    {
        public PromotionCommentGroup() { }

        [Key]
        public int GroupID { get; set; }

        public int SchoolID { get; set; }

        [StringLength(32), Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; }

        [ForeignKey("SchoolID")]
        public School School { get; set; }

        [InverseProperty("Group")]
        public List<PromotionComment> Comments { get; set; }

        [InverseProperty("PromotionCommentGroup")]
        public List<Class> Classes { get; set; }
    }

    public class PromotionComment
    {
        public PromotionComment() { }

        [Key]
        public int CommentID { get; set; }

        public int GroupID { get; set; }

        public byte LowerBound { get; set; }
        public byte UpperBound { get; set; }

        [StringLength(128)]
        public string Comment { get; set; }

        [ForeignKey("GroupID")]
        public PromotionCommentGroup Group { get; set; }
    }

    /// <summary>
    /// Assessments are unique to subjects and are graded based on the TotalScore in the Assessment definition
    /// </summary>
    public class AssessmentResult
    {
        public AssessmentResult() { }

        [Key, Column(Order = 1)]
        public int AssessmentID { get; set; }

        [Key, Column(Order = 2)]
        public int StudentID { get; set; }

        public decimal Score { get; set; }

        public int? GradeID { get; set; }

        [ForeignKey("GradeID")]
        public Grade Grade { get; set; }

        [ForeignKey("StudentID")]
        public Student Student { get; set; }

        [ForeignKey("AssessmentID")]
        public Assessment Assessment { get; set; }
    }

    /// <summary>
    /// Scores are always entered in percentages or saved as percentages
    /// </summary>
    public class OtherExamScore
    {
        public OtherExamScore() { }

        [Key, Column(Order = 1)]
        public int OtherExamID { get; set; }

        [Key, Column(Order = 2)]
        public int SubjectID { get; set; }

        [Key, Column(Order = 3)]
        public int StudentID { get; set; }

        public decimal Score { get; set; }

        public int? GradeID { get; set; }

        [ForeignKey("GradeID")]
        public Grade Grade { get; set; }

        [ForeignKey("StudentID")]
        public Student Student { get; set; }

        [ForeignKey("SubjectID")]
        public Subject Subject { get; set; }

        [ForeignKey("OtherExamID")]
        public OtherExam Exam { get; set; }
    }

    /// <summary>
    /// Student Result. Has a reference to the student picture at the time of creation... via the guidstring
    /// </summary>
    public class OtherExamResult : UploadedFile
    {
        public OtherExamResult() { }

        [Key, Column(Order = 1)]
        public int OtherExamID { get; set; }

        [Key, Column(Order = 2)]
        public int StudentID { get; set; }

        public decimal LowestScore { get; set; }
        public decimal AverageScore { get; set; }
        public decimal HighestScore { get; set; }
        public byte Position { get; set; }

        public byte SubjectCount { get; set; }

        public int? BestSubjectID { get; set; }

        [StringLength(512)]
        public string TeacherComment { get; set; }

        [StringLength(512)]
        public string PerformanceComment { get; set; }

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

        [ForeignKey("StudentID")]
        public Student Student { get; set; }

        [ForeignKey("OtherExamID")]
        public OtherExam Exam { get; set; }

        [ForeignKey("BestSubjectID")]
        public Subject BestSubject { get; set; }
    }

    /// <summary>
    /// Student Result. Has a reference to the student picture at the time of creation... via the guidstring
    /// </summary>
    public class StudentResult : UploadedFile
    {
        public StudentResult() { }

        [Key, Column(Order = 1)]
        public int ClassID { get; set; }

        [Key, Column(Order = 2)]
        public int StudentID { get; set; }

        public decimal LowestScore { get; set; }
        public decimal AverageScore { get; set; }
        public decimal HighestScore { get; set; }
        public decimal Improvement { get; set; }  // points improvement from last term
        public byte Position { get; set; }

        public byte SubjectCount { get; set; }

        public int? BestSubjectID { get; set; }

        [StringLength(512)]
        public string TeacherComment { get; set; }

        [StringLength(512)]
        public string PrincipalComment { get; set; }

        public byte GradeCountA { get; set; }
        public byte GradeCountB { get; set; }
        public byte GradeCountC { get; set; }
        public byte GradeCountD { get; set; }
        public byte GradeCountE { get; set; }
        public byte GradeCountF { get; set; }

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

        [ForeignKey("StudentID")]
        public Student Student { get; set; }

        [ForeignKey("ClassID")]
        public Class Class { get; set; }

        [ForeignKey("BestSubjectID")]
        public Subject BestSubject { get; set; }
    }

    /// <summary>
    /// Student Result external to the current server
    /// </summary>
    public class StudentPastResult
    {
        public StudentPastResult() { }

        [Key, Column(Order = 1)]
        public int StudentID { get; set; }

        [Key, Column(Order = 2)]
        public int TemplateID { get; set; }

        public short Year { get; set; }

        public decimal? Term1Score { get; set; }

        public decimal? Term2Score { get; set; }

        public decimal? Term3Score { get; set; }

        [ForeignKey("StudentID")]
        public Student Student { get; set; }

        [ForeignKey("TemplateID")]
        public SubjectTemplate Subject { get; set; }
    }

    public class ClassResult
    {
        public ClassResult() { }

        [Key]
        public int ClassID { get; set; }

        public byte ResultCount { get; set; }
        public byte SubjectCount { get; set; }

        public decimal LowestAverage { get; set; }
        public decimal MeanAverage { get; set; }
        public decimal BestAverage { get; set; }

        public int BestStudentID { get; set; }
        public int BestSubjectID { get; set; }

        public DateTime AnalysisTime { get; set; }

        [ForeignKey("BestStudentID")]
        public Student BestStudent { get; set; }

        [ForeignKey("BestSubjectID")]
        public Subject BestSubject { get; set; }

        [ForeignKey("ClassID")]
        public Class Class { get; set; }
    }

    public class TermResult
    {
        public TermResult() { }

        [Key]
        public int TermID { get; set; }

        public short ResultCount { get; set; }
        public short SubjectCount { get; set; }

        public decimal LowestAverage { get; set; }
        public decimal MeanAverage { get; set; }
        public decimal BestAverage { get; set; }

        public int BestStudentID { get; set; }
        public int BestSubjectID { get; set; }
        public int? BestClassID { get; set; }

        public DateTime AnalysisTime { get; set; }

        [ForeignKey("BestStudentID")]
        public Student BestStudent { get; set; }

        [ForeignKey("BestSubjectID")]
        public Subject BestSubject { get; set; }

        [ForeignKey("BestClassID")]
        public ClassResult BestClass { get; set; }

        [ForeignKey("TermID")]
        public Term Term { get; set; }
    }

    /// <summary>
    /// Gives statistics for a particular subject category in a particular term. 
    /// </summary>
    public class TermSubjectCategoryStats
    {
        public TermSubjectCategoryStats() { }

        [Key, Column(Order = 1)]
        public int TermID { get; set; }

        [Key, Column(Order = 2)]
        public byte CategoryID { get; set; }

        public decimal LowestScore { get; set; }
        public decimal AverageScore { get; set; }
        public decimal HighestScore { get; set; }

        public short ACount { get; set; }
        public short BCount { get; set; }
        public short CCount { get; set; }
        public short DCount { get; set; }
        public short ECount { get; set; }
        public short FCount { get; set; }

        public short ResultCount { get; set; }
        public byte SubjectCount { get; set; }

        public decimal BestStudentAverage { get; set; }
        public int? BestStudentID { get; set; }

        public decimal BestSubjectAverage { get; set; }
        public int? BestSubjectID { get; set; }

        [ForeignKey("BestStudentID")]
        public Student BestStudent { get; set; }

        [ForeignKey("BestSubjectID")]
        public Subject BestSubject { get; set; }

    }

    public class TermSubjectCategoryLevelStats
    {
        public TermSubjectCategoryLevelStats() { }

        [Key, Column(Order = 1)]
        public int TermID { get; set; }

        [Key, Column(Order = 2)]
        public byte CategoryID { get; set; }

        [Key, Column(Order = 3)]
        public byte LevelID { get; set; }

        public decimal LowestScore { get; set; }
        public decimal AverageScore { get; set; }
        public decimal HighestScore { get; set; }

        public short ResultCount { get; set; }
        public byte SubjectCount { get; set; }

        public decimal BestAverage { get; set; }
        public int? BestStudentID { get; set; }

        [ForeignKey("BestStudentID")]
        public Student BestStudent { get; set; }

    }

    /// <summary>
    /// Gives statistics for a particular subject Definition in a particular term. Replaces COURSE_DEFINITION in IPSEDU
    /// </summary>
    public class TermSubjectStats
    {
        public TermSubjectStats() { }

        [Key, Column(Order = 1)]
        public int TermID { get; set; }

        [Key, Column(Order = 2)]
        public int TemplateID { get; set; }

        public decimal LowestScore { get; set; }
        public decimal AverageScore { get; set; }
        public decimal HighestScore { get; set; }

        public byte SubjectCount { get; set; }
        public short ResultCount { get; set; }
        public int? BestStudentID { get; set; }

        [ForeignKey("BestStudentID")]
        public Student BestStudent { get; set; }

        [ForeignKey("TemplateID")]
        public SubjectTemplate Template { get; set; }

        [ForeignKey("TermID")]
        public Term Term { get; set; }

    }

    public class ClassTypeStats
    {
        public ClassTypeStats() { }

        [Key, Column(Order = 1)]
        public int SchoolID { get; set; }

        [Key, Column(Order = 2)]
        public byte ClassLevelID { get; set; }

        [Key, Column(Order = 3)]
        public byte ClassTypeID { get; set; }

        public short ClassCount { get; set; }
        public int ResultCount { get; set; }

        public decimal CurrentAverage { get; set; }
        public decimal LowestAverage { get; set; }
        public decimal MeanAverage { get; set; }
        public decimal BestAverage { get; set; }
    }



    /// <summary>
    /// Replaces ACADEMIC_YEAR. Compute it from averages in StudentResults and YearSubjectStats for Best Subject
    /// </summary>
    public class SchoolYearStats
    {
        public SchoolYearStats() { }

        [Key, Column(Order = 1)]
        public int SchoolID { get; set; }

        [Key, Column(Order = 2)]
        public short StartYear { get; set; }

        [StringLength(32), Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        public decimal LowestAverage { get; set; }
        public decimal MeanAverage { get; set; }
        public decimal BestAverage { get; set; }

        public short ResultCount { get; set; }
        public int? BestStudentID { get; set; }

        public short SubjectCount { get; set; }
        public int? BestSubjectID { get; set; }

        public DateTime AnalysisTime { get; set; }

        [ForeignKey("BestSubjectID")]
        public Subject BestSubject { get; set; }

        [ForeignKey("BestStudentID")]
        public Student BestStudent { get; set; }

        [ForeignKey("SchoolID")]
        public School School { get; set; }
    }

    /// <summary>
    /// Year Statistics for a particular class (it merges classes of the same level and class type). Replaces AY_CLASS
    /// Compute from StudentResults for terms in the specified academic year
    /// Display Summaries of student results for the three terms in consideration... and the averages
    /// Year subjects are very vague since they aggregate different students, teachers and terms... 
    /// </summary>
    public class YearClassStats
    {
        public YearClassStats() { }

        [Key, Column(Order = 1)]
        public int SchoolID { get; set; }

        [Key, Column(Order = 2)]
        public short StartYear { get; set; }

        [Key, Column(Order = 3)]
        public byte ClassLevelID { get; set; }

        [Key, Column(Order = 4)]
        public byte ClassTypeID { get; set; }


        public decimal LowestAverage { get; set; }
        public decimal MeanAverage { get; set; }
        public decimal BestAverage { get; set; }

        public int? BestStudentID { get; set; }

        [ForeignKey("BestStudentID")]
        public Student BestStudent { get; set; }

        [ForeignKey("SchoolID")]
        public School School { get; set; }
    }

    /// <summary>
    /// Year statistics per subject template...
    /// </summary>
    public class YearSubjectStats
    {
        public YearSubjectStats() { }

        [Key, Column(Order = 1)]
        public short YearID { get; set; }

        [Key, Column(Order = 2)]
        public int TemplateID { get; set; }

        public decimal LowestAverage { get; set; }
        public decimal MeanAverage { get; set; }
        public decimal BestAverage { get; set; }

        public byte SubjectCount { get; set; }
        public short ResultCount { get; set; }
        public int? BestStudentID { get; set; }

        [ForeignKey("BestStudentID")]
        public Student BestStudent { get; set; }

        [ForeignKey("TemplateID")]
        public SubjectTemplate Template { get; set; }

    }

}