using AppSecrets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace IEGen.Models
{
    public class FileContext : DbContext
    {
        public static string ConnString = "Data Source=" + ConfigurationManager.ConnectionStrings["FileDataSource"].ConnectionString + Secrets.LocalConnPwdString;

        public FileContext() : base(ConnString) { Configuration.LazyLoadingEnabled = false; }

        public DbSet<ExtLogoFile> LogoFileList { get; set; }
        public DbSet<ExtPictureFile> PictureFileList { get; set; }
        public DbSet<ExtResultFile> ResultFileList { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //polymorphic classes
            modelBuilder.Entity<ExtLogoFile>().Map(m => m.MapInheritedProperties());
            modelBuilder.Entity<ExtPictureFile>().Map(m => m.MapInheritedProperties());
            modelBuilder.Entity<ExtResultFile>().Map(m => m.MapInheritedProperties());

        }
    }

    public class UserContext : DbContext
    {
        public UserContext() : base(IEContext.ConnString)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
        }

        public DbSet<AccessGroup> AccessGroupList { get; set; }
        public DbSet<AccessGroupRole> AccessGroupRoleList { get; set; }
        public DbSet<IEUser> IEUserList { get; set; }
        public DbSet<UserRole> UserRoleList { get; set; }
    }

    public class IEContext : DbContext
    {
        public static string ConnString = "Data Source=" + ConfigurationManager.ConnectionStrings["DataSource"].ConnectionString + Secrets.LocalConnPwdString;
        //public static string ConnString = Secrets.AzureTestConnString;
        //public static string ConnString = Secrets.AzureConnString;

        public IEContext() : base(ConnString)
        {
            //Database.SetInitializer<OrganizationContext>(new MigrateDatabaseToLatestVersion<OrganizationContext, RetreatMS.Migrations.Configuration>());

            Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<AccessGroup> AccessGroupList { get; set; }
        public DbSet<AccessGroupRole> AccessGroupRoleList { get; set; }
        public DbSet<PasswordReset> PasswordResetList { get; set; }
        public DbSet<PasswordResetComplete> PasswordResetCompleteList { get; set; }

        public DbSet<IEUser> IEUserList { get; set; }
        public DbSet<UserDefaults> UserDefaultsList { get; set; }
        public DbSet<UserAccountComplaint> UserAccountComplaintList { get; set; }
        public DbSet<UserRole> UserRoleList { get; set; }
        public DbSet<EmailMessage> EmailMessageList { get; set; }
        public DbSet<EmailSettings> EmailSettingsList { get; set; }

        public DbSet<School> SchoolList { get; set; }
        public DbSet<SchoolReg> SchoolRegList { get; set; }
        public DbSet<SchoolRequest> SchoolRequestList { get; set; }
        public DbSet<SchoolData> SchoolDataList { get; set; }
        public DbSet<Location> LocationList { get; set; }
        public DbSet<Student> StudentList { get; set; }
        public DbSet<StudentPastResult> StudentPastResultList { get; set; }
        public DbSet<Teacher> TeacherList { get; set; }
        public DbSet<TeacherRole> TeacherRoleList { get; set; }
        public DbSet<CTeacher> CTeacherList { get; set; }

        public DbSet<Term> TermList { get; set; }
        public DbSet<GradeGroup> GradeGroupList { get; set; }
        public DbSet<Grade> GradeList { get; set; }
        public DbSet<SkillGroup> SkillGroupList { get; set; }
        public DbSet<SkillGrade> SkillGradeList { get; set; }
        public DbSet<Class> ClassList { get; set; }
        public DbSet<ClassArm> ClassArmList { get; set; }
        public DbSet<SubjectTemplate> SubjectTemplateList { get; set; }
        public DbSet<Subject> SubjectList { get; set; }

        public DbSet<OtherExamType> OtherExamTypeList { get; set; }
        public DbSet<OtherExam> OtherExamList { get; set; }
        //public DbSet<OtherExamSubject> OtherExamSubjectList { get; set; }
        public DbSet<ScoreEntry> ScoreEntryList { get; set; }
        public DbSet<ComplexScoreFormat> ComplexScoreFormatList { get; set; }
        public DbSet<ClassScoreFormat> ClassScoreFormatList { get; set; }
        public DbSet<SubjectScoreFormat> SubjectScoreFormatList { get; set; }
        public DbSet<ComplexScoreEntry> ComplexScoreEntryList { get; set; }
        public DbSet<Attendance> AttendanceList { get; set; }
        public DbSet<StudentSkills> StudentSkillsList { get; set; }
        //public DbSet<FacultyComments> FacultyCommentsList { get; set; }
        public DbSet<ImprovementCommentGroup> ImprovementCommentGroupList { get; set; }
        public DbSet<ImprovementComment> ImprovementCommentList { get; set; }
        public DbSet<PerformanceCommentGroup> PerformanceCommentGroupList { get; set; }
        public DbSet<PerformanceComment> PerformanceCommentList { get; set; }
        public DbSet<PromotionCommentGroup> PromotionCommentGroupList { get; set; }
        public DbSet<PromotionComment> PromotionCommentList { get; set; }
        public DbSet<Assessment> AssessmentList { get; set; }

        public DbSet<AssessmentResult> AssessmentResultList { get; set; }
        public DbSet<OtherExamScore> OtherExamScoreList { get; set; }
        public DbSet<OtherExamResult> OtherExamResultList { get; set; }
        public DbSet<StudentResult> StudentResultList { get; set; }
        public DbSet<SchoolYearStats> SchoolYearStatsList { get; set; }
        public DbSet<YearClassStats> YearClassStatsList { get; set; }
        public DbSet<TermSubjectStats> TermSubjectStatsList { get; set; }
        public DbSet<ClassResult> ClassResultList { get; set; }
        public DbSet<TermResult> TermResultList { get; set; }
        public DbSet<TermSubjectCategoryStats> TermSubjectCategoryStatsList { get; set; }
        public DbSet<TermSubjectCategoryLevelStats> TermSubjectCategoryLevelStatsList { get; set; }

        public DbSet<ClassTypeStats> ClassTypeStatsList { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // use the following to add two applications on the same database... make sure to replicate for all the contexts
            //modelBuilder.HasDefaultSchema("PerfectHR");

            modelBuilder.Properties<decimal>().Configure(d => d.HasPrecision(9, 5));

            modelBuilder.Properties<DateTime>().Configure(d => d.HasColumnType("date"));

            modelBuilder.Entity<AccessGroup>().Property(p => p.TimeChanged).HasColumnType("datetime2").HasPrecision(2);
            modelBuilder.Entity<PasswordReset>().Property(p => p.RequestTime).HasColumnType("datetime2").HasPrecision(2);

            modelBuilder.Entity<Subject>().Property(p => p.TimeEntered).HasColumnType("datetime2").HasPrecision(2);
            modelBuilder.Entity<Subject>().Property(p => p.TimeVerified).HasColumnType("datetime2").HasPrecision(2);

            modelBuilder.Entity<ClassResult>().Property(p => p.AnalysisTime).HasColumnType("datetime2").HasPrecision(2);
            modelBuilder.Entity<TermResult>().Property(p => p.AnalysisTime).HasColumnType("datetime2").HasPrecision(2);
            modelBuilder.Entity<SchoolData>().Property(p => p.AnalysisTime).HasColumnType("datetime2").HasPrecision(2);

            //polymorphic classes
            //modelBuilder.Entity<StaffPicture>().Map(m => m.MapInheritedProperties());
            //modelBuilder.Entity<StaffPicture>().Property(m => m.StaffPictureID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);



            //ClosedTicket
            //modelBuilder.Entity<ClosedTicket>()
            //            .Property(o => o.TicketID)
            //            .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);


        }
    }
}