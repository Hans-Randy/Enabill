using System;

namespace Enabill.Models.Dto
{
	public class LogEntry
	{
		#region PROPERTIES

		public int Id { get; set; }

		public string Application { get; set; }
		public string Host { get; set; }
		public string Level { get; set; }
		public string Logger { get; set; }
		public string Message { get; set; }

		public DateTime TimeStamp { get; set; }

		#endregion PROPERTIES
	}
}