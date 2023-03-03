using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("LeaveBalances")]
	public class LeaveBalance
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int LeaveBalanceID { get; internal set; }

		[Required, EnumDataType(typeof(LeaveTypeEnum))]
		public int LeaveType { get; internal set; }

		[Required]
		public int UserID { get; internal set; }

		[Required]
		public double Balance { get; internal set; }

		[Required]
		public double LeaveCredited { get; internal set; }

		[Required]
		public double LeaveTaken { get; internal set; }

		public double ManualAdjustment { get; internal set; }

		[Required]
		public DateTime BalanceDate { get; internal set; }

		#endregion PROPERTIES

		#region LEAVE BALANCES

		internal void Save()
		{
			var temp = LeaveBalanceRepo.GetLeaveBalance(this.UserID, (LeaveTypeEnum)this.LeaveType, this.BalanceDate.Date);
			if (temp != null)
				this.LeaveBalanceID = temp.LeaveBalanceID;

			LeaveBalanceRepo.Save(this);
		}

		#endregion LEAVE BALANCES

		internal static LeaveBalance UpdateLeaveBalance(User user, LeaveTypeEnum leaveType, DateTime startDate, DateTime date, double startOfMonthLeaveBalance)
		{
			//DateTime prevMonthStartDate = date.ToFirstDayOfMonth().AddMonths(-1);
			var monthStart = date.ToFirstDayOfMonth();

			if (date < EnabillSettings.SiteStartDate)
			{
				return null;
			}

			if (monthStart < user.EmployStartDate)
			{
				monthStart = user.EmployStartDate;
			}

			monthStart = monthStart.Date;

			var leaveList = user.GetLeave(leaveType, ApprovalStatusType.Approved, monthStart, monthStart.ToLastDayOfMonth());

			var temp = user.GetLeaveBalance(leaveType, date);

			if (temp == null)
			{
				temp = new LeaveBalance()
				{
					UserID = user.UserID,
					BalanceDate = date
				};
			}

			double leaveTaken = 0;

			temp.LeaveType = (int)leaveType;
			temp.LeaveCredited = 0;
			temp.Balance = 0;

			//work out the number days for the calculated adj
			foreach (var day in WorkDay.GetWorkableDays(true, monthStart, monthStart.ToLastDayOfMonth()))
			{
				var leave = leaveList.SingleOrDefault(l => l.DateFrom <= day.WorkDate && l.DateTo >= day.WorkDate);

				if (leave != null)
				{
					if (leave.NumberOfHours == null)
					{
						leaveTaken++;
					}
					else
					{
						leaveTaken += leave.NumberOfDays;
					}
				}
			}

			if (leaveType == LeaveTypeEnum.Annual)
			{
				if (monthStart.IsInCurrentMonth())
				{
					temp.LeaveCredited = 0;
				}
				else
				{
					var proRataDate = new DateTime(2017, 6, 1, 0, 0, 0); //Prior to this date, leave credited was pro rata based on unpaid leave
					if (monthStart < proRataDate)
						temp.LeaveCredited = user.GetLeaveBalanceCreditAmount(monthStart);
					else //After this date, employee gets full leave credit irrespective of how many unpaid leave days are taken
						temp.LeaveCredited = Math.Round(EnabillSettings.AnnualLeaveAvailableToStaff / 12.00, 2); //num of months in year
				}

				temp.Balance = Math.Round(startOfMonthLeaveBalance, 2);
			}
			else
			{
				temp.LeaveCredited = 0;
			}

			//Get prior month adjustments
			var leavePriorMonthManualAdjustment = user.GetLeaveManualAdjustment(leaveType, date.AddMonths(-1));

			// Only include prior month manual adjustment if it is not the previous month from the month in which the calculation starts
			if (leavePriorMonthManualAdjustment != null && startDate != date)
			{
				if (leaveType == LeaveTypeEnum.Annual)
				{
					temp.Balance += leavePriorMonthManualAdjustment.ManualAdjustment;
				}
			}
			else
			{
				temp.ManualAdjustment = 0;
			}

			//Get current month adjustments
			var leaveManualAdjustment = user.GetLeaveManualAdjustment(leaveType, date);

			if (leaveManualAdjustment != null)
			{
				temp.ManualAdjustment = leaveManualAdjustment.ManualAdjustment;
			}
			else
			{
				temp.ManualAdjustment = 0;
			}

			temp.LeaveTaken = leaveTaken;
			temp.Save();

			return temp;
		}
	}
}