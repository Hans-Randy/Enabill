using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class FlexiBalanceReportModel
	{
		#region INITIALIZATION

		public FlexiBalanceReportModel(DateTime selectedMonth)
		{
			this.SelectedMonth = selectedMonth;
			this.FlexiTimeReport = this.LoadFlexiTimeReportModel(selectedMonth);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public DateTime SelectedMonth { get; set; }

		public List<FlexiTimeReportModel> FlexiTimeReport { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<FlexiTimeReportModel> LoadFlexiTimeReportModel(DateTime selectedMonth)
		{
			var model = new List<FlexiTimeReportModel>();
			var flexiTimeUsers = UserRepo.GetAllActive().Where(u => u.IsFlexiTimeUser && u.IsActive && u.EmployStartDate.ToFirstDayOfMonth() <= selectedMonth.ToFirstDayOfMonth()).ToList();

			foreach (var user in flexiTimeUsers)
			{
				var previousMonthFlexiStatement = new FlexiTimeMonthModel(user, selectedMonth.AddMonths(-1).ToFirstDayOfMonth());
				var selectedMonthFlexiStatement = new FlexiTimeMonthModel(user, selectedMonth.ToFirstDayOfMonth());
				var startDate = user.EmployStartDate > EnabillSettings.SiteStartDate ? user.EmployStartDate : EnabillSettings.SiteStartDate;

				var flexiTimeReportModel = new FlexiTimeReportModel()
				{
					UserID = user.UserID,
					FullName = selectedMonthFlexiStatement.User.FullName,
					EmploymentType = ((EmploymentTypeEnum)selectedMonthFlexiStatement.User.EmploymentTypeID).ToString(),
					Manager = selectedMonthFlexiStatement.User.Manager.FullName,
					SelectedMonth = selectedMonth.ToFirstDayOfMonth(),
					SelectedMonthsOpeningBalance = selectedMonthFlexiStatement.SelectedMonthsFlexiStatement.OpeningBalance,
					SelectedMonthsDelta = selectedMonthFlexiStatement.SelectedMonthsFlexiStatement.DeltaHours,
					SelectedMonthsClosingBalance = selectedMonthFlexiStatement.SelectedMonthsFlexiStatement.OpeningBalance + selectedMonthFlexiStatement.SelectedMonthsFlexiStatement.DeltaHours,
					PreviousMonth = selectedMonth.AddMonths(-1).ToFirstDayOfMonth(),
					PreviousMonthsClosingBalance = user.EmployStartDate.ToFirstDayOfMonth() == selectedMonth.ToFirstDayOfMonth() ? previousMonthFlexiStatement.SelectedMonthsFlexiStatement.OpeningBalance : previousMonthFlexiStatement.SelectedMonthsFlexiStatement.OpeningBalance + previousMonthFlexiStatement.SelectedMonthsFlexiStatement.DeltaHours,
					PendingApprovalsToDate = UserRepo.GetTotalLeaveHoursForUserForDates(user.UserID, ApprovalStatusType.Pending, startDate, selectedMonth.AddMonths(-1).ToLastDayOfMonth()),
					WorkAllocationExceptionsToDate = UserRepo.GetTimeCaptureExceptions(user.UserID, startDate, selectedMonth.AddMonths(-1).ToLastDayOfMonth()).ToList().Sum(wae => wae.AllocationDifference),
					LastWorkSessionDate = user.GetLastWorkSession(),
					BalanceAuditTrailChanges = BalanceAuditTrailRepo.GetAll((int)BalanceTypeEnum.Flexi, user.UserID).Count()
				};

				model.Add(flexiTimeReportModel);
			}

			model.ToList();

			return model;
		}

		#endregion FUNCTIONS
	}
}