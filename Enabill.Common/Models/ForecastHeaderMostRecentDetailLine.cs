using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwForecastHeaderMostRecentDetailLines")]
	public class ForecastHeaderMostRecentDetailLine
	{
		#region PROPERTIES

		[Key]
		public int ForecastDetailID { get; set; }

		public int ForecastHeaderID { get; set; }
		public int ModifiedByUserID { get; set; }
		public int Period { get; set; }

		public double Adjustment { get; set; }
		public double AllocationPercentage { get; set; }
		public double Amount { get; set; }
		public double HourlyRate { get; set; }

		public string Reference { get; set; }
		public string Remark { get; set; }

		public DateTime EntryDate { get; set; }

		#endregion PROPERTIES
	}
}