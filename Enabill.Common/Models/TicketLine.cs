using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("TicketLines")]
	public class TicketLine
	{
		#region PROPERTIES

		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int TicketLineID { get; private set; }

		[Required]
		public int TicketID { get; set; }

		[Required]
		public int TicketLineSourceTypeID { get; set; }

		[Required]
		public int UserCreated { get; set; }

		[Required, MaxLength]
		public string Body { get; set; }

		[Required, MaxLength(128)]
		public string FromAddress { get; set; }

		[Required, MaxLength(5000)]
		public string ToAddress { get; set; }

		[Required]
		public DateTime DateCreated { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public void Save() => TicketLineRepo.Save(this);

		public User GetUserCreated() => FeedbackPostRepo.GetUser(this.UserCreated);

		#endregion FUNCTIONS

		#region TICKETLINE ATTACHMENT

		public TicketLineAttachment AddAttachment(string name, byte[] data)
		{
			var attachment = new TicketLineAttachment()
			{
				AttachmentName = name,
				Attachment = data,
				TicketLineID = this.TicketLineID
			};

			attachment.Save();

			return attachment;
		}

		public List<TicketLineAttachment> GetAttachments() => TicketLineRepo.GetAttachments(this.TicketLineID).ToList();

		#endregion TICKETLINE ATTACHMENT
	}
}