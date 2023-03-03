using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwUserTimeSplitTotalHoursPerPeriod")]
	public class UserTimeSplitTotalHoursPerPeriod
	{
		#region PROPERTIES

		[Key]
		public string UKey { get; set; }

		public int UserID { get; set; }

		public double TotalHours { get; set; }

		public string DepartmentName { get; set; }
		public string Period { get; set; }
		public string UserName { get; set; }

		#endregion PROPERTIES
	}
}