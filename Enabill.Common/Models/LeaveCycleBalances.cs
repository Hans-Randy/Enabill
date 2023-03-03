using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("LeaveCycleBalances")]
	public class LeaveCycleBalance
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int LeaveCycleBalanceID { get; set; }

		[Required]
		public int Active { get; set; }

		[Required, EnumDataType(typeof(LeaveTypeEnum))]
		public int LeaveTypeID { get; set; }

		[Required]
		public int UserID { get; set; }

		[Required]
		public double ClosingBalance { get; set; }

		public double ManualAdjustment { get; set; }

		[Required]
		public double OpeningBalance { get; set; }

		[Required]
		public double Taken { get; set; }

		[Required]
		public DateTime EndDate { get; set; }

		[Required]
		public DateTime LastUpdatedDate { get; set; }

		[Required]
		public DateTime StartDate { get; set; }

		#endregion PROPERTIES
	}
}