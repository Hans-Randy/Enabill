using System.Collections.Generic;

namespace Enabill.Web.Models
{
	public class DailyProcessExceptionModel
	{
		#region INITIALIZATION

		public DailyProcessExceptionModel(List<string> processExceptions)
		{
			this.ProcessExceptions = processExceptions;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<string> ProcessExceptions { get; set; }

		#endregion PROPERTIES
	}
}