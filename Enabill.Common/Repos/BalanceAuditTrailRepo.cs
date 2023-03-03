using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class BalanceAuditTrailRepo : BaseRepo
	{
		#region BALANCE AUDIT TRAIL SPECIFIC

		public static BalanceAuditTrail GetByUniqueKey(int userID, DateTime balanceDate, int balanceTypeID, int balanceChangeTypeID) => DB.BalanceAuditTrails
					.SingleOrDefault(bat => bat.UserID == userID && bat.BalanceDate == balanceDate && bat.BalanceTypeID == balanceTypeID && bat.BalanceChangeTypeID == balanceChangeTypeID);

		public static IEnumerable<BalanceAuditTrail> GetAll(int balanceTypeID, DateTime balanceDate) => DB.BalanceAuditTrails
					.Where(bat => bat.BalanceTypeID == balanceTypeID && bat.BalanceDate == balanceDate);

		public static IEnumerable<BalanceAuditTrail> GetAll(int balanceTypeID, int userID) => DB.BalanceAuditTrails
					.Where(bat => bat.BalanceTypeID == balanceTypeID && bat.UserID == userID);

		public static void Save(BalanceAuditTrail balanceAuditTrail)
		{
			if (balanceAuditTrail.BalanceAuditTrailID == 0)
				DB.BalanceAuditTrails.Add(balanceAuditTrail);
			DB.SaveChanges();
		}

		public static void Delete(BalanceAuditTrail balanceAuditTrail)
		{
			DB.BalanceAuditTrails.Remove(balanceAuditTrail);
			DB.SaveChanges();
		}

		#endregion BALANCE AUDIT TRAIL SPECIFIC
	}
}