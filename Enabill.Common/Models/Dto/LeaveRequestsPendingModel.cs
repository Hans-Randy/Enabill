using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models.Dto
{
	public class LeaveRequestsPendingModel
	{
		#region INITIALIZATION

		public LeaveRequestsPendingModel(DateTime dateFrom)
		{
			this.Leaves = LeaveRepo.GetLeaveRequestsPending(dateFrom).ToList();
		}

		public LeaveRequestsPendingModel(DateTime dateFrom, int managerID)
		{
			this.Leaves = LeaveRepo.GetLeaveRequestsPending(dateFrom, managerID).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<UserLeave> Leaves { get; set; }

		#endregion PROPERTIES
	}
}