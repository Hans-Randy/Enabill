using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwClientProjectTickets")]
	public class ClientProjectTicket
	{
		#region PROPERTIES

		[Key]
		public int TicketID { get; internal set; }

		public int? ClientID { get; internal set; }
		public int? ProjectID { get; internal set; }

		//public int? UserID { get; internal set; }
		public string TicketSubject { get; internal set; }

		public string ToAddress { get; internal set; }

		#endregion PROPERTIES
	}
}