using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class UserCostToCompanyRepo : BaseRepo
	{
		#region USER COST TO COMPANY SPECIFIC

		internal static IEnumerable<UserCostToCompany> GetAll() => DB.UserCostToCompanies;

		internal static IEnumerable<UserCostToCompany> GetAllForMonth(int period) => DB.UserCostToCompanies
					.Where(uc => uc.Period == period);

		internal static void Save(UserCostToCompany userCostToCompany)
		{
			userCostToCompany.ModifiedDate = DateTime.Now;

			if (userCostToCompany.UserCostToCompanyID == 0)
				DB.UserCostToCompanies.Add(userCostToCompany);

			DB.SaveChanges();
		}

		#endregion USER COST TO COMPANY SPECIFIC

		internal static IEnumerable<UserCostToCompany> GetAllForMonthToBeRemoved() => from uctc in DB.UserCostToCompanies
																					  join _uh in DB.UserHistories on uctc.UserID equals _uh.UserID into _uhGroup
																					  from uh in _uhGroup.DefaultIfEmpty()
																					  where uh.UserID == null
																					  select uctc;

		internal static void Delete(UserCostToCompany userCostToCompany)
		{
			if (userCostToCompany == null || userCostToCompany.UserCostToCompanyID <= 0)
				return;

			DB.UserCostToCompanies.Remove(userCostToCompany);
		}
	}
}