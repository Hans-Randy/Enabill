using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("FeedbackThreads")]
	public class FeedbackThread
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int FeedbackThreadID { get; private set; }

		[Required]
		public int FeedbackTypeID { get; internal set; }

		[Required]
		public int FeedbackUrgencyTypeID { get; internal set; }

		public int? UserClosed { get; internal set; }

		[Required, MinLength(3), MaxLength(512)]
		public string FeedbackSubject { get; set; }

		public DateTime? DateClosed { get; internal set; }

		#endregion PROPERTIES

		#region INITIALIZATION

		public bool IsClosed => this.DateClosed.HasValue;

		public string FeedBackTypeName => FeedbackTypeRepo.GetByID(this.FeedbackTypeID).FeedbackTypeName;

		public string FeedBackUrgencyTypeName => FeedbackUrgencyTypeRepo.GetByID(this.FeedbackUrgencyTypeID).FeedbackUrgencyTypeName;

		public DateTime DateStarted => this.GetInitialFeedbackPost().DateAdded;

		#endregion INITIALIZATION

		#region FEEDBACK THREAD

		public static FeedbackThread GetByID(User userRequesting, int feedbackThreadID)
		{
			if (feedbackThreadID <= 0)
				return GetNew();

			var thread = FeedbackThreadRepo.GetByID(feedbackThreadID);

			if (!userRequesting.HasRole(UserRoleType.FeedbackAdmin) && userRequesting.UserID != thread.GetThreadUser().UserID)
				return GetNew();

			return thread;
		}

		public static FeedbackThread GetNew() => new FeedbackThread()
		{
			FeedbackSubject = string.Empty,
			FeedbackTypeID = 1,
			FeedbackUrgencyTypeID = 1
		};

		public FeedbackPost GetInitialFeedbackPost() => FeedbackThreadRepo.GetInitialFeedbackPost(this.FeedbackThreadID);

		public User GetThreadUser()
		{
			var post = this.GetInitialFeedbackPost();

			if (post == null)
				return new User();

			return post.GetUser();
		}

		public void CloseThread(User userClosing)
		{
			if (this.IsClosed)
				return;

			if (this.GetThreadUser().UserID != userClosing.UserID && !userClosing.HasRole(UserRoleType.FeedbackAdmin))
				throw new FeedbackException("You do not have the required permissions to close this feedback thread. Action cancelled.");

			this.UserClosed = userClosing.UserID;
			this.DateClosed = DateTime.Now.ToCentralAfricanTime();

			this.Save();
		}

		public void Save(User userClosing)
		{
			if (this.GetThreadUser().UserID != userClosing.UserID && !userClosing.HasRole(UserRoleType.FeedbackAdmin))
				throw new FeedbackException("You do not have the required permissions to edit/update this feedback thread. Action cancelled.");

			this.Save();
		}

		private void Save() => FeedbackThreadRepo.Save(this);

		public static List<FeedbackThread> GetAll(User userRetrieving, bool isOpen = true)
		{
			if (userRetrieving.HasRole(UserRoleType.FeedbackAdmin))
				return GetAll(isOpen);

			return GetAllForUser(userRetrieving, isOpen);
		}

		internal static List<FeedbackThread> GetAll(bool isOpen = true) => FeedbackThreadRepo.GetAll(isOpen)
					.OrderBy(t => t.GetInitialFeedbackPost().DateAdded)
					.ToList();

		internal static List<FeedbackThread> GetAllForUser(User userRetrieving, bool isOpen) =>
			/*
				if (!userRetrieving.HasRole(UserRoleType.FeedbackAdmin))
				isOpen = true;

				if (!isOpen)
				return new List<FeedbackThread>();
			*/

			FeedbackThreadRepo.GetAllForUser(userRetrieving.UserID, isOpen)
					.OrderBy(t => t.GetInitialFeedbackPost().DateAdded)
					.ToList();

		#endregion FEEDBACK THREAD

		#region FEEDBACK POST

		public FeedbackPost AddPost(User userAdding, string feedbackPostText)
		{
			if (this.GetThreadUser().UserID != userAdding.UserID && !userAdding.HasRole(UserRoleType.FeedbackAdmin))
				throw new FeedbackException("You do not have the required permissions to submit a post to this feedback thread.");

			if (this.IsClosed)
				throw new FeedbackException("This thread has been closed previously and therefore, no posts can be added to this thread.");

			var post = new FeedbackPost()
			{
				FeedbackThreadID = this.FeedbackThreadID,
				UserID = userAdding.UserID,
				PostText = feedbackPostText,
				DateAdded = DateTime.Now.ToCentralAfricanTime()
			};

			post.Save();

			return post;
		}

		public List<FeedbackPost> GetPosts(User userRequesting) => userRequesting.UserID != this.GetThreadUser().UserID && !userRequesting.HasRole(UserRoleType.FeedbackAdmin)
				? new List<FeedbackPost>()
				: FeedbackThreadRepo.GetPosts(this.FeedbackThreadID)
						.OrderBy(p => p.DateAdded)
						.ToList();

		#endregion FEEDBACK POST

		#region USERS

		public List<User> GetThreadUsers() => FeedbackThreadRepo.GetThreadUsers(this.FeedbackThreadID)
					.OrderBy(u => u.FirstName)
					.ThenBy(u => u.LastName)
					.ToList();

		#endregion USERS
	}
}