using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class TicketReferenceNumberRepo : BaseRepo
	{
		internal static void Save(TicketReferenceNumber ticketReferenceNum)
		{
			if (ticketReferenceNum.TicketReferenceNumberID <= 0)
				DB.TicketReferenceNumbers.Add(ticketReferenceNum);

			DB.SaveChanges();
		}

		public static TicketReferenceNumber GetByID(int ticketReferenceID) => DB.TicketReferenceNumbers.Where(i => i.TicketReferenceNumberID == ticketReferenceID).SingleOrDefault();

		public static string GetNewReferenceNumber(int clientID, string subject)
		{
			string projectCode = string.Empty;
			int referenceNumber = 0;

			if (string.IsNullOrEmpty(subject) || clientID == 0)
				return null;

			var ticketRefNum = new TicketReferenceNumber
			{
				TicketSubject = subject
			};
			Save(ticketRefNum);

			var client = ClientRepo.GetByID(clientID);

			if (client != null)
			{
				projectCode = client.ClientName.Substring(0, 3).ToUpper();
			}

			referenceNumber = ticketRefNum.TicketReferenceNumberID;

			return string.Format(projectCode + referenceNumber.ToString("D4"));
		}
	}
}