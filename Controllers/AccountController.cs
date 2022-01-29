using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using IEGen.Models;
using Z.EntityFramework.Plus;

namespace IEGen.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpGet]
        public ActionResult SignOut()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.ExternalBearer);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        [Route("Account/Delete/{UserId:int}")]
        public ActionResult Delete(int UserId, string email)
        {
            if (!new IEContext().IEUserList.Any(l => l.UserID == UserId && l.Email == email))
                return RedirectToAction("Index", "Home");

            var model = new DeleteAccountPageViewModel();
            model.Delete = new DeleteAccountViewModel { UserID = UserId, Email = email };
            model.Update = new UpdateEmailViewModel { PreviousEmail = email, Email = "" };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAccount(string delete, DeleteAccountViewModel model)
        {
            var context = new IEContext();
            context.UserAccountComplaintList.Add(new UserAccountComplaint()
            {
                Email = model.Email,
                UserID = model.UserID,
                Complaint = model.Complaint
            });
            string successMsg = "Thank you very much. Your Complaint was successfully logged. We will work on it.";

            try
            {
                if (!string.IsNullOrEmpty(delete))
                {
                    var user = await UserManager.FindByNameAsync(model.Email);
                    if (user != null)
                    {
                        var result = await UserManager.DeleteAsync(user);
                    }
                    
                    var usr = new IEUser { UserID = model.UserID };
                    context.IEUserList.Attach(usr);
                    context.Entry(usr).State = System.Data.Entity.EntityState.Deleted;

                    successMsg = "Your Account was successfully deleted!";
                }

                context.Configuration.ValidateOnSaveEnabled = false;
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                General.AlertElmah(ex);
                return DefaultErrorAlert();
            }
            var alertVM = AlertViewModel.Create(successMsg, AlertTypes.Success);
            return PartialView("_Alert", alertVM);
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        //[AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        private async Task SendResetPasswordMail(ResetPasswordMailModel model)
        {
            using (IEContext context = new IEContext())
            {
                DateTime now = DateTime.Now;
                string expTime = General.FullTimeString(now.AddHours(2));
                PasswordReset pr = new PasswordReset() { UserID = model.UserID, RequestTime = now };
                context.PasswordResetList.Add(pr);
                context.SaveChanges();
                var pwdResetToken = (await UserManager.GeneratePasswordResetTokenAsync(model.AspUserId)).Substring(0,10);

                EmailObject obj = EmailSettings.BlankMail(context);
                obj.MailMessage.ButtonUrl = Url.Action("ResetPassword", "Account", new { resetId = pr.ResetID, code = pwdResetToken }, Request.Url.Scheme);
                obj.MailMessage.ToEmail = model.Email;
                obj.MailMessage.Subject = "Reset Password";
                obj.MailMessage.Name = model.Name;
                obj.MailMessage.Body =
                    "<p>You recently requested to reset your password on the Imavate Education Platform. Use the button below to reset it. " +
                    "<strong>This password reset is only valid for 2 hours till " + expTime + " (UTC).</strong></p>" +
                    "<mb>Reset Password</mb>" +
                    "<p>If you did not request a password reset, you can safely ignore this email. if you have questions, please ask by replying this email.</p>";

                var msg = obj.MailMessage;
                context.EmailMessageList.Add(msg);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    General.AlertElmah(ex);
                }
                //foreach (var err in await SendEmailAsync(obj))
                //    General.AlertElmah(new Exception(err));
            }
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);

                if (user == null)
                    return JSRedirect(Url.Action("ForgotPasswordConfirmation", "Account"));

                var context = new IEContext();

                var u = context.IEUserList.Where(l => l.Email == model.Email).Select(l => new { l.UserID, l.Name }).FirstOrDefault();

                if (u != null)
                {
                    var m = new ResetPasswordMailModel { UserID = u.UserID, Email = model.Email, Name = u.Name, AspUserId = user.Id };
                    m.MailHeaderUrl = AppSecrets.Secrets.MailHeaderUrl;

                    await SendResetPasswordMail(m);
                }

                return JSRedirect(Url.Action("ForgotPasswordConfirmation", "Account"));
            }

            // If we got this far, something failed, redisplay form
            General.AlertElmah(new Exception("Forgot Password Model State is Invalid"));
            return DefaultErrorAlert();
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Account/ResetPassword/{resetId:int}")]
        public ActionResult ResetPassword(int resetId, string code)
        {
            using (IEContext context = new IEContext())
            {
                DateTime expTime = DateTime.Now.AddHours(-2.0);
                if (!context.PasswordResetList.Any(l => l.ResetID == resetId && l.RequestTime > expTime))
                    return View("ResetPasswordExpired");
            }
            if (code != null) return View();

            return View("404");
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPasswordAction(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null)
                    return JSRedirect(Url.Action("ResetPasswordConfirmation", "Account"));

                using (IEContext context = new IEContext())
                {
                    var pr = context.PasswordResetList.Where(l => l.ResetID == model.ResetID).FirstOrDefault();

                    if (pr != null)
                    {
                        if (!context.IEUserList.Any(l => l.UserID == pr.UserID && l.Email == model.Email))
                        {
                            General.AlertElmah(new Exception("Reset Password Attempt for mismatched User ID and email"));
                            return JSRedirect(Url.Action("ResetPasswordConfirmation", "Account"));
                        }

                        var prc = new PasswordResetComplete { ActionTime = DateTime.Now, UserID = pr.UserID, RequestTime = pr.RequestTime };
                        context.PasswordResetCompleteList.Add(prc);
                        context.Entry(pr).State = System.Data.Entity.EntityState.Deleted;
                        context.SaveChanges();

                        DateTime expTime = DateTime.Now.AddHours(-2);
                        context.PasswordResetList.Where(l => l.RequestTime < expTime).Delete();
                    }

                }

                var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return JSRedirect(Url.Action("ResetPasswordConfirmation", "Account"));
                }

                AlertViewModel alertViewModel1 = AlertViewModel.Create("Reset Password was not successful. Please see errors below.", AlertTypes.Error);
                foreach (string error in result.Errors)
                {
                    alertViewModel1.AddAlert(error, AlertTypes.Error);
                    General.AlertElmah(new Exception(error));
                }
                Response.StatusCode = 500;
                return PartialView("_Alert", alertViewModel1);
            }
            else
                General.AlertElmah(new Exception("Reset Password Model State is Invalid"));

            return DefaultErrorAlert();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}