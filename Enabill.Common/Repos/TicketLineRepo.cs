using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class TicketLineRepo : BaseRepo
	{
		internal static IEnumerable<TicketLineAttachment> GetAttachments(int ticketLineID) => DB.TicketLineAttachments.Where(a => a.TicketLineID == ticketLineID);

		public static IEnumerable<TicketLine> GetLinesForTicket(int ticketID) => DB.TicketLines.Where(l => l.TicketID == ticketID).OrderBy(l => l.TicketLineID);

		public static TicketLine GetFirstTicketLine(int ticketID)
		{
			var ticketLines = DB.TicketLines
										   .Where(t => t.TicketID == ticketID)
										   .ToList();

			if (ticketLines.Count == 0)
				return null;

			var minDate = ticketLines.Min(t => t.DateCreated);

			return ticketLines.Single(t => t.DateCreated == minDate);
		}

		public static TicketLine GetLastTicketLine(int ticketID)
		{
			var ticketLines = DB.TicketLines
										   .Where(t => t.TicketID == ticketID)
										   .ToList();

			if (ticketLines.Count == 0)
				return null;

			var maxDate = ticketLines.Max(t => t.DateCreated);

			return ticketLines.Single(t => t.DateCreated == maxDate);
		}

		internal static void Save(TicketLine ticketLine)
		{
			if (ticketLine.TicketLineID <= 0)
				DB.TicketLines.Add(ticketLine);

			DB.SaveChanges();

			// Create Ticket Log History Record
			TicketLogRepo.CreateTicketLog(TicketLogType.Ticket, ticketLine.TicketID, ticketLine.TicketLineID);
		}
	}
}