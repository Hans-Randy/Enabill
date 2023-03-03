using System.Collections.Generic;
using System.Linq;

namespace Enabill.Models.Dto
{
	public class TimesheetTablePrintModel
	{
		#region INITIALIZATION

		public TimesheetTablePrintModel(List<WorkSessionExtendedModel> sessions)
		{
			this.WorkItems = sessions;
			this.TotalWorkHours = sessions.Sum(s => s.TotalHours);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public double TotalWorkHours { get; private set; }

		public List<WorkSessionExtendedModel> WorkItems { get; private set; }

		#endregion PROPERTIES
	}
}