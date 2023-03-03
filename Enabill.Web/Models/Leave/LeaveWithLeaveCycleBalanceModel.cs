using System.Collections.Generic;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class LeaveWithLeaveCycleBalanceModel
	{
		#region INITIALIZATION

		public LeaveWithLeaveCycleBalanceModel(LeaveTypeEnum leaveType)
		{
			this._users = UserRepo.GetAllActive();
			this._leaveType = leaveType;
			this.GetLeaveWithLeaveCycleBalances();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		private LeaveTypeEnum _leaveType;

		private IEnumerable<User> _users;

		private IList<LeaveWithLeaveCycleBalance> _leavesWithLeaveCycleBalances;

		public IList<LeaveWithLeaveCycleBalance> LeavesWithLeaveCycleBalances => this._leavesWithLeaveCycleBalances;

		#endregion PROPERTIES

		#region FUNCTIONS

		private void GetLeaveWithLeaveCycleBalances()
		{
			this._leavesWithLeaveCycleBalances = new List<LeaveWithLeaveCycleBalance>();
			foreach (var user in this._users)
			{
				var leaveCycleBalance = LeaveCycleBalanceRepo.GetLastLeaveCycleBalanceForUser(user.UserID, this._leaveType);

				if (leaveCycleBalance != null)
				{
					var leaveWithLeaveCycleBalance = new LeaveWithLeaveCycleBalance();
					var approvalStatus = ApprovalStatusType.Approved;
					leaveWithLeaveCycleBalance.LeaveCycleBalance = leaveCycleBalance;
					leaveWithLeaveCycleBalance.Leaves = user.GetLeave(this._leaveType, approvalStatus, leaveCycleBalance.StartDate, leaveCycleBalance.EndDate);
					leaveWithLeaveCycleBalance.User = user;
					leaveWithLeaveCycleBalance.LeaveType = this._leaveType;
					this._leavesWithLeaveCycleBalances.Add(leaveWithLeaveCycleBalance);
				}
			}
		}

		#endregion FUNCTIONS
	}
}