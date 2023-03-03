using System;

namespace Enabill.Models.Dto
{
	public class ExpenseReportModel
	{
		#region PROPERTIES

		public bool Billable { get; set; }
		public bool IsActive { get; set; }
		public bool Locked { get; set; }

		public int ClientID { get; set; }
		public int EmploymentTypeID { get; set; }
		public int ExpenseCategoryTypeID { get; set; }
		public int ProjectID { get; set; }
		public int UserID { get; set; }

		public double Amount { get; set; }

		//public string Amount { get; set; }
		public double? Mileage { get; set; }

		public string Active { get; set; }
		public string Approved { get; set; }
		public string BillableStatus { get; set; }
		public string ClientName { get; set; }
		public string Employee { get; set; }
		public string EmploymentType { get; set; }
		public string ExpenseCategoryType { get; set; }
		public string ManagedBy { get; set; }
		public string Notes { get; set; }
		public string ProjectName { get; set; }

		public DateTime ExpenseDate { get; set; }
		public DateTime? DateManaged { get; set; }

		public string ExpenseAmount { get; internal set; }

		#endregion PROPERTIES
	}
}