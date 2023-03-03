using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Web.Models
{
	public class DashboardViewModel
	{
		#region INITIALIZATION

		public DashboardViewModel(User user)
		{
			if (user.EmploymentTypeID == (int)EmploymentTypeEnum.HourlyContractor)
				this.TimeTableHeading = "My Timesheet - Current Month";
			else
				this.TimeTableHeading = "My Timesheet - Last 21 Days";

			this.RecentWorkSessions = this.LoadRecentWorkSessions(user);
			this.CurrentTimeBalance = new CurrentTimeBalanceModel(user);
			this.CurrentFlexiBalance = new CurrentFlexiBalanceModel(user);
			this.CurrentLeaveBalance = new CurrentLeaveBalanceModel(user);
			this.ManagerDashboardModel = new ManagerDashboardModel(user);
			this.UserLeave = this.LoadUserLeave(user);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public string TimeTableHeading { get; private set; }

		public DateTime LeaveDateFrom { get; private set; }
		public DateTime LeaveDateTo { get; private set; }

		public CurrentFlexiBalanceModel CurrentFlexiBalance { get; private set; }
		public CurrentLeaveBalanceModel CurrentLeaveBalance { get; private set; }
		public CurrentTimeBalanceModel CurrentTimeBalance { get; private set; }
		public ManagerDashboardModel ManagerDashboardModel { get; private set; }

		public List<TimeCalendarModel> RecentWorkSessions { get; private set; }
		public List<UserLeaveModel> UserLeave { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public List<TimeCalendarModel> LoadRecentWorkSessions(User user)
		{
			var fromDate = DateTime.Today.AddDays(-20);
			var toDate = DateTime.Now;

			if (user.EmploymentTypeID == (int)EmploymentTypeEnum.HourlyContractor)
			{
				fromDate = DateTime.Now.ToFirstDayOfMonth();
				toDate = DateTime.Now.ToLastDayOfMonth();
			}

			var model = new List<TimeCalendarModel>();

			foreach (var day in Enabill.Helpers.GetDaysInDateSpan(fromDate, toDate))
			{
				model.Add(new TimeCalendarModel(user, day, null));
			}

			return model;
		}

		private List<NoteDetailModel> LoadNotes(User user)
		{
			var model = new List<NoteDetailModel>();

			model.AddRange(user.GetDetailedNotes(DateTime.Today.AddDays(-14), DateTime.Now));

			return model;
		}

		private List<UserLeaveModel> LoadUserLeave(User user)
		{
			this.LeaveDateFrom = DateTime.Today;
			this.LeaveDateTo = this.LeaveDateFrom.AddMonths(12);

			var model = new List<UserLeaveModel>();
			user.GetLeave(this.LeaveDateFrom, this.LeaveDateTo).Where(l => l.ApprovalStatus == (int)ApprovalStatusType.Approved || l.ApprovalStatus == (int)ApprovalStatusType.Pending).OrderBy(l => l.DateFrom).ThenBy(l => l.DateTo).ToList().ForEach(l => model.Add(new UserLeaveModel(user, l)));

			return model;
		}

		#endregion FUNCTIONS
	}
}