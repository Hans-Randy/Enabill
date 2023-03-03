using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class FeedbackPostRepo : BaseRepo
	{
		internal static FeedbackThread GetFeedbackThread(int feedbackThreadID) => DB.FeedbackThreads
					.SingleOrDefault(fbt => fbt.FeedbackThreadID == feedbackThreadID);

		internal static void Save(FeedbackPost post)
		{
			if (post.FeedbackPostID <= 0)
				DB.FeedbackPosts.Add(post);

			DB.SaveChanges();
		}

		#region USER

		internal static User GetUser(int userID) => DB.Users
					.SingleOrDefault(p => p.UserID == userID);

		#endregion USER
	}
}