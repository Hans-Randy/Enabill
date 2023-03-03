using System;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class UserDetailModel
	{
		#region INITIALIZATION

		public UserDetailModel(User user)
		{
			this.RegionName = RegionRepo.GetByID(user.RegionID).RegionName;
			this.BillableIndicatorName = BillableIndicatorRepo.GetByID(user.BillableIndicatorID).BillableIndicatorName;
			this.DivisionName = DivisionRepo.GetByID(user.DivisionID).DivisionName;
			this.EmploymentTypeName = UserRepo.GetEmploymentType(user.EmploymentTypeID).EmploymentTypeName;
			this.LeaveBalance = this.PopulateLeaveBalance(user);
			this.HasPendingLeave = user.GetLeave(ApprovalStatusType.Pending, new DateTime(1900, 1, 1), new DateTime(2999, 1, 1)).Count > 0;

			this.User = user;
			this.LoadManager(user);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool HasPendingLeave { get; private set; }

		public string BillableIndicatorName { get; private set; }
		public string DivisionName { get; private set; }
		public string EmploymentTypeName { get; private set; }
		public string FlexiBalance { get; private set; }
		public string LeaveBalance { get; private set; }
		public string RegionName { get; private set; }

		public User Manager { get; private set; }
		public User User { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private void LoadManager(User user)
		{
			this.Manager = null;

			if (user.ManagerID != null)
			{
				this.Manager = UserRepo.GetByID(user.ManagerID.Value);
			}
		}

		public static string PopulateFlexiBalance(User user, bool includeToday) => user.CalculateFlexiBalanceOnDate(DateTime.Today, includeToday) + " Hours";

		private string PopulateLeaveBalance(User user)
		{
			double leaveBalance = user.CalculateCurrentLeaveBalance(LeaveTypeEnum.Annual);

			if (leaveBalance == 0.0009)
				return "No balance";

			return leaveBalance.ToString() + " Days";
		}

		#endregion FUNCTIONS
	}
}