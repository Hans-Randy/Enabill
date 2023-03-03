using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class LeaveBalanceRepo : BaseRepo
	{
		#region LEAVE BALANCE SPECIFIC

		internal static LeaveBalance GetLeaveBalancePriorToDate(int userID, LeaveTypeEnum leaveType, DateTime date) => (from lb in DB.LeaveBalances
																														join u in DB.Users on lb.UserID equals u.UserID
																														orderby lb.BalanceDate descending
																														where lb.UserID == userID && lb.LeaveType == (int)leaveType && lb.BalanceDate <= date && date >= u.EmployStartDate && date > EnabillSettings.SiteStartDate
																														select lb
								   ).FirstOrDefault();

		internal static LeaveBalance GetLeaveBalance(int userID, LeaveTypeEnum leaveType, DateTime date) => DB.LeaveBalances
					.SingleOrDefault(l => l.UserID == userID && (LeaveTypeEnum)l.LeaveType == leaveType && l.BalanceDate == date);

		internal static void Save(LeaveBalance leaveBalance)
		{
			if (leaveBalance.LeaveBalanceID == 0)
				DB.LeaveBalances.Add(leaveBalance);

			DB.SaveChanges();
		}

		internal static void Delete(LeaveBalance initialBalance)
		{
			if (initialBalance == null)
				return;

			DB.LeaveBalances.Remove(initialBalance);
			DB.SaveChanges();
		}

		public static IEnumerable<LeaveBalanceSummarisedReportModel> GetLeaveBalanceExtendedReportForDate(LeaveTypeEnum leaveType, DateTime date)
		{
			var startDate = date.ToFirstDayOfMonth();
			var endDate = date.ToLastDayOfMonth();
			var lastMonthForInactiveEmployees = DateTime.Today.AddMonths(-1);

			//Get all leave balances as at date for all users
			var allUserLeaveBalances = (from lb in DB.LeaveBalances
										join u in DB.Users on lb.UserID equals u.UserID
										where (u.IsActive || (u.EmployEndDate != null && u.EmployEndDate >= lastMonthForInactiveEmployees)) && lb.LeaveType == (int)leaveType && lb.BalanceDate == date
										select lb
										)
										.ToList();

			//Get the individual leave taken for the month
			var individualLeaveDays = (from lb in DB.IndividualLeaveDays
									   where lb.BalanceDate == date
									   && lb.WorkDate >= startDate && lb.WorkDate <= endDate
									   select (new LeaveBalanceExtendedReportModel()
									   {
										   UserID = lb.UserID,
										   FullName = lb.FullName,
										   BalanceDate = lb.BalanceDate,
										   OpeningBalance = lb.OpeningBalance,
										   LeaveTaken = lb.LeaveTaken,
										   LeaveCredited = lb.LeaveCredited,
										   ManualAdjustment = lb.ManualAdjustment,
										   LeavePeriodStartDate = lb.LeavePeriodStartDate,
										   LeavePeriodEndDate = lb.LeavePeriodEndDate,
										   WorkDate = lb.WorkDate,
										   ApprovalStatus = lb.ApprovalStatus,
										   LeaveTypeID = lb.LeaveTypeID,
										   LeaveTypeName = lb.LeaveTypeName,
										   NormalHours = lb.NormalHours,
										   NumberOfDays = lb.NumberOfDays,
										   HoursTaken = lb.HoursTaken,
										   Remark = lb.Remark,
										   Manager = lb.Manager
									   })
							).ToList();

			var leaveBalanceSummarisedReportModels = new List<LeaveBalanceSummarisedReportModel>();

			foreach (var user in allUserLeaveBalances)
			{
				var leaveBalanceSummarisedReportModel = new LeaveBalanceSummarisedReportModel();
				int employmentTypeID = UserRepo.GetByID(user.UserID).EmploymentTypeID;
				leaveBalanceSummarisedReportModel.FullName = UserRepo.GetByID(user.UserID).FullName;
				leaveBalanceSummarisedReportModel.PayrollRefNo = UserRepo.GetByID(user.UserID).PayrollRefNo;
				leaveBalanceSummarisedReportModel.EmploymentType = ((EmploymentTypeEnum)employmentTypeID).ToString();
				leaveBalanceSummarisedReportModel.LeaveType = ((LeaveTypeEnum)user.LeaveType).ToString();
				leaveBalanceSummarisedReportModel.DateFrom = startDate;
				leaveBalanceSummarisedReportModel.DateTo = endDate;
				leaveBalanceSummarisedReportModel.OpeningBalance = user.Balance;

				if (leaveType == LeaveTypeEnum.Sick || leaveType == LeaveTypeEnum.Compassionate)
				{
					leaveBalanceSummarisedReportModel.BalanceDate = user.BalanceDate;
					leaveBalanceSummarisedReportModel.LeaveTaken = user.LeaveTaken;
				}

				leaveBalanceSummarisedReportModel.LeaveCredited = user.LeaveCredited == 0 && user.LeaveType == (int)LeaveTypeEnum.Annual ? UserRepo.GetByID(user.UserID).GetLeaveBalanceCreditAmountProvisional(user.BalanceDate) : user.LeaveCredited;
				leaveBalanceSummarisedReportModel.ManualAdjustment = user.ManualAdjustment;
				leaveBalanceSummarisedReportModel.Approved = individualLeaveDays.Where(i => i.UserID == user.UserID && i.LeaveTypeID == (int)leaveType && i.ApprovalStatus == (int)ApprovalStatusType.Approved).DefaultIfEmpty().Sum(i => i?.NumberOfDays ?? 0.00);
				leaveBalanceSummarisedReportModel.Pending = individualLeaveDays.Where(i => i.UserID == user.UserID && i.LeaveTypeID == (int)leaveType && i.ApprovalStatus == (int)ApprovalStatusType.Pending).DefaultIfEmpty().Sum(i => i?.NumberOfDays ?? 0.00);
				leaveBalanceSummarisedReportModel.ClosingBalance = leaveBalanceSummarisedReportModel.OpeningBalance + leaveBalanceSummarisedReportModel.LeaveCredited + leaveBalanceSummarisedReportModel.ManualAdjustment - leaveBalanceSummarisedReportModel.Approved - leaveBalanceSummarisedReportModel.Pending;
				//The other leavetype totals below are for use on the Annual Leave Report only
				leaveBalanceSummarisedReportModel.Sick = individualLeaveDays.Where(i => i.UserID == user.UserID && i.LeaveTypeID == (int)LeaveTypeEnum.Sick && i.ApprovalStatus == (int)ApprovalStatusType.Approved).DefaultIfEmpty().Sum(i => i?.NumberOfDays ?? 0.00);
				leaveBalanceSummarisedReportModel.Compassionate = individualLeaveDays.Where(i => i.UserID == user.UserID && i.LeaveTypeID == (int)LeaveTypeEnum.Compassionate && i.ApprovalStatus == (int)ApprovalStatusType.Approved).DefaultIfEmpty().Sum(i => i?.NumberOfDays ?? 0.00);
				leaveBalanceSummarisedReportModel.Study = individualLeaveDays.Where(i => i.UserID == user.UserID && i.LeaveTypeID == (int)LeaveTypeEnum.Study && i.ApprovalStatus == (int)ApprovalStatusType.Approved).DefaultIfEmpty().Sum(i => i?.NumberOfDays ?? 0.00);
				leaveBalanceSummarisedReportModel.Maternity = individualLeaveDays.Where(i => i.UserID == user.UserID && i.LeaveTypeID == (int)LeaveTypeEnum.Maternity && i.ApprovalStatus == (int)ApprovalStatusType.Approved).DefaultIfEmpty().Sum(i => i?.NumberOfDays ?? 0.00);
				leaveBalanceSummarisedReportModel.Relocation = individualLeaveDays.Where(i => i.UserID == user.UserID && i.LeaveTypeID == (int)LeaveTypeEnum.Relocation && i.ApprovalStatus == (int)ApprovalStatusType.Approved).DefaultIfEmpty().Sum(i => i?.NumberOfDays ?? 0.00);
				leaveBalanceSummarisedReportModel.Unpaid = individualLeaveDays.Where(i => i.UserID == user.UserID && i.LeaveTypeID == (int)LeaveTypeEnum.Unpaid && i.ApprovalStatus == (int)ApprovalStatusType.Approved).DefaultIfEmpty().Sum(i => i?.NumberOfDays ?? 0.00);
				leaveBalanceSummarisedReportModel.Manager = UserRepo.GetByID(user.UserID).Manager.FullName;
				leaveBalanceSummarisedReportModel.NextMonthApprovedLeave = UserRepo.GetTotalLeaveDaysForUserForDates(user.UserID, ApprovalStatusType.Approved, user.BalanceDate.AddMonths(1).ToFirstDayOfMonth(), user.BalanceDate.AddMonths(1).ToLastDayOfMonth());
				leaveBalanceSummarisedReportModel.NextMonthPendingLeave = UserRepo.GetTotalLeaveDaysForUserForDates(user.UserID, ApprovalStatusType.Pending, user.BalanceDate.AddMonths(1).ToFirstDayOfMonth(), user.BalanceDate.AddMonths(1).ToLastDayOfMonth());
				leaveBalanceSummarisedReportModels.Add(leaveBalanceSummarisedReportModel);
			}

			return leaveBalanceSummarisedReportModels;
		}

		public static IEnumerable<LeaveOtherReportModel> GetLeaveOtherReportForDate(LeaveTypeEnum leaveType, DateTime date)
		{
			var startDate = date.ToFirstDayOfMonth();
			var endDate = date.ToLastDayOfMonth();

			//Get all leave balances as at date for all users
			var allUserLeaveBalances = (from lb in DB.LeaveBalances
										join u in DB.Users on lb.UserID equals u.UserID
										where u.IsActive && lb.LeaveType == (int)leaveType && lb.BalanceDate == date
										select lb
										)
										.ToList();

			//Get the individual leave taken for the month
			var individualLeaveDays = (from lb in DB.IndividualLeaveDays
									   where lb.BalanceDate == date
									   && lb.WorkDate >= startDate && lb.WorkDate <= endDate
									   select (new LeaveBalanceExtendedReportModel()
									   {
										   UserID = lb.UserID,
										   FullName = lb.FullName,
										   BalanceDate = lb.BalanceDate,
										   OpeningBalance = lb.OpeningBalance,
										   LeaveTaken = lb.LeaveTaken,
										   LeaveCredited = lb.LeaveCredited,
										   ManualAdjustment = lb.ManualAdjustment,
										   LeavePeriodStartDate = lb.LeavePeriodStartDate,
										   LeavePeriodEndDate = lb.LeavePeriodEndDate,
										   WorkDate = lb.WorkDate,
										   ApprovalStatus = lb.ApprovalStatus,
										   LeaveTypeID = lb.LeaveTypeID,
										   LeaveTypeName = lb.LeaveTypeName,
										   NormalHours = lb.NormalHours,
										   NumberOfDays = lb.NumberOfDays,
										   HoursTaken = lb.HoursTaken,
										   Remark = lb.Remark,
										   Manager = lb.Manager
									   })
							).ToList();

			var leaveBalanceOtherReportModels = new List<LeaveOtherReportModel>();

			foreach (var user in allUserLeaveBalances)
			{
				var leaveBalanceOtherReportModel = new LeaveOtherReportModel
				{
					FullName = UserRepo.GetByID(user.UserID).FullName,
					PayrollRefNo = UserRepo.GetByID(user.UserID).PayrollRefNo,
					EmploymentType = ((EmploymentTypeEnum)UserRepo.GetByID(user.UserID).EmploymentTypeID).ToString(),
					Manager = UserRepo.GetByID(user.UserID).Manager.FullName,
					LeaveType = ((LeaveTypeEnum)user.LeaveType).ToString(),
					DateFrom = startDate,
					DateTo = endDate,
					Approved = individualLeaveDays.Where(i => i.UserID == user.UserID && i.LeaveTypeID == (int)leaveType && i.ApprovalStatus == (int)ApprovalStatusType.Approved).DefaultIfEmpty().Sum(i => i?.NumberOfDays ?? 0.00),
					Pending = individualLeaveDays.Where(i => i.UserID == user.UserID && i.LeaveTypeID == (int)leaveType && i.ApprovalStatus == (int)ApprovalStatusType.Pending).DefaultIfEmpty().Sum(i => i?.NumberOfDays ?? 0.00)
				};

				if (leaveBalanceOtherReportModel.Approved > 0 || leaveBalanceOtherReportModel.Pending > 0)
					leaveBalanceOtherReportModels.Add(leaveBalanceOtherReportModel);
			}

			return leaveBalanceOtherReportModels;
		}

		public static LeaveBalanceDate GetLatestLeaveBalance(int userID, int leaveType) =>
			DB.LeaveBalances
			.Where(lb => lb.UserID == userID && lb.LeaveType == leaveType && lb.LeaveCredited != 0)
			.OrderByDescending(t => t.BalanceDate)
			.Select(n => new LeaveBalanceDate
			{
				Balance = n.Balance,
				BalanceDate = n.BalanceDate
			}).FirstOrDefault();

		public static LeaveBalanceDate GetLatestLeaveCycleBalance(int userID, int leaveType) =>
			DB.LeaveCycleBalances
			.Where(lb => lb.UserID == userID && lb.LeaveTypeID == leaveType)
			.OrderByDescending(t => t.StartDate)
			.Select(n => new LeaveBalanceDate
			{
				Balance = n.ClosingBalance,
				BalanceDate = n.StartDate
			}).FirstOrDefault();

		public static bool CheckAvailableDays(int userID, string externalRef, int leaveType, DateTime dateFrom, DateTime dateTo, bool isApproving)
		{
			//Check that Employee has enough available days
			double availableDays = 0;
			var lastBalanceDate = DateTime.MinValue;

			//Leave Type
			switch (leaveType)
			{
				case (int)LeaveTypeEnum.Annual:
					//Get current leave balance
					var lb = GetLatestLeaveBalance(userID, leaveType);

					availableDays = lb.Balance;
					lastBalanceDate = lb.BalanceDate;

					//Get Leave Credit per user
					double credit = User.GetLeaveDaysCredit(externalRef);

					//Add number of days still to be credited up to the date being applied for
					availableDays += GetLeaveCreditForFutureDate(lastBalanceDate, dateTo, credit);

					break;

				case (int)LeaveTypeEnum.Sick:
					//Get current leave cycle balance
					var lcb = GetLatestLeaveCycleBalance(userID, leaveType);

					availableDays = leaveType == (int)LeaveTypeEnum.Sick ? 30 : 3;
					lastBalanceDate = lcb.BalanceDate;

					break;
			}

			//Subtract number of days in the future of the last leave balance date that have alread been applied for, including any pending days
			availableDays -= GetNumberOfLeaveDaysAfterLastBalanceDate(userID, leaveType, lastBalanceDate);

			//Subtract the number of days being applied for
			if (!isApproving)
				availableDays -= GetNumberOfLeaveDaysForPeriod(dateFrom, dateTo);

			//If insufficient days, notify user but allow to go ahead if desired
			return availableDays > 0;
		}

		internal static double GetLeaveCreditForFutureDate(DateTime lastBalanceDate, DateTime dateTo, double credit)
		{
			double daysCredit = 0;

			daysCredit = ((dateTo.Year - lastBalanceDate.Year) * 12) + dateTo.Month - lastBalanceDate.Month;
			daysCredit *= credit;

			return Math.Truncate(100 * daysCredit) / 100;
		}

		internal static double GetNumberOfLeaveDaysAfterLastBalanceDate(int userID, int leaveType, DateTime balanceDate)
		{
			double days = 0;

			foreach (var period in GetLeavePeriodsAfterLastBalanceDate(userID, leaveType, balanceDate))
			{
				if (period.DateFrom < balanceDate)
					period.DateFrom = balanceDate;

				days += GetNumberOfLeaveDaysForPeriod(period.DateFrom, period.DateTo);
			}

			return Math.Truncate(100 * days) / 100;
		}

		internal static IEnumerable<LeavePeriod> GetLeavePeriodsAfterLastBalanceDate(int userID, int leaveType, DateTime balanceDate) =>
			DB.Leaves
			.Where(l => l.UserID == userID && l.LeaveType == leaveType && l.DateTo >= balanceDate)
			.Select(x => new LeavePeriod
			{
				DateFrom = x.DateFrom,
				DateTo = x.DateTo
			}).ToList();

		internal static IEnumerable<LeavePeriod> GetLeavePeriodsAfterLastCycleBalanceDate(int userID, int leaveType, DateTime balanceDate) =>
			DB.Leaves
			.Where(l => l.UserID == userID && l.LeaveType == leaveType && l.DateTo >= balanceDate && l.ApprovalStatus == (int)ApprovalStatusType.Pending)
			.Select(x => new LeavePeriod
			{
				DateFrom = x.DateFrom,
				DateTo = x.DateTo
			}).ToList();

		internal static double GetNumberOfLeaveDaysForPeriod(DateTime dateFrom, DateTime dateTo)
		{
			double days = 0;

			days = WorkDay.GetWorkableDays(true, dateFrom, dateTo).Count;

			return Math.Truncate(100 * days) / 100;
		}

		#endregion LEAVE BALANCE SPECIFIC
	}
}