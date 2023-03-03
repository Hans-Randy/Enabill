using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class TicketLineAttachmentRepo : BaseRepo
	{
		internal static void Save(TicketLineAttachment attachment)
		{
			if (attachment.TicketLineAttachmentID <= 0)
				DB.TicketLineAttachments.Add(attachment);

			DB.SaveChanges();
		}

		#region USER

		internal static User GetUser(int userID) => DB.Users
					.SingleOrDefault(p => p.UserID == userID);

		#endregion USER

		public static List<TicketLineAttachment> GetTicketAttachments(int ticketID) => (from a in DB.TicketLineAttachments
																						join l in DB.TicketLines on a.TicketLineID equals l.TicketLineID
																						join t in DB.Tickets on l.TicketID equals t.TicketID
																						where t.TicketID == ticketID
																						select a).ToList();

		public static TicketLineAttachment GetAttachmentByID(int ticketLineAttachmentID) => DB.TicketLineAttachments
				.SingleOrDefault(a => a.TicketLineAttachmentID == ticketLineAttachmentID);
	}
}