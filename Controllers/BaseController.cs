using AutoMapper;
using DataTables.Mvc;
using IEGen.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IEGen.Controllers
{
    public class BaseController : Controller
    {
        public ActionResult DefaultErrorAlert()
        {
            return DefaultErrorAlert("Apologies. An Error occurred! Please refresh your page and try again. Thank you");
        }

        public ActionResult DefaultErrorAlert(string errMsg)
        {
            Response.StatusCode = 400;
            AlertViewModel alertViewModel = AlertViewModel.Create(errMsg, AlertTypes.Error);
            return PartialView("_Alert", alertViewModel);
        }

        public ActionResult DefaultInfoAlert(string msg)
        {
            AlertViewModel alertViewModel = AlertViewModel.Create(msg, AlertTypes.Info);
            return PartialView("_Alert", alertViewModel);
        }

        public ActionResult DefaultSuccessAlert(string msg)
        {
            AlertViewModel alertViewModel = AlertViewModel.Create(msg, AlertTypes.Success);
            return PartialView("_Alert", alertViewModel);
        }

        public ActionResult DefaultWarningAlert(string msg)
        {
            AlertViewModel alertViewModel = AlertViewModel.Create(msg, AlertTypes.Warning);
            return PartialView("_Alert", alertViewModel);
        }

        public ActionResult DefaultDangerAlert(string msg)
        {
            AlertViewModel alertViewModel = AlertViewModel.Create(msg, "fa-exclamation-triangle", AlertTypes.Error);
            return PartialView("_Alert", alertViewModel);
        }

        public ActionResult PartialViewRedirect(string location)
        {
            return PartialView("_JSRedirect", new JSRedirectViewModel() { Location = location });
        }

        public ActionResult JSRedirect(string location)
        {
            return JavaScript("window.location='" + location + "'");
        }

        public void LoadFromInfo(EmailMessage mail, EmailObject obj)
        {
            if (string.IsNullOrEmpty(mail.FromEmail))
            {
                mail.FromEmail = obj.FromEmail;
                mail.FromName = obj.FromName;
            }
        }

        public async Task<List<string>> SendEmailAsync(EmailObject emailObject)
        {
            List<string> errList = new List<string>();
            List<Task> taskList = new List<Task>();

            LoadFromInfo(emailObject.MailMessage, emailObject);

            if (emailObject.UseDefaultParameters)
            {
                taskList.Add(SendDefMailAsync(emailObject.MailMessage));
            }
            else
            {
                SmtpClient smtp = emailObject.SmtpClient;
                taskList.Add(SendMailAsync(smtp, emailObject.MailMessage));
            }

            try
            {
                await Task.WhenAll(taskList).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                General.AlertElmah(exc);
                foreach (Task faulted in taskList.Where(t => t.IsFaulted))
                {
                    errList.Add(faulted.Exception.InnerException.Message);
                }
            }

            return errList;
        }

        public async Task<List<string>> SendEmailsAsync(EmailObjectMany emailObject)
        {
            List<string> errList = new List<string>();
            List<Task> taskList = new List<Task>();

            if (emailObject.UseDefaultParameters)
            {
                foreach (EmailMessage m in emailObject.MailMessages)
                {
                    LoadFromInfo(m, emailObject);
                    taskList.Add(SendDefMailAsync(m));
                }
            }
            else
            {
                foreach (EmailMessage m in emailObject.MailMessages)
                {
                    SmtpClient smtp = emailObject.CloneSmtpClient();
                    LoadFromInfo(m, emailObject);
                    taskList.Add(SendMailAsync(smtp, m));
                }
            }

            try
            {
                await Task.WhenAll(taskList).ConfigureAwait(false);

                //ShowMessage("Registration Approved. Registrant Notified!");
            }
            catch (Exception exc)
            {
                foreach (Task faulted in taskList.Where(t => t.IsFaulted))
                {
                    errList.Add(faulted.Exception.InnerException.Message);
                }

                General.AlertElmah(exc);
            }

            return errList;
        }

        async Task SendMailAsync(SmtpClient smtp, EmailMessage m)
        {
            try
            {
                await smtp.SendMailAsync(m.GetSmtpMessage());
            }
            catch (Exception em)
            {
                throw new Exception(m.ToEmail + ": " + em.Message);
            }
        }

        async Task SendDefMailAsync(EmailMessage m)
        {
            try
            {
                await EmailSettings.GetSendGridClient().SendEmailAsync(m.GetSendGridMessage());
            }
            catch (Exception em)
            {
                throw new Exception(em.Message);
            }
        }

        public void SendEmail(EmailObject emailObject)
        {
            List<string> errList = new List<string>();

            LoadFromInfo(emailObject.MailMessage, emailObject);

            try
            {
                if (emailObject.UseDefaultParameters)
                {
                    SendDefMail(emailObject.MailMessage);
                }
                else
                {
                    SmtpClient smtp = emailObject.SmtpClient;
                    SendMail(smtp, emailObject.MailMessage);
                }
            }
            catch (Exception exc)
            {
                General.AlertElmah(exc);
            }
        }

        public void SendEmails(EmailObjectMany emailObject)
        {
            try
            {
                if (emailObject.UseDefaultParameters)
                {
                    foreach (EmailMessage m in emailObject.MailMessages)
                    {
                        LoadFromInfo(m, emailObject);
                        SendDefMail(m);
                    }
                }
                else
                {
                    foreach (EmailMessage m in emailObject.MailMessages)
                    {
                        SmtpClient smtp = emailObject.CloneSmtpClient();
                        LoadFromInfo(m, emailObject);
                        SendMail(smtp, m);
                    }
                }
            }
            catch (Exception exc)
            {
                General.AlertElmah(exc);
            }
        }

        private void SendMail(SmtpClient smtp, EmailMessage m)
        {
            try
            {
                smtp.Send(m.GetSmtpMessage());
            }
            catch (Exception em)
            {
                var msg = m.ToEmail + ": " + em.Message;
                if (em.InnerException != null)
                {
                    msg += "; Inner Exception: " + em.InnerException.Message;

                    if (em.InnerException.InnerException != null)
                        msg += "; Inner Exception 2: " + em.InnerException.InnerException.Message;
                }
                throw new Exception(m.ToEmail + ": " + em.Message);
            }
        }

        private void SendDefMail(EmailMessage m)
        {
            try
            {
                EmailSettings.GetSendGridClient().SendEmailAsync(m.GetSendGridMessage());
            }
            catch (Exception em)
            {
                throw new Exception(em.Message);
            }
        }

        public string SortDirStr(Column.OrderDirection dir)
        {
            return dir == Column.OrderDirection.Ascendant ? " asc" : " desc";
        }


        #region Public Lists and other Methods
        public List<SelectListItem> GetClassList(IEContext context, int? TermID)
        {
            var list = new List<SelectListItem>();

            var gpList = context.ClassList.Where(l => l.TermID == TermID)
                                .Select(t => new { t.Arm.Name, t.ClassID }).ToList()
                                .Select(l => new SelectListItem { Value = l.ClassID.ToString(), Text = l.Name });

            list.AddRange(gpList);
            return list;
        }

        public List<SelectListItem> GetTermList(IEContext context, int SchoolID)
        {
            var list = new List<SelectListItem>();

            var gpList = context.TermList.Where(l => l.SchoolID == SchoolID)
                                .Select(t => new { t.Name, t.TermID }).ToList()
                                .Select(l => new SelectListItem { Value = l.TermID.ToString(), Text = l.Name });

            list.AddRange(gpList);
            return list;
        }

        public List<SelectListItem> GetTeacherList(IEContext context, int SchoolID)
        {
            var list = new List<SelectListItem>();

            var gpList = context.TeacherList.Where(l => l.SchoolID == SchoolID && !l.HasLeft)
                                .Select(t => new { t.Name, t.TeacherID }).ToList()
                                .Select(l => new SelectListItem { Value = l.TeacherID.ToString(), Text = l.Name });

            list.AddRange(gpList);
            return list;
        }

        public List<SelectListItem> GetLocationList(IEContext context)
        {
            var list = new List<SelectListItem>();

            var gpList = context.LocationList
                                .Select(t => new { t.Name, t.LocationID, t.StateID }).ToList()
                                .Select(l => new SelectListItem { Value = l.LocationID.ToString(), Text = General.StateName(l.StateID) + " - " + l.Name });

            list.AddRange(gpList);
            return list;
        }

        public List<SelectListItem> GetLevelList(byte SchoolTypeID)
        {
            var tList = Enum.GetValues(typeof(ClassLevel)).Cast<ClassLevel>();

            switch (SchoolTypeID)
            {
                case (byte)SchoolType.NurseryPrimary: tList = tList.Where(e => e < ClassLevel.JS1); break;
                case (byte)SchoolType.Secondary: tList = tList.Where(e => e >= ClassLevel.JS1 && e <= ClassLevel.SS3); break;
                default: break;
            }

            return tList.Select(e => new SelectListItem
                                    {
                                        Value = ((int)e).ToString(),
                                        Text = Eval.GetDisplayName(typeof(ClassLevel), (byte)e)
                                    }).ToList();
        }

        public List<LevelModel> GetLevelModelList(byte SchoolTypeID)
        {
            var tList = Enum.GetValues(typeof(ClassLevel)).Cast<ClassLevel>();

            switch (SchoolTypeID)
            {
                case (byte)SchoolType.NurseryPrimary: tList = tList.Where(e => e < ClassLevel.JS1); break;
                case (byte)SchoolType.Secondary: tList = tList.Where(e => e >= ClassLevel.JS1 && e <= ClassLevel.SS3); break;
                default: break;
            }

            return tList.Select(e => new LevelModel
            {
                LevelID = ((byte)e),
                LevelName = Eval.GetDisplayName(typeof(ClassLevel), (byte)e)
            }).ToList();
        }

        public List<SelectListItem> GetLevelList(IEContext context, int SchoolID)
        {
            var typeID = context.SchoolList.Where(l => l.SchoolID == SchoolID).Select(l => l.TypeID).First();

            var tList = Enum.GetValues(typeof(ClassLevel))
                                    .Cast<ClassLevel>();

            switch (typeID)
            {
                case (byte)SchoolType.NurseryPrimary: tList = tList.Where(e => e < ClassLevel.JS1); break;
                case (byte)SchoolType.Secondary: tList = tList.Where(e => e >= ClassLevel.JS1 && e <= ClassLevel.SS3); break;
                default: break;
            }

            return tList.Select(e => new SelectListItem
            {
                Value = ((int)e).ToString(),
                Text = Eval.GetDisplayName(typeof(ClassLevel), (byte)e)
            }).ToList();
        }

        public int? GetCurrentTermID(IEContext context, int SchoolID)
        {
            return context.TermList.Where(l => l.SchoolID == SchoolID && context.ClassList.Any(c => c.TermID == l.TermID))
                          .OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).Select(l => (int?)l.TermID).FirstOrDefault();
        }

        public TermMiniModel GetCurrentTerm(IEContext context, int SchoolID)
        {
            return context.TermList.Where(l => l.SchoolID == SchoolID && context.ClassList.Any(c => c.TermID == l.TermID))
                          .OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).ProjectToFirstOrDefault<TermMiniModel>();
        }

        public SchoolTermMiniModel GetCurrentSchoolTerm(IEContext context, int SchoolID)
        {
            return context.TermList.Where(l => l.SchoolID == SchoolID && context.ClassList.Any(c => c.TermID == l.TermID))
                          .OrderByDescending(l => l.SchoolYear).ThenByDescending(l => l.TermNumber).ProjectToFirstOrDefault<SchoolTermMiniModel>();
        }

        public List<SelectListItem> GetExamTypeList(IEContext context, int? SchoolID)
        {
            var list = new List<SelectListItem>();

            var gpList = context.OtherExamTypeList.Where(l => l.SchoolID == SchoolID && !l.IsDisabled)
                                .Select(t => new { t.Name, t.TypeID }).ToList()
                                .Select(l => new SelectListItem { Value = l.TypeID.ToString(), Text = l.Name });

            list.AddRange(gpList);
            return list;
        }

        public string RenderRazorViewToString(Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
            // checking the view inside the controller  
            if (viewResult.View != null)
            {
                using (var sw = new System.IO.StringWriter())
                {
                    var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                    viewResult.View.Render(viewContext, sw);
                    viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                    return sw.GetStringBuilder().ToString();
                }
            }
            else
                return "View cannot be found.";
        }
        #endregion

        #region SQL Database Script Methods

        public void AssignScoreGrades(int SubjectID, int GradeGroupID)
        {
            using (SqlConnection conn = new SqlConnection(IEContext.ConnString))
            {
                conn.Open();

                string qs = @"
                    UPDATE ScoreEntries
                    SET GradeID = g.GradeID
                    FROM ScoreEntries e, Grades g
                    WHERE e.SubjectID = @SubjectID and g.GradeGroupID = @GradeGroupID
	                    and (ISNULL(e.CAScore, 0) + e.ExamScore) >= g.LowerBound
	                    and (ISNULL(e.CAScore, 0) + e.ExamScore) < (g.UpperBound + 1)
                    ";

                SqlCommand cmd = new SqlCommand(qs, conn);

                SqlParameter subParam = new SqlParameter();
                subParam.ParameterName = "@SubjectID";
                subParam.Value = SubjectID;
                cmd.Parameters.Add(subParam);

                SqlParameter gdParam = new SqlParameter();
                gdParam.ParameterName = "@GradeGroupID";
                gdParam.Value = GradeGroupID;
                cmd.Parameters.Add(gdParam);

                cmd.ExecuteNonQuery();
            }
        }

        public void AssignClassGrades(int ClassID, int GradeGroupID)
        {
            using (SqlConnection conn = new SqlConnection(IEContext.ConnString))
            {
                conn.Open();

                string qs = @"
                    UPDATE ScoreEntries
                    SET GradeID = g.GradeID
                    FROM ScoreEntries e, Subjects s, Grades g
                    WHERE e.SubjectID = s.SubjectID and s.ClassID = @ClassID and g.GradeGroupID = @GradeGroupID
	                    and (ISNULL(e.CAScore, 0) + e.ExamScore) >= g.LowerBound
	                    and (ISNULL(e.CAScore, 0) + e.ExamScore) < (g.UpperBound + 1)
                    ";

                SqlCommand cmd = new SqlCommand(qs, conn);

                SqlParameter subParam = new SqlParameter();
                subParam.ParameterName = "@ClassID";
                subParam.Value = ClassID;
                cmd.Parameters.Add(subParam);

                SqlParameter gdParam = new SqlParameter();
                gdParam.ParameterName = "@GradeGroupID";
                gdParam.Value = GradeGroupID;
                cmd.Parameters.Add(gdParam);

                cmd.ExecuteNonQuery();
            }
        }

        public void PrepareNewClassSkills(int ClassID)
        {
            var cqs = @"INSERT INTO StudentSkills(ClassID, StudentID, SerialNumber)
                        SELECT @ClassID, r.StudentID, r.SN + (SELECT COUNT(1) FROM StudentSkills WHERE ClassID = @ClassID)
                        FROM (SELECT StudentID, ROW_NUMBER() OVER(ORDER BY Surname, FirstName) AS SN 
                              FROM Students s WHERE ClassID = @ClassID and NOT EXISTS (SELECT 1 FROM StudentSkills se WHERE se.StudentID = s.StudentID and se.ClassID = @ClassID)) r;";

            var hqs = @"UPDATE StudentSkills
                        SET SerialNumber = o.SN
                        FROM StudentSkills e, 
	                         (SELECT StudentID, ROW_NUMBER() OVER(ORDER BY SerialNumber) AS SN 
	                          FROM StudentSkills s WHERE s.ClassID = @ClassID) o
                        WHERE e.ClassID = @ClassID and e.StudentID = o.StudentID";

            using (SqlConnection conn = new SqlConnection(IEContext.ConnString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(cqs, conn);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@ClassID";
                param.Value = ClassID;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();

                SqlCommand cmd1 = new SqlCommand(hqs, conn);

                SqlParameter param1 = new SqlParameter();
                param1.ParameterName = "@ClassID";
                param1.Value = ClassID;
                cmd1.Parameters.Add(param1);

                cmd1.ExecuteNonQuery();
            }
        }

        public void PrepareScoreSheets(int ClassID)
        {
            var cqs = @"INSERT INTO ScoreEntries(SubjectID, StudentID, SerialNumber)
                        SELECT c.SubjectID, s.StudentID, SN + e.PSN
                        FROM Subjects c, 
                             (SELECT StudentID, ROW_NUMBER() OVER(ORDER BY Surname, FirstName) AS SN FROM Students s WHERE ClassID = @ClassID) s,
	                         (SELECT SubjectID, (SELECT COUNT(1) FROM ScoreEntries WHERE SubjectID = s.SubjectID) PSN FROM Subjects s WHERE ClassID = @ClassID) e
                        WHERE c.ClassID = @ClassID and c.SubjectID = e.SubjectID 
                            and NOT EXISTS (SELECT 1 FROM ScoreEntries se WHERE se.StudentID = s.StudentID and se.SubjectID = c.SubjectID)";

            var hqs = @"UPDATE ScoreEntries
                        SET SerialNumber = o.SN
                        FROM ScoreEntries e, 
	                         (SELECT StudentID, e.SubjectID, ROW_NUMBER() OVER(PARTITION BY e.SubjectID ORDER BY SerialNumber) AS SN 
	                          FROM ScoreEntries e, Subjects s 
	                          WHERE s.SubjectID = e.SubjectID and s.ClassID = @ClassID) o
                        WHERE e.SubjectID = o.SubjectID and e.StudentID = o.StudentID";

            using (var conn = new SqlConnection(IEContext.ConnString))
            {
                conn.Open();

                var cmd = new SqlCommand(cqs, conn);

                var param = new SqlParameter
                {
                    ParameterName = "@ClassID",
                    Value = ClassID
                };
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();

                var cmd1 = new SqlCommand(hqs, conn);

                var param1 = new SqlParameter
                {
                    ParameterName = "@ClassID",
                    Value = ClassID
                };
                cmd1.Parameters.Add(param1);

                cmd1.ExecuteNonQuery();
            }
        }

        public void AddAllClassSubjects(int SchoolID, int ClassID, byte LevelID, short Year, byte TermNumber)
        {
            var qs = "INSERT INTO Subjects(ClassID, TemplateID, ResultName, SchoolYear, TermNumber, TimeEntered, TimeVerified, PercentCorrected)";

            if (TermNumber == 1)
                qs += @"SELECT @ClassID, TemplateID, ResultName, @Year, 1, '0001-01-01', '0001-01-01', 0
                        FROM SubjectTemplates t 
                        WHERE SchoolID = @SchoolID and ClassLevelID = @LevelID and HasTerm1 = 1";
            else if (TermNumber == 2)
                qs += @"SELECT @ClassID, TemplateID, ResultName, @Year, 2, '0001-01-01', '0001-01-01', 0
                        FROM SubjectTemplates t 
                        WHERE SchoolID = @SchoolID and ClassLevelID = @LevelID and HasTerm2 = 1";
            else if (TermNumber == 3)
                qs += @"SELECT @ClassID, TemplateID, ResultName, @Year, 3, '0001-01-01', '0001-01-01', 0
                        FROM SubjectTemplates t 
                        WHERE SchoolID = @SchoolID and ClassLevelID = @LevelID and HasTerm3 = 1";

            qs += " and not exists (SELECT 1 FROM Subjects s WHERE s.ClassID = @ClassID and s.TemplateID = t.TemplateID)";

            using (SqlConnection conn = new SqlConnection(IEContext.ConnString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(qs, conn);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@ClassID";
                param.Value = ClassID;
                cmd.Parameters.Add(param);

                SqlParameter sparam = new SqlParameter();
                sparam.ParameterName = "@SchoolID";
                sparam.Value = SchoolID;
                cmd.Parameters.Add(sparam);

                SqlParameter lparam = new SqlParameter();
                lparam.ParameterName = "@LevelID";
                lparam.Value = LevelID;
                cmd.Parameters.Add(lparam);

                SqlParameter yparam = new SqlParameter();
                yparam.ParameterName = "@Year";
                yparam.Value = Year;
                cmd.Parameters.Add(yparam);

                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteTermScores(int TermID)
        {
            var cqs = @"UPDATE ScoreEntries 
                        SET CAScore = null, ExamScore = null
                        FROM ScoreEntries e, Subjects s, Classes c
                        WHERE e.SubjectID = s.SubjectID and s.ClassID = c.ClassID and c.TermID = @TermID";

            var hqs = @"UPDATE Subjects
                        SET PercentCorrected = 0, EnteredByID = NULL, VerifiedByID = NULL, TimeEntered = '1/1/0001', TimeVerified = '1/1/0001'
                        FROM Subjects s, Classes c
                        WHERE s.ClassID = c.ClassID and c.TermID = @TermID";

            using (SqlConnection conn = new SqlConnection(IEContext.ConnString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(cqs, conn);
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@TermID",
                    Value = TermID
                });

                cmd.ExecuteNonQuery();

                SqlCommand cmd1 = new SqlCommand(hqs, conn);
                cmd1.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@TermID",
                    Value = TermID
                });

                cmd1.ExecuteNonQuery();
            }
        }

        public void DeleteRearrangeTermScores(int TermID)
        {
            var dqs = @"DELETE ScoreEntries 
                        FROM ScoreEntries e, Students s, Subjects sb, Classes c
                        WHERE e.SubjectID = sb.SubjectID and e.StudentID = s.StudentID and sb.ClassID = c.ClassID 
	                        and (s.ClassID is null or s.ClassID != c.ClassID) and c.TermID = @TermID";

            var cqs = @"UPDATE ScoreEntries
                        SET CAScore = null, ExamScore = null, SerialNumber = g.SN
                        FROM ScoreEntries e, Subjects s, Classes c,
	                         (SELECT s.StudentID, s.ClassID, ROW_NUMBER() OVER(PARTITION BY s.ClassID ORDER BY Surname, FirstName) AS SN 
	                          FROM Students s, Classes c
	                          WHERE s.ClassID = c.ClassID and c.TermID = @TermID) g
                        WHERE e.StudentID = g.StudentID and  e.SubjectID = s.SubjectID and s.ClassID = c.ClassID and c.TermID = @TermID";

            var hqs = @"UPDATE Subjects
                        SET PercentCorrected = 0, EnteredByID = NULL, VerifiedByID = NULL, TimeEntered = '1/1/0001', TimeVerified = '1/1/0001'
                        FROM Subjects s, Classes c
                        WHERE s.ClassID = c.ClassID and c.TermID = @TermID";

            using (SqlConnection conn = new SqlConnection(IEContext.ConnString))
            {
                conn.Open();

                SqlCommand dcmd = new SqlCommand(dqs, conn);
                dcmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@TermID",
                    Value = TermID
                });

                dcmd.ExecuteNonQuery();

                SqlCommand cmd = new SqlCommand(cqs, conn);
                cmd.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@TermID",
                    Value = TermID
                });

                cmd.ExecuteNonQuery();

                SqlCommand cmd1 = new SqlCommand(hqs, conn);
                cmd1.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@TermID",
                    Value = TermID
                });

                cmd1.ExecuteNonQuery();
            }
        }

        #endregion

        #region Deprecated SQL Methods

        public void PrepareNewClassComments(int ClassID)
        {
            var cqs = @"INSERT INTO FacultyComments (ClassID, StudentID)
                        SELECT ClassID, StudentID FROM Students S
                        WHERE ClassID = @ClassID
	                        AND NOT EXISTS(SELECT 1 FROM FacultyComments C WHERE C.ClassID = S.ClassID AND C.StudentID = S.StudentID)";

            using (SqlConnection conn = new SqlConnection(IEContext.ConnString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(cqs, conn);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@ClassID";
                param.Value = ClassID;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();
            }
        }

        public void PrepareNewTermComments(int TermID)
        {
            var cqs = @"INSERT INTO FacultyComments (ClassID, StudentID)
                        SELECT S.ClassID, S.StudentID FROM Students S, Classes C
                        WHERE S.ClassID = C.ClassID AND C.TermID = @TermID
	                        AND NOT EXISTS(SELECT 1 FROM FacultyComments F WHERE F.ClassID = S.ClassID AND F.StudentID = S.StudentID)";

            using (SqlConnection conn = new SqlConnection(IEContext.ConnString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(cqs, conn);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@TermID";
                param.Value = TermID;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();
            }
        }

        public void LoadLastTermComments(int TermID, int SchoolID)
        {
            var cqs = @"UPDATE FacultyComments
                        SET CTComment = g.CTComment
                        FROM FacultyComments f, Classes c,
	                        (SELECT StudentID, CTComment FROM FacultyComments fp, Classes cp 
	                         WHERE fp.ClassID = cp.ClassID and cp.TermID = (SELECT MAX(TermID) FROM Terms WHERE SchoolID = @SchoolID and TermID < @TermID)
	                        ) g
                        WHERE f.ClassID = c.ClassID and f.StudentID = g.StudentID and c.TermID = @TermID";

            using (SqlConnection conn = new SqlConnection(IEContext.ConnString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(cqs, conn);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@TermID";
                param.Value = TermID;
                cmd.Parameters.Add(param);

                SqlParameter sparam = new SqlParameter();
                sparam.ParameterName = "@SchoolID";
                sparam.Value = SchoolID;
                cmd.Parameters.Add(sparam);

                cmd.ExecuteNonQuery();
            }
        }

        public void LoadPerformanceComments(int TermID)
        {
            var cqs = @"UPDATE FacultyComments
                        SET PComment = REPLACE(
					                        REPLACE(REPLACE(p.Comment, '{:NAME}', s.FirstName), 
							                        '{:SEX1}', CASE WHEN s.IsMale = 1 THEN 'he' ELSE 'she' END),
					                        '{:SEX2}', CASE WHEN s.IsMale = 1 THEN 'him' ELSE 'her' END)
                        FROM FacultyComments f, Classes c, Students s, StudentResults r, 
	                         PerformanceCommentGroups g, PerformanceComments p
                        WHERE f.ClassID = c.ClassID and c.ClassID = s.ClassID and f.StudentID = s.StudentID
	                        and s.ClassID = r.ClassID and s.StudentID = r.StudentID and c.TermID = @TermID 
	                        and c.PerformanceCommentGroupID = g.GroupID and g.GroupID = p.GroupID
	                        and r.AverageScore >= p.LowerBound and r.AverageScore < p.UpperBound + 1";

            using (SqlConnection conn = new SqlConnection(IEContext.ConnString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(cqs, conn);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@TermID";
                param.Value = TermID;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();
            }
        }

        public void HarmonizeScoreEntrySNs(int SubjectID)
        {
            var cqs = @"UPDATE ScoreEntries
                        SET SerialNumber = o.SN
                        FROM ScoreEntries e, (SELECT StudentID, ROW_NUMBER() OVER(ORDER BY SerialNumber) AS SN FROM ScoreEntries WHERE SubjectID = @SubjectID) o
                        WHERE e.SubjectID = @SubjectID and e.StudentID = o.StudentID";

            using (SqlConnection conn = new SqlConnection(IEContext.ConnString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(cqs, conn);

                SqlParameter param = new SqlParameter();
                param.ParameterName = "@SubjectID";
                param.Value = SubjectID;
                cmd.Parameters.Add(param);

                cmd.ExecuteNonQuery();
            }
        }

        #endregion
    }
}