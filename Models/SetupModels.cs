using AppSecrets;
using Elmah;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace IEGen.Models
{
    public class AccessGroup
    {
        [Key, ScaffoldColumn(false)]
        public short AccessGroupID { get; set; }

        [StringLength(128)]
        [Index(IsUnique = true)]
        [Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        public bool IsDefault { get; set; }

        public int? ChangedByID { get; set; }

        public DateTime? TimeChanged { get; set; }

        [ForeignKey("ChangedByID")]
        public IEUser ChangedBy { get; set; }

        [InverseProperty("AccessGroup")]
        public List<IEUser> Users { get; set; }

        [InverseProperty("AccessGroup")]
        public List<AccessGroupRole> GroupRoles { get; set; }
    }

    public class AccessGroupRole
    {
        [Key]
        [Column(Order = 1)]
        public short AccessGroupID { get; set; }

        [Key]
        [Column(Order = 2)]
        public byte RoleID { get; set; }

        public UserRole Role { get; set; }

        [ForeignKey("AccessGroupID")]
        public AccessGroup AccessGroup { get; set; }
    }

    public enum UserType : byte
    {
        [Display(Name ="Application Administrator")]
        AppAdmin = 1,

        [Display(Name = "School Administrator")]
        SchoolAdmin,

        Teacher,
        Student
    }

    public class IEUser
    {
        [Key]
        public int UserID { get; set; }

        [Index(IsUnique = true), StringLength(128)]
        public string Email { get; set; }

        [StringLength(128), Required(ErrorMessage = "Name is compulsory!")]
        public string Name { get; set; }

        [StringLength(32)]
        public string PhoneNumber { get; set; }

        public short AccessGroupID { get; set; }

        public byte TypeID { get; set; }

        public int SchoolID { get; set; }

        [ForeignKey("AccessGroupID")]
        public AccessGroup AccessGroup { get; set; }
    }

    /// <summary>
    /// User Defaults... Only AppAdmins and SchoolAdmins will have User Defaults...
    /// </summary>
    public class UserDefaults
    {
        [Key, Column(Order = 1)]
        public int UserID { get; set; }

        [Key, Column(Order = 2)]
        public int? SchoolID { get; set; }

        public int? TermID { get; set; }
        public int? ClassID { get; set; }

        [ForeignKey("ClassID")]
        public Class Class { get; set; }

        [ForeignKey("TermID")]
        public Term Term { get; set; }

        [ForeignKey("SchoolID")]
        public School School { get; set; }
    }

    public class UserAccountComplaint
    {
        [Key]
        public int ComplaintID { get; set; }

        public string Email { get; set; }

        public int UserID { get; set; }

        public DateTime LogDate { get; set; }

        [StringLength(512)]
        public string Complaint { get; set; }

        [ForeignKey("UserID")]
        public IEUser User { get; set; }
    }

    public class UserRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public byte RoleID { get; set; }

        [StringLength(128)]
        public string Name { get; set; }
    }

    public class PasswordReset
    {
        [Key]
        public int ResetID { get; set; }

        public int UserID { get; set; }

        public DateTime RequestTime { get; set; }
    }

    public class PasswordResetComplete
    {
        [Key]
        public int ResetID { get; set; }

        public int UserID { get; set; }

        public DateTime RequestTime { get; set; }

        public DateTime ActionTime { get; set; }
    }

    /// <summary>
    /// Defines an Email Message
    /// </summary>
    public class EmailMessage
    {
        public EmailMessage()
        {
            TimeSent = DateTime.Now;
        }

        [Key]
        public int MessageID { get; set; }

        public DateTime TimeSent { get; set; }

        [StringLength(128)]
        public string FromEmail { get; set; }

        [StringLength(128)]
        public string FromName { get; set; }

        [StringLength(128)]
        public string ToEmail { get; set; }

        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(512)]
        public string ButtonUrl { get; set; }

        [StringLength(256)]
        public string DeleteAccountUrl { get; set; }

        [StringLength(512)]
        public string HeaderUrl { get; set; }

        [StringLength(512)]
        public string Subject { get; set; }

        public string FullSubject { get { return "Imavate Education: " + Subject; } }

        [StringLength(2000)]
        public string Body { get; set; }

        public string FormattedBody
        {
            get
            {
                string prefix = @"
<table style='background-color:#f6f6f6;width:100%;'>
    <tr><td colspan='3'>&nbsp;</td></tr>
        <td>&nbsp;</td>
        <td style='font-family:Arial; font-size:14px; line-height:1.4285; width:600px; padding:0px; margin:auto !important; background-color:#ffffff'>
            <table style='border-spacing:0px'>
                <tr>
                    <td style='padding:0px;text-align:center'>
                        <img src='$#HEADERSRC'
                             height='100' width='600' style='border-style:solid;border-width:0px;font-size:large;font-weight:bold' alt='Imavate Education'><br><br>
                    </td>
                </tr>
                ";

                prefix = prefix.Replace("$#HEADERSRC", string.IsNullOrEmpty(HeaderUrl) ? "https://i.imgur.com/DdaQpFT.png" : HeaderUrl);

                string helloLine = "<tr><td style='Padding:3px'>Hello <strong>" + Name + ",</strong><br><br></td></tr>";
                string bodyPart = Body.Replace("</p>", "</td></tr>").Replace("<p>", "<tr><td style='padding:3px'>");

                string suffix = @"
                <tr><td style='Padding:3px'>&nbsp;<br>Thank you,</td></tr>
                <tr><td style='Padding:3px'><strong>Imavate Education</strong><br><br></td></tr>
                ";

                if (!string.IsNullOrEmpty(ButtonUrl))
                {
                    string btnStart = "<tr><td style='Padding:20px 3px;text-align:center'><a href='" + ButtonUrl + "' style='text-decoration:none'><span style='background-color:#5cb85c;border-style:solid;border-color:#4cae4c;color:#ffffff;font-weight:bold;padding:10px;'>";
                    string btnEnd = "</span></a></td></tr>";
                    bodyPart = bodyPart.Replace("</mb>", btnEnd).Replace("<mb>", btnStart);

                    string btnURLSection = @"
                <tr>
                    <td style='padding:3px;word-break:break-word'><hr>
                        <span style='font-size:11px'>If you are having trouble with the button above, copy and paste the URL below into your web browser.<br><br>$#LNK<br><br></span>
                    </td>
                </tr>
                    ";

                    suffix += btnURLSection.Replace("$#LNK", ButtonUrl);
                }

                string ending = @"
            </table>
        </td>
        <td>&nbsp;</td>
    </tr>
    <tr>
        <td>&nbsp;</td>
        <td style='padding:3px;text-align:center'>&nbsp;<br>
            <span style='font-size:11px;color:#333333'>
                You received this email because someone registered with your email address on the Imavate Education Portal.<br>
                Ensure delivery of future emails by adding $#APPEMAIL to your address book.
            </span>
        </td>
        <td>&nbsp;</td>
    </tr>
                ";

                ending = ending.Replace("$#APPEMAIL", FromEmail);

                if (!string.IsNullOrEmpty(DeleteAccountUrl))
                {

                    string delAccSection = @"
    <tr>
        <td>&nbsp;</td>
        <td style='text-align:center'>
            <a href='$#CBK' style='text-decoration:none'>
                <span style='background-color:#888888;border-style:solid;border-width:2px;border-color:#666666;font-size:11px;color:#ffffff;padding:2px;'>Delete Account</span>
            </a><br><br>
        </td>
        <td>&nbsp;</td>
    </tr>
                    ";

                    ending += delAccSection.Replace("$#CBK", DeleteAccountUrl);
                }

                ending += @"
    <tr>
        <td>&nbsp;</td>
        <td style='text-align:center;font-size:11px'>Imavate Education / PO Box 4516, Apapa, Lagos</td>
        <td>&nbsp;</td>
    </tr>
</table>
                    ";

                return prefix + helloLine + bodyPart + suffix + ending;
            }
        }

        public SendGridMessage GetSendGridMessage()
        {
            return MailHelper.CreateSingleEmail(new EmailAddress(FromEmail, FromName), new EmailAddress(ToEmail, null), FullSubject, "", FormattedBody);
        }

        public MailMessage GetSmtpMessage()
        {
            MailMessage mailMessage = new MailMessage { Subject = FullSubject, Body = FormattedBody, IsBodyHtml = true, From = new MailAddress(FromEmail, FromName) };

            mailMessage.To.Add(ToEmail);

            return mailMessage;
        }
    }

    /// <summary>
    /// Primary object used to create a single email message
    /// </summary>
    public class EmailObject
    {
        internal EmailObject()
        {
        }

        public string FromEmail { get; set; }

        public string FromName { get; set; }

        public SmtpClient SmtpClient { get; set; }

        public EmailMessage MailMessage { get; set; }

        public bool UseDefaultParameters { get; set; }

        public string ApplicationPath { get; set; }

        public string LoginPath { get { return ApplicationPath + "/Account/Login"; } }
    }

    /// <summary>
    /// Object used to create multiple email messages
    /// </summary>
    public class EmailObjectMany : EmailObject
    {
        internal EmailObjectMany()
        {
        }

        private new EmailMessage MailMessage { get; set; }

        public List<EmailMessage> MailMessages { get; set; }

        public SmtpClient CloneSmtpClient()
        {
            SmtpClient sc = new SmtpClient
            {
                Host = SmtpClient.Host,
                Port = SmtpClient.Port,
                Credentials = SmtpClient.Credentials,
                EnableSsl = SmtpClient.EnableSsl
            };

            return sc;
        }
    }

    /// <summary>
    /// Stores Email Settings 
    /// </summary>
    public class EmailSettings
    {
        [Key]
        public byte SettingsID { get; set; }

        [StringLength(128)]
        public string SmtpServer { get; set; }

        public int SmtpPort { get; set; }

        public bool UseDefaultMailParameters { get; set; }

        public bool EnableSslMail { get; set; }

        [StringLength(128)]
        public string EmailFrom { get; set; }

        [StringLength(128)]
        public string MailUser { get; set; }

        [StringLength(128)]
        public string MailPassword { get; set; }

        [StringLength(128)]
        public string ApplicationPath { get; set; }

        [StringLength(128)]
        public string SMSAppUrl { get; set; }

        public int BulkSendBatchSize { get; set; }

        public static SendGridClient GetSendGridClient()
        {
            return new SendGridClient(Secrets.SendGridApiKey, null, null, "v3", null);
        }

        public static EmailObject BlankMail(IEContext context)
        {
            var emailObject = new EmailObject();
            var emailSettings = context.EmailSettingsList.First();
            emailObject.MailMessage = new EmailMessage();
            if (emailSettings.UseDefaultMailParameters)
            {
                emailObject.FromEmail = Secrets.SendGridMailFrom;
                emailObject.FromName = Secrets.SendGridMailFromName;
            }
            else
            {
                emailObject.FromEmail = emailSettings.MailUser;
                emailObject.FromName = emailSettings.EmailFrom;
                var pwd = string.IsNullOrEmpty(emailSettings.MailPassword) ? null : General.Decrypt(emailSettings.MailPassword);

                emailObject.SmtpClient = new SmtpClient()
                {
                    Credentials = new NetworkCredential(emailSettings.MailUser, pwd),
                    Host = emailSettings.SmtpServer,
                    Port = emailSettings.SmtpPort,
                    EnableSsl = emailSettings.EnableSslMail
                };
            }
            emailObject.UseDefaultParameters = emailSettings.UseDefaultMailParameters;
            return emailObject;
        }

        public static EmailObjectMany BlankMails(IEContext context)
        {
            EmailObjectMany emailObjectMany = new EmailObjectMany();
            EmailSettings emailSettings = ((IQueryable<EmailSettings>)context.EmailSettingsList).First<EmailSettings>();
            emailObjectMany.MailMessages = new List<EmailMessage>();
            if (emailSettings.UseDefaultMailParameters)
            {
                emailObjectMany.FromEmail = Secrets.SendGridMailFrom;
                emailObjectMany.FromName = Secrets.SendGridMailFromName;
            }
            else
            {
                emailObjectMany.FromEmail = emailSettings.MailUser;
                emailObjectMany.FromName = emailSettings.EmailFrom;
                var pwd = string.IsNullOrEmpty(emailSettings.MailPassword) ? null : General.Decrypt(emailSettings.MailPassword);

                emailObjectMany.SmtpClient = new SmtpClient()
                {
                    Credentials = new NetworkCredential(emailSettings.MailUser, pwd),
                    Host = emailSettings.SmtpServer,
                    Port = emailSettings.SmtpPort,
                    EnableSsl = emailSettings.EnableSslMail
                };
            }
            emailObjectMany.UseDefaultParameters = emailSettings.UseDefaultMailParameters;
            return emailObjectMany;
        }
    }

    public class Eval
    {
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field == null)
                return "";
            DescriptionAttribute[] customAttributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (customAttributes != null && customAttributes.Length != 0)
                return customAttributes[0].Description;
            DisplayAttribute customAttribute = field.GetCustomAttribute<DisplayAttribute>(false);
            if (customAttribute != null)
            {
                string name = customAttribute.GetName();
                if (!string.IsNullOrEmpty(name))
                    return name;
            }
            return value.ToString();
        }

        public static string GetDisplayName(Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field == (FieldInfo)null)
                return "";
            DisplayAttribute customAttribute = field.GetCustomAttribute<DisplayAttribute>(false);
            if (customAttribute != null)
            {
                string name = customAttribute.GetName();
                if (!string.IsNullOrEmpty(name))
                    return name;
            }
            return value.ToString();
        }

        public static string GetDisplayName(Type EnumType, byte value)
        {
            if (!Enum.IsDefined(EnumType, (object)value))
                return "";
            object obj = Enum.Parse(EnumType, value.ToString());
            FieldInfo field = EnumType.GetField(obj.ToString());
            if (field == (FieldInfo)null)
                return "";
            DisplayAttribute customAttribute = field.GetCustomAttribute<DisplayAttribute>(false);
            if (customAttribute != null)
            {
                string name = customAttribute.GetName();
                if (!string.IsNullOrEmpty(name))
                    return name;
            }
            return obj.ToString();
        }

        public static IEnumerable<T> OldEnumToList<T>()
        {
            Type enumType = typeof(T);
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");
            Array values = Enum.GetValues(enumType);
            List<T> objList = new List<T>(values.Length);
            foreach (int num in values)
                objList.Add((T)Enum.Parse(enumType, num.ToString()));
            return (IEnumerable<T>)objList;
        }

        public static IEnumerable<T> EnumToList<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static T Max<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Max<T>();
        }

        public static T Min<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Min<T>();
        }
    }

    public class ExternalFile
    {
        [Key]
        [StringLength(128)]
        public string ExternalFileID { get; set; }

        public byte[] Content { get; set; }
    }

    public class ExtLogoFile : ExternalFile
    {
        public ExtLogoFile()
        {
        }

        public ExtLogoFile(ExternalFile file)
        {
            ExternalFileID = file.ExternalFileID;
            Content = file.Content;
        }
    }

    public class ExtPictureFile : ExternalFile
    {
        public ExtPictureFile()
        {
        }

        public ExtPictureFile(ExternalFile file)
        {
            ExternalFileID = file.ExternalFileID;
            Content = file.Content;
        }
    }

    public class ExtResultFile : ExternalFile
    {
        public ExtResultFile()
        {
        }

        public ExtResultFile(ExternalFile file)
        {
            ExternalFileID = file.ExternalFileID;
            Content = file.Content;
        }
    }

    public enum FileType : byte
    {
        Logo = 1,
        Picture,
        Result
    }

    public class General
    {
        public static bool UseAzureFileStorage = false;

        public static int MaxPictureSizeKB = 100;
        public static int MaxUploadedFileSizeKB = 25600; //25MB

        public const int MinYear = 1950;
        public const int MaxYear = 2019;

        public static short DefAccessGroupID = 3;
        public static string DefaultTimeZoneID = "W. Central Africa Standard Time";
        public static int DefSkillGroupID = 1;
        public static int DefGradeGroupID = 1;

        static char[] _invalids;

        /// <summary>Replaces characters in <c>text</c> that are not allowed in 
        /// file names with the specified replacement character.</summary>
        /// <param name="text">Text to make into a valid filename. The same string is returned if it is valid already.</param>
        /// <param name="replacement">Replacement character, or null to simply remove bad characters.</param>
        /// <param name="fancy">Whether to replace quotes and slashes with the non-ASCII characters ” and ⁄.</param>
        /// <returns>A string that can be used as a filename. If the output string would otherwise be empty, returns "_".</returns>
        public static string MakeValidFileName(string text, char? replacement = '_', bool fancy = true)
        {
            StringBuilder sb = new StringBuilder(text.Length);
            var invalids = _invalids ?? (_invalids = Path.GetInvalidFileNameChars());
            bool changed = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (invalids.Contains(c))
                {
                    changed = true;
                    var repl = replacement ?? '\0';
                    if (fancy)
                    {
                        if (c == '"') repl = '”'; // U+201D right double quotation mark
                        else if (c == '\'') repl = '’'; // U+2019 right single quotation mark
                        else if (c == '/') repl = '⁄'; // U+2044 fraction slash
                    }
                    if (repl != '\0')
                        sb.Append(repl);
                }
                else
                    sb.Append(c);
            }

            if (sb.Length == 0)
                return "_";

            return changed ? sb.ToString() : text;
        }

        public static string AddLineBreaksToTooltip(string tooltip, int lineLength)
        {
            string str = "";
            int num = 0;
            for (int index = 0; index < tooltip.Length; ++index)
            {
                ++num;
                if (num >= lineLength && (int)tooltip[index] == 32)
                {
                    str = str + " " + Environment.NewLine;
                    num = 0;
                }
                else
                    str += tooltip[index].ToString();
            }
            return str;
        }

        public static string FileSizeInfo(decimal size)
        {
            if (size > 1000000)
                return (size / (1024 * 1024)).ToString("F") + " MB";

            if (size > 1000)
                return (size / (1024)).ToString("F") + " KB";

            return size.ToString() + " bytes";
        }

        public static string FullDateString(DateTime? date)
        {
            if (!date.HasValue)
                return "";
            return date.Value.ToString("dddd, d MMMM yyyy");
        }

        public static string FullDateFromSQL(DateTime? date)
        {
            if (!date.HasValue)
                return "";
            if (date.Value == new DateTime(1900, 1, 1))
                return "";
            return date.Value.ToString("dddd, d MMMM yyyy");
        }

        public static string DateString(DateTime? date)
        {
            if (!date.HasValue)
                return "";
            return date.Value.ToString("d MMMM yyyy");
        }

        public static string FullTimeString(DateTime? date)
        {
            if (!date.HasValue)
                return "";
            return date.Value.ToString("dddd, d MMMM yyyy h:mm tt");
        }

        public static List<SelectListItem> GetTimeZoneList()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            foreach (TimeZoneInfo tzi in TimeZoneInfo.GetSystemTimeZones())
                list.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id });

            return list;
        }

        public static bool IsValidTimeZoneID(string TimeZoneID)
        {
            return TimeZoneInfo.GetSystemTimeZones().Any(l => l.Id == TimeZoneID);
        }

        public static TimeZoneInfo GetTimeZone(string TimeZoneID)
        {
            if (IsValidTimeZoneID(TimeZoneID))
                return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneID);
            else
                return TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneID);
        }

        public static string GetTimeZoneName(string TimeZoneID)
        {
            return GetTimeZone(TimeZoneID).DisplayName;
        }

        public static DateTime GetUTCTime(DateTime time, string TimeZoneID)
        {
            return TimeZoneInfo.ConvertTimeToUtc(time, GetTimeZone(TimeZoneID));
        }

        public static DateTime GetLocalTime(DateTime time, string TimeZoneID)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(time, GetTimeZone(TimeZoneID));
        }

        public static string GetLocalTimeString(DateTime utcTime, string TimeZoneID)
        {
            return FullTimeString(TimeZoneInfo.ConvertTimeFromUtc(utcTime, GetTimeZone(TimeZoneID)));
        }

        public static string FullDateInputString(DateTime? date)
        {
            if (!date.HasValue)
                return "";
            return date.Value.ToString("yyyy-MM-dd");
        }

        public static string FullTimeInputString(DateTime? date)
        {
            if (!date.HasValue)
                return "";
            return date.Value.ToString("yyyy-MM-ddTHH:mm");
        }

        public static string ShortTimeString(DateTime? date)
        {
            if (!date.HasValue || date.Value == DateTime.MinValue)
                return "";
            return date.Value.ToString("yyyy-MM-dd HH:mm");
        }

        public static string CapitalizeFirst(string str)
        {
            return string.IsNullOrWhiteSpace(str) ? "" : str.Substring(0, 1).ToUpper() + str.Substring(1);
        }

        public static string Json(object obj)
        {
            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(obj);
        }

        public static bool HasRole(string roleIDs, byte roleID)
        {
            return ((IEnumerable<string>)roleIDs.Split(new string[1] { "," }, StringSplitOptions.None)).Contains<string>(roleID.ToString());
        }

        public static bool HasRole(List<string> RoleIDList, byte RoleID)
        {
            return RoleIDList.Contains(RoleID.ToString());
        }

        public static bool HasRole(string roleIDs, UserRoles Role)
        {
            return ((IEnumerable<string>)roleIDs.Split(new string[1] { "," }, StringSplitOptions.None)).Contains<string>(((byte)Role).ToString());
        }

        public static bool HasRole(List<string> RoleIDList, UserRoles Role)
        {
            return RoleIDList.Contains(((byte)Role).ToString());
        }

        public static List<string> GetRoleIDList(string roleIDs)
        {
            return ((IEnumerable<string>)roleIDs.Split(new string[1] { "," }, StringSplitOptions.None)).ToList<string>();
        }

        public static string DisplayMoney(string CurrencyCode, decimal amount)
        {
            if (amount < decimal.Zero)
                return CurrencyCode + " " + Math.Abs(amount).ToString("N");
            return CurrencyCode + " " + amount.ToString("N");
        }

        public static string RandomString(int length)
        {
            Random random = new Random();
            return new string(Enumerable.Repeat<string>("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", length).Select<string, char>((Func<string, char>)(s => s[random.Next(s.Length)])).ToArray<char>());
        }

        public static void AlertElmah(Exception ex)
        {
            try
            {
                if (ex.InnerException == null)
                    ErrorSignal.FromCurrentContext().Raise(ex);
                else if (ex.InnerException.InnerException == null)
                    ErrorSignal.FromCurrentContext().Raise(ex.InnerException);
                else
                    ErrorSignal.FromCurrentContext().Raise(ex.InnerException.InnerException);
            }
            catch (Exception) { }
        }
            
        /// <summary>
        /// To be used for non-admin page controllers... it will trigger a request from the client browser that is subject to caching...
        /// </summary>
        /// <param name="tab">Header selected tab</param>
        /// <returns></returns>
        public static HeaderViewModel GetHeaderModel(byte tab)
        {
            return new HeaderViewModel() { SelectedTab = tab };
        }

        public static HeaderViewModel GetHeaderModel(HeaderTabs tab)
        {
            return new HeaderViewModel() { SelectedTab = (byte)tab };
        }

        /// <summary>
        /// client requests for the headers will hit this action
        /// </summary>
        /// <param name="user"></param>
        /// <param name="tab"></param>
        /// <returns></returns>
        [OutputCache(Duration = 10, VaryByParam = "user;tab", Location = OutputCacheLocation.Client, NoStore = true)]
        public static HeaderViewModel GetHeaderModel(string user, int tab)
        {
            return GetHeaderModel(user, tab > byte.MaxValue || tab < 0 ? (byte)HeaderTabs.Home : (byte)tab);
        }

        public static HeaderViewModel GetHeaderModel(string user, HeaderTabs tab)
        {
            return GetHeaderModel(user, (byte)tab);
        }

        public static HeaderViewModel GetHeaderModel(string user, byte tab)
        {
            var model = new HeaderViewModel
            {
                SelectedTab = tab
            };

            using (var context = new UserContext())
            {
                var ua = context.IEUserList.Where(l => l.Email == user).Select(s => new
                {
                    s.UserID,
                    s.Name,
                    s.SchoolID,
                    s.TypeID,
                    s.AccessGroup.GroupRoles
                }).First();

                model.UserID = ua.UserID;                
                model.SchoolID = ua.SchoolID;
                model.LoggedInUser = ua.Name;
                model.Roles = ua.GroupRoles;
                model.UserTypeID = ua.TypeID;
            }
            return model;
        }

        /// <summary>
        /// client requests for the header logo will hit this action. Cached on the client for one day
        /// </summary>
        /// <param name="SchoolID">School ID</param>
        /// <returns></returns>
        public static LogoViewModel GetHeaderLogo(int SchoolID)
        {
            if (SchoolID == 0)
                return new LogoViewModel();

            var model = new LogoViewModel();
            using (var context = new UserContext())
            {
                var sch = new IEContext().SchoolList.Where(l => l.SchoolID == SchoolID).Select(l => new { l.GuidString, l.Name, l.Website }).First();

                var guid = sch.GuidString;
                if (!string.IsNullOrEmpty(guid))
                    model.LogoSrc = "data:image/png;base64," + Convert.ToBase64String(new School { GuidString = guid }.DownloadFile());

                model.Name = sch.Name;
                model.Url = sch.Website;
            }
            return model;
        }

        public static string FormatEmail(string email)
        {
            return string.IsNullOrEmpty(email) ? "" : email.Replace(".", "\x200B.").Replace("@", "\x200B@");
        }

        public static string Encrypt(string unencryptedString)
        {
            return string.IsNullOrEmpty(unencryptedString) ? unencryptedString : Encryption64.Encrypt(unencryptedString.ToString(), Secrets.CryptoKey);
        }

        public static string Decrypt(string encryptedString)
        {
            return string.IsNullOrEmpty(encryptedString) ? encryptedString : Encryption64.Decrypt(encryptedString.Replace(" ", "+"), Secrets.CryptoKey);
        }


        public static string StateName(byte StateID)
        {
            return Eval.GetDisplayName(typeof(State), StateID);
        }

        public static string ImprovementString(decimal Improvement)
        {
            return Improvement == 0 ? "-" : Improvement.ToString("F") + "%";
        }

        public static string ImprovementDisplay(decimal Improvement)
        {
            return ImprovementString(Improvement) + (Improvement > 0 ? " <span style='color: green !important;'><i class='fa fa-arrow-up'></i></span>" :
                                                    (Improvement < 0 ? " <span style='color: #999 !important;'><i class='fa fa-arrow-down'></i></span>" : ""));
        }

        public static string PositionDisplay(byte Position, int StudentCount)
        {
            var postfix = "th";

            //assuming the class size cannot be up to 100
            if(Position < 4 || Position > 20)
            {
                var rem = Position % 10;
                if (rem == 1) postfix = "st";
                else if (rem == 2) postfix = "nd";
                else if (rem == 3) postfix = "rd";
            }

            return Position.ToString() + postfix + (StudentCount > 0 ? " of " + StudentCount.ToString() : "");
        }

        public static string SendEmails()
        {
            var context = new IEContext();

            var url = context.EmailSettingsList.Select(l => l.ApplicationPath).First() + "Service/SendMails";

            var result = "";
            try
            {
                result = new WebClient().DownloadString(url);
            }
            catch (Exception ex)
            {
                AlertElmah(ex);
                result = "Failed! Error:" + ex.Message;
            }

            return result;
        }

        public static string TermName(short SchoolYear, byte TermNumber)
        {
            string num = "";

            switch (TermNumber)
            {
                case 1: num = "1st"; break;
                case 2: num = "2nd"; break;
                case 3: num = "3rd"; break;
            }

            return SchoolYear.ToString() + "/" + (SchoolYear + 1).ToString() + " " + num + " Term";
        }

        public static string YearName(short SchoolYear)
        {
            return SchoolYear.ToString() + "/" + (SchoolYear + 1).ToString();
        }

        /// <summary>
        /// Product Cost
        /// </summary>
        /// <param name="SizeID"></param>
        /// <param name="ProductID"></param>
        /// <returns></returns>
        public static int ProductPrice(byte SizeID, byte ProductID)
        {
            switch (ProductID)
            {
                case (byte)IEProduct.PreviousTerm: return (20000 + (SizeID - 1) * 5000);
                case (byte)IEProduct.ScoreSheet: return (20000 + (SizeID - 1) * 5000);
                case (byte)IEProduct.ManualEntry: return (20000 + (SizeID - 1) * 5000);
                case (byte)IEProduct.ResultPrint: return (20000 + (SizeID - 1) * 5000);
                case (byte)IEProduct.ResultPackage: return (40000 + (SizeID - 1) * 10000);
                case (byte)IEProduct.BroadsheetPrint: return (10000 + (SizeID - 1) * 3000);
                case (byte)IEProduct.SubjectAnalysis: return (10000 + (SizeID - 1) * 3000);
                case (byte)IEProduct.AnalysisBooklet: return (20000 + (SizeID - 1) * 5000);
                case (byte)IEProduct.AnalysisBooklet30: return (15000 + (SizeID - 1) * 5000);
                case (byte)IEProduct.AnalysisCD: return (10000 + (SizeID - 1) * 5000);
                case (byte)IEProduct.Online: return (15000 + (SizeID - 1) * 3000);
                case (byte)IEProduct.Online30: return (10000 + (SizeID - 1) * 3000);
                default: return 0;
            }
        }
    }

    public class UploadedFile
    {
        public UploadedFile() { }

        [StringLength(260)]
        public string FileName { get; set; }

        [StringLength(256)]
        public string ContentType { get; set; }

        [StringLength(256)]
        public string Description { get; set; }

        public DateTime UploadTime { get; set; }

        public int FileSize { get; set; }

        [StringLength(128)]
        public string GuidString { get; set; }

        public virtual FileType FileType { get { return FileType.Result; } }

        public string ExternalFileID { get { return GuidString; } }

        public void UploadFile(byte[] FileData)
        {
            GuidString = Guid.NewGuid().ToString();

            if (General.UseAzureFileStorage)
            {
                CloudBlockBlob blockBlob = GetBlobContainer(FileType).GetBlockBlobReference(ExternalFileID);
                blockBlob.UploadFromByteArray(FileData, 0, FileData.Length, null, null, null);
            }
            else
            {
                ExternalFile exFile = new ExternalFile
                {
                    ExternalFileID = ExternalFileID,
                    Content = FileData
                };

                FileContext fContext = new FileContext();

                switch (FileType)
                {
                    case FileType.Logo: fContext.LogoFileList.Add(new ExtLogoFile(exFile)); break;
                    case FileType.Picture: fContext.PictureFileList.Add(new ExtPictureFile(exFile)); break;
                    case FileType.Result: fContext.ResultFileList.Add(new ExtResultFile(exFile)); break;
                }

                fContext.SaveChanges();
            }
        }

        public byte[] DownloadFile()
        {
            if (General.UseAzureFileStorage)
            {
                CloudBlockBlob blockBlob = GetBlobContainer(FileType).GetBlockBlobReference(ExternalFileID);
                MemoryStream ms = new MemoryStream();
                blockBlob.DownloadToStream(ms, null, null, null);

                return ms.ToArray();
            }
            else
            {
                switch (FileType)
                {
                    case FileType.Logo:
                        return new FileContext().LogoFileList.Where(f => f.ExternalFileID == ExternalFileID).Select(f => f.Content).First();

                    case FileType.Picture:
                        return new FileContext().PictureFileList.Where(f => f.ExternalFileID == ExternalFileID).Select(f => f.Content).First();

                    case FileType.Result:
                        return new FileContext().ResultFileList.Where(f => f.ExternalFileID == ExternalFileID).Select(f => f.Content).First();

                    default: return null;
                }
            }
        }

        public void DeleteFile()
        {
            if (General.UseAzureFileStorage)
            {
                CloudBlockBlob blockBlob = GetBlobContainer(FileType).GetBlockBlobReference(ExternalFileID);
                blockBlob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots, null, null, null);
            }
            else
            {
                FileContext fContext = new FileContext();
                switch (FileType)
                {
                    case FileType.Logo:
                        var sFile = new ExtLogoFile { ExternalFileID = ExternalFileID };
                        fContext.LogoFileList.Attach(sFile);
                        fContext.Entry(sFile).State = System.Data.Entity.EntityState.Deleted;
                        break;

                    case FileType.Picture:
                        var pFile = new ExtPictureFile { ExternalFileID = ExternalFileID };
                        fContext.PictureFileList.Attach(pFile);
                        fContext.Entry(pFile).State = System.Data.Entity.EntityState.Deleted;
                        break;

                    case FileType.Result:
                        var rFile = new ExtResultFile { ExternalFileID = ExternalFileID };
                        fContext.ResultFileList.Attach(rFile);
                        fContext.Entry(rFile).State = System.Data.Entity.EntityState.Deleted;
                        break;
                }

                fContext.SaveChanges();
            }
        }

        private static CloudBlobContainer GetBlobContainer(FileType type)
        {
            string ConnString = Secrets.AzureStorageConnString;

            string ContainerName = Secrets.AzureBlobContainer;

            switch (type)
            {
                case FileType.Logo: ContainerName += "logos"; break;
                case FileType.Picture: ContainerName += "pictures"; break;
                case FileType.Result: ContainerName += "results"; break;
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            return blobClient.GetContainerReference(ContainerName);
        }
    }

    /// <summary>
    /// Encryption Class
    /// </summary>
    public static class Encryption64
    {
        #region members

        private const string DEFAULT_KEY = "#kl?+@<z";

        #endregion

        public static string Encrypt(string stringToEncrypt, string key)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream;

            // Check whether the key is valid, otherwise make it valid
            CheckKey(ref key);

            des.Key = HashKey(key, des.KeySize / 8);
            des.IV = HashKey(key, des.KeySize / 8);
            byte[] inputBytes = Encoding.UTF8.GetBytes(stringToEncrypt);

            cryptoStream = new CryptoStream(memoryStream, des.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(inputBytes, 0, inputBytes.Length);
            cryptoStream.FlushFinalBlock();

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public static string Decrypt(string stringToDecrypt, string key)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream;

            // Check whether the key is valid, otherwise make it valid
            CheckKey(ref key);

            des.Key = HashKey(key, des.KeySize / 8);
            des.IV = HashKey(key, des.KeySize / 8);
            byte[] inputBytes = Convert.FromBase64String(stringToDecrypt);

            cryptoStream = new CryptoStream(memoryStream, des.CreateDecryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(inputBytes, 0, inputBytes.Length);
            cryptoStream.FlushFinalBlock();

            Encoding encoding = Encoding.UTF8;
            return encoding.GetString(memoryStream.ToArray());
        }

        /// <summary>
        /// Make sure the used key has a length of exact eight characters.
        /// </summary>
        /// <param name="keyToCheck">Key being checked.</param>
        private static void CheckKey(ref string keyToCheck)
        {
            keyToCheck = keyToCheck.Length > 8 ? keyToCheck.Substring(0, 8) : keyToCheck;
            if (keyToCheck.Length < 8)
            {
                for (int i = keyToCheck.Length; i < 8; i++)
                {
                    keyToCheck += DEFAULT_KEY[i];
                }
            }
        }

        /// <summary>
        /// Hash a key.
        /// </summary>
        /// <param name="key">Key being hashed.</param>
        /// <param name="length">Length of the output.</param>
        /// <returns></returns>
        private static byte[] HashKey(string key, int length)
        {
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();

            // Hash the key
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] hash = sha1.ComputeHash(keyBytes);

            // Truncate hash
            byte[] truncatedHash = new byte[length];
            Array.Copy(hash, 0, truncatedHash, 0, length);
            return truncatedHash;
        }
    }

}