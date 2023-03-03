using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enabill.Models
{
	[Table("TimesheetApprovals")]
	public class TimesheetApproval
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int TimesheetApprovalID { get; private set; }

		[Required]
		public int UserID { get; internal set; }

		//This is required for audit trails
		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[Required, MinLength(3), MaxLength(128)]
		public string UserManaged { get; internal set; }

		[Required]
		public DateTime DateManaged { get; internal set; }

		// MonthDate will be saved as the first day of that month, so the timesheet for aug 2011 will be the record with date '2011-08-01'
		[Required]
		public DateTime MonthDate { get; internal set; }

		#endregion PROPERTIES

		#region TIMESHEET APPROVAL

		//public static void LockManagersStaffsTimesheetsForMonth(User userLocking, DateTime monthDate)
		//{
		//    LockManagersStaffsTimesheets(userLocking, monthDate, monthDate);
		//}

		//public static void LockManagersStaffsTimesheets(User userLocking, DateTime dateFrom, DateTime dateTo)
		//{
		//    if (!userLocking.HasRole(UserRoleType.Manager))
		//        throw new TimesheetApprovalException("You do not have the required permissions to lock timesheets. Action cancelled.");

		//    dateFrom = dateFrom.ToFirstDayOfMonth();
		//    dateTo = dateTo.ToLastDayOfMonth();

		//    using (TransactionScope ts = new TransactionScope())
		//    {
		//        userLocking.GetStaffOfManager().ForEach(u => u.LockTimesheets(userLocking, dateFrom, dateTo));

		//        ts.Complete();
		//    }
		//}

		//public static void UnlockManagersStaffsTimesheetsForMonth(User userUnlocking, DateTime monthDate)
		//{
		//    UnlockManagersStaffsTimesheets(userUnlocking, monthDate, monthDate);
		//}

		//public static void UnlockManagersStaffsTimesheets(User userUnlocking, DateTime dateFrom, DateTime dateTo)
		//{
		//    if (!userUnlocking.HasRole(UserRoleType.Manager))
		//        throw new TimesheetApprovalException("You do not have the required permissions to unlock timesheets. Action cancelled.");

		//    dateFrom = dateFrom.ToFirstDayOfMonth();
		//    dateTo = dateTo.ToLastDayOfMonth();

		//    using (TransactionScope ts = new TransactionScope())
		//    {
		//        userUnlocking.GetStaffOfManager().ForEach(x => x.UnlockTimesheets(userUnlocking, dateFrom, dateTo));
		//    }
		//}

		#endregion TIMESHEET APPROVAL
	}
}