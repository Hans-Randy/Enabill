using Enabill.Models;

namespace Enabill.Repos
{
	public class FlexiBalanceRepo : BaseRepo
	{
		#region FLEXIBALANCE SPECIFIC

		internal static void Save(FlexiBalance flexiBalance)
		{
			if (flexiBalance.FlexiBalanceID == 0)
				DB.FlexiBalances.Add(flexiBalance);

			DB.SaveChanges();
		}

		internal static void Delete(FlexiBalance initialBalance)
		{
			if (initialBalance == null || initialBalance.FlexiBalanceID == 0)
				return;

			DB.FlexiBalances.Remove(initialBalance);
			DB.SaveChanges();
		}

		#endregion FLEXIBALANCE SPECIFIC
	}
}