using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwForecastWithInvoices")]
	public class ForecastWithInvoice
	{
		#region PROPERTIES

		[Key]
		[EnumDataType(typeof(BillingMethodType))]
		public int BillingMethod { get; set; }

		public int DivisionID { get; set; }
		public int InvoiceID { get; set; }
		public int Period { get; set; }
		public int RegionID { get; set; }

		public double ActualAmount { get; set; }
		public double ForecastAmount { get; set; }
		public double ProjectedAmount { get; set; }

		public string Client { get; set; }
		public string Division { get; set; }
		public string OrderNo { get; set; }
		public string Region { get; set; }

		#endregion PROPERTIES
	}
}