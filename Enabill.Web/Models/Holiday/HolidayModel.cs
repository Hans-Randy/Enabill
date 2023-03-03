using System;
using System.ComponentModel.DataAnnotations;

namespace Enabill.Web.Models.Holiday
{
	public class HolidayModel
	{
		#region PROPERTIES

		public int HolidayId { get; set; }

		[Required]
		public bool IsFixed { get; set; }

		[Required]
		public bool IsRepeated { get; set; }

		[Required]
		public DateTime Date { get; set; }

		[Required]
		public string Name { get; set; }

		#endregion PROPERTIES
	}
}
	