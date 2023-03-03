using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("InvoiceRuleContacts")]
	public class InvoiceRuleContact
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int InvoiceRuleContactID { get; internal set; }

		[Required]
		public int ContactID { get; internal set; }

		[Required]
		public int InvoiceRuleID { get; internal set; }

		public virtual Contact Contact { get; set; }

		#endregion PROPERTIES
	}
}