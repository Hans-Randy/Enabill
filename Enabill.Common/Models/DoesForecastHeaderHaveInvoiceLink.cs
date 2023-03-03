using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwDoesForecastHeaderHaveInvoiceLinks")]
	public class DoesForecastHeaderHaveInvoiceLinks
	{
		#region PROPERTIES

		[Key]
		public int ForecastHeaderID { get; set; }

		public int Period { get; set; }

		public string HasInvoicesLinked { get; set; }

		#endregion PROPERTIES
	}
}