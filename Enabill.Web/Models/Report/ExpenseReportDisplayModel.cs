using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models.Dto
{
	public class ExpenseReportDisplayModel
	{
		#region INITIALIZATION

		public ExpenseReportDisplayModel(DateTime dateFrom, DateTime dateTo, int userID = 0, int employmentTypeID = 0, int clientID = 0, int projectID = 0, int expenseCategoryTypeID = 0, string lockedStatus = "", string billableStatus = "", string activeStatus = "")
		{
			this.ExpenseReport = this.LoadExpenseReportModel(dateFrom, dateTo, userID, employmentTypeID, clientID, projectID, expenseCategoryTypeID, lockedStatus, billableStatus, activeStatus);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public DateTime ExpenseReportDateFrom { get; private set; }
		public DateTime ExpenseReportDateTo { get; private set; }

		public List<ExpenseReportModel> ExpenseReport { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<ExpenseReportModel> LoadExpenseReportModel(DateTime dateFrom, DateTime dateTo, int userID, int employmentTypeID, int clientID, int projectID, int expenseCategoryTypeID, string lockedStatus, string billableStatus, string activeStatus)
		{
			var model = ExpenseRepo.GetReportInfo(dateFrom, dateTo, userID, employmentTypeID, clientID, projectID, expenseCategoryTypeID, lockedStatus, billableStatus, activeStatus).ToList();

			return model;
		}

		#endregion FUNCTIONS
	}
}