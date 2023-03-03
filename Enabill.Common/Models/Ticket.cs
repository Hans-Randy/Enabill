using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("Tickets")]
	public class Ticket
	{
		#region PROPERTIES

		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int TicketID { get; private set; }

		public bool IsDeleted { get; set; }

		[Required]
		public int ClientID { get; set; }

		public int ProjectID { get; set; }
		public int Effort { get; set; }
		public int Priority { get; set; }

		[Required]
		public int TicketStatus { get; set; }

		[Required]
		[EnumDataType(typeof(TicketTypeEnum))]
		public int TicketType { get; set; }

		[Required]
		public int UserCreated { get; set; }

		public int? UserModified { get; set; }
		public int? UserAssigned { get; set; }

		public double? TimeSpent { get; set; }

		[Required, MaxLength(128)]
		public string FromAddress { get; set; }

		[MaxLength(512)]
		public string TicketDetails { get; set; }

		[Required, MaxLength(20)]
		public string TicketReference { get; set; }

		[Required, MaxLength(512)]
		public string TicketSubject { get; set; }

		[Required]
		public DateTime DateCreated { get; set; }

		public DateTime? DateModified { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public void Save()
		{
			if (this.TicketID <= 0 && string.IsNullOrEmpty(this.TicketReference))
				this.TicketReference = TicketReferenceNumberRepo.GetNewReferenceNumber(this.ClientID, this.TicketSubject);
			TicketRepo.Save(this);
		}

		public TicketLine AddTicketLine(TicketLineSourceType ticketLineSourceType, string fromAddress, string toAddress, string body, User userCreated)
		{
			var ticketLine = new TicketLine
			{
				TicketID = this.TicketID,
				TicketLineSourceTypeID = (int)ticketLineSourceType
			};
			if (!string.IsNullOrEmpty(fromAddress))
				ticketLine.FromAddress = fromAddress;
			if (!string.IsNullOrEmpty(toAddress))
				ticketLine.ToAddress = toAddress;
			else
				ticketLine.ToAddress = fromAddress;
			ticketLine.Body = body;
			ticketLine.UserCreated = userCreated.UserID;
			ticketLine.DateCreated = DateTime.Now;
			ticketLine.Save();

			return ticketLine;
		}

		public TicketLog AddTicketLog(int ticketID, TicketLogType ticketLogType, int ticketLineID)
		{
			var ticketLog = new TicketLog
			{
				TicketID = ticketID,
				TicketLogTypeID = (int)ticketLogType,
				Source = ticketLineID,
				DateCreated = DateTime.Now
			};
			ticketLog.Save();

			return ticketLog;
		}

		#endregion FUNCTIONS
	}
}