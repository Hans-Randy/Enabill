using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("TicketStatus")]
	public class TicketStatus
	{
		#region PROPERTIES

		[Key]
		public int TicketStatusID { get; set; }

		[Required, MaxLength(50)]
		public string TicketStatusName { get; set; }

		#endregion PROPERTIES
	}
}