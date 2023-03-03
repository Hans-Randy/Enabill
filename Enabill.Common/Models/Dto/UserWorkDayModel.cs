using System;
using System.Collections.Generic;

namespace Enabill.Models
{
	public class UserWorkDayModel
	{
		#region PROPERTIES

		public bool HasException { get; set; }
		public bool IsAfterEndDate { get; set; }
		public bool IsBeforeStartDay { get; set; }
		public bool IsFlexiDay { get; set; }
		public bool IsLeaveDay { get; set; }
		public bool IsPendingFlexiDay { get; set; }
		public bool IsPendingLeaveDay { get; set; }

		public double AllocatedTime { get; set; }
		public double TotalTime { get; set; }
		public double UnAllocatedTime { get; set; }

		public UserWorkDayStatus Status { get; set; }
		public WorkDayReason Reason { get; set; }

		public User User { get; set; }
		public WorkDay WorkDay { get; set; }

		public IList<FlexiDay> FlexiDays { get; set; }
		public IList<LeaveDayModel> Leave { get; set; }
		public IList<WorkAllocation> WorkAllocations { get; set; }
		public IList<WorkSession> WorkSessions { get; set; }

		#endregion PROPERTIES
	}

	public class LeaveDayModel
	{
		#region PROPERTIES

		public int UserId { get; set; }

		public DateTime LeaveDate { get; set; }

		public ApprovalStatusType ApprovalStatus { get; set; }
		public LeaveTypeEnum LeaveType { get; set; }

		#endregion PROPERTIES
	}

	public class TimesheetSummaryModel
	{
		#region PROPERTIES

		public double TotalAfterEndDate { get; set; }
		public double TotalAllocatedTime { get; set; }
		public double TotalBeforeStartDays { get; set; }
		public double TotalExceetptions { get; set; }
		public double TotalFlexiDays { get; set; }
		public double TotalLeaves { get; set; }
		public double TotalTime { get; set; }
		public double TotalUnallocatedTime { get; set; }
		public double TotalWorkableDays { get; set; }
		public double TotalWorkAllocations { get; set; }
		public double TotalWorkDays { get; set; }
		public double TotalWorkSessions { get; set; }

		public User User { get; set; }

		#endregion PROPERTIES
	}

	public enum UserWorkDayStatus
	{
		All = 1,
		Unapproved = 2,
		Approved = 3,
		Exception = 4
	}

	public enum WorkDayReason
	{
		NotWorkable = 1,
		LeaveDay = 2,
		FlexiDay = 3,
		BeforeStartDate = 4,
		AfterEndDate = 5,
		CorrectTimeAllocation = 6,
		NoTimeAllocation = 7,
		IncorrectTimeAllocation = 8
	}
}