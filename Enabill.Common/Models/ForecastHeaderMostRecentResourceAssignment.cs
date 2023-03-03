using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwForecastHeaderMostRecentResourceAssignments")]
	public class ForecastHeaderMostRecentResourceAssignment
	{
		#region PROPERTIES

		[Key]
		public int ForecastResourceAssignmentID { get; set; }

		public int ForecastDetailID { get; set; }
		public int UserID { get; set; }

		public double PercentageAllocation { get; set; }

		public string Resource { get; set; }

		#endregion PROPERTIES
	}
}