using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models.Dto
{
	public class UserLeaveGeneralReportModel
	{
		#region INITIALIZATION

		public UserLeaveGeneralReportModel(DateTime dateFrom, DateTime dateTo, int employeeID = 0, int employmentTypeID = 0, int managerID = 0, int leaveTypeID = 0, bool isPartial = false, int approvalStatusID = 0)
		{
			this.UserLeaveGeneralReport = this.LoadUserLeaveReportModel(dateFrom, dateTo, employeeID, employmentTypeID, managerID, leaveTypeID, approvalStatusID);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public DateTime LeaveGeneralReportDateFrom { get; private set; }
		public DateTime LeaveGeneralReportDateTo { get; private set; }

		public List<UserLeaveGeneralModel> UserLeaveGeneralReport { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<UserLeaveGeneralModel> LoadUserLeaveReportModel(DateTime dateFrom, DateTime dateTo, int employeeID = 0, int employmentTypeID = 0, int managerID = 0, int leaveTypeID = 0, int approvalStatusID = 0) => UserLeaveGeneralRepo.GetGeneralLeave(dateFrom, dateTo, employeeID, employmentTypeID, managerID, leaveTypeID, approvalStatusID).ToList();

		#endregion FUNCTIONS
	}
}