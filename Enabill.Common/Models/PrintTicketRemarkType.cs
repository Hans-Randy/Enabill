using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("PrintTicketRemarkOptions")]
	public class PrintTicketRemarkOption
	{
		#region PROPERTIES

		[Required]
		public int PrintTicketRemarkOptionID { get; internal set; }

		[Required, MaxLength(100)]
		public string PrintTicketRemarkOptionName { get; internal set; }

		#endregion PROPERTIES
	}
}