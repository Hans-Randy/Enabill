using System;
using Enabill.Models;

namespace Enabill.Repos
{
	public class TicketLogRepo : BaseRepo
	{
		internal static void Save(TicketLog ticketLog)
		{
			if (ticketLog.TicketLogID <= 0)
				DB.TicketLogs.Add(ticketLog);

			DB.SaveChanges();
		}

		public static void CreateTicketLog(TicketLogType ticketLogType, int ticketID, int source)
		{
			var ticketLog = new TicketLog
			{
				TicketID = ticketID,
				TicketLogTypeID = (int)ticketLogType,
				Source = source,
				DateCreated = DateTime.Now
			};

			Save(ticketLog);
		}
	}
}