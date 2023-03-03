using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("InvoiceCredits")]
	public class InvoiceCredit
	{
		#region PROPERTIES

		//TODO Add Audit Trigger
		[Key]
		public int InvoiceCreditID { get; internal set; }

		[Required]
		public int InvoiceID { get; set; }

		public int? WorkAllocationID { get; set; }

		[Required]
		public double CreditAmount { get; set; }

		[MaxLength(128)]
		public string CreatedBy { get; internal set; }

		[Required, MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		#endregion PROPERTIES

		#region INITIALIZATION

		[NotMapped]
		public bool CanEdit
		{
			get
			{
				var inv = this.GetInvoice();

				return inv.IsOpen || inv.IsInProgress;
			}
		}

		[NotMapped]
		public bool CanDelete => this.CanEdit;

		#endregion INITIALIZATION

		#region WORK ALLOCATIONS

		private WorkAllocation GetWorkAllocation()
		{
			if (!this.WorkAllocationID.HasValue || this.WorkAllocationID.Value <= 0)
				return null;

			return WorkAllocationRepo.GetByID(this.WorkAllocationID.Value);
		}

		#endregion WORK ALLOCATIONS

		#region INVOICES

		private Invoice GetInvoice() => InvoiceRepo.GetByID(this.InvoiceID);

		#endregion INVOICES
	}
}