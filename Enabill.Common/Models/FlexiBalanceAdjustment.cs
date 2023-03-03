using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("FlexiBalanceAdjustments")]
	public class FlexiBalanceAdjustment
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int FlexiBalanceAdjustmentID { get; set; }

		[Required]
		public int UserID { get; internal set; }

		[Required]
		public double Adjustment { get; set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; set; }

		[Required]
		public string Remark { get; set; }

		[Required, MaxLength(128)]
		public string UserAdjusted { get; set; }

		[Required]
		public DateTime DateAdjusted { get; set; }

		[Required]
		public DateTime ImplementationDate { get; set; }

		#endregion PROPERTIES
	}
}