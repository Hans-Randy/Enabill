using Enabill.Models;

namespace Enabill.Repos
{
	public class TicketStatusChangeRepo : BaseRepo
	{
		internal static void Save(TicketStatusChange ticketStatusChange)
		{
			if (ticketStatusChange.TicketStatusChangeID <= 0)
				DB.TicketStatusChanges.Add(ticketStatusChange);

			DB.SaveChanges();
		}
	}
}