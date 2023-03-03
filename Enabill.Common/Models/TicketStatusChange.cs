using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("TicketStatusChanges")]
	public class TicketStatusChange
	{
		#region PROPERTIES

		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int TicketStatusChangeID { get; private set; }

		public int FromStatus { get; set; }

		[Required]
		public int TicketID { get; set; }

		[Required]
		public int ToStatus { get; set; }

		[Required]
		public int UserID { get; set; }

		[Required]
		public string Remark { get; set; }

		[Required]
		public DateTime DateCreated { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public void Save() => TicketStatusChangeRepo.Save(this);

		#endregion FUNCTIONS
	}
}