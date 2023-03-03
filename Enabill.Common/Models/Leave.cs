using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;
using NLog;
using ServiceStack.Text;

namespace Enabill.Models
{
	[Table("Leaves")]
	public class Leave
	{
		#region LOGGER

		private static Logger logger = LogManager.GetCurrentClassLogger();

		#endregion LOGGER

		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int LeaveID { get; internal set; }

		[Required]
		[EnumDataType(typeof(ApprovalStatusType))]
		public int ApprovalStatus { get; set; }

		[Required]
		[EnumDataType(typeof(LeaveTypeEnum))]
		public int LeaveType { get; internal set; }

		[Required]
		public int UserID { get; internal set; }

		//This field is required to track exactly how many hours were taken on a partial leave day..
		//If this value is null, then a full day was taken, and the numbers of days will be a multiple of 1,0
		//If this value is not null, that was the amount of hours taken and recalculating the numbers of days
		//will have to be done if daily work hours for the user is changed after the date that the leave record was captured
		public int? NumberOfHours { get; internal set; }

		[Required]
		public double NumberOfDays { get; internal set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[MaxLength(128)]
		public string ManagedBy { get; internal set; }

		[MaxLength(200)]
		public string Remark { get; set; }

		[Required]
		public DateTime DateFrom { get; set; }

		[Required]
		public DateTime DateRequested { get; internal set; }

		[Required]
		public DateTime DateTo { get; set; }

		public DateTime? DateManaged { get; internal set; }

		[NotMapped]
		public DateTime? WorkDate { get; set; }

		#endregion PROPERTIES

		#region INITIALIZATION

		public bool IsPartialDay => this.NumberOfHours != null;

		public bool IsFullDay => !this.IsPartialDay;

		[NotMapped]
		public int GetNumberOfHours => this.NumberOfHours ?? (int)(this.GetUser().WorkHoursOnDate(this.DateTo) * this.NumberOfDays);

		#endregion INITIALIZATION

		#region LEAVE

		internal void Save(User userSaving)
		{
			//internal method so the logic about the user saving will
			//be on the public method that calls this one

			if (this.DateFrom > this.DateTo)
				throw new LeaveException("Invalid dates were detected. Please revise your from and to dates.");

			if (this.NumberOfDays <= 0)
			{
				if (this.IsFullDay) // not the same day
					throw new LeaveException("The selected days only contain weekends and/or public holidays. Please reconfirm your leave dates.");
				else // if (this.IsPartialDay) // same day
					throw new LeaveException("The selected day is either a weekend or public holiday. Please reconfirm your leave date.");
			}

			if (this.LeaveID == 0)
			{
				this.ManagedBy = null;
				this.DateManaged = null;
			}
			else
			{
				this.ManagedBy = userSaving == null ? "System" : userSaving.FullName;
				this.DateManaged = DateTime.Now.ToCentralAfricanTime();
			}

			this.LastModifiedBy = userSaving == null ? "System" : userSaving.FullName;
			LeaveRepo.Save(this);
		}

		public void ApproveLeaveRequest(User userManaging)
		{
			var user = this.GetUser();

			if (!userManaging.CanManage(user))
			{
				throw new UserRoleException(string.Format($"You do not have the required permissions to approve leave for {user.FullName}. Action cancelled."));
			}

			//Check if sufficient days are available
			if (this.LeaveType == (int)LeaveTypeEnum.Annual || this.LeaveType == (int)LeaveTypeEnum.Sick)
			{
				bool daysAvailable = LeaveBalanceRepo.CheckAvailableDays(user.UserID, user.ExternalRef, this.LeaveType, this.DateFrom, this.DateTo, true);
				if (!daysAvailable)
				{
					throw new LeaveException("There are insufficient days available, which includes any pending days.");
				}
			}

			this.ApprovalStatus = (int)ApprovalStatusType.Approved;

			logger.Debug("APPROVING LEAVE SECTION===================================");
			logger.Debug($"User managing: {userManaging.FullName}");
			logger.Debug($"Leave object: \n{this.Dump()}");
			logger.Debug("==========================================================");

			this.Save(userManaging);

			//Leave Type
			switch (this.LeaveType)
			{
				case (int)LeaveTypeEnum.Compassionate:
				case (int)LeaveTypeEnum.Maternity:
				case (int)LeaveTypeEnum.Relocation:
				case (int)LeaveTypeEnum.Study:
					double taken = this.NumberOfDays < 1 ? this.NumberOfDays : 1;
					//Fredrik: taken is set to one here because in the method below RecalculateLeaveCycleBalances, it loops through a range thus accounting 1 for each day.
					user.RecalculateLeaveCycleBalances(user.UserID, (LeaveTypeEnum)this.LeaveType, this.DateFrom, this.DateTo, taken, userManaging);
					break;
			}

			//Log an entry in the BalanceAuditTrail before updating the balance. This audit is only to highlight changes that has an impacted a previously calculated balance
			this.CreateBalanceAuditTrail(this, userManaging, "approved");
		}

		public void DeclineLeaveRequest(User userManaging)
		{
			var user = this.GetUser();

			if (!userManaging.CanManage(user))
			{
				throw new UserRoleException(string.Format($"You do not have the required permissions to decline leave for {user.FullName}. Action cancelled."));
			}

			var prevStatus = (ApprovalStatusType)this.ApprovalStatus;

			this.ApprovalStatus = (int)ApprovalStatusType.Declined;

			logger.Debug("DECLINING LEAVE SECTION================================");
			logger.Debug("Exception was not thrown. Leave request will now be saved");
			logger.Debug($"User managing: {userManaging.FullName}");
			logger.Debug($"Leave object: \n{this.Dump()}");
			logger.Debug("==========================================================");

			this.Save(userManaging);

			if (prevStatus == ApprovalStatusType.Approved)
			{
				if (this.LeaveType == (int)LeaveTypeEnum.Sick || this.LeaveType == (int)LeaveTypeEnum.Compassionate)
				{
					double taken = this.NumberOfDays < 1 ? this.NumberOfDays * -1 : -1;

					user.RecalculateLeaveCycleBalances(user.UserID, (LeaveTypeEnum)this.LeaveType, this.DateFrom, this.DateTo, taken, userManaging);
				}

				//Log an entry in the BalanceAuditTrail before updating the balance. This audit is only to highlight changes that has an impacted a previously calculated balance
				this.CreateBalanceAuditTrail(this, userManaging, "declined");
			}
		}

		#endregion LEAVE

		#region BALANCE AUDIT TRAIL

		public void CreateBalanceAuditTrail(Leave leave, User changedBy, string status)
		{
			var balanceAuditTrails = new List<BalanceAuditTrail>();

			if (leave.DateFrom.IsInThePast())
			{
				//A change to leave will impact both leave and flexi balance
				var date = this.DateFrom;

				while (date.ToFirstDayOfMonth() < DateTime.Today.ToFirstDayOfMonth())
				{
					//Leave balance audit trail
					var leaveBalance = LeaveBalanceRepo.GetLeaveBalance(leave.UserID, (LeaveTypeEnum)leave.LeaveType, date.AddMonths(1).ToFirstDayOfMonth());

					if (leaveBalance != null)
					{
						var leaveBalanceAuditTrail = new BalanceAuditTrail
						{
							UserID = leave.UserID,
							BalanceTypeID = (int)BalanceTypeEnum.Leave,
							BalanceChangeTypeID = (int)BalanceChangeTypeEnum.Leave,
							BalanceDate = date.AddMonths(1).ToFirstDayOfMonth(),
							BalanceBefore = leaveBalance.Balance,
							ChangeSummary = "Leave for period " + leave.DateFrom.ToExceptionDisplayString() + " to " + leave.DateTo.ToExceptionDisplayString() + " has been " + status + " by " + changedBy.FullName + " on " + DateTime.Today.ToExceptionDisplayString(),
							ChangedBy = changedBy.UserID,
							HoursChanged = leave.GetNumberOfHoursForDate(),
							DateChanged = DateTime.Today,
							BalanceAfter = leaveBalance.Balance
						};
						balanceAuditTrails.Add(leaveBalanceAuditTrail);
					}

					//Flexi balance audit trail
					var flexiBalance = leave.GetUser().GetFlexiBalance(date.AddMonths(1).ToFirstDayOfMonth());

					if (flexiBalance != null)
					{
						var flexiBalanceAuditTrail = new BalanceAuditTrail
						{
							UserID = leave.UserID,
							BalanceTypeID = (int)BalanceTypeEnum.Flexi,
							BalanceChangeTypeID = (int)BalanceChangeTypeEnum.Leave,
							BalanceDate = date.AddMonths(1).ToFirstDayOfMonth(),
							BalanceBefore = flexiBalance.FinalBalance,
							ChangeSummary = "Leave for period " + leave.DateFrom.ToExceptionDisplayString() + " to " + leave.DateTo.ToExceptionDisplayString() + " has been " + status + " by " + changedBy.FullName + " on " + DateTime.Today.ToExceptionDisplayString(),
							ChangedBy = changedBy.UserID,
							HoursChanged = leave.GetNumberOfHoursForDate(),
							DateChanged = DateTime.Today,
							BalanceAfter = flexiBalance != null ? flexiBalance.FinalBalance : 0
						};
						balanceAuditTrails.Add(flexiBalanceAuditTrail);
					}

					date = date.AddMonths(1);
				}
			}

			int monthsToRecalc = DateTime.Today.Year != leave.DateFrom.Year ? 12 - leave.DateFrom.Month + DateTime.Today.Month : DateTime.Today.Month - 1;
			this.GetUser().ExecuteFlexiBalanceLeaveBalanceProcess(changedBy, monthsToRecalc);

			foreach (var bat in balanceAuditTrails)
			{
				bat.BalanceAfter = (BalanceTypeEnum)bat.BalanceTypeID == BalanceTypeEnum.Leave ? LeaveBalanceRepo.GetLeaveBalance(leave.UserID, (LeaveTypeEnum)leave.LeaveType, bat.BalanceDate.ToFirstDayOfMonth()) == null ? 0 : LeaveBalanceRepo.GetLeaveBalance(leave.UserID, (LeaveTypeEnum)leave.LeaveType, bat.BalanceDate.ToFirstDayOfMonth()).Balance : leave.GetUser().GetFlexiBalance(bat.BalanceDate.ToFirstDayOfMonth()).FinalBalance;
				bat.HoursChanged = bat.BalanceAfter - bat.BalanceBefore;
				BalanceAuditTrailRepo.Save(bat);
			}
		}

		#endregion BALANCE AUDIT TRAIL

		#region USER

		internal User GetUser() => UserRepo.GetByID(this.UserID);

		#endregion USER

		private void ConfigureLeaveDates(ref DateTime startDate, ref DateTime endDate)
		{
			var listOfWorkableDays = WorkDay.GetWorkableDays(true, startDate, endDate).Select(w => w.WorkDate).ToList();

			if (listOfWorkableDays.Count == 0)
				throw new LeaveException("Your leave day/s must contain workable days. This leave request contains only weekend days or public holidays. Action cancelled.");

			while (!listOfWorkableDays.Contains(startDate))
				startDate = startDate.AddDays(1);

			while (!listOfWorkableDays.Contains(endDate))
				endDate = endDate.AddDays(-1);
		}

		internal void ConfigureLeaveValues(User userExecuting, User user)
		{
			var listOfWorkableDays = WorkDay.GetWorkableDays(true, this.DateFrom.Date, this.DateTo.Date).Select(w => w.WorkDate).ToList();

			if (listOfWorkableDays.Count == 0)
				throw new LeaveException("Your leave day/s must contain workable days. This leave request contains only weekend days or public holidays. Action cancelled.");

			while (!listOfWorkableDays.Contains(this.DateFrom))
				this.DateFrom = this.DateFrom.AddDays(1);

			while (!listOfWorkableDays.Contains(this.DateTo))
				this.DateTo = this.DateTo.AddDays(-1);

			//leave recorded with a null NumOfHours means the leave was a full day, making the daily hours worked at the time unnecessary
			if (!this.NumberOfHours.HasValue)
			{
				this.NumberOfDays = listOfWorkableDays.Count;
				this.Save(userExecuting);

				return;
			}

			//else recalculate the numberOfDays using the daily work hours and the number of hours of leave taken
			double dailyWorkHoursForUser = user.WorkHoursOnDate(this.DateFrom.Date);

			this.NumberOfDays = this.NumberOfHours.Value / dailyWorkHoursForUser;

			if (this.NumberOfDays > 1)
				this.NumberOfDays = 1;

			this.Save(userExecuting);
		}

		internal double GetNumberOfHoursForDate() => this.NumberOfHours ?? this.GetUser().WorkHoursOnDate(this.DateTo);

		internal void Delete() => LeaveRepo.Delete(this);
	}
}