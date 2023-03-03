using System;
using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class ExpenseIndexModel
	{
		#region INITIALIZATION

		public ExpenseIndexModel(User user, DateTime expenseDate)
		{
			this.StandardAmtColumns = 7; // if column are added are removed from the view, change this value
			this.ExpenseDate = expenseDate;

			this.User = user;

			this.LoadCalendar(user, expenseDate);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int StandardAmtColumns { get; private set; }

		public DateTime ExpenseDate { get; private set; }

		public User User { get; private set; }
		public Dictionary<int, string> ExpenseList { get; private set; }

		public List<ExpenseCalendarModel> Calendar { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private void LoadCalendar(User user, DateTime expenseDate)
		{
			var ExpenseList = user.GetExpensesForDateSpan(expenseDate.ToFirstDayOfMonth(), expenseDate.ToLastDayOfMonth());

			this.Calendar = new List<ExpenseCalendarModel>();

			ExpenseList.ForEach(w => this.Calendar.Add(new ExpenseCalendarModel(w.ExpenseID, w.ExpenseDate, w.Amount,w.ClientID, w.ClientName, w.ProjectName, w.ExpenseCategoryTypeName, w.Billable, w.Locked, user)));
		}

		#endregion FUNCTIONS
	}
}