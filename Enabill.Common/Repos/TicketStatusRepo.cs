using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class TicketStatusRepo : BaseRepo
	{
		public static IEnumerable<TicketStatus> GetByAll() => DB.TicketStatus;

		public static TicketStatus GetByID(int ticketStatusID) => DB.TicketStatus.Where(s => s.TicketStatusID == ticketStatusID).SingleOrDefault();

		public static TicketStatus GetByName(string ticketStatus) => DB.TicketStatus.Where(s => s.TicketStatusName == ticketStatus).SingleOrDefault();
	}
}