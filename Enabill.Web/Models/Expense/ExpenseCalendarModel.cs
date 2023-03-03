using System;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class ExpenseCalendarModel
	{
		#region INITIALIZATION

		public ExpenseCalendarModel(int expenseID, DateTime expenseDate, double amount, int clientID,string clientName, string projectName, string expenseCategoryTypeName, bool billable, bool locked, User user)
		{
			this.ExpenseID = expenseID;
			this.ExpenseCategoryTypeName = expenseCategoryTypeName;
			this.ClientName = clientName;
			this.ProjectName = projectName;
			this.ExpenseDate = expenseDate;
			this.WeekDay = expenseDate.DayOfWeek.ToString();
			this.Amount = amount;
			this.Billable = billable;
			this.Locked = locked;
			this.ClientID = clientID;

			this.User = user;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool Billable { get; private set; }
		public bool Locked { get; private set; }

		public int ExpenseID { get; private set; }

		public double Amount { get; set; }

		public int ClientID { get; private set; }
		public string ClientName { get; private set; }
		public string ExpenseCategoryTypeName { get; private set; }
		public string ProjectName { get; private set; }
		public string WeekDay { get; private set; }

		public DateTime ExpenseDate { get; private set; }

		public User User { get; private set; }

		#endregion PROPERTIES
	}
}