using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("InvoiceRuleActivities")]
	public class InvoiceRuleActivity
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int InvoiceRuleActivityID { get; internal set; }

		[Required]
		public int ActivityID { get; internal set; }

		[Required]
		public int InvoiceRuleID { get; internal set; }

		public virtual Activity Activity { get; set; }

		#endregion PROPERTIES
	}
}