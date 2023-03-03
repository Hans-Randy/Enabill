using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Repos
{
	public class TicketFilterRepo : BaseRepo
	{
		public static IEnumerable<TicketFilter> GetAll() => DB.TicketFilters;
	}
}