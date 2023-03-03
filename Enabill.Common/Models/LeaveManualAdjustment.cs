using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("LeaveManualAdjustments")]
	public class LeaveManualAdjustment
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int LeaveManualAdjustmentID { get; set; }

		[Required, EnumDataType(typeof(LeaveTypeEnum))]
		public int LeaveTypeID { get; set; }

		[Required]
		public int UserID { get; set; }

		[Required]
		public double ManualAdjustment { get; set; }

		[Required]
		public string Remark { get; set; }

		[Required]
		public DateTime EffectiveDate { get; set; }

		#endregion PROPERTIES
	}
}