using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("ExpenseApprovals")]
	public class ExpenseApproval
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ExpenseApprovalID { get; private set; }

		[Required]
		public int ExpenseID { get; internal set; }

		[Required]
		public int UserID { get; internal set; }

		//This is required for audit trails
		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[Required, MinLength(3), MaxLength(128)]
		public string UserManaged { get; internal set; }

		[Required]
		public DateTime DateManaged { get; internal set; }

		// MonthDate will be saved as the first day of that month, so the timesheet for aug 2011 will be the record with date '2011-08-01'
		[Required]
		public DateTime MonthDate { get; internal set; }

		#endregion PROPERTIES
	}
}