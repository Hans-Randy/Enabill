using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace Enabill.Web.Models
{
	#region Models

	public class ChangePasswordModel
	{
		#region PROPERTIES

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Current password")]
		public string OldPassword { get; set; }

		[Required]
		//[ValidatePasswordLength]
		[DataType(DataType.Password)]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
		[Display(Name = "New password")]
		[RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Please see password requirements above.")]
		public string NewPassword { get; set; }

		#endregion PROPERTIES
	}

	public class LogOnModel
	{
		#region PROPERTIES

		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }

		public bool IsAccesDenied { get; set; }
		public bool IsLoginUnsuccessful { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[Required]
		[Display(Name = "User name")]
		public string UserName { get; set; }

		#endregion PROPERTIES
	}

	#endregion Models

	#region Services

	// The FormsAuthentication type is sealed and contains static members, so it is difficult to
	// unit test code that calls its members. The interface and helper class below demonstrate
	// how to create an abstract wrapper around such a type in order to make the AccountController
	// code unit testable.

	public interface IMembershipService
	{
		int MinPasswordLength { get; }

		bool ValidateUser(string userName, string password);

		MembershipCreateStatus CreateUser(string userName, string password, string email);

		bool ChangePassword(string userName, string oldPassword, string newPassword);
	}

	public class AccountMembershipService : IMembershipService
	{
		private readonly MembershipProvider provider;

		public AccountMembershipService()
			: this(null)
		{
		}

		public AccountMembershipService(MembershipProvider provider)
		{
			this.provider = provider ?? Membership.Provider;
		}

		public int MinPasswordLength => this.provider.MinRequiredPasswordLength;

		public bool ValidateUser(string userName, string password)
		{
			if (string.IsNullOrEmpty(userName))
				throw new ArgumentException("Value cannot be null or empty.", nameof(userName));
			if (string.IsNullOrEmpty(password))
				throw new ArgumentException("Value cannot be null or empty.", nameof(password));

			return this.provider.ValidateUser(userName, password);
		}

		public MembershipCreateStatus CreateUser(string userName, string password, string email)
		{
			if (string.IsNullOrEmpty(userName))
				throw new ArgumentException("Value cannot be null or empty.", nameof(userName));
			if (string.IsNullOrEmpty(password))
				throw new ArgumentException("Value cannot be null or empty.", nameof(password));
			if (string.IsNullOrEmpty(email))
				throw new ArgumentException("Value cannot be null or empty.", nameof(email));

			this.provider.CreateUser(userName.Trim(), password, email.Trim(), null, null, true, null, out var status);

			return status;
		}

		public bool ChangePassword(string userName, string oldPassword, string newPassword)
		{
			if (string.IsNullOrEmpty(userName))
				throw new ArgumentException("Value cannot be null or empty.", nameof(userName));
			if (string.IsNullOrEmpty(oldPassword))
				throw new ArgumentException("Value cannot be null or empty.", nameof(oldPassword));
			if (string.IsNullOrEmpty(newPassword))
				throw new ArgumentException("Value cannot be null or empty.", nameof(newPassword));
			//if (newPassword.Length < MinPasswordLength) throw new ArgumentException(string.Format($"Password does not meet the minimum required length of {MinPasswordLength} characters."));

			// The underlying ChangePassword() will throw an exception rather
			// than return false in certain failure scenarios.
			try
			{
				var currentUser = this.provider.GetUser(userName, true /* userIsOnline */);
				return currentUser.ChangePassword(oldPassword, newPassword);
			}
			catch (ArgumentException)
			{
				return false;
			}
			catch (MembershipPasswordException)
			{
				return false;
			}
		}
	}

	public interface IFormsAuthenticationService
	{
		void SignIn(string userName, bool createPersistentCookie);

		void SignOut();
	}

	public class FormsAuthenticationService : IFormsAuthenticationService
	{
		public void SignIn(string userName, bool createPersistentCookie)
		{
			if (string.IsNullOrEmpty(userName))
				throw new ArgumentException("Value cannot be null or empty.", nameof(userName));

			FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
		}

		public void SignOut() => FormsAuthentication.SignOut();
	}

	#endregion Services

	#region Validation

	public static class AccountValidation
	{
		public static string ErrorCodeToString(MembershipCreateStatus createStatus)
		{
			// See http://go.microsoft.com/fwlink/?LinkID=177550 for
			// a full list of status codes.
			switch (createStatus)
			{
				case MembershipCreateStatus.DuplicateUserName:
					return "Username already exists. Please enter a different user name.";

				case MembershipCreateStatus.DuplicateEmail:
					return "A username for that e-mail address already exists. Please enter a different e-mail address.";

				case MembershipCreateStatus.InvalidPassword:
					return "The password provided is invalid. Please enter a valid password value.";

				case MembershipCreateStatus.InvalidEmail:
					return "The e-mail address provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidAnswer:
					return "The password retrieval answer provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidQuestion:
					return "The password retrieval question provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidUserName:
					return "The user name provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.ProviderError:
					return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				case MembershipCreateStatus.UserRejected:
					return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				default:
					return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
			}
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class ValidatePasswordLengthAttribute : ValidationAttribute, IClientValidatable
	{
		private const string defaultErrorMessage = "'{0}' must be at least {1} characters long.";
		private readonly int minCharacters = Membership.Provider.MinRequiredPasswordLength;

		public ValidatePasswordLengthAttribute()
			: base(defaultErrorMessage)
		{
		}

		public override string FormatErrorMessage(string name) => string.Format(CultureInfo.CurrentCulture, this.ErrorMessageString, name, this.minCharacters);

		public override bool IsValid(object value) => value is string valueAsString && valueAsString.Length >= this.minCharacters;

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context) => new[]{
				new ModelClientValidationStringLengthRule(this.FormatErrorMessage(metadata.GetDisplayName()), this.minCharacters, int.MaxValue)
			};
	}

	#endregion Validation
}