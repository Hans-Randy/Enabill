using System.Collections.Generic;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class FeedbackInstanceModel
	{
		#region INITIALIZATION

		public FeedbackInstanceModel(User currentUser, FeedbackThread thread)
		{
			this.Thread = thread;
			this.Posts = thread.GetPosts(currentUser);
			this.ThreadClosedBy = string.Empty;

			if (thread.DateClosed != null)
				this.ThreadClosedBy = UserRepo.GetByID(thread.UserClosed.Value).FullName;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public string ThreadClosedBy { get; internal set; }

		public FeedbackThread Thread { get; internal set; }

		public List<FeedbackPost> Posts { get; internal set; }

		#endregion PROPERTIES
	}
}