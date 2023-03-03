using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("TicketPriorities")]
	public class TicketPriority
	{
		#region PROPERTIES

		[Key]
		[EnumDataType(typeof(TicketPriorityEnum))]
		public int TicketPriorityID { get; internal set; }

		[Required, MaxLength(50)]
		public string TicketPriorityName { get; internal set; }

		#endregion PROPERTIES
	}
}