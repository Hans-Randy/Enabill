using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("ForecastInvoiceLinks")]
	public class ForecastInvoiceLink
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ForecastInvoiceLinkID { get; set; }

		[Required]
		public int ForecastDetailID { get; set; }

		[Required]
		public int ForecastHeaderID { get; set; }

		[Required]
		public int InvoiceID { get; set; }

		#endregion PROPERTIES
	}
}