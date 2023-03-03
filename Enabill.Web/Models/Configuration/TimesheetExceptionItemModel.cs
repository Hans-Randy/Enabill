using System;
using Enabill.Models.Dto;

namespace Enabill.Web.Models
{
	public class TimesheetExceptionItemModel
	{
		#region INITIALIZATION

		public TimesheetExceptionItemModel(WorkAllocationExceptionModel ex)
		{
			this.Username = ex.UserName;
			this.WorkDay = ex.WorkDate;
			this.Message = ex.ExceptionDetail;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public string Message { get; private set; }
		public string Username { get; private set; }

		public DateTime WorkDay { get; private set; }

		#endregion PROPERTIES
	}
}