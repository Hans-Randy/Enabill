using System;

namespace Enabill.Models.Dto
{
	public class MonthlyActivityCostDTOModel
	{
		#region PROPERTIES

		public int ActivityID { get; set; }
		public int ClientID { get; set; }
		public int DepartmentID { get; set; }
		public int ProjectID { get; set; }
		public int RegionID { get; set; }

		public double ActivityGrossProfit { get; set; }
		public double ActivityGrossProfitPercentage { get; set; }
		public double ActivityInvoiceValue { get; set; }
		public double ActivitysWorkSessionTotalCost { get; set; }

		public string ActivityName { get; set; }
		public string ClientName { get; set; }
		public string DepartmentName { get; set; }
		public string ProjectGroupCode { get; set; }
		public string ProjectName { get; set; }
		public string RegionName { get; set; }

		public DateTime MonthDate { get; set; }

		#endregion PROPERTIES
	}
}