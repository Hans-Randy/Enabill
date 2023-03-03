using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class FeedbackThreadRepo : BaseRepo
	{
		#region FEEDBACK THREAD SPECIFIC

		internal static FeedbackThread GetByID(int feedbackThreadID) => DB.FeedbackThreads
					.SingleOrDefault(t => t.FeedbackThreadID == feedbackThreadID);

		internal static void Save(FeedbackThread feedbackThread)
		{
			if (feedbackThread.FeedbackThreadID <= 0)
				DB.FeedbackThreads.Add(feedbackThread);

			DB.SaveChanges();
		}

		internal static IEnumerable<FeedbackThread> GetAll(bool isOpen)
		{
			if (isOpen)
			{
				return DB.FeedbackThreads.Where(t => t.DateClosed == null);
			}

			return DB.FeedbackThreads.Where(t => t.DateClosed != null);
		}

		internal static IEnumerable<FeedbackThread> GetAllForUser(int userID, bool isOpen)
		{
			if (isOpen)
			{
				IEnumerable<FeedbackThread> openList = (from fbt in DB.FeedbackThreads
														join fbp in DB.FeedbackPosts on fbt.FeedbackThreadID equals fbp.FeedbackThreadID
														where fbp.UserID == userID & fbt.DateClosed == null
														select fbt
													   )
													   .Distinct();

				return openList.Where(m => m.GetInitialFeedbackPost().UserID == userID);
			}

			IEnumerable<FeedbackThread> closedList = (from fbt in DB.FeedbackThreads
													  join fbp in DB.FeedbackPosts on fbt.FeedbackThreadID equals fbp.FeedbackThreadID
													  where fbp.UserID == userID & fbt.DateClosed != null
													  select fbt
													 )
													 .Distinct();

			return closedList.Where(m => m.GetInitialFeedbackPost().UserID == userID);
		}

		#endregion FEEDBACK THREAD SPECIFIC

		#region FEEDBACK POSTS

		internal static FeedbackPost GetInitialFeedbackPost(int feedbackThreadID)
		{
			var postList = DB.FeedbackPosts
											.Where(p => p.FeedbackThreadID == feedbackThreadID)
											.ToList();

			if (postList.Count == 0)
				return null;

			var minDate = postList.Min(p => p.DateAdded);

			return postList.Single(p => p.DateAdded == minDate);
		}

		internal static IEnumerable<FeedbackPost> GetPosts(int feedbackThreadID) => DB.FeedbackPosts
					.Where(p => p.FeedbackThreadID == feedbackThreadID);

		#endregion FEEDBACK POSTS

		#region USERS

		internal static IEnumerable<User> GetThreadUsers(int threadID) => (from t in DB.FeedbackThreads
																		   join p in DB.FeedbackPosts on t.FeedbackThreadID equals p.FeedbackThreadID
																		   join u in DB.Users on p.UserID equals u.UserID
																		   where threadID == t.FeedbackThreadID
																		   select u
										 )
										 .Distinct();

		#endregion USERS
	}
}