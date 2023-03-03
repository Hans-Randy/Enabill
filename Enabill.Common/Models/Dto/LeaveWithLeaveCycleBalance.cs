using System.Collections.Generic;

namespace Enabill.Models.Dto
{
	public class LeaveWithLeaveCycleBalance
	{
		#region PROPERTIES

		public LeaveTypeEnum LeaveType { get; set; }

		public LeaveCycleBalance LeaveCycleBalance { get; set; }
		public User User { get; set; }

		public IList<Leave> Leaves { get; set; }

		#endregion PROPERTIES
	}
}