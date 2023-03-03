using System;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class ClientProjectTicketRepo : BaseRepo
	{
		public static IQueryable<ClientProjectTicket> GetForProjectSubject(string supportEmail, string subject) => DB.ClientProjectTickets.Where(f => f.ToAddress.IndexOf(supportEmail, StringComparison.OrdinalIgnoreCase) >= 0 && f.TicketSubject.ToLower().Replace(" ", "") == subject.ToLower().Replace(" ", ""));
	}
}