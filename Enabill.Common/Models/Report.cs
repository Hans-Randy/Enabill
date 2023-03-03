using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("Reports")]
	public class Report
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ReportID { get; private set; }

		public string ReportName { get; private set; }

		#endregion PROPERTIES
	}
}