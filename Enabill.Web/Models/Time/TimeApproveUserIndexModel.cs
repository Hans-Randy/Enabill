using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class TimeApproveUserIndexModel
	{
		#region INITIALIZATION

		public TimeApproveUserIndexModel(DateTime dateFrom, DateTime dateTo, int managerID)
		{
			this.DateFrom = InputHistory.GetDateTime(HistoryItemType.ApprovalDateFrom, DateTime.Today.ToFirstDayOfMonth()).Value;
			this.DateTo = InputHistory.GetDateTime(HistoryItemType.ApprovalDateTo, DateTime.Today.ToLastDayOfMonth()).Value;

			if (managerID == 0)
				this.TimeUsers = UserRepo.GetAll().OrderBy(u => u.FirstName).ThenBy(ul => ul.LastName).ToList();
			else
				this.TimeUsers = UserRepo.GetStaffForManager(managerID).OrderBy(u => u.FirstName).ThenBy(ul => ul.LastName).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int TimeApprovalUser { get; private set; }

		public DateTime DateFrom { get; private set; }
		public DateTime DateTo { get; private set; }

		public List<User> TimeUsers { get; private set; }

		#endregion PROPERTIES
	}
}