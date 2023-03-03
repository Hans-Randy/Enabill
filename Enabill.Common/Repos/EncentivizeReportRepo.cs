using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class EncentivizeReportRepo : BaseRepo
	{
		public static List<WorkDay> GetWorkDays(DateTime? startDate, DateTime? endDate)
		{
			if (!startDate.HasValue)
				startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.ToFirstDayOfMonth().Day);

			if (!endDate.HasValue)
				endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.ToLastDayOfMonth().Day);

			var qWd = (from r in DB.WorkDays
					   where r.IsWorkable && r.WorkDate >= startDate && r.WorkDate < endDate
					   select r).ToList();

			return qWd.ToList();
		}

		public static List<User> GetReportRepoUsers()
		{
			var qU = (from r in DB.Users
					  join rr in DB.UserRoles on r.UserID equals rr.UserID
					  where r.IsActive && rr.RoleID == 4096
					  select r).Distinct();

			return qU.ToList();
		}

		public static List<User> GetNonReportRepoUsers()
		{
			var qU = (from r in DB.Users
					  join rr in DB.UserRoles on r.UserID equals rr.UserID
					  where rr.RoleID != 4096
					  where r.IsActive
					  select r).Distinct();

			return qU.ToList();
		}

		public static List<WorkSession> GetWorkSessions(DateTime? startDate, DateTime? endDate)
		{
			var qWs = from r in DB.WorkSessions
					  where r.StartTime >= startDate && r.StartTime < endDate
					  select r;

			return qWs.ToList();
		}

		public static List<WorkAllocation> GetWorkAllocations(DateTime? startDate, DateTime? endDate)
		{
			var qWs = from r in DB.WorkAllocations
					  where r.DayWorked >= startDate && r.DayWorked < endDate
					  select r;

			return qWs.ToList();
		}

		public static List<Leave> GetLeaveAllocations(DateTime? startDate, DateTime? endDate)
		{
			var qWs = from r in DB.Leaves
					  where (r.DateFrom >= startDate && r.DateFrom < endDate) || (r.DateTo > endDate && r.DateFrom < endDate)
					  select r;

			return qWs.ToList();
		}

		public static List<LeaveType> GetLeaveTypes()
		{
			var lTypes = from r in DB.LeaveTypes
						 orderby r.LeaveTypeName
						 select r;

			return lTypes.ToList();
		}
	}
}