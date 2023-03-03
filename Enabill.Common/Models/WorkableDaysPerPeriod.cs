using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwWorkableDaysPerPeriod")]
	public class WorkableDaysPerPeriod
	{
		#region PROPERTIES

		[Key]
		public int Period { get; set; }

		public int NumberOfDays { get; set; }

		#endregion PROPERTIES
	}
}