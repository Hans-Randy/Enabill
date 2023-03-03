using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("ForecastHeaders")]
	public class ForecastHeader
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ForecastHeaderID { get; set; }

		[Required]
		[EnumDataType(typeof(BillingMethodType))]
		public int BillingMethod { get; set; }

		public int? ActivityID { get; set; }
		public int? ClientID { get; set; }
		public int? DivisionID { get; set; }
		public int? InvoiceCategoryID { get; set; }
		public int? ProjectID { get; set; }
		public int? RegionID { get; set; }
		public int? UserID { get; set; }

		[Required]
		public double Probability { get; set; }

		public string Activity { get; set; }
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