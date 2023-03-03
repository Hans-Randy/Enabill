using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class LeaveRepo : BaseRepo
	{
		#region LEAVE SPECIFIC

		public static Leave GetByID(int leaveID) => DB.Leaves
					.SingleOrDefault(l => l.LeaveID == leaveID);

		public static IEnumerable<Leave> GetApprovedLeaveByLeaveType(LeaveTypeEnum leaveType) => DB.Leaves.Where(l => l.LeaveType == (int)leaveType && l.ApprovalStatus == (int)ApprovalStatusType.Approved).OrderBy(l => l.UserID);

		public static IEnumerable<UserLeave> GetLeaveRequestsPending(DateTime dateFrom) => from r in DB.Leaves
																						   join ru in DB.Users on r.UserID equals ru.UserID
																						   join rm in DB.Users on ru.ManagerID equals rm.UserID
																						   where r.DateRequested <= dateFrom && r.ApprovalStatus == (int)ApprovalStatusType.Pending
																						   orderby rm.LastName, rm.FirstName, ru.FirstName, ru.LastName, r.DateFrom
																						   select new UserLeave
																						   {
																							   User = ru,
																							   Leave = r,
																							   Manager = rm
																						   };

		public static IEnumerable<UserLeave> GetLeaveRequestsPending(DateTime dateFrom, int managerID) => from r in DB.Leaves
																										  join ru in DB.Users on r.UserID equals ru.UserID
																										  join rm in DB.Users on ru.ManagerID equals rm.UserID
																										  where r.DateRequested <= dateFrom && r.ApprovalStatus == (int)ApprovalStatusType.Pending && rm.UserID == managerID
																										  orderby rm.LastName, rm.FirstName, ru.FirstName, ru.LastName, r.DateFrom
																										  select new UserLeave
																										  {
																											  User = ru,
																											  Leave = r,
																											  Manager = rm
																										  };

		public static void Save(Leave leave)
		{
			if (leave.LeaveID == 0)
				DB.Leaves.Add(leave);

			DB.SaveChanges();
		}

		internal static void Delete(Leave leave)
		{
			DB.Leaves.Remove(leave);
			DB.SaveChanges();
		}

		#endregion LEAVE SPECIFIC
	}
}