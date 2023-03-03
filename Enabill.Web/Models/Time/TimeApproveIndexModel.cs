using System;
using System.Collections.Generic;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Web.Models
{
	public class TimeApproveIndexModel
	{
		#region INITIALIZATION

		public TimeApproveIndexModel(User user, DateTime dateFrom, DateTime dateTo, string callingPage, int invoiceID = 0)
		{
			var userExceptionDetails = new List<WorkAllocationExceptionModel>();
			//user.IsTimeCaptured(dateFrom, dateTo, user, out userExceptionDetails);
			//DateFrom = InputHistory.GetDateTime(HistoryItemType.ApprovalDateFrom, DateTime.Today.ToFirstDayOfMonth()).Value;
			//DateTo = InputHistory.GetDateTime(HistoryItemType.ApprovalDateTo, DateTime.Today.ToLastDayOfMonth()).Value;
			this.DateFrom = dateFrom;
			this.DateTo = dateTo;
			this.TimeApprovalUser = InputHistory.Get(HistoryItemType.TimeApprovalUser, 0);
			this.InvoiceID = invoiceID;
			this.CallingPage = callingPage;
			this.StandardAmtColumns = 9; // if column are added are removed from the viw, change this value
			this.User = user;
			this.WorkDay = dateFrom;
			this.LoadCalendar(user, dateFrom, dateTo);
			this.CanTimesheetLockStatusBeManaged = user.CanTimesheetLockStatusBeManaged(dateFrom);
			this.IsTimesheetLocked = true;

			if (this.CanTimesheetLockStatusBeManaged)
				this.IsTimesheetLocked = user.IsTimesheetLockedForDate(dateFrom);

			this.TotlAmtColumns = this.StandardAmtColumns + this.ActivityList.Count;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool CanTimesheetLockStatusBeManaged { get; private set; }
		public bool IsTimesheetLocked { get; private set; }

		public int ApprovalStatus { get; private set; }
		public int InvoiceID { get; private set; }
		public int StandardAmtColumns { get; private set; }
		public int TimeApprovalUser { get; private set; }
		public int TotlAmtColumns { get; private set; }

		public string CallingPage { get; private set; }

		public DateTime DateFrom { get; private set; }
		public DateTime DateTo { get; private set; }
		public DateTime WorkDay { get; private set; }

		public User User { get; private set; }

		public Dictionary<int, string> ActivityList { get; private set; }
		public Dictionary<int, string> NoWorkSessionActivityList { get; private set; }

		public List<ActivityDetail> ActivityDetails { get; private set; }
		public List<TimeCalendarModel> Calendar { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private void LoadCalendar(User user, DateTime dateFrom, DateTime dateTo)
		{
			this.ActivityList = new Dictionary<int, string>();

			//if (dateFrom < user.EmployStartDate)
			//    dateFrom = user.EmployStartDate;

			this.ActivityDetails = user.GetWorkedActivitiesForDateSpan(dateFrom, dateTo);

			this.ActivityDetails.ForEach(a => this.ActivityList.Add(a.ActivityID, string.Format($"{a.ClientName}<br />{a.ProjectName}<br />{a.ActivityName}")));

			//user.GetWorkedActivitiesForDateSpan(dateFrom, dateTo.AddDays(1))
			//            .ForEach(a => ActivityList.Add(a.ActivityID, string.Format($"{a.ClientName}<br />{a.ProjectName}<br />{a.ActivityName}")));

			this.Calendar = new List<TimeCalendarModel>();
			Enabill.Helpers.GetDaysInDateSpan(dateFrom, dateTo).ForEach(w => this.Calendar.Add(new TimeCalendarModel(user, w, this.ActivityList)));
		}

		#endregion FUNCTIONS
	}
}