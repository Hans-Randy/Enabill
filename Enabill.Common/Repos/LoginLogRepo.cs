using Enabill.Models;

namespace Enabill.Repos
{
	public class LoginLogRepo : BaseRepo
	{
		#region LOGIN LOG SPECIFIC

		internal static void Save(LoginLog loginLog)
		{
			if (loginLog.LoginLogID <= 0)
				DB.LoginLogs.Add(loginLog);

			DB.SaveChanges();
		}

		#endregion LOGIN LOG SPECIFIC
	}
}