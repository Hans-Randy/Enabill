using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class UserPreferenceRepo : BaseRepo
	{
		#region USER PREFERENCE SPECIFIC

		internal static void Save(UserPreference userPreference)
		{
			if (userPreference.UserPreferenceID == 0)
				DB.UserPreferences.Add(userPreference);

			DB.SaveChanges();
		}

		internal static UserPreference GetByUserID(int userID) => DB.UserPreferences
					.SingleOrDefault(up => up.UserID == userID);

		#endregion USER PREFERENCE SPECIFIC
	}
}