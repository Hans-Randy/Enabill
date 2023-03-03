using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Repos
{
	public class TicketPriorityRepo : BaseRepo
	{
		public static IEnumerable<TicketPriority> GetAll() => DB.TicketPriorities;
	}
}