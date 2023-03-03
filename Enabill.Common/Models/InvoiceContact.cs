using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("InvoiceContacts")]
	public class InvoiceContact
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int InvoiceContactID { get; internal set; }

		[Required]
		public int ContactID { get; internal set; }

		[Required]
		public int InvoiceID { get; internal set; }

		public virtual Contact Contact { get; internal set; }

		#endregion PROPERTIES
	}
}