using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("ForecastResourceAssignments")]
	public class ForecastResourceAssignment
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ForecastResourceAssignmentID { get; set; }

		[Required]
		public int ForecastDetailID { get; set; }

		public int UserID { get; set; }

		public double PercentageAllocation { get; set; }

		public string Resource { get; set; }

		#endregion PROPERTIES
	}
}