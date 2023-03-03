using System;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class FlexiTimeMonthModel
	{
		#region INITIALIZATION

		public FlexiTimeMonthModel(User user, DateTime selectedMonth)
		{
			this.User = user;
			this.LoadSelectedMonthsFlexiStatement(user, selectedMonth);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public FlexiTimeMonthSummaryModel SelectedMonthsFlexiStatement { get; set; }
		public User User { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public void LoadSelectedMonthsFlexiStatement(User user, DateTime selectedMonth)
		{
			var model = new FlexiTimeMonthSummaryModel(user, selectedMonth, 0);
			this.SelectedMonthsFlexiStatement = model;

			var selectedMonthsFlexiBalance = user.GetFlexiBalance(selectedMonth);

			if (selectedMonthsFlexiBalance == null)
				this.SelectedMonthsFlexiStatement.OpeningBalance = user.FlexiBalanceTakeOn;
			else
				this.SelectedMonthsFlexiStatement.OpeningBalance = selectedMonthsFlexiBalance.FinalBalance;
		}

		#endregion FUNCTIONS
	}
}