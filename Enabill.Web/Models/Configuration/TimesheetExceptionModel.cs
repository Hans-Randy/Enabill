using System.Collections.Generic;
using Enabill.Models.Dto;

namespace Enabill.Web.Models
{
	public class TimesheetExceptionModel
	{
		#region INITIALIZATION

		public TimesheetExceptionModel(List<WorkAllocationExceptionModel> exceptions)
		{
			var model = new List<TimesheetExceptionItemModel>();
			foreach (var waE in exceptions)
			{
				model.Add(new TimesheetExceptionItemModel(waE));
			}

			this.Exceptions = model;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<TimesheetExceptionItemModel> Exceptions { get; private set; }

		#endregion PROPERTIES
	}
}