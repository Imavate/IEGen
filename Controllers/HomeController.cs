using AutoMapper;
using IEGen.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Z.EntityFramework.Plus;

namespace IEGen.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var model = new HomeIndexPageViewModel();

            var hVM = General.GetHeaderModel(User.Identity.Name, HeaderTabs.Home);

            var context = new IEContext();
            var def = context.UserDefaultsList.Where(l => l.UserID == hVM.UserID && l.SchoolID == hVM.SchoolID).Select(l => new { School = l.School.Name, Term = l.Term.Name }).FirstOrDefault();

            if(def != null)
            {
                model.TermName = def.Term;
                model.SchoolName = string.IsNullOrEmpty(def.School) ? "Select School Term" : def.School;
            }
            else
            {
                model.TermName = "Select School Term";
                model.SchoolName = "Select School";
            }

            model.HeaderViewModel = hVM;
            
            return View(model);
        }


        #region Registration Actions

        [AllowAnonymous]
        [Route("Start")]
        [Route("School/New")]
        public ActionResult NewSchool()
        {
            ViewBag.SiteKey = AppSecrets.Secrets.RecaptchaSiteKey;
            return View();
        }

        [AllowAnonymous]
        public ActionResult SubmitSchoolRequest(AddSchoolRegViewModel model)
        {
            try
            {
                var errMsg = "";

                if (!IsValidCaptcha(out errMsg))
                {
                    General.AlertElmah(new Exception("Captcha Validation Error [Submit School Request]: " + errMsg));
                    return DefaultErrorAlert("Please validate that you are not a robot!");
                }
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert("Please validate that you are not a robot!");
            }

            var context = new IEContext();

            var gp = new SchoolReg();
            Mapper.Map(model, gp);

            gp.RegDate = DateTime.Today;

            context.SchoolRegList.Add(gp);

            try
            {
                context.EmailMessageList.AddRange(GetSchoolRequestMails(model));

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert("Your Request was submitted successfully! We will contact you shortly.");
        }

        private List<EmailMessage> GetSchoolRequestMails(AddSchoolRegViewModel model)
        {
            string body = "<p>We received a request from you on the Imavate Education Portal.</p>" +
                          "<p>We will get in touch with you using the details you provided alongisde the request as follows:</p><br>" +
                          "<p>School: <strong>" + model.Name + "</strong>.</p>" +
                          "<p>Phone: <strong>" + model.Phone + "</strong>.</p><br>" +
                          "<p>If you did not make or authorize this request, please let us know by responding to this email.</p>";

            var schoolMail = new EmailMessage
            {
                ToEmail = model.Email,
                Subject = "School Request Received",
                Name = model.ContactPerson,
                Body = body
            };

            var admBody = "<p>New School Request as follows: </p><br>" +
                          "<p><strong>School:</strong> <br>" + model.Name + "</p>" +
                          "<p><strong>Contact Person:</strong> <br>" + model.ContactPerson + "</p>" +
                          "<p><strong>Type:</strong> <br>" + Eval.GetDisplayName(model.SchoolType) + "</p>" +
                          "<p><strong>Email:</strong> <br>" + model.Email + "</p>" +
                          "<p><strong>Phone:</strong> <br>" + model.Phone + "</p>" +
                          "<p><strong>Address:</strong> <br>" + model.Address + "</p>" +
                          "<p><strong>Notes:</strong> <br>" + model.Notes + "</p>";

            var adminMail = new EmailMessage
            {
                ToEmail = AppSecrets.Secrets.CorrespondenceEmail,
                Subject = "New School Request",
                Name = "Imavate Education",
                Body = admBody
            };

            return new List<EmailMessage> { schoolMail, adminMail };
        }

        [AllowAnonymous]
        [Route("Teacher/New")]
        public ActionResult NewTeacher()
        {
            ViewBag.SiteKey = AppSecrets.Secrets.RecaptchaSiteKey;
            return View();
        }

        [AllowAnonymous]
        public ActionResult SubmitNewTeacher(EditCTeacherViewModel model)
        {
            try
            {
                var errMsg = "";

                if (!IsValidCaptcha(out errMsg))
                {
                    General.AlertElmah(new Exception("Captcha Validation Error [Submit School Request]: " + errMsg));
                    return DefaultErrorAlert("Please validate that you are not a robot!");
                }
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert("Please validate that you are not a robot!");
            }

            var context = new IEContext();

            var gp = new CTeacher();
            Mapper.Map(model, gp);

            context.CTeacherList.Add(gp);

            try
            {
                context.EmailMessageList.AddRange(GetNewTeacherMails(model));

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }

            return DefaultSuccessAlert("Your Request was submitted successfully! We will contact you shortly.");
        }

        private List<EmailMessage> GetNewTeacherMails(EditCTeacherViewModel model)
        {
            string body = "<p>You created a new teacher profile on the Imavate Education Portal.</p>" +
                          "<p>We will get in touch with you using the details you provided alongisde the request as follows:</p><br>" +
                          "<p>School: <strong>" + model.SchoolName + "</strong>.</p>" +
                          "<p>Phone: <strong>" + model.Phone + "</strong>.</p><br>" +
                          "<p>If you did not make or authorize this request, please let us know by responding to this email.</p>";

            var userMail = new EmailMessage
            {
                ToEmail = model.Email,
                Subject = "Teacher Registration Received",
                Name = model.Name,
                Body = body
            };

            var admBody = "<p>New Teacher Request as follows: </p><br>" +
                          "<p><strong>Name:</strong> <br>" + model.Name + "</p>" +
                          "<p><strong>Email:</strong> <br>" + model.Email + "</p>" +
                          "<p><strong>Phone:</strong> <br>" + model.Phone + "</p>" +
                          "<p><strong>Sex:</strong> <br>" + model.Sex + "</p>" +
                          "<p><strong>Designation:</strong> <br>" + model.Designation + "</p>" +
                          "<p><strong>School:</strong> <br>" + model.SchoolName + "</p>" +
                          "<p><strong>School Type:</strong> <br>" + Eval.GetDisplayName(model.SchoolType) + "</p>" +
                          "<p><strong>Address:</strong> <br>" + model.SchoolAddress + "</p>";

            var adminMail = new EmailMessage
            {
                ToEmail = AppSecrets.Secrets.CorrespondenceEmail,
                Subject = "New Teacher Request",
                Name = "Administrator",
                Body = admBody
            };

            return new List<EmailMessage> { userMail, adminMail };
        }


        private bool IsValidCaptcha(out string error)
        {
            // Validate Captcha here
            bool retVal = false;
            error = "";
            string[] resultFromGoogle = new string[] { "false", "" };
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify");

            req.ProtocolVersion = HttpVersion.Version11;
            req.Timeout = 0x7530;
            req.Method = "POST";
            req.UserAgent = "reCAPTCHA/ASP.NET";
            req.ContentType = "application/x-www-form-urlencoded";

            string ip = "";

            try
            {
                //ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();

                //The X-Forwarded-For (XFF) HTTP header field is a de facto standard for identifying the originating 
                //IP address of a client connecting to a web server through an HTTP proxy or load balancer

                string pip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(pip))
                {
                    ip = Request.ServerVariables["REMOTE_ADDR"];
                }
                else
                {
                    string[] addresses = pip.Split(',');
                    if (addresses.Length != 0)
                    {
                        ip = addresses[0];
                    }
                }

            }
            catch (Exception) { }

            string Fdata = string.Format("secret={0}&response={1}&remoteip={2}",
                new object[]{
                        HttpUtility.UrlEncode(AppSecrets.Secrets.RecaptchaSecretKey),
                        HttpUtility.UrlEncode(Request["g-recaptcha-response"]),
                        HttpUtility.UrlEncode(ip)
                    });

            byte[] resData = Encoding.ASCII.GetBytes(Fdata);
            using (Stream rStream = req.GetRequestStream())
            {
                rStream.Write(resData, 0, resData.Length);
            }
            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (TextReader readStream = new StreamReader(wResponse.GetResponseStream(), Encoding.UTF8))
                    {
                        JObject joResponse = JObject.Parse(readStream.ReadToEnd());
                        retVal = joResponse["success"].ToString().ToLower() == "true";

                        if (!retVal) error = joResponse["error-codes"].ToString();
                    }
                }
            }
            catch (WebException ex)
            {
                General.AlertElmah(ex);
                error = ex.InnerException.ToString();
            }

            return retVal;
        }


        #endregion


        #region Service Actions

        [AllowAnonymous]
        [Route("Service/SendMails")]
        public async Task<ActionResult> SendBulkEmails()
        {
            var context = new IEContext();
            var eset = context.EmailSettingsList.First();
            if (!eset.UseDefaultMailParameters) return await SendBulkEmails1(eset);

            var batchSize = eset.BulkSendBatchSize;
            if (batchSize < 1) batchSize = 100;


            EmailObjectMany emObj = EmailSettings.BlankMails(context);

            var delDate = DateTime.Now.AddDays(-7);
            context.EmailMessageList.Where(l => l.TimeSent < delDate).Delete();

            var msgList = context.EmailMessageList.Take(batchSize).ToList();

            if (!msgList.Any()) return Json("No mails to send!", JsonRequestBehavior.AllowGet);

            emObj.MailMessages.AddRange(msgList);

            bool hasErrors = false;
            var errors = await SendEmailsAsync(emObj);
            if (errors.Any())
            {
                foreach (string err in errors)
                    General.AlertElmah(new Exception("Mailing Error: " + err));

                hasErrors = true;
            }

            try
            {
                foreach (var m in msgList)
                    context.Entry(m).State = EntityState.Deleted;

                await context.SaveChangesAsync();
            }
            catch (Exception ex) { General.AlertElmah(ex); }

            var retMsg = "Completed " + (hasErrors ? "with Errors" : "Successfully!") + " Message Count:" + msgList.Count;

            return Json(retMsg, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> SendBulkEmails1(EmailSettings eset)
        {
            var context = new IEContext();
            var batchSize = eset.BulkSendBatchSize;
            if (batchSize < 1) batchSize = 100;

            EmailObjectMany emObj = EmailSettings.BlankMails(context);

            string fromEmail = emObj.FromEmail;

            var delDate = DateTime.Now.AddDays(-7);
            context.EmailMessageList.Where(l => l.TimeSent < delDate).Delete();

            var delList = new List<EmailMessage>();

            var msgList = context.EmailMessageList.Take(batchSize).ToList();



            if (!msgList.Any()) return Json("No mails to send!", JsonRequestBehavior.AllowGet);

            bool hasErrors = false;
            foreach (var msg in msgList)
            {
                if (fromEmail != msg.FromEmail && emObj.MailMessages.Any())
                {
                    var errs = await SendEmailsAsync(emObj);
                    if (errs.Any())
                    {
                        foreach (string err in errs)
                            General.AlertElmah(new Exception("Mailing Error: " + err));

                        hasErrors = true;
                    }

                    fromEmail = msg.FromEmail;
                    var sc = new System.Net.Mail.SmtpClient()
                    {
                        Credentials = new System.Net.NetworkCredential(fromEmail, ""),
                        Host = eset.SmtpServer,
                        Port = eset.SmtpPort,
                        EnableSsl = eset.EnableSslMail
                    };
                    emObj.SmtpClient = sc;
                    emObj.MailMessages.Clear();
                }

                emObj.MailMessages.Add(msg);
                delList.Add(msg);
            }

            var errors = await SendEmailsAsync(emObj);
            if (errors.Any())
            {
                foreach (string err in errors)
                    General.AlertElmah(new Exception("Mailing Error: " + err));

                hasErrors = true;
            }

            try
            {
                foreach (var m in delList)
                    context.Entry(m).State = EntityState.Deleted;

                await context.SaveChangesAsync();
            }
            catch (Exception ex) { General.AlertElmah(ex); }

            var retMsg = "Completed " + (hasErrors ? "with Errors" : "Successfully!") + " Message Count:" + msgList.Count;

            return Json(retMsg, JsonRequestBehavior.AllowGet);
        }

        #endregion

    }
}