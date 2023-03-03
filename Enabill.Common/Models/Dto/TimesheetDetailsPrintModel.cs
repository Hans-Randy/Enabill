using System.Collections.Generic;
using System.Linq;

namespace Enabill.Models.Dto
{
	public class TimesheetDetailsPrintModel
	{
		public TimesheetDetailsPrintModel(List<WorkAllocationExtendedModel> allocations)
		{
			this.WorkItems = allocations;
			this.TotalWorkHours = allocations.Sum(a => a.WorkAllocation.HoursBilled ?? a.WorkAllocation.HoursWorked);
		}

		#region PROPERTIES

		public double TotalWorkHours { get; private set; }

		public List<WorkAllocationExtendedModel> WorkItems { get; private set; }

		#endregion PROPERTIES
	}
}