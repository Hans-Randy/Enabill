using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public abstract class UserLeaveGeneralRepo : BaseRepo
	{
		#region LEAVE

		public static List<LeaveType> GetLeaveTypes()
		{
			var lTypes = from r in DB.LeaveTypes
						 orderby r.LeaveTypeName
						 select r;

			return lTypes.ToList();
		}

		public static List<ApprovalStatus> GetApprovalStatus()
		{
			var status = from a in DB.ApprovalStatus
						 orderby a.ApprovalStatusName
						 select a;

			return status.ToList();
		}

		public static bool GetValidLeaveDayByUserID(int userID, DateTime leaveDay)
		{
			// Check if any leave records
			bool result = DB.Leaves
						.Where
						(
							l => l.UserID == userID
							&& l.DateFrom <= leaveDay && leaveDay <= l.DateTo
							&& l.ApprovalStatus == (int)ApprovalStatusType.Approved
						).Count() > 0;

			if (!result)
				return false;

			// Check if any full day leave records
			result = DB.Leaves
						.Where
						(
							l => l.UserID == userID
							&& l.DateFrom <= leaveDay && leaveDay <= l.DateTo
							&& l.NumberOfDays >= 1
							&& l.ApprovalStatus == (int)ApprovalStatusType.Approved
						).Count() > 0;

			if (result)
			{
				return true;
			}
			else // Check for the sum of partial leave records
			{
				// Partial leave hours
				double leaveHours = DB.Leaves
							.Where
							(
								l => l.UserID == userID
								&& l.DateFrom == leaveDay
								&& l.NumberOfDays < 1
								&& l.ApprovalStatus == (int)ApprovalStatusType.Approved
							)
							.Sum(l => (double?)l.NumberOfHours) ?? 0;

				// Hours worked for the day
				double workedHours = DB.WorkAllocations
							.Where
							(
								w => w.UserID == userID
								&& w.DayWorked == leaveDay
							)
							.Sum(w => (double?)w.HoursWorked) ?? 0;

				// Required work hours for the employee
				double requiredHours = DB.Users
								.Where(u => u.UserID == userID)
								.Select(u => (int)u.WorkHours).SingleOrDefault();

				// Get required work hours for the particular day
				var user = new User
				{
					UserID = userID
				};
				double requiredHoursForDay = user.GetTotalWorkTime(leaveDay);

				// Calculate if outstanding
				if (workedHours > 0)
				{
					return requiredHoursForDay <= (leaveHours + workedHours);
				}
				else
				{
					return requiredHours <= (leaveHours + workedHours);
				}
			}
		}

		public static List<UserLeaveGeneralModel> GetGeneralLeave(DateTime dateFrom, DateTime dateTo, int employeeID = 0, int employmentTypeID = 0, int managerID = 0, int leaveTypeID = 0, int approvalStatusID = 0)
		{
			var data = from u in DB.Users.AsEnumerable()
					   join m in DB.Users on u.ManagerID equals m.UserID
					   join e in DB.EmploymentTypes on u.EmploymentTypeID equals e.EmploymentTypeID
					   join l in DB.Leaves on u.UserID equals l.UserID
					   join lt in DB.LeaveTypes on l.LeaveType equals lt.LeaveTypeID
					   join a in DB.ApprovalStatus on l.ApprovalStatus equals a.ApprovalStatusID
					   where u.IsActive && l.DateFrom >= dateFrom && l.DateTo <= dateTo
					   select (new UserLeaveGeneralModel()
					   {
						   EmployeeID = u.UserID,
						   FullName = u.FullName,
						   PayrollRefNo = u.PayrollRefNo,
						   EmploymentTypeID = u.EmploymentTypeID,
						   EmploymentType = e.EmploymentTypeName,
						   ManagerID = m.UserID,
						   Manager = m.FullName,
						   DateFrom = l.DateFrom,
						   DateTo = l.DateTo,
						   LeaveTypeID = lt.LeaveTypeID,
						   LeaveType = lt.LeaveTypeName,
						   NumberOfDays = l.NumberOfDays,
						   IsPartial = l.NumberOfDays < 1,
						   ApprovalStatusID = a.ApprovalStatusID,
						   ApprovalStatus = a.ApprovalStatusName
					   });

			if (employeeID != 0)
				data = data.Where(d => d.EmployeeID == employeeID);

			if (employmentTypeID != 0)
				data = data.Where(d => d.EmploymentTypeID == employmentTypeID);

			if (managerID != 0)
				data = data.Where(d => d.ManagerID == managerID);

			if (leaveTypeID != 0)
				data = data.Where(d => d.LeaveTypeID == leaveTypeID);

			//if (isPartial != false)
			//    data = data.Where(d => d.IsPartial == isPartial);

			if (approvalStatusID != 0)
				data = data.Where(d => d.ApprovalStatusID == approvalStatusID);

			return data.Distinct().OrderBy(u => u.FullName).ThenBy(d => d.DateFrom).ToList();
		}

		#endregion LEAVE
	}
}