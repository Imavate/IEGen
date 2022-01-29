using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace IEGen.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }

    public class ResetPasswordMailModel : BaseMail
    {
        public string AspUserId { get; set; }

        public string Password { get; set; }
    }

    public class UpdateEmailMailModel
    {
        public string OldEmail { get; set; }

        public string NewEmail { get; set; }

        public string ButtonUrl { get; set; }

        public int UserID { get; set; }

        public string Name { get; set; }
    }

    public class UpdateEmailViewModel
    {
        public string PreviousEmail { get; set; }

        [Display(Name = "New Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [Required(ErrorMessage = "New Email is required")]
        public string Email { get; set; }

        [Display(Name = "Confirm Email")]
        [EmailAddress]
        [Compare("Email", ErrorMessage = "Email and Confirmation Email do not match.")]
        public string ConfirmEmail { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Password and Confirmation Password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class DeleteAccountPageViewModel
    {
        public UpdateEmailViewModel Update { get; set; }

        public DeleteAccountViewModel Delete { get; set; }
    }

    public class DeleteAccountViewModel
    {
        public int UserID { get; set; }

        public string Email { get; set; }

        [Required(ErrorMessage = "Please let us know why you thought of leaving...")]
        [Display(Name = "If we have upset you in anyway please let us know")]
        [StringLength(512)]
        public string Complaint { get; set; }
    }
}