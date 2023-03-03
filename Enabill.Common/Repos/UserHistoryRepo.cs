using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class UserHistoryRepo : BaseRepo
	{
		#region USER HISTORY SPECIFIC

		internal static IEnumerable<UserHistory> GetAllForPeriod(int period) => DB.UserHistories
					.Where(uh => uh.Period == period);

		#endregion USER HISTORY SPECIFIC

		#region USERS IN MONTH

		internal static IEnumerable<User> GetUsersInPeriod(int period) => from u in DB.Users
																		  join uh in DB.UserHistories on u.UserID equals uh.UserID
																		  where uh.Period == period
																		  select u;

		#endregion USERS IN MONTH
	}
}