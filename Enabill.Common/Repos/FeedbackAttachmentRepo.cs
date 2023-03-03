using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class FeedbackAttachmentRepo : BaseRepo
	{
		internal static void Save(FeedbackAttachment attachment)
		{
			if (attachment.FeedbackAttachmentID <= 0)
				DB.FeedbackAttachments.Add(attachment);

			DB.SaveChanges();
		}

		#region USER

		internal static User GetUser(int userID) => DB.Users
					.SingleOrDefault(p => p.UserID == userID);

		#endregion USER
	}
}