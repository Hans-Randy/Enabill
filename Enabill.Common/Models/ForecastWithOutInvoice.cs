using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwForecastWithOutInvoices")]
	public class ForecastWithOutInvoice
	{
		#region PROPERTIES

		[Key]
		public int ForecastHeaderID { get; set; }

		[EnumDataType(typeof(BillingMethodType))]
		public int BillingMethod { get; set; }

		public int DivisionID { get; set; }
		public int ForecastDetailID { get; set; }
		public int InvoiceCategoryID { get; set; }
		public int Period { get; set; }
		public int RegionID { get; set; }

		public double ForecastAmount { get; set; }
		public double Probability { get; set; }

		public string Client { get; set; }
		public string Division { get; set; }
		public string InvoiceCategory { get; set; }
		public string Project { get; set; }
		public string Region { get; set; }
		public string Remark { get; set; }
		public string Resource { get; set; }

		#endregion PROPERTIES
	}
}