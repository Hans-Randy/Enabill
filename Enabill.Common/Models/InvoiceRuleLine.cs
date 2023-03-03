using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("InvoiceRuleLines")]
	public class InvoiceRuleLine
	{
		#region PROPERTIES

		[Key]
		public int InvoiceRuleLineID { get; set; }

		[Required]
		public int InvoiceRuleID { get; set; }

		[Required]
		public int Period { get; set; }

		public int? InvoiceID { get; set; }

		[Required]
		public double AccrualAmount { get; set; }

		[Required]
		public double CustomerAmount { get; set; }

		[MaxLength(500)]
		public string DefaultDescription { get; set; }

		#endregion PROPERTIES

		#region INITIALIZATION

		[NotMapped]
		public bool CanEdit =>
				//if an invoice has been created from this invoice rule line, an invoice line will be created and this
				//model will have the id of the inv line created... if null, no invoice line created yet.
				!this.InvoiceID.HasValue;

		#endregion INITIALIZATION
	}
}