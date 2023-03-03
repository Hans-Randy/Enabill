using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class ExpenseCategoryTypeRepo : BaseRepo
	{
		#region EXPENSE SPECIFIC

		public static ExpenseCategoryType GetByID(int expenseCategoryTypeID) => DB.ExpenseCategoryTypes
					.FirstOrDefault(e => e.ExpenseCategoryTypeID == expenseCategoryTypeID);

		public static List<ExpenseCategoryType> GetAll() => DB.ExpenseCategoryTypes.ToList();

		#endregion EXPENSE SPECIFIC
	}
}