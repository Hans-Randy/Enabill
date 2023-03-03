using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("TicketLogs")]
	public class TicketLog
	{
		#region PROPERTIES

		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int TicketLogID { get; private set; }

		[Required]
		public int Source { get; set; }

		[Required]
		public int TicketLogTypeID { get; set; }

		[Required]
		public int TicketID { get; set; }

		[Required]
		public DateTime DateCreated { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public void Save() => TicketLogRepo.Save(this);

		#endregion FUNCTIONS
	}
}