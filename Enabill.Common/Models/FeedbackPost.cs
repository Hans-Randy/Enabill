using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("FeedbackPosts")]
	public class FeedbackPost
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int FeedbackPostID { get; private set; }

		[Required]
		public int FeedbackThreadID { get; internal set; }

		[Required]
		public int UserID { get; internal set; }

		[Required, MaxLength]
		public string PostText { get; internal set; }

		[Required]
		public DateTime DateAdded { get; internal set; }

		#endregion PROPERTIES

		#region INITIALIZATION

		public bool IsFirstPostInThread => this.FeedbackPostID == FeedbackThreadRepo.GetInitialFeedbackPost(this.FeedbackThreadID).FeedbackPostID;

		#endregion INITIALIZATION

		#region FEEDBACK POST

		public FeedbackThread GetFeedbackThread() => FeedbackPostRepo.GetFeedbackThread(this.FeedbackThreadID);

		internal void Save() => FeedbackPostRepo.Save(this);

		#endregion FEEDBACK POST

		#region USER

		public User GetUser() => FeedbackPostRepo.GetUser(this.UserID);

		#endregion USER
	}
}