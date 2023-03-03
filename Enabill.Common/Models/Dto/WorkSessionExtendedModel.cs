using System;
using System.Collections.Generic;
using System.Linq;

namespace Enabill.Models.Dto
{
	public class WorkSessionExtendedModel
	{
		#region INITIALIZATION

		public WorkSessionExtendedModel()
		{
			this.WorkSessions = new List<WorkSession>();
			this.WorkAllocations = new List<WorkAllocation>();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public double TotalHours => this.WorkAllocations.Sum(wa => wa.HoursBilled ?? wa.HoursWorked);

		public DateTime DayWorked { get; internal set; }

		public User User { get; internal set; }

		public List<WorkAllocation> WorkAllocations { get; internal set; }
		public List<WorkSession> WorkSessions { get; internal set; }

		#endregion PROPERTIES
	}
}