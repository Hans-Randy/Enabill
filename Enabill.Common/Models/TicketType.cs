using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("TicketTypes")]
	public class TicketType
	{
		#region PROPERTIES

		[Key]
		public int TicketTypeID { get; set; }

		[Required, MaxLength(50)]
		public string TicketTypeName { get; set; }

		#endregion PROPERTIES
	}
}