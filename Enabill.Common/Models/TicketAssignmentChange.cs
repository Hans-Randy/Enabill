using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("TicketAssignmentChanges")]
	public class TicketAssignmentChange
	{
		#region PROPERTIES

		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int TicketAssignmentChangeID { get; private set; }

		public int FromUser { get; set; }

		[Required]
		public int TicketID { get; set; }

		[Required]
		public int ToUser { get; set; }

		[Required]
		public int UserID { get; set; }

		[Required]
		public string Remark { get; set; }

		[Required]
		public DateTime DateCreated { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public void Save() => TicketAssignmentChangeRepo.Save(this);

		#endregion FUNCTIONS
	}
}