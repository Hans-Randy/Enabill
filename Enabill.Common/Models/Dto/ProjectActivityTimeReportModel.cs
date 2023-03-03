using System;

namespace Enabill.Models.Dto
{
	public class ProjectActivityTimeReportModel
	{
		#region PROPERTIES

		public int Period { get; set; }

		public double HoursQuoted { get; set; }
		public double HoursWorked { get; set; }

		public string Activity { get; set; }
		public string Client { get; set; }
		public string FullName { get; set; }
		public string ProjectManager { get; set; }
		public string Project { get; set; }
		public string Remark { get; set; }
		public string TicketReference { get; set; }
		public string UserName { get; set; }

		public DateTime DayWorked { get; set; }

		#endregion PROPERTIES
	}
}