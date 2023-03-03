using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("TicketReferenceNumbers")]
	public class TicketReferenceNumber
	{
		#region PROPERTIES

		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int TicketReferenceNumberID { get; private set; }

		[Required, MaxLength(512)]
		public string TicketSubject { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public void Save() => TicketReferenceNumberRepo.Save(this);

		#endregion FUNCTIONS
	}
}