using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("ForecastDetails")]
	public class ForecastDetail
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ForecastDetailID { get; set; }

		[Required]
		public int ForecastHeaderID { get; set; }

		[Required]
		public int ModifiedByUserID { get; set; }

		[Required]
		public int Period { get; set; }

		[Required]
		public double Amount { get; set; }

		public double Adjustment { get; set; }
		public double AllocationPercentage { get; set; }
		public double HourlyRate { get; set; }

		public string Reference { get; set; }
		public string Remark { get; set; }

		[Required]
		public DateTime EntryDate { get; set; }

		#endregion PROPERTIES
	}
}