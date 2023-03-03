using System;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;
using Enabill.Emailing;
using Enabill.Models;
using Enabill.Repos;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	// Don't inherit from BaseController because our site will get stuck in an infinite
	// loop if the user logging in has to reset his password.
	// Check the OnAuthorization action in BaseController, that is where the error occurs.

	public class AccountController : Controller
	{
		public IFormsAuthenticationService FormsService { get; set; }
		public IMembershipService MembershipService { get; set; }
		public LogOnModel LogOnModel { get; set; }
		private static OpenIdRelyingParty openid = new OpenIdRelyingParty();
		private readonly string emailDomain = Enabill.Code.Constants.EMAILDOMAIN;

		protected override void Initialize(RequestContext requestContext)
		{
			if (this.FormsService == null)
				this.FormsService = new FormsAuthenticationService();
			if (this.MembershipService == null)
				this.MembershipService = new EnabillMembershipService();
			if (this.LogOnModel == null)
				this.LogOnModel = new LogOnModel() { IsLoginUnsuccessful = false, IsAccesDenied = false };

			base.Initialize(requestContext);
		}

		[HttpGet]
		public ActionResult LogOn()
		{
			if (Settings.Current.CurrentUser != null)
				return this.RedirectToAction("Index", "Home");

			this.ViewBag.ShowForgottenPasswordLink = true;

			return this.View(this.LogOnModel);
		}

		// TODO: remove reference to LoginModel - not in this form..
		[ValidateInput(false)]
		public ActionResult SocialAuthLogin(string returnUrl)
		{
			var response = openid.GetResponse();

			if (response == null)
			{
				// Stage 2: user submitting Identifier
				try
				{
					var request = openid.CreateRequest("https://www.google.com/accounts/o8/id");
					// get extra information
					request.AddExtension(new ClaimsRequest
					{
						Email = DemandLevel.Require,
					});

					return request.RedirectingResponse.AsActionResult();
				}
				catch (ProtocolException ex)
				{
					return this.View("CustomError", new ErrorModel("Social Logon Error", ex.Message, ex));
				}
			}
			else
			{
				// Stage 3: OpenID Provider sending assertion response
				switch (response.Status)
				{
					case AuthenticationStatus.Authenticated:
						this.Session["FriendlyIdentifier"] = response.ClaimedIdentifier;
						var claimsResponse = response.GetExtension<ClaimsResponse>();
						string email = null;

						if (claimsResponse != null)
						{
							email = claimsResponse.Email;
						}

						if (claimsResponse == null || email == null)
						{
							// TODO: log issue
							break; //fall through to error
						}

						FormsAuthentication.SetAuthCookie(response.ClaimedIdentifier, false);
						var user = UserRepo.GetByEmail(email);

						if (user != null)
						{
							LoginLog.CaptureNewLogin(user.UserName, user.FullName);

							if (!user.CanLogin)
							{
								this.LogOnModel.IsAccesDenied = true;
								return this.View(this.LogOnModel);
							}

							Settings.Current.LoginUser(user, true);

							if (this.Url.IsLocalUrl(returnUrl))
								return this.Redirect(returnUrl);

							return this.RedirectToAction("Index", "Home");
						}
						break;

					case AuthenticationStatus.Canceled:
						// fall through to error
						break;

					case AuthenticationStatus.Failed:
						// fall through to error
						break;
				}

				LoginLog.CaptureNewLogin("google login", null);
				this.LogOnModel.IsLoginUnsuccessful = true;
				this.ViewBag.ShowForgottenPasswordLink = true;

				return this.View("LogOn", this.LogOnModel);
			}
		}

		[HttpPost]
		public ActionResult LogOn(string returnUrl, LogOnModel model)
		{
			this.LogOnModel = model;

			if (this.ModelState.IsValid)
			{
				this.LogOnModel.IsLoginUnsuccessful = false;
				this.LogOnModel.IsAccesDenied = false;

				//Users tend to enter their full email address as username which returns an invalid login error
				//leading to them resetting their password each time they logon as they think the password is incorrect
				//remove the email address portion if entered to eliminate this issue

				if (this.MembershipService.ValidateUser(this.LogOnModel.UserName.Replace(this.emailDomain, ""), this.LogOnModel.Password))
				{
					var user = UserRepo.GetForLogin(this.LogOnModel.UserName);
					LoginLog.CaptureNewLogin(this.LogOnModel.UserName, user.FullName);

					if (!user.CanLogin)
					{
						this.LogOnModel.IsAccesDenied = true;
						return this.View(this.LogOnModel);
					}

					Settings.Current.LoginUser(user, true);

					if (this.Url.IsLocalUrl(returnUrl))
						return this.Redirect(returnUrl);

					return this.RedirectToAction("Index", "Home");
				}

				LoginLog.CaptureNewLogin(this.LogOnModel.UserName, null);
				this.LogOnModel.IsLoginUnsuccessful = true;
			}

			// If we got this far, something failed, redisplay form
			this.ViewBag.ShowForgottenPasswordLink = true;

			return this.View(this.LogOnModel);
		}

		public ActionResult LogOff()
		{
			Settings.Current.LogUserOff();

			return this.RedirectToAction("Index", "Home");
		}

		[HttpGet]
		public ActionResult ForgotPassword()
		{
			if (Settings.Current.CurrentUserID > 0)
				return this.RedirectToAction("Index", "Home");

			return this.View();
		}

		[HttpPost]
		public ActionResult ForgotPassword(FormCollection form)
		{
			string host = this.Request.Url.Authority;
			if (!host.Contains(Settings.SiteAddress))
				return this.View("CustomError", new ErrorModel("Development Blocker", "This action cannot be used from the dev site"));

			if (Settings.Current.CurrentUser != null)
				return this.View("CustomError", new ErrorModel("You are already logged in. Please use the 'Change Password' functionality to update your password."));

			var user = UserRepo.GetForForgottenPassword(form["Name"]);

			if (user == null)
				return this.View("CustomError", new ErrorModel("A user with these details could not be found. Please use your email address to retrieve your password."));

			if (user.Password == null)
				return this.View("CustomError", new ErrorModel("If you are logging in for the first time, please use the link 'Please click to change password' in your introductory email."));

			user.SetForgottenPasswordToken();
			EnabillEmails.SendRecoverPasswordEmail(user);

			return this.RedirectToAction("LogOn");
		}

		[HttpGet]
		public ActionResult RecoverPassword(Guid? id) // id = validation token
		{
			if (!id.HasValue || id == new Guid())
				return this.View("CustomError", new ErrorModel("Validation Error.", "The validation code received is required to validate your attempt to recover your password."));

			var user = Enabill.Models.User.GetByForgottenPasswordToken(id.Value);

			if (user == null)
				return this.View("CustomError", new ErrorModel("Validation Error", "The validation code could not be linked to a user in our records. Please try again. We apologize for any inconvieniences caused."));

			if (!user.IsActive)
				return this.View("CustomError", new ErrorModel("Deactivated Account", "This account has been deactivated. Please speak to a system administrator if this is an unexpected problem. We apologize for any inconvieniences caused."));

			user.Password = Helpers.HashedString(Settings.DefaultUserPassword);
			user.MustResetPwd = true;
			user.Save(user);

			this.FormsService.SignIn(user.UserName, false);
			Settings.Current.CurrentUser = user;

			return this.RedirectToAction("ChangePassword");
		}

		[Authorize]
		public ActionResult ChangePassword()
		{
			this.ViewBag.PasswordLength = this.MembershipService.MinPasswordLength;

			return this.View();
		}

		[Authorize, HttpPost]
		public ActionResult ChangePassword(ChangePasswordModel model)
		{
			if (this.ModelState.IsValid)
			{
				if (this.MembershipService.ChangePassword(this.User.Identity.Name, model.OldPassword, model.NewPassword))
				{
					Settings.Current.CurrentUser = UserRepo.GetByID(Settings.Current.CurrentUserID);
					return this.View("ChangePasswordSuccess");
				}
				else
				{
					this.ModelState.AddModelError("", "The current password is incorrect or the new password is invalid, or the new password does not meet the minimum length requirement.");
				}
			}

			// If we got this far, something failed, redisplay form
			this.ViewBag.PasswordLength = this.MembershipService.MinPasswordLength;

			return this.View(model);
		}
	}
}