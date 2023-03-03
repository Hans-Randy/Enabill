using System;
using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class LeaveIndexModel
	{
		#region INITIALIZATION

		public LeaveIndexModel(User user)
		{
			this.SD = InputHistory.GetDateTime(HistoryItemType.LeaveStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(-1)).Value;
			this.ED = InputHistory.GetDateTime(HistoryItemType.LeaveEndDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(6).AddDays(-1)).Value;

			this.ConfigureDates(user, this.SD, this.ED);

			this.StartDate = (user.EmployStartDate > this.SD ? user.EmployStartDate : this.SD);
			this.EndDate = this.ED;

			this.User = user;
			this.LeaveDays = CommonModelFunctions.LoadLeaveDays(user, this.StartDate, this.ED);
			this.LeaveBalanceModel = new LeaveBalanceModel(user, this.StartDate, this.EndDate);
			this.UserLeaveManualAdjustments = new UserLeaveManualAdjustmentModel(user);
		}

		public LeaveIndexModel(User user, DateTime startDate, DateTime endDate)
		{
			this.SD = startDate;
			this.ED = endDate;
			this.ConfigureDates(user, this.SD, this.ED);

			this.StartDate = (user.EmployStartDate > this.SD ? user.EmployStartDate : this.SD);
			this.EndDate = this.ED;

			this.User = user;
			this.LeaveDays = CommonModelFunctions.LoadLeaveDays(user, this.StartDate, this.ED);
			this.LeaveBalanceModel = new LeaveBalanceModel(user, this.StartDate, this.EndDate);
			this.UserLeaveManualAdjustments = new UserLeaveManualAdjustmentModel(user);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public DateTime SD { get; private set; }
		public DateTime ED { get; private set; }
		public DateTime StartDate { get; private set; }
		public DateTime EndDate { get; private set; }

		public LeaveBalanceModel LeaveBalanceModel { get; private set; }
		public User User { get; private set; }
		public UserLeaveManualAdjustmentModel UserLeaveManualAdjustments { get; private set; }

		public List<UserLeaveModel> LeaveDays { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private void ConfigureDates(User user, DateTime startDate, DateTime endDate)
		{
			if (startDate > endDate)
			{
				this.StartDate = startDate;
				startDate = endDate.ToFirstDayOfMonth();
				endDate = this.StartDate.ToLastDayOfMonth();
			}

			if (startDate < Settings.SiteStartDate)
				startDate = Settings.SiteStartDate;

			if (startDate < user.EmployStartDate)
				startDate = user.EmployStartDate.ToFirstDayOfMonth();

			if (endDate < Settings.SiteStartDate)
				endDate = DateTime.Today.ToLastDayOfMonth();

			this.SD = startDate;
			this.ED = endDate;
		}

		#endregion FUNCTIONS
	}
}