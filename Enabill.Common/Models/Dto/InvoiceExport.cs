namespace Enabill.Models.Dto
{
	public class InvoiceExportModel
	{
		#region PROPERTIES

		public double? Amount { get; set; }
		public double? HoursWorked { get; set; }
		public double? Rate { get; set; }

		public string Customer { get; set; }
		public string CustomerCode { get; set; }
		public string DepartmentCode { get; set; }
		public string Description { get; set; }
		public string DocumentDate { get; set; }
		public string GLCode { get; set; }

		#endregion PROPERTIES
	}
}