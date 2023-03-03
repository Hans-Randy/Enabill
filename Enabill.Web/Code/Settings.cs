using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web
{
	public class Settings
	{
		#region SITE SETTINGS

		public static string SiteAddress
		{
			get
			{
				string siteAddress = Enabill.Code.Constants.SITEADDRESS;

				if (string.IsNullOrEmpty(siteAddress))
					return "";

				return siteAddress;
			}
		}

		public static string DefaultUserPassword => EnabillSettings.DefaultUserPassword;

		public static DateTime SiteStartDate => EnabillSettings.SiteStartDate;

		public static string DefaultFromEmailAddress => Enabill.Code.Constants.EMAILADDRESSDEFAULTFROM;

		#endregion SITE SETTINGS

		#region CONFIGURATION

		public static Settings Current
		{
			get
			{
				Settings s = null;
				try
				{ s = HttpContext.Current.Session["Settings"] as Settings; }
				catch { }

				if (s == null)
				{
					s = new Settings();
					HttpContext.Current.Session["Settings"] = s;
				}

				return s;
			}
		}

		public void ResetCache() => this.menus = null;

		/// <summary>
		/// Return the list of menu items a user can see
		/// </summary>
		private List<Menu> menus;

		public List<Menu> VisibleMenus
		{
			get
			{
				if (this.CurrentUser == null)
					return new List<Menu>();

				if (this.menus?.Count > 0)
					return this.menus;

				this.menus = MenuRepo.GetByRoleBW(this.CurrentUser.GetRoleBW());

				if (!this.CurrentUser.IsFlexiTimeUser)
				{
					var m = this.menus.SingleOrDefault(x => string.Equals(x.Controller, "flexitime", StringComparison.OrdinalIgnoreCase));

					if (m != null)
						this.menus.Remove(m);
				}

				return this.menus;
			}
		}

		#endregion CONFIGURATION

		#region CONSUMER SETTINGS AND FUNCTIONS

		public const string ShortDateDisplayFormat = "MMM, d";
		public const string DateDisplayFormat = "yyyy-MM-dd";
		public const string LongDateDisplayFormat = "yyyy-MM-dd HH:mm:ss";
		public const string FullDateDisplayFormat = "dd MMMM yyyy";

		public static string PageUrl
		{
			get
			{
				string[] parts = HttpContext.Current.Request.Url.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

				if (parts == null)
					return string.Empty;
				string url = string.Empty;

				for (int i = 0; i < 2 && i < parts.Length; i++)
				{
					url += parts[i] + (i == 0 ? "-" : "");
				}

				return url;
			}
		}

		public static bool ShowStory
		{
			get
			{
				if (Current.CurrentUser == null)
					return false;

				if (!Current.CurrentUser.HasRole(UserRoleType.StoryWriter))
					return false;

				string url = PageUrl;

				if (string.IsNullOrEmpty(url))
					return false;

				if (url.StartsWith("Story"))
					return false;

				return true;
			}
		}

		#endregion CONSUMER SETTINGS AND FUNCTIONS

		#region CURRENT SESSION PROPERTIES

		//public int CurrentProjectID { get; set; }
		public static Exception AppError { get; set; }

		/// <summary>
		/// The current user logged in
		/// </summary>
		private User currentUser;

		public User CurrentUser
		{
			get => this.currentUser;
			set
			{
				this.currentUser = value;
				this.currentUserPreferences = null;

				if (this.currentUser != null)
					this.currentUserPreferences = this.currentUser.GetUserPreference();
			}
		}

		public int CurrentUserID
		{
			get
			{
				if (this.currentUser != null)
					return this.currentUser.UserID;

				return 0;
			}
		}

		/// <summary>
		/// The Global System User
		/// </summary>
		private User systemUser;

		public User SystemUser => this.systemUser ?? (this.systemUser = EnabillSettings.GetSystemUser());

		/// <summary>
		/// The preferences of the user currently logged in
		/// </summary>
		private UserPreference currentUserPreferences;

		public UserPreference CurrentUserPreferences => this.currentUserPreferences;

		/// <summary>
		/// The user that we operate as. i.e.: Impersonation
		/// </summary>
		private User contextUser;

		public User ContextUser
		{
			get
			{
				if (this.contextUser == null)
				{
					this.contextUser = this.currentUser;

					if (this.contextUserPreferences == null)
						this.SetUpCopntextUserPreferences();
				}

				return this.contextUser;
			}

			set
			{
				this.contextUser = value;
				this.SetUpCopntextUserPreferences();
			}
		}

		private void SetUpCopntextUserPreferences()
		{
			this.contextUserPreferences = UserPreference.GetDefaultPreference();

			if (this.contextUser?.UserID > 0)
				this.contextUserPreferences = this.contextUser.GetUserPreference();
		}

		public int ContextUserID
		{
			get
			{
				if (this.contextUser != null)
					return this.contextUser.UserID;

				return 0;
			}
		}

		/// <summary>
		/// The user that is currently logged in's preferences
		/// </summary>
		private UserPreference contextUserPreferences;

		public UserPreference ContextUserPreferences => this.contextUserPreferences;

		#endregion CURRENT SESSION PROPERTIES

		#region CURRENT SESSION FUNCTIONS

		internal void LoginUser(string userName, bool createPersistentCookie)
		{
			var user = UserRepo.GetByUserName(userName);
			this.LoginUser(user, createPersistentCookie);
		}

		internal void LoginUser(User user, bool createPersistentCookie)
		{
			if (user == null)
				return;

			FormsAuthentication.SetAuthCookie(user.UserName, createPersistentCookie);

			Current.CurrentUser = user;
			Current.ResetCache();
		}

		internal void LogUserOff()
		{
			Current.CurrentUser = null;
			FormsAuthentication.SignOut();
		}

		public DateTime GetContextUserDayStartTime(DateTime date)
		{
			var userPreference = this.ContextUserPreferences;

			if (userPreference?.DefaultWorkSessionStartTime.HasValue != true)
				return date.Date.AddHours(9);

			date = date.AddHours(userPreference.DefaultWorkSessionStartTime.Value.Hour);
			return date.AddMinutes(userPreference.DefaultWorkSessionStartTime.Value.Minute);
		}

		public DateTime GetContextUserDayEndTime(DateTime date)
		{
			var userPreference = this.contextUserPreferences;

			if (userPreference?.DefaultWorkSessionEndTime.HasValue != true)
				return this.GetContextUserDayStartTime(date).AddHours(this.ContextUser.WorkHours);

			date = date.AddHours(userPreference.DefaultWorkSessionEndTime.Value.Hour);
			return date.AddMinutes(userPreference.DefaultWorkSessionEndTime.Value.Minute);
		}

		#endregion CURRENT SESSION FUNCTIONS

		#region PASSPHRASE SETTINGS

		private bool passphraseIsValid { get; set; }

		public bool PassphraseIsValid => this.passphraseIsValid;

		private string passphrase { get; set; }

		public string Passphrase
		{
			get => this.passphrase;

			set
			{
				this.passphrase = value;
				this.passphraseIsValid = Helpers.ConfirmPassphraseIsValid(this.passphrase);
			}
		}

		#endregion PASSPHRASE SETTINGS
	}
}