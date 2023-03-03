using Enabill.Models;

namespace Enabill.Repos
{
	public class TicketAssignmentChangeRepo : BaseRepo
	{
		internal static void Save(TicketAssignmentChange ticketAssignmentChange)
		{
			if (ticketAssignmentChange.TicketAssignmentChangeID <= 0)
				DB.TicketAssignmentChanges.Add(ticketAssignmentChange);

			DB.SaveChanges();
		}
	}
}