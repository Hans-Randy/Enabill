using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("FeedbackAttachments")]
	public class FeedbackAttachment
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int FeedbackAttachmentID { get; private set; }

		[Required]
		public int FeedbackPostID { get; internal set; }

		public byte[] Attachment { get; internal set; }

		[MaxLength(512)]
		public string AttachmentName { get; internal set; }

		#endregion PROPERTIES

		#region FEEDBACK ATTACHMENT

		internal void Save() => FeedbackAttachmentRepo.Save(this);

		#endregion FEEDBACK ATTACHMENT
	}
}