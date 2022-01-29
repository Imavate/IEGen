namespace IEGen.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Configuration;

    public class IPSContext : DbContext
    {
        public static string ConnString = "Data Source=" + ConfigurationManager.ConnectionStrings["IPSDataSource"].ConnectionString + AppSecrets.Secrets.IPSConnPwdString;

        public IPSContext() : base(ConnString)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.AutoDetectChangesEnabled = false;
        }

        public virtual DbSet<ED_ACADEMIC_TERM> ED_ACADEMIC_TERM { get; set; }
        public virtual DbSet<ED_CLASS> ED_CLASS { get; set; }
        public virtual DbSet<ED_COURSE> ED_COURSE { get; set; }
        public virtual DbSet<ED_COURSE_DEFINITION> ED_COURSE_DEFINITION { get; set; }
        public virtual DbSet<ED_STUDENT> ED_STUDENT { get; set; }
        public virtual DbSet<ED_STUDENT_COURSE> ED_STUDENT_COURSE { get; set; }
        public virtual DbSet<ED_TERM_GRADE> ED_TERM_GRADE { get; set; }
        public virtual DbSet<FILE_IMAGE> FILE_IMAGE { get; set; }
        public virtual DbSet<INSTITUTION> INSTITUTIONs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ED_ACADEMIC_TERM>()
                .Property(e => e.TERM_CAPTION)
                .IsUnicode(false);

            modelBuilder.Entity<ED_ACADEMIC_TERM>()
                .Property(e => e.MEAN_AVERAGE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_ACADEMIC_TERM>()
                .Property(e => e.BEST_AVERAGE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_ACADEMIC_TERM>()
                .Property(e => e.LOWEST_AVERAGE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_ACADEMIC_TERM>()
                .HasMany(e => e.ED_CLASS)
                .WithOptional(e => e.ED_ACADEMIC_TERM)
                .HasForeignKey(e => e.ACADEMIC_TERM_ID);

            modelBuilder.Entity<ED_ACADEMIC_TERM>()
                .HasMany(e => e.ED_COURSE1)
                .WithOptional(e => e.ED_ACADEMIC_TERM1)
                .HasForeignKey(e => e.ACADEMIC_TERM_ID);

            modelBuilder.Entity<ED_ACADEMIC_TERM>()
                .HasMany(e => e.ED_STUDENT_COURSE)
                .WithOptional(e => e.ED_ACADEMIC_TERM)
                .HasForeignKey(e => e.ACADEMIC_TERM_ID);

            modelBuilder.Entity<ED_CLASS>()
                .Property(e => e.MEAN_AVERAGE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_CLASS>()
                .Property(e => e.BEST_AVERAGE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_CLASS>()
                .Property(e => e.CLASS_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<ED_CLASS>()
                .Property(e => e.LOWEST_AVERAGE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_CLASS>()
                .Property(e => e.RED_FLAG_CUT_OFF)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_CLASS>()
                .HasMany(e => e.ED_COURSE1)
                .WithOptional(e => e.ED_CLASS1)
                .HasForeignKey(e => e.CLASS_ID);

            modelBuilder.Entity<ED_CLASS>()
                .HasMany(e => e.ED_STUDENT1)
                .WithOptional(e => e.ED_CLASS1)
                .HasForeignKey(e => e.CLASS_ID);

            modelBuilder.Entity<ED_COURSE>()
                .Property(e => e.COURSE_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<ED_COURSE>()
                .Property(e => e.LECTURER_CONTACT)
                .IsUnicode(false);

            modelBuilder.Entity<ED_COURSE>()
                .Property(e => e.AVERAGE_SCORE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_COURSE>()
                .Property(e => e.HIGHEST_SCORE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_COURSE>()
                .Property(e => e.RESULT_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<ED_COURSE>()
                .Property(e => e.LOWEST_SCORE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_COURSE>()
                .Property(e => e.PERCENT_CORRECTED)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_COURSE>()
                .HasMany(e => e.ED_ACADEMIC_TERM)
                .WithOptional(e => e.ED_COURSE)
                .HasForeignKey(e => e.BEST_COURSE);

            modelBuilder.Entity<ED_COURSE>()
                .HasMany(e => e.ED_CLASS)
                .WithOptional(e => e.ED_COURSE)
                .HasForeignKey(e => e.BEST_COURSE);

            modelBuilder.Entity<ED_STUDENT>()
                .Property(e => e.DISPLAY_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<ED_STUDENT>()
                .Property(e => e.STUDENT_CODE)
                .IsUnicode(false);

            modelBuilder.Entity<ED_STUDENT>()
                .Property(e => e.CURRENT_AVERAGE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_STUDENT>()
                .Property(e => e.MEAN_AVERAGE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_STUDENT>()
                .Property(e => e.BEST_AVERAGE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_STUDENT>()
                .HasMany(e => e.ED_ACADEMIC_TERM)
                .WithOptional(e => e.ED_STUDENT)
                .HasForeignKey(e => e.BEST_STUDENT);

            modelBuilder.Entity<ED_STUDENT>()
                .HasMany(e => e.ED_CLASS)
                .WithOptional(e => e.ED_STUDENT)
                .HasForeignKey(e => e.BEST_STUDENT);

            modelBuilder.Entity<ED_STUDENT>()
                .HasMany(e => e.ED_COURSE)
                .WithOptional(e => e.ED_STUDENT)
                .HasForeignKey(e => e.BEST_STUDENT);

            modelBuilder.Entity<ED_STUDENT_COURSE>()
                .Property(e => e.PERCENT_SCORE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_STUDENT_COURSE>()
                .Property(e => e.AVERAGE_SCORE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_STUDENT_COURSE>()
                .Property(e => e.HIGHEST_SCORE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<ED_STUDENT_COURSE>()
                .Property(e => e.COMMENTS)
                .IsUnicode(false);

            modelBuilder.Entity<ED_TERM_GRADE>()
                .Property(e => e.LOWER_BOUND)
                .HasPrecision(9, 6);

            modelBuilder.Entity<ED_TERM_GRADE>()
                .Property(e => e.UPPER_BOUND)
                .HasPrecision(9, 6);

            modelBuilder.Entity<ED_TERM_GRADE>()
                .Property(e => e.GRADE_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<FILE_IMAGE>()
                .Property(e => e.FILE_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<FILE_IMAGE>()
                .Property(e => e.FILE_TYPE)
                .IsUnicode(false);

            modelBuilder.Entity<FILE_IMAGE>()
                .Property(e => e.DESCRIPTION)
                .IsUnicode(false);

            modelBuilder.Entity<FILE_IMAGE>()
                .HasMany(e => e.ED_STUDENT)
                .WithOptional(e => e.FILE_IMAGE)
                .HasForeignKey(e => e.PICTURE_FILE_ID);

            modelBuilder.Entity<FILE_IMAGE>()
                .HasMany(e => e.INSTITUTIONs)
                .WithOptional(e => e.FILE_IMAGE)
                .HasForeignKey(e => e.LOGO_FILE_ID);

            modelBuilder.Entity<FILE_IMAGE>()
                .HasMany(e => e.INSTITUTIONs1)
                .WithOptional(e => e.FILE_IMAGE1)
                .HasForeignKey(e => e.DEF_STUDENT_PIC_ID);

            modelBuilder.Entity<INSTITUTION>()
                .Property(e => e.INSTITUTION_NAME)
                .IsUnicode(false);

            modelBuilder.Entity<INSTITUTION>()
                .Property(e => e.CURRENT_AVERAGE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<INSTITUTION>()
                .Property(e => e.MEAN_AVERAGE)
                .HasPrecision(9, 8);

            modelBuilder.Entity<INSTITUTION>()
                .Property(e => e.BEST_AVERAGE)
                .HasPrecision(9, 8);
        }
    }

    public enum IPSLevel : byte
    {
        [Display(Name = "JS 1")]
        JS1 = 1,

        [Display(Name = "JS 2")]
        JS2 = 2,

        [Display(Name = "JS 3")]
        JS3 = 3,

        [Display(Name = "SS 1")]
        SS1 = 4,

        [Display(Name = "SS 2")]
        SS2 = 5,

        [Display(Name = "SS 3")]
        SS3 = 6,

        [Display(Name = "Play Group")]
        PlayGroup = 16,

        [Display(Name = "Reception")]
        Reception = 17,

        [Display(Name = "Nursery 1")]
        Nursery1 = 18,

        [Display(Name = "Nursery 2")]
        Nursery2 = 19,

        [Display(Name = "Basic 1")]
        Basic1 = 20,

        [Display(Name = "Basic 2")]
        Basic2 = 21,

        [Display(Name = "Basic 3")]
        Basic3 = 22,

        [Display(Name = "Basic 4")]
        Basic4 = 23,

        [Display(Name = "Basic 5")]
        Basic5 = 24,

        [Display(Name = "Basic 6")]
        Basic6 = 25,

    }

    public partial class ED_ACADEMIC_TERM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ED_ACADEMIC_TERM()
        {
            ED_CLASS = new HashSet<ED_CLASS>();
            ED_COURSE1 = new HashSet<ED_COURSE>();
            ED_STUDENT_COURSE = new HashSet<ED_STUDENT_COURSE>();
            ED_TERM_GRADE = new HashSet<ED_TERM_GRADE>();
        }

        [Key]
        public int TERM_ID { get; set; }

        public int? A_YEAR_ID { get; set; }

        public int? INSTITUTION_ID { get; set; }

        public byte? TERM_NUMBER { get; set; }

        [StringLength(50)]
        public string TERM_CAPTION { get; set; }

        public decimal? MEAN_AVERAGE { get; set; }

        public decimal? BEST_AVERAGE { get; set; }

        public int? BEST_COURSE { get; set; }

        public int? BEST_STUDENT { get; set; }

        public int? STUDENT_COUNT { get; set; }

        public short? COURSE_COUNT { get; set; }

        public DateTime? NEXT_RESUMPTION_DATE { get; set; }

        public decimal? LOWEST_AVERAGE { get; set; }

        public byte? DAYS_OPENED { get; set; }

        public short? ZX_LOC_ID { get; set; }

        public int? ZX_LOC_DATA_ID { get; set; }

        public virtual ED_COURSE ED_COURSE { get; set; }

        public virtual ED_STUDENT ED_STUDENT { get; set; }

        public virtual INSTITUTION INSTITUTION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_CLASS> ED_CLASS { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_COURSE> ED_COURSE1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_STUDENT_COURSE> ED_STUDENT_COURSE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_TERM_GRADE> ED_TERM_GRADE { get; set; }
    }

    public partial class ED_CLASS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ED_CLASS()
        {
            ED_COURSE1 = new HashSet<ED_COURSE>();
            ED_STUDENT1 = new HashSet<ED_STUDENT>();
        }

        [Key]
        public int CLASS_ID { get; set; }

        public int? INSTITUTION_ID { get; set; }

        public int? SET_ID { get; set; }

        public int? ACADEMIC_TERM_ID { get; set; }

        public short? ACADEMIC_LEVEL_ID { get; set; }

        public short? REG_COURSE_COUNT { get; set; }

        public short? REG_STUDENT_COUNT { get; set; }

        public short? RES_COURSE_COUNT { get; set; }

        public short? RES_STUDENT_COUNT { get; set; }

        public decimal? MEAN_AVERAGE { get; set; }

        public decimal? BEST_AVERAGE { get; set; }

        public int? BEST_COURSE { get; set; }

        public int? BEST_STUDENT { get; set; }

        [StringLength(100)]
        public string CLASS_NAME { get; set; }

        public decimal? LOWEST_AVERAGE { get; set; }

        public byte? RESULT_TYPE_ID { get; set; }

        public bool? SHOW_POSITION { get; set; }

        public bool? SHOW_SUMMARY_GRADE { get; set; }

        public bool? SHOW_ATTENDANCE { get; set; }

        public bool? SHOW_QUALITATIVE_SKILL { get; set; }

        public bool? USE_PERFORMANCE_COMMENT { get; set; }

        public decimal? RED_FLAG_CUT_OFF { get; set; }

        public byte? DAYS_OPENED { get; set; }

        public bool? SHOW_CATEGORY_ANALYSIS { get; set; }

        public bool? USE_PROMOTIONAL_COMMENT { get; set; }

        public bool? USE_IMPROVEMENT_COMMENT { get; set; }

        public bool? IS_PROMOTIONAL_CLASS { get; set; }

        public int? AY_CLASS_ID { get; set; }

        public bool? IS_3T_PROMOTION_BASIS { get; set; }

        public bool? IS_3T_RESULT_BASIS { get; set; }

        public DateTime? TIME_QS_ENTERED { get; set; }

        public short? ZX_LOC_ID { get; set; }

        public int? ZX_LOC_DATA_ID { get; set; }

        public virtual ED_ACADEMIC_TERM ED_ACADEMIC_TERM { get; set; }

        public virtual ED_COURSE ED_COURSE { get; set; }

        public virtual ED_STUDENT ED_STUDENT { get; set; }

        public virtual INSTITUTION INSTITUTION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_COURSE> ED_COURSE1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_STUDENT> ED_STUDENT1 { get; set; }
    }

    public partial class ED_COURSE_DEFINITION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ED_COURSE_DEFINITION()
        {
            ED_COURSE = new HashSet<ED_COURSE>();
        }

        [Key]
        public int DEFINITION_ID { get; set; }

        public int? INSTITUTION_ID { get; set; }

        public short? CATEGORY_ID { get; set; }

        [StringLength(100)]
        public string DEFINITION_NAME { get; set; }

        [StringLength(50)]
        public string RESULT_NAME { get; set; }

        public short? ZX_LOC_ID { get; set; }

        public int? ZX_LOC_DATA_ID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_COURSE> ED_COURSE { get; set; }

        public virtual INSTITUTION INSTITUTION { get; set; }
    }

    public partial class ED_COURSE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ED_COURSE()
        {
            ED_ACADEMIC_TERM = new HashSet<ED_ACADEMIC_TERM>();
            ED_CLASS = new HashSet<ED_CLASS>();
            ED_STUDENT_COURSE = new HashSet<ED_STUDENT_COURSE>();
        }

        [Key]
        public int COURSE_ID { get; set; }

        public int? INSTITUTION_ID { get; set; }

        public short? CATEGORY_ID { get; set; }

        public int? DEFINITION_ID { get; set; }

        public short? COURSE_YEAR { get; set; }

        [StringLength(100)]
        public string COURSE_NAME { get; set; }

        public byte? COURSE_WEIGHT { get; set; }

        public bool? HAS_RESULTS { get; set; }

        public int? UNVERIFIED_RESULTS { get; set; }

        public short? ACADEMIC_LEVEL_ID { get; set; }

        public int? ACADEMIC_TERM_ID { get; set; }

        public int? LECTURER { get; set; }

        [StringLength(100)]
        public string LECTURER_CONTACT { get; set; }

        public decimal? AVERAGE_SCORE { get; set; }

        public decimal? HIGHEST_SCORE { get; set; }

        public short? STUDENT_COUNT { get; set; }

        public int? CLASS_ID { get; set; }

        [StringLength(50)]
        public string RESULT_NAME { get; set; }

        public decimal? LOWEST_SCORE { get; set; }

        public byte? SHEET_COL_NUMBER { get; set; }

        public int? BEST_STUDENT { get; set; }

        public int? ENTERED_BY { get; set; }

        public DateTime? TIME_ENTERED { get; set; }

        public int? VERIFIED_BY { get; set; }

        public DateTime? TIME_VERIFIED { get; set; }

        public decimal? PERCENT_CORRECTED { get; set; }

        public short? ZX_LOC_ID { get; set; }

        public int? ZX_LOC_DATA_ID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_ACADEMIC_TERM> ED_ACADEMIC_TERM { get; set; }

        public virtual ED_ACADEMIC_TERM ED_ACADEMIC_TERM1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_CLASS> ED_CLASS { get; set; }

        public virtual ED_CLASS ED_CLASS1 { get; set; }

        public virtual ED_STUDENT ED_STUDENT { get; set; }

        public virtual ED_COURSE_DEFINITION ED_COURSE_DEFINITION { get; set; }

        public virtual INSTITUTION INSTITUTION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_STUDENT_COURSE> ED_STUDENT_COURSE { get; set; }
    }

    public partial class ED_STUDENT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ED_STUDENT()
        {
            ED_ACADEMIC_TERM = new HashSet<ED_ACADEMIC_TERM>();
            ED_CLASS = new HashSet<ED_CLASS>();
            ED_COURSE = new HashSet<ED_COURSE>();
            ED_STUDENT_COURSE = new HashSet<ED_STUDENT_COURSE>();
        }

        [Key]
        public int STUDENT_ID { get; set; }

        public int? PERSON_ID { get; set; }

        [StringLength(200)]
        public string DISPLAY_NAME { get; set; }

        public int? INSTITUTION_ID { get; set; }

        public int? PROGRAMME_ID { get; set; }

        [StringLength(50)]
        public string STUDENT_CODE { get; set; }

        public short? LEVEL_ID { get; set; }

        public bool? IS_ACTIVE { get; set; }

        public int? SET_ID { get; set; }

        public int? CLASS_ID { get; set; }

        public int? PICTURE_FILE_ID { get; set; }

        public decimal? CURRENT_AVERAGE { get; set; }

        public decimal? MEAN_AVERAGE { get; set; }

        public decimal? BEST_AVERAGE { get; set; }

        public short? ZX_LOC_ID { get; set; }

        public int? ZX_LOC_DATA_ID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_ACADEMIC_TERM> ED_ACADEMIC_TERM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_CLASS> ED_CLASS { get; set; }

        public virtual ED_CLASS ED_CLASS1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_COURSE> ED_COURSE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_STUDENT_COURSE> ED_STUDENT_COURSE { get; set; }

        public virtual FILE_IMAGE FILE_IMAGE { get; set; }

        public virtual INSTITUTION INSTITUTION { get; set; }
    }

    public partial class ED_STUDENT_COURSE
    {
        [Key]
        public int SC_ID { get; set; }

        public int? STUDENT_ID { get; set; }

        public int? COURSE_ID { get; set; }

        public decimal? PERCENT_SCORE { get; set; }

        public decimal? AVERAGE_SCORE { get; set; }

        public decimal? HIGHEST_SCORE { get; set; }

        [StringLength(2000)]
        public string COMMENTS { get; set; }

        public int? ACADEMIC_TERM_ID { get; set; }

        public byte? WEIGHT_UPLOADED { get; set; }

        public short? ACADEMIC_LEVEL_ID { get; set; }

        public short? CATEGORY_ID { get; set; }

        public int? GRADE_ID { get; set; }

        public short? ZX_LOC_ID { get; set; }

        public int? ZX_LOC_DATA_ID { get; set; }

        public virtual ED_ACADEMIC_TERM ED_ACADEMIC_TERM { get; set; }

        public virtual ED_COURSE ED_COURSE { get; set; }

        public virtual ED_STUDENT ED_STUDENT { get; set; }

        public virtual ED_TERM_GRADE ED_TERM_GRADE { get; set; }
    }

    public partial class ED_TERM_GRADE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ED_TERM_GRADE()
        {
            ED_STUDENT_COURSE = new HashSet<ED_STUDENT_COURSE>();
        }

        [Key]
        public int GRADE_ID { get; set; }

        public int? TERM_ID { get; set; }

        public decimal? LOWER_BOUND { get; set; }

        public decimal? UPPER_BOUND { get; set; }

        [StringLength(10)]
        public string GRADE_NAME { get; set; }

        public byte? SG_NUMBER { get; set; }

        public short? ZX_LOC_ID { get; set; }

        public int? ZX_LOC_DATA_ID { get; set; }

        public virtual ED_ACADEMIC_TERM ED_ACADEMIC_TERM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_STUDENT_COURSE> ED_STUDENT_COURSE { get; set; }
    }

    public partial class FILE_IMAGE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FILE_IMAGE()
        {
            ED_STUDENT = new HashSet<ED_STUDENT>();
            INSTITUTIONs = new HashSet<INSTITUTION>();
            INSTITUTIONs1 = new HashSet<INSTITUTION>();
        }

        [Key]
        public int FILE_ID { get; set; }

        [StringLength(100)]
        public string FILE_NAME { get; set; }

        [StringLength(50)]
        public string FILE_TYPE { get; set; }

        public int? FILE_SIZE { get; set; }

        [Column(TypeName = "image")]
        public byte[] CONTENT { get; set; }

        [StringLength(255)]
        public string DESCRIPTION { get; set; }

        public short? ZX_LOC_ID { get; set; }

        public int? ZX_LOC_DATA_ID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_STUDENT> ED_STUDENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<INSTITUTION> INSTITUTIONs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<INSTITUTION> INSTITUTIONs1 { get; set; }
    }

    [Table("INSTITUTION")]
    public partial class INSTITUTION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public INSTITUTION()
        {
            ED_ACADEMIC_TERM = new HashSet<ED_ACADEMIC_TERM>();
            ED_CLASS = new HashSet<ED_CLASS>();
            ED_COURSE = new HashSet<ED_COURSE>();
            ED_STUDENT = new HashSet<ED_STUDENT>();
        }

        [Key]
        public int INSTITUTION_ID { get; set; }

        public short? TYPE_ID { get; set; }

        [StringLength(255)]
        public string INSTITUTION_NAME { get; set; }

        public byte? STATE_ID { get; set; }

        public short? LGA_ID { get; set; }

        public int? LOGO_FILE_ID { get; set; }

        public decimal? CURRENT_AVERAGE { get; set; }

        public decimal? MEAN_AVERAGE { get; set; }

        public decimal? BEST_AVERAGE { get; set; }

        public int? DEF_STUDENT_PIC_ID { get; set; }

        public short? ZX_LOC_ID { get; set; }

        public int? ZX_LOC_DATA_ID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_ACADEMIC_TERM> ED_ACADEMIC_TERM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_CLASS> ED_CLASS { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_COURSE> ED_COURSE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ED_STUDENT> ED_STUDENT { get; set; }

        public virtual FILE_IMAGE FILE_IMAGE { get; set; }

        public virtual FILE_IMAGE FILE_IMAGE1 { get; set; }
    }
}
