using System;
using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web
{
	public interface ISettings
	{
		int ContextUserID { get; }
		int CurrentUserID { get; }
		string Passphrase { get; set; }
		bool PassphraseIsValid { get; }

		User CurrentUser { get; set; }
		User ContextUser { get; set; }
		User SystemUser { get; }
		UserPreference CurrentUserPreferences { get; }
		UserPreference ContextUserPreferences { get; }

		List<Menu> VisibleMenus { get; }

		DateTime GetContextUserDayEndTime(DateTime date);

		DateTime GetContextUserDayStartTime(DateTime date);

		void LoginUser(string userName, bool createPersistentCookie);

		void LoginUser(User user, bool createPersistentCookie);

		void LogUserOff();

		void ResetCache();
	}
}