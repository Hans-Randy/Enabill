namespace Enabill.Models.Dto
{
	public class PercentageAllocationModel
	{
		#region PROPERTIES

		public int OrderByMonth { get; set; }

		public decimal PercentageTotalHoursWorkedForMonth { get; set; }
		public decimal TotalHoursWorkedForMonth { get; set; }
		public decimal SumOfHoursWorked { get; set; }

		public string ActivityName { get; set; }
		public string ClientName { get; set; }
		public string DivisionCode { get; set; }
		public string Manager { get; set; }
		public string PayrollRefNo { get; set; }
		public string Person { get; set; }
		public string ProjectBillingMethod { get; set; }
		public string ProjectName { get; set; }
		public string ReportType { get; set; }
		public string UserBillableIndicator { get; set; }
		public string WorkMonth { get; set; }

		#endregion PROPERTIES
	}
}