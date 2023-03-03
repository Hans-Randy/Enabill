using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class LeaveBalanceModel
	{
		#region INITIALIZATION

		public LeaveBalanceModel(User user, DateTime startDate, DateTime endDate)
		{
			this.StartDate = startDate;

			if (startDate == user.EmployStartDate)
				startDate = startDate.ToFirstDayOfMonth();

			if (startDate < EnabillSettings.SiteStartDate)
				startDate = EnabillSettings.SiteStartDate;

			this.StartingBalance = new LeaveBalanceMonthModel(user, startDate);

			startDate = startDate.ToFirstDayOfMonth();
			this.LeaveBalances = this.LoadLeaveBalances(user, startDate, endDate);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public DateTime StartDate { get; private set; }

		public LeaveBalanceMonthModel StartingBalance { get; private set; }

		public List<LeaveBalanceMonthModel> LeaveBalances { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		internal List<LeaveBalanceMonthModel> LoadLeaveBalances(User user, DateTime startDate, DateTime endDate)
		{
			var model = new List<LeaveBalanceMonthModel>();

			while (startDate <= endDate)
			{
				model.Add(new LeaveBalanceMonthModel(user, startDate));
				startDate = startDate.AddMonths(1);
			}

			return model.OrderBy(m => m.Date).ToList();
		}

		#endregion FUNCTIONS
	}
}