using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("TicketLineAttachments")]
	public class TicketLineAttachment
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int TicketLineAttachmentID { get; private set; }

		[Required]
		public int TicketLineID { get; internal set; }

		public byte[] Attachment { get; internal set; }

		[MaxLength(512)]
		public string AttachmentName { get; internal set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		internal void Save() => TicketLineAttachmentRepo.Save(this);

		#endregion FUNCTIONS
	}
}