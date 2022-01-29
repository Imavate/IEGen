using System;
using SendGrid;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Web;
using SendGrid.Helpers.Mail;
using System.Web.Mvc;

namespace IEGen.Models
{
    public class AlertItem
    {
        public string AlertText { get; set; }

        public string IconClass { get; set; }

        public AlertTypes AlertType { get; set; }

        public bool IsError { get { return AlertType == AlertTypes.Error; } }

        public bool IsWarning { get { return AlertType == AlertTypes.Warning; } }

        public bool IsSuccess { get { return AlertType == AlertTypes.Success; } }

        public bool IsInfo { get { return AlertType == AlertTypes.Info; } }

        public string FullIconClass { get { return "fa " + IconClass + " fa-li fa-lg"; } }

        public string DivClass
        {
            get
            {
                switch (AlertType)
                {
                    case AlertTypes.Error:
                        return "alert alert-danger";
                    case AlertTypes.Warning:
                        return "alert alert-warning";
                    case AlertTypes.Success:
                        return "alert alert-success";
                    case AlertTypes.Info:
                        return "alert alert-info";
                    default:
                        return "";
                }
            }
        }
    }

    public enum AlertTypes : byte
    {
        Error = 1,
        Warning = 2,
        Success = 3,
        Info = 4,
    }

    public class AlertViewModel
    {
        private AlertViewModel()
        {
            this.Alerts = new List<AlertItem>();
        }

        public List<AlertItem> Alerts { get; set; }

        public void AddAlert(string alertText, string iconClass, AlertTypes alertType)
        {
            this.Alerts.Add(new AlertItem()
            {
                AlertText = alertText,
                IconClass = iconClass,
                AlertType = alertType
            });
        }

        public void AddAlert(string alertText, AlertTypes alertType)
        {
            switch (alertType)
            {
                case AlertTypes.Error:
                    AddAlert(alertText, "fa-times-circle", AlertTypes.Error);
                    break;
                case AlertTypes.Warning:
                    AddAlert(alertText, "fa-exclamation-triangle", AlertTypes.Warning);
                    break;
                case AlertTypes.Success:
                    AddAlert(alertText, "fa-check-circle", AlertTypes.Success);
                    break;
                case AlertTypes.Info:
                    AddAlert(alertText, "fa-info-circle", AlertTypes.Info);
                    break;
            }
        }

        public static AlertViewModel Create(string alertText, string iconClass, AlertTypes alertType)
        {
            return new AlertViewModel()
            {
                Alerts = { new AlertItem() { AlertText = alertText, IconClass = iconClass, AlertType = alertType } }
            };
        }

        public static AlertViewModel Create(string alertText, AlertTypes alertType)
        {
            switch (alertType)
            {
                case AlertTypes.Error:
                    return Create(alertText, "fa-times-circle", AlertTypes.Error);
                case AlertTypes.Warning:
                    return Create(alertText, "fa-exclamation-triangle", AlertTypes.Warning);
                case AlertTypes.Success:
                    return Create(alertText, "fa-check-circle", AlertTypes.Success);
                case AlertTypes.Info:
                    return Create(alertText, "fa-info-circle", AlertTypes.Info);
                default:
                    return null;
            }
        }
    }

    public class BaseMail
    {
        public int UserID { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string MailHeaderUrl { get; set; }
    }

    public class BasePageViewModel
    {
        public bool IsAdminPage { get; set; }

        public HeaderViewModel HeaderViewModel { get; set; }
    }

    public class EmailNotFoundMailModel : BaseMail
    {
    }

    public enum HeaderTabs : byte
    {
        Home = 1,
        Data,
        Report,
        Setup,
        Admin,
    }

    public class HeaderViewModel
    {
        public string LogoUrl { get; set; }

        public string LoggedInUser { get; set; }

        public int SchoolID { get; set; }

        public int UserID { get; set; }

        public byte UserTypeID { get; set; }

        public byte SelectedTab { get; set; }

        public List<AccessGroupRole> Roles { get; set; }

        public string HomeClass { get { return SelectedTab != (byte)HeaderTabs.Home ? "" : "active"; } }
        public string DataClass { get { return SelectedTab != (byte)HeaderTabs.Data ? "" : "active"; } }
        public string ReportClass { get { return SelectedTab != (byte)HeaderTabs.Report ? "" : "active"; } }
        public string SetupClass { get { return SelectedTab != (byte)HeaderTabs.Setup ? "" : "active"; } }
        public string AdminClass { get { return SelectedTab != (byte)HeaderTabs.Admin ? "" : "active"; } }

        public bool Has_ManageAccess { get { return Roles.Any(l => l.RoleID == (byte)UserRoles.ManageAccessGroups); } }
        public bool Has_ManageUsers { get { return Roles.Any(l => l.RoleID == (byte)UserRoles.ManageUsers); } }
        public bool Has_SetupMailingSystem { get { return Roles.Any(l => l.RoleID == (byte)UserRoles.SetupMailingSystem); } }
        public bool Has_ViewAuditTrail { get { return Roles.Any(l => l.RoleID == (byte)UserRoles.ViewAuditTrail); } }
        public bool Has_ManageSystemAudit { get { return Roles.Any(l => l.RoleID == (byte)UserRoles.ManageSystemAudit); } }
        public bool Has_ManageOtherSettings { get { return Roles.Any(l => l.RoleID == (byte)UserRoles.ManageOtherSettings); } }
        public bool Has_ManageTeachers { get { return Roles.Any(l => l.RoleID == (byte)UserRoles.ManageTeachers); } }
        public bool Has_ManageSchools { get { return Roles.Any(l => l.RoleID == (byte)UserRoles.ManageSchools); } }
        public bool Has_ManageStudents { get { return Roles.Any(l => l.RoleID == (byte)UserRoles.ManageStudents); } }
        public bool Has_ViewReports { get { return Roles.Any(l => l.RoleID == (byte)UserRoles.ViewReports); } }
        public bool Has_ViewSetup
        {
            get
            {
                return Roles.Any(l => l.RoleID == (byte)UserRoles.ManageTerm || l.RoleID == (byte)UserRoles.EditSchoolDetails ||
                                      l.RoleID == (byte)UserRoles.ManageStudents);
            }
        }
        public bool Has_DataEntry
        {
            get
            {
                return Roles.Any(l => l.RoleID == (byte)UserRoles.EnterAttendance || l.RoleID == (byte)UserRoles.EnterScores ||
                                      l.RoleID == (byte)UserRoles.EnterSkills || l.RoleID == (byte)UserRoles.EditStudent);
            }
        }
        public bool Is_IEAdmin
        {
            get
            {
                return Roles.Any(l => l.RoleID == (byte)UserRoles.ManageSchools || l.RoleID == (byte)UserRoles.ManageTeachers ||
                                      l.RoleID == (byte)UserRoles.ManageUsers || l.RoleID == (byte)UserRoles.ManageOtherSettings);
            }
        }


        public bool IsAppAdmin { get { return UserTypeID == (byte)UserType.AppAdmin; } }
        public bool IsSchoolAdmin { get { return UserTypeID == (byte)UserType.SchoolAdmin; } }
        public bool IsTeacher { get { return UserTypeID == (byte)UserType.Teacher; } }
        public bool IsStudent { get { return UserTypeID == (byte)UserType.Student; } }

        public bool IsAdmin { get { return IsAppAdmin || IsSchoolAdmin; } }
    }

    public class LogoViewModel
    {
        public string Url { get; set; }    // http://www.imavate-edu.com

        public string Name { get; set; }     // Imavate Education

        public string LogoSrc { get; set; }

        public short Height { get; set; }  // 85

        public short Width { get; set; }   // 350

        public int ImgWidth { get { return 88 * Width / Height; } }

        public bool NoLogo { get { return string.IsNullOrEmpty(LogoSrc); } }
    }

    public class JSRedirectViewModel
    {
        public string Location { get; set; }
    }

    public enum UserRoles : byte
    {
        [Display(Name = "System Admin: Manage User Security")]
        ManageAccessGroups = 1,

        [Display(Name = "System Admin: Manage Users")]
        ManageUsers,

        [Display(Name = "System Admin: Setup Mailing System")]
        SetupMailingSystem,

        [Display(Name = "System Admin: View Audit Trail")]
        ViewAuditTrail,

        [Display(Name = "System Admin: Manage System Audit")]
        ManageSystemAudit,

        [Display(Name = "System Admin: Manage Other Settings")]
        ManageOtherSettings,

        [Display(Name = "System Admin: Manage Schools")]
        ManageSchools,

        [Display(Name = "System Admin: Manage Teachers")]
        ManageTeachers,

        [Display(Name = "Setup: Edit School Details")]
        EditSchoolDetails,

        [Display(Name = "Setup: Manage School Term")]
        ManageTerm,

        [Display(Name = "Setup: Manage Students")]
        ManageStudents,

        [Display(Name = "Reports: View School Reports")]
        ViewReports,

        [Display(Name = "Data: Enter Subject Scores")]
        EnterScores,

        [Display(Name = "Data: Enter Qualitative Skills")]
        EnterSkills,

        [Display(Name = "Data: Enter Student Attendance")]
        EnterAttendance,

        [Display(Name = "Data: Edit Student Info")]
        EditStudent,   // only edit name / class, cannot delete
    }

    /// <summary>
    /// Compares two numbers to each other, ensuring that one is less than or equal to the other
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CompareNumbersAttribute : ValidationAttribute, IClientValidatable
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareNumbersAttribute"/> class.
        /// </summary>
        /// <param name="otherPropertyName">Name of the compare to date property.</param>
        /// <param name="allowEquality">if set to <c>true</c> equal dates are allowed.</param>
        public CompareNumbersAttribute(string otherPropertyName, bool allowEquality = true)
        {
            AllowEquality = allowEquality;
            OtherPropertyName = otherPropertyName;
        }

        #region Properties

        /// <summary>
        /// Gets the name of the  property to compare to
        /// </summary>
        public string OtherPropertyName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether dates could be the same
        /// </summary>
        public bool AllowEquality { get; private set; }


        #endregion

        /// <summary>
        /// Validates the specified value with respect to the current validation attribute.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> class.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = ValidationResult.Success;
            var otherValue = validationContext.ObjectType.GetProperty(OtherPropertyName).GetValue(validationContext.ObjectInstance, null);
            if (value != null)
            {
                decimal currentDecimalValue;
                if (decimal.TryParse(value.ToString(), out currentDecimalValue))
                {

                    if (otherValue != null)
                    {
                        decimal otherDecimalValue;
                        if (decimal.TryParse(otherValue.ToString(), out otherDecimalValue))
                        {
                            if (!OtherPropertyName.ToLower().Contains("max"))
                            {
                                if (currentDecimalValue < otherDecimalValue)
                                {
                                    result = new ValidationResult(ErrorMessage);
                                }
                            }
                            else
                            {
                                if (currentDecimalValue > otherDecimalValue)
                                {
                                    result = new ValidationResult(ErrorMessage);
                                }
                            }
                            if (currentDecimalValue == otherDecimalValue && !AllowEquality)
                            {
                                result = new ValidationResult(ErrorMessage);
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// When implemented in a class, returns client validation rules for that class.
        /// </summary>
        /// <param name="metadata">The model metadata.</param>
        /// <param name="context">The controller context.</param>
        /// <returns>
        /// The client validation rules for this validator.
        /// </returns>
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = ErrorMessage,
                ValidationType = "comparenumbers"
            };
            rule.ValidationParameters["otherpropertyname"] = OtherPropertyName;
            rule.ValidationParameters["allowequality"] = AllowEquality ? "true" : "";
            yield return rule;
        }

        private object _typeId = new object();
        public override object TypeId
        {
            get
            {
                return _typeId;
            }
        }
    }

    /// <summary>
    /// Compares two numbers to each other, ensuring that one is less than or equal to the other
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MaxValueAttribute : ValidationAttribute, IClientValidatable
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxValueAttribute"/> class.
        /// </summary>
        /// <param name="otherPropertyName">Name of the compare to date property.</param>
        /// <param name="allowEquality">if set to <c>true</c> equal dates are allowed.</param>
        public MaxValueAttribute(string otherPropertyName, bool allowEquality = true)
        {
            AllowEquality = allowEquality;
            OtherPropertyName = otherPropertyName;
        }

        #region Properties

        /// <summary>
        /// Gets the name of the  property to compare to
        /// </summary>
        public string OtherPropertyName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether dates could be the same
        /// </summary>
        public bool AllowEquality { get; private set; }


        #endregion

        /// <summary>
        /// Validates the specified value with respect to the current validation attribute.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> class.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = ValidationResult.Success;
            var otherValue = validationContext.ObjectType.GetProperty(OtherPropertyName).GetValue(validationContext.ObjectInstance, null);
            if (value != null)
            {
                decimal currentDecimalValue;
                if (decimal.TryParse(value.ToString(), out currentDecimalValue))
                {

                    if (otherValue != null)
                    {
                        decimal otherDecimalValue;
                        if (decimal.TryParse(otherValue.ToString(), out otherDecimalValue))
                        {
                            if (currentDecimalValue > otherDecimalValue)
                            {
                                result = new ValidationResult(ErrorMessage);
                            }
                            if (currentDecimalValue == otherDecimalValue && !AllowEquality)
                            {
                                result = new ValidationResult(ErrorMessage);
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// When implemented in a class, returns client validation rules for that class.
        /// </summary>
        /// <param name="metadata">The model metadata.</param>
        /// <param name="context">The controller context.</param>
        /// <returns>
        /// The client validation rules for this validator.
        /// </returns>
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = ErrorMessage,
                ValidationType = "maxvalue"
            };
            rule.ValidationParameters["otherpropertyname"] = OtherPropertyName;
            rule.ValidationParameters["allowequality"] = AllowEquality ? "true" : "";
            yield return rule;
        }
    }

    /// <summary>
    /// Compares two numbers to each other, ensuring that one is greater than or equal to the other
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MinValueAttribute : ValidationAttribute, IClientValidatable
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MinValueAttribute"/> class.
        /// </summary>
        /// <param name="otherPropertyName">Name of the compare to date property.</param>
        /// <param name="allowEquality">if set to <c>true</c> equal dates are allowed.</param>
        public MinValueAttribute(string otherPropertyName, bool allowEquality = true)
        {
            AllowEquality = allowEquality;
            OtherPropertyName = otherPropertyName;
        }

        #region Properties

        /// <summary>
        /// Gets the name of the  property to compare to
        /// </summary>
        public string OtherPropertyName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether dates could be the same
        /// </summary>
        public bool AllowEquality { get; private set; }


        #endregion

        /// <summary>
        /// Validates the specified value with respect to the current validation attribute.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> class.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = ValidationResult.Success;
            var otherValue = validationContext.ObjectType.GetProperty(OtherPropertyName).GetValue(validationContext.ObjectInstance, null);
            if (value != null)
            {
                decimal currentDecimalValue;
                if (decimal.TryParse(value.ToString(), out currentDecimalValue))
                {

                    if (otherValue != null)
                    {
                        decimal otherDecimalValue;
                        if (decimal.TryParse(otherValue.ToString(), out otherDecimalValue))
                        {
                            if (currentDecimalValue < otherDecimalValue)
                            {
                                result = new ValidationResult(ErrorMessage);
                            }
                            if (currentDecimalValue == otherDecimalValue && !AllowEquality)
                            {
                                result = new ValidationResult(ErrorMessage);
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// When implemented in a class, returns client validation rules for that class.
        /// </summary>
        /// <param name="metadata">The model metadata.</param>
        /// <param name="context">The controller context.</param>
        /// <returns>
        /// The client validation rules for this validator.
        /// </returns>
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = ErrorMessage,
                ValidationType = "minvalue"
            };
            rule.ValidationParameters["otherpropertyname"] = OtherPropertyName;
            rule.ValidationParameters["allowequality"] = AllowEquality ? "true" : "";
            yield return rule;
        }
    }

    /// <summary>
    /// Ensures that a given number falls within a range
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CustomRangeAttribute : ValidationAttribute, IClientValidatable
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MinValueAttribute"/> class.
        /// </summary>
        /// <param name="minPropertyName">Name of the lower bound property.</param>
        /// <param name="maxPropertyName">Name of the upper bound property.</param>
        /// <param name="allowEquality">if set to <c>true</c> boundary values are allowed.</param>
        public CustomRangeAttribute(string minPropertyName, string maxPropertyName, bool allowEquality = true)
        {
            AllowEquality = allowEquality;
            MinPropertyName = minPropertyName;
            MaxPropertyName = maxPropertyName;
        }

        #region Properties

        /// <summary>
        /// Gets the name of the lower bound of the range
        /// </summary>
        public string MinPropertyName { get; private set; }

        /// <summary>
        /// Gets the name of the upper bound of the range
        /// </summary>
        public string MaxPropertyName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether dates could be the same
        /// </summary>
        public bool AllowEquality { get; private set; }


        #endregion

        /// <summary>
        /// Validates the specified value with respect to the current validation attribute.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> class.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = ValidationResult.Success;
            var minValue = validationContext.ObjectType.GetProperty(MinPropertyName).GetValue(validationContext.ObjectInstance, null);
            var maxValue = validationContext.ObjectType.GetProperty(MaxPropertyName).GetValue(validationContext.ObjectInstance, null);
            if (value != null)
            {
                decimal currentDecimalValue;
                if (decimal.TryParse(value.ToString(), out currentDecimalValue))
                {
                    if (minValue != null && maxValue != null)
                    {
                        decimal minDecimalValue;
                        decimal maxDecimalValue;
                        if (decimal.TryParse(minValue.ToString(), out minDecimalValue) && decimal.TryParse(maxValue.ToString(), out maxDecimalValue))
                        {
                            if (currentDecimalValue < minDecimalValue || currentDecimalValue > maxDecimalValue)
                            {
                                result = new ValidationResult(ErrorMessage);
                            }
                            if ((currentDecimalValue == minDecimalValue || currentDecimalValue == maxDecimalValue) && !AllowEquality)
                            {
                                result = new ValidationResult(ErrorMessage);
                            }
                        }
                    }
                    else if (minValue != null)
                    {
                        decimal minDecimalValue;
                        if (decimal.TryParse(minValue.ToString(), out minDecimalValue))
                        {
                            if (currentDecimalValue < minDecimalValue)
                            {
                                result = new ValidationResult(ErrorMessage);
                            }
                            if (currentDecimalValue == minDecimalValue && !AllowEquality)
                            {
                                result = new ValidationResult(ErrorMessage);
                            }
                        }
                    }
                    else if (maxValue != null)
                    {
                        decimal maxDecimalValue;
                        if (decimal.TryParse(maxValue.ToString(), out maxDecimalValue))
                        {
                            if (currentDecimalValue > maxDecimalValue)
                            {
                                result = new ValidationResult(ErrorMessage);
                            }
                            if (currentDecimalValue == maxDecimalValue && !AllowEquality)
                            {
                                result = new ValidationResult(ErrorMessage);
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// When implemented in a class, returns client validation rules for that class.
        /// </summary>
        /// <param name="metadata">The model metadata.</param>
        /// <param name="context">The controller context.</param>
        /// <returns>
        /// The client validation rules for this validator.
        /// </returns>
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = ErrorMessage,
                ValidationType = "customrange"
            };
            rule.ValidationParameters["minpropertyname"] = MinPropertyName;
            rule.ValidationParameters["maxpropertyname"] = MaxPropertyName;
            rule.ValidationParameters["allowequality"] = AllowEquality ? "true" : "";
            yield return rule;
        }
    }

    /// <summary>
    /// Compares two dates to each other, ensuring that one is larger than the other
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CompareDatesAttribute : ValidationAttribute, IClientValidatable
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareDatesAttribute"/> class.
        /// </summary>
        /// <param name="otherPropertyName">Name of the compare to date property.</param>
        /// <param name="allowEquality">if set to <c>true</c> equal dates are allowed.</param>
        public CompareDatesAttribute(string otherPropertyName, bool allowEquality = true)
        {
            AllowEquality = allowEquality;
            OtherPropertyName = otherPropertyName;
        }

        #region Properties

        /// <summary>
        /// Gets the name of the  property to compare to
        /// </summary>
        public string OtherPropertyName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether dates could be the same
        /// </summary>
        public bool AllowEquality { get; private set; }


        #endregion

        /// <summary>
        /// Validates the specified value with respect to the current validation attribute.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult"/> class.
        /// </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = ValidationResult.Success;
            var otherValue = validationContext.ObjectType.GetProperty(OtherPropertyName)
                .GetValue(validationContext.ObjectInstance, null);
            if (value != null)
            {
                if (value is DateTime)
                {

                    if (otherValue != null)
                    {
                        if (otherValue is DateTime)
                        {
                            if (!OtherPropertyName.ToLower().Contains("max"))
                            {
                                if ((DateTime)value < (DateTime)otherValue)
                                {
                                    result = new ValidationResult(ErrorMessage);
                                }
                            }
                            else
                            {
                                if ((DateTime)value > (DateTime)otherValue)
                                {
                                    result = new ValidationResult(ErrorMessage);
                                }
                            }
                            if ((DateTime)value == (DateTime)otherValue && !AllowEquality)
                            {
                                result = new ValidationResult(ErrorMessage);
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// When implemented in a class, returns client validation rules for that class.
        /// </summary>
        /// <param name="metadata">The model metadata.</param>
        /// <param name="context">The controller context.</param>
        /// <returns>
        /// The client validation rules for this validator.
        /// </returns>
        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = ErrorMessage,
                ValidationType = "comparedates"
            };
            rule.ValidationParameters["otherpropertyname"] = OtherPropertyName;
            rule.ValidationParameters["allowequality"] = AllowEquality ? "true" : "";
            yield return rule;
        }
    }


    public class HomeIndexPageViewModel : BasePageViewModel
    {
        public string SchoolName { get; set; }
        public string TermName { get; set; }
    }

}