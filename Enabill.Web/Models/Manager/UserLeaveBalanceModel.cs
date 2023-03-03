using System;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class UserLeaveBalanceModel
	{
		#region INITIALIZATION

		public UserLeaveBalanceModel(User user, LeaveBalance leaveBalance)
		{
			this.UserID = user.UserID;
			this.UserFullName = user.FullName;
			this.LeaveBalance = leaveBalance.Balance;
			this.LeaveBalanceDate = leaveBalance.BalanceDate;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int UserID { get; private set; }

		public double LeaveBalance { get; private set; }

		public string UserFullName { get; private set; }

		public DateTime LeaveBalanceDate { get; private set; }

		#endregion PROPERTIES
	}
}