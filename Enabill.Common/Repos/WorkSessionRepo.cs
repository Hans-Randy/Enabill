using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class WorkSessionRepo : BaseRepo
	{
		#region WORK SESSION SPECIFIC

		internal static WorkSession GetByID(int workSessionID) => DB.WorkSessions
					.SingleOrDefault(ws => ws.WorkSessionID == workSessionID);

		public static string GetLastWorkSessionForUser(int userID)
		{
			var result = (from w in DB.UserLastWorkSessions
						  where w.UserID == userID
						  select w)
						 .SingleOrDefault();

			if (result != null)
				return result.LastWorkSessionDate.ToShortDateString();
			else
				return "No worksessions found for user.";
		}

		#endregion WORK SESSION SPECIFIC

		#region TIME APPROVAL

		public static List<User> GetUsersWithWorkSessionStatus(DateTime startDate, DateTime endDate, int workSessionStatus) => (from u in DB.Users
																																join ws in DB.WorkSessions on u.UserID equals ws.UserID
																																where ws.StartTime >= startDate && ws.EndTime < endDate
																																&& ws.WorkSessionStatusID == workSessionStatus
																																select u).Distinct().OrderBy(u => u.UserName).ToList();

		public static List<User> GetUsersForManagerWithWorkSessionStatus(DateTime startDate, DateTime endDate, int workSessionStatus, int managerID) => (from u in DB.Users
																																						 join ws in DB.WorkSessions on u.UserID equals ws.UserID
																																						 where ws.StartTime >= startDate && ws.EndTime < endDate
																																						 && ws.WorkSessionStatusID == workSessionStatus
																																						 && u.ManagerID == managerID
																																						 select u).Distinct().OrderBy(u => u.UserName).ToList();

		#endregion TIME APPROVAL
	}
}