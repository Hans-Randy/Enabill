using System.Collections.Generic;

namespace Enabill.Web.Models.Holiday
{
	public class HolidayViewModel
	{
		#region PROPERTIES
		
		public bool IsWorkDaySaved { get; set; }
		
		public int SelectedYear { get; set; }		

		public HolidayModel Holiday { get; set; }
		
		public List<HolidayLiteModel> HolidayList { get; set; }
		public List<string> CalendarHolidays { get; set; }
		public List<string> NonWorkDays { get; set; }
	
		#endregion PROPERTIES
	}
}