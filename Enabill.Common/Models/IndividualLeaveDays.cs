using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("vwIndividualLeaveDays")]
	public class IndividualLeaveDay
	{
		#region PROPERTIES

		[Key]
		public int UserID { get; set; }

		public int ApprovalStatus { get; set; }
		public int LeaveTypeID { get; set; }

		public double HoursTaken { get; set; }
		public double LeaveCredited { get; set; }
		public double LeaveTaken { get; set; }
		public double ManualAdjustment { get; set; }
		public double NormalHours { get; set; }
		public double NumberOfDays { get; set; }
		public double OpeningBalance { get; set; }

		public string FullName { get; set; }
		public string LeaveTypeName { get; set; }
		public string Manager { get; set; }
		public string Remark { get; set; }
		public string UserName { get; set; }

		public DateTime BalanceDate { get; set; }
		public DateTime LeavePeriodEndDate { get; set; }
		public DateTime LeavePeriodStartDate { get; set; }
		public DateTime WorkDate { get; set; }

		#endregion PROPERTIES
	}
}