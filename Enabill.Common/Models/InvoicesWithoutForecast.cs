using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwInvoicesWithoutForecasts")]
	public class InvoicesWithoutForecast
	{
		#region PROPERTIES

		[Key]
		public int InvoiceID { get; set; }

		public int Period { get; set; }
		public int RegionID { get; set; }
		public int? InvoiceRuleID { get; set; }

		public double AccrualExclVAT { get; set; }
		public double? ProvisionalAccrualAmount { get; set; }

		public string BillingMethodName { get; set; }
		public string ClientName { get; set; }
		public string OrderNo { get; set; }
		public string RegionName { get; set; }

		#endregion PROPERTIES
	}
}