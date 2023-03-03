using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("BalanceAuditTrails")]
	public class BalanceAuditTrail
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int BalanceAuditTrailID { get; set; }

		[Required]
		public int BalanceChangeTypeID { get; set; }

		[Required]
		public int BalanceTypeID { get; set; }

		[Required]
		public int ChangedBy { get; set; }

		[Required]
		public int UserID { get; set; }

		[Required]
		public double BalanceAfter { get; set; }

		[Required]
		public double BalanceBefore { get; set; }

		[Required]
		public double HoursChanged { get; set; }

		[Required]
		public string ChangeSummary { get; set; }

		[Required]
		public DateTime BalanceDate { get; set; }

		[Required]
		public DateTime DateChanged { get; set; }

		#endregion PROPERTIES

		#region INITIALIZATION

		[NotMapped]
		public string UserFullName => Repos.UserRepo.GetByID(this.UserID).FullName;

		[NotMapped]
		public string ChangedByFullName => Repos.UserRepo.GetByID(this.ChangedBy).FullName;

		#endregion INITIALIZATION
	}
}