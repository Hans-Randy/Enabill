using System;

namespace Enabill.Web.Models.Holiday
{
	public class HolidayLiteModel
	{
		#region INITIALIZATION
		
		public HolidayLiteModel(Enabill.Models.Holiday e, int year)
		{
			this.Id = e.HolidayID;
			this.HolidayName = e.HolidayName;
			this.Date = e.Date.AddYears(year - e.Date.Year); //.ToString("MMM dd ddd");
			this.IsFixedDate = e.IsFixedDate;
			this.IsRepeated = e.IsRepeated;
		}		

		#endregion INITIALIZATION

		#region PROPERTIES

		public int Id { get; set; }
		
		public bool IsFixedDate { get; set; }

		public bool IsRepeated { get; set; }
		
		public string HolidayName { get; set; }
		
		public DateTime Date { get; set; }

		#endregion PROPERTIES
	}
}
	