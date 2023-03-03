using System;
using System.Collections.Generic;

namespace Enabill.Models.Dto
{
	public class LogItem
	{
		#region PROPERTIES

		public string LogSource { get; set; }

		public DateTime LogDate { get; set; }

		public LogType LogType { get; set; }

		public IList<LogMessage> LogMessages { get; set; }

		#endregion PROPERTIES
	}

	public enum LogType
	{
		LeaveBalance = 1,
		UserDeactivation = 2,
		LeaveCycleBalance = 3,
		TimeSheet = 4
	}

	public class LeaveBalanceLog
	{
		#region PROPERTIES

		public int LeaveID { get; set; }
		public int UserID { get; set; }

		public string UserName { get; set; }

		#endregion PROPERTIES
	}

	public class LeaveCycleBalanceLog
	{
		#region PROPERTIES

		public string UserName { get; set; }

		#endregion PROPERTIES
	}

	public class LogMessage
	{
		#region PROPERTIES

		public int IndentLevel { get; set; }

		public string LogMessageContent { get; set; }

		#endregion PROPERTIES
	}
}