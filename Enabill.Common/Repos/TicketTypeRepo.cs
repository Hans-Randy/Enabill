using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Repos
{
	public class TicketTypeRepo : BaseRepo
	{
		public static IEnumerable<TicketType> GetAll() => DB.TicketTypes;
	}
}