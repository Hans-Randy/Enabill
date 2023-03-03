using System;
using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class UserProfileModel
	{
		#region INITIALIZATION

		public UserProfileModel(User user)
		{
			this.User = user;
			this.UserDetails = new UserDetailModel(user);
			this.Calendar = new TimeIndexModel(user, DateTime.Today);

			this.CurrentTimeBalance = new CurrentTimeBalanceModel(user);

			this.CurrentFlexiBalance = new CurrentFlexiBalanceModel(user);
			this.FlexiDays = user.GetFlexiDaysForDateSpan(DateTime.Today.ToFirstDayOfMonth(), DateTime.Today.ToLastDayOfMonth());

			this.CurrentLeaveBalance = new CurrentLeaveBalanceModel(user);
			this.LeaveDays = CommonModelFunctions.LoadLeaveDays(user, DateTime.Today.ToFirstDayOfMonth(), DateTime.Today.ToLastDayOfMonth());
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public CurrentFlexiBalanceModel CurrentFlexiBalance { get; set; }
		public CurrentLeaveBalanceModel CurrentLeaveBalance { get; set; }
		public CurrentTimeBalanceModel CurrentTimeBalance { get; private set; }
		public TimeIndexModel Calendar { get; set; }
		public User User { get; set; }
		public UserDetailModel UserDetails { get; set; }

		public List<FlexiDay> FlexiDays { get; set; }
		public List<UserLeaveModel> LeaveDays { get; set; }

		#endregion PROPERTIES
	}
}