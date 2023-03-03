using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class TimeCalendarModel
	{
		#region INITIALIZATION

		public TimeCalendarModel(User user, DateTime workDay, Dictionary<int, string> activityIDList)
		{
			this.HasExceptions = false;
			this.IsBeforeStartDate = false;
			this.WorkDayStatusClass = "error";

			this.User = user;
			this.WeekDay = workDay.DayOfWeek.ToString();
			this.WorkDay = workDay;
			this.WorkDayClass = this.SetUpWorkDayClass(workDay);
			this.DailyWorkSessions = user.GetWorkSessions(workDay);
			this.WorkSessionStatus = WorkSessionStatusType.UnApproved;

			foreach (var worksession in this.DailyWorkSessions)
			{
				this.WorkSessionStatus = (WorkSessionStatusType)worksession.WorkSessionStatusID;

				if (this.WorkSessionStatus == WorkSessionStatusType.Exception)
					break;
			}

			switch (this.WorkSessionStatus)
			{
				case WorkSessionStatusType.Approved:
					this.NrOfApprovedDays++;
					break;

				case WorkSessionStatusType.UnApproved:
					this.NrOfUnapprovedDays++;
					break;
			}

			if (workDay < user.EmployStartDate)
			{
				this.IsBeforeStartDate = true;
				this.WorkSessionStatus = WorkSessionStatusType.BeforeStartDate;
			}

			this.WorkDayStatusClass = this.WorkSessionStatus.ToString() == "Exception" || (workDay.IsWorkableDay() && this.WorkSessionStatus == 0 && !this.IsLeaveDay && !this.IsFlexiDay) ? "error" : "";
			this.HasExceptions = this.WorkSessionStatus.ToString() == "Exception" || (workDay.IsWorkableDay() && this.WorkSessionStatus == 0 && !this.IsLeaveDay && !this.IsFlexiDay);
			this.AllocatedTime = user.GetAllocatedTime(workDay);
			this.UnallocatedTime = user.GetUnallocatedTime(workDay);
			this.AllocationClass = this.UnallocatedTime == 0 ? string.Empty : "error";
			this.IsFlexiDay = user.IsFlexiDayTakenOnDate(workDay);
			this.IsLeaveDay = user.IsAnyLeaveTakenOnDate(workDay);
			this.IsPendingLeaveDay = user.IsAnyLeaveTakenOnDate(workDay, ApprovalStatusType.Pending);
			this.DayHasNotes = user.GetNotes(workDay, workDay).Count > 0;
			this.WorkAllocations = this.LoadDayAllocations(user, workDay, activityIDList);
			this.WorkAllocationsInclRemarks = this.LoadDayAllocationsInclRemarks(user, workDay, activityIDList);
			this.FlexiOrLeaveDayText = "";

			if (workDay.IsWorkableDay())
			{
				if (this.IsFlexiDay)
				{
					this.FlexiOrLeaveDayText = "Flexi";
					this.WorkDayStatusClass = "";
					this.HasExceptions = false;
				}
				else if (this.IsLeaveDay)
				{
					this.Leave = user.GetAnyLeaveForDate(workDay);
					this.FlexiOrLeaveDayText = ((LeaveTypeEnum)this.Leave.LeaveType).ToString();
					this.WorkDayStatusClass = "";
					this.HasExceptions = false;
				}
				else if (this.IsPendingLeaveDay)
				{
					this.Leave = user.GetAnyLeaveForDate(workDay, ApprovalStatusType.Pending);
					this.FlexiOrLeaveDayText = ((LeaveTypeEnum)this.Leave.LeaveType).ToString() + "(unapproved)";
					this.WorkDayStatusClass = "error";
					this.HasExceptions = true;
				}
			}
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool DayHasNotes { get; internal set; }
		public bool HasExceptions { get; private set; }
		public bool IsBeforeStartDate { get; private set; }
		public bool IsDeclinedLeaveDay { get; private set; }
		public bool IsFlexiDay { get; internal set; }
		public bool IsLeaveDay { get; internal set; }
		public bool IsPendingLeaveDay { get; internal set; }

		public int NrOfApprovedDays { get; private set; }
		public int NrOfUnapprovedDays { get; private set; }

		public double AllocatedTime { get; private set; }
		public double UnallocatedTime { get; private set; }

		public string AllocationClass { get; private set; }
		public string DayUsedForLeaveOrFlexiDay { get; internal set; }
		public string FlexiOrLeaveDayText { get; private set; }
		public string WeekDay { get; private set; }
		public string WorkDayClass { get; private set; }
		public string WorkDayStatusClass { get; private set; }

		public DateTime WorkDay { get; private set; }

		public WorkSessionStatusType WorkSessionStatus { get; private set; }

		public Leave Leave { get; private set; }
		public User User { get; private set; }

		public List<double> WorkAllocations { get; internal set; }
		public List<WorkAllocation> WorkAllocationsInclRemarks { get; private set; }
		public List<WorkSession> DailyWorkSessions { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<double> LoadDayAllocations(User user, DateTime workDay, Dictionary<int, string> activityIDList)
		{
			var model = new List<double>();

			if (activityIDList == null || activityIDList.Count == 0)
				return model;

			foreach (int id in activityIDList.Keys)
			{
				double daySum = user.GetWorkAllocationsForActivityForDate(id, workDay).Sum(wa => wa.HoursWorked);

				if (daySum <= 0)
					model.Add(0);
				else
					model.Add(daySum);
			}

			return model;
		}

		private List<WorkAllocation> LoadDayAllocationsInclRemarks(User user, DateTime workDay, Dictionary<int, string> activityIDList)
		{
			var model = new List<WorkAllocation>();

			if (activityIDList == null || activityIDList.Count == 0)
				return model;

			foreach (int id in activityIDList.Keys)
			{
				model.AddRange(user.GetWorkAllocationsForActivityForDate(id, workDay));
			}

			return model;
		}

		public double CalculateDailyHoursWorked(List<WorkSession> DailyWorkSessions)
		{
			double gross = 0;

			foreach (var ws in DailyWorkSessions)
			{
				gross += ws.GrossTime;
			}

			return gross;
		}

		public double CalculateDailyLunchTime(List<WorkSession> DailyWorkSessions)
		{
			double lunchTime = 0;

			foreach (var ws in DailyWorkSessions)
			{
				lunchTime += ws.LunchTime;
			}

			return lunchTime;
		}

		public string SetUpWorkDayClass(DateTime WDay)
		{
			if (WDay == DateTime.Today)
				return "today";

			//if ((int)WDay.DayOfWeek == 0 || (int)WDay.DayOfWeek == 6)
			//  return "weekend";

			if (!WDay.IsWorkableDay())
				return "weekend";

			return string.Empty;
		}

		#endregion FUNCTIONS
	}
}