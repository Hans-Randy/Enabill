using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using NLog;
using ServiceStack.Text;

namespace Enabill.Repos
{
	public class LeaveCycleBalanceRepo : BaseRepo
	{
		#region LOGGER

		private static Logger logger = LogManager.GetCurrentClassLogger();

		#endregion LOGGER

		#region LEAVE CYCLE BALANCE SPECIFIC

		public static IEnumerable<LeaveCycleBalance> GetLeaveCycleBalance(int userID, LeaveTypeEnum leaveTypeID, DateTime date) => DB.LeaveCycleBalances
					.Where(lcb => lcb.UserID == userID && (LeaveTypeEnum)lcb.LeaveTypeID == leaveTypeID && date >= lcb.StartDate && date <= lcb.EndDate);

		public static IEnumerable<LeaveCycleBalance> GetLeaveCycleBalance(int userID) => DB.LeaveCycleBalances
					.Where(lcb => lcb.UserID == userID);

		public static IEnumerable<LeaveCycleBalance> GetAll() => DB.LeaveCycleBalances;

		public static void DeactivateLeaveCycleBalanceRecords(int userID, out int lcbCount, out string userName)
		{
			lcbCount = 0;
			userName = "";

			var outParam1 = new SqlParameter
			{
				ParameterName = "Count",
				SqlDbType = SqlDbType.Int,
				Direction = ParameterDirection.Output
			};

			var outParam2 = new SqlParameter
			{
				ParameterName = "UserName",
				SqlDbType = SqlDbType.VarChar,
				Size = 50,
				Direction = ParameterDirection.Output
			};

			DB.Database.ExecuteSqlCommand("EXEC DeactivateLeaveCycleBalanceRecords @UserID, @Count OUTPUT, @UserName OUTPUT",
						   new SqlParameter("UserID", userID),
						   outParam1,
						   outParam2);
			lcbCount = (int)outParam1.Value;
			userName = outParam2.Value.ToString();
		}

		public static void CorrectLeaveCycleBalanceRecords(int userID, int leaveType) => DB.Database.ExecuteSqlCommand("EXEC CorrectLeaveCycleBalanceRecords {0}, {1}", userID, leaveType);

		public static IEnumerable<LeaveCycleBalance> GetLeaveCyclesThatExpireToday() => DB.LeaveCycleBalances
				  .Where(lcb => lcb.EndDate <= DateTime.Today && lcb.Active == 1);

		public static LeaveCycleBalance GetLeaveCycleBalanceForUserDate(int userID, LeaveTypeEnum leaveType, DateTime date) => DB.LeaveCycleBalances
					.SingleOrDefault(l => l.UserID == userID && l.LeaveTypeID == (int)leaveType && date >= l.StartDate && date <= l.EndDate && l.Active == 1);

		public static LeaveCycleBalance GetLeaveCycleBalanceForUserPeriod(int userID, LeaveTypeEnum leaveType, DateTime date) => DB.Database.SqlQuery<LeaveCycleBalance>("EXEC GetDaysInLeaveCycleByUser {0}, {1}, {2}", userID, (int)leaveType, date).FirstOrDefault();

		public static LeaveCycleBalance GetLastLeaveCycleBalanceForUser(int userID, LeaveTypeEnum leaveType) => DB.LeaveCycleBalances
				  .Where(l => l.UserID == userID && l.LeaveTypeID == (int)leaveType && l.Active == 1)
				  .SingleOrDefault();

		public static LeaveCycleExtendedReportModel GetLastestCycleExtendedReportModel(int userID, LeaveTypeEnum leaveType) => (from lcb in DB.LeaveCycleBalances
																																join u in DB.Users on lcb.UserID equals u.UserID
																																join e in DB.EmploymentTypes on u.EmploymentTypeID equals e.EmploymentTypeID
																																join m in DB.Users on u.ManagerID equals m.UserID
																																join l in DB.LeaveTypes on lcb.LeaveTypeID equals l.LeaveTypeID
																																where lcb.UserID == userID
																																&& lcb.LeaveTypeID == (int)leaveType
																																&& lcb.Active == 1
																																select (new LeaveCycleExtendedReportModel()
																																{
																																	FullName = u.FullName,
																																	EmploymentType = e.EmploymentTypeName,
																																	PayrollRefNo = u.PayrollRefNo,
																																	Manager = m.FullName,
																																	LeaveType = l.LeaveTypeName,
																																	DateFrom = lcb.StartDate,
																																	DateTo = lcb.EndDate,
																																	OpeningBalance = lcb.OpeningBalance,
																																	Taken = lcb.Taken,
																																	ManualAdjustment = lcb.ManualAdjustment,
																																	ClosingBalance = lcb.ClosingBalance,
																																	Status = "",
																																	LastUpdated = lcb.LastUpdatedDate
																																})
					)
					.SingleOrDefault();

		public static void Save(LeaveCycleBalance leaveCycleBalance)
		{
			// Nico - Commented out code above and added code below as it needs to detect the modified state in addition to the added state
			//logger.Trace($"LeaveCycleBalance to be updated: \n{leaveCycleBalance.Dump()}");

			if (leaveCycleBalance.LeaveCycleBalanceID == 0)
			{
				DB.Entry(leaveCycleBalance).State = EntityState.Added;
				DB.LeaveCycleBalances.Add(leaveCycleBalance);
			}
			else
			{
				DB.Entry(leaveCycleBalance).State = EntityState.Modified;
			}

			DB.SaveChanges();
		}

		internal static void Delete(LeaveCycleBalance leaveCycleBalance)
		{
			if (leaveCycleBalance == null)
				return;

			DB.LeaveCycleBalances.Remove(leaveCycleBalance);
			DB.SaveChanges();
		}

		#endregion LEAVE CYCLE BALANCE SPECIFIC
	}
}