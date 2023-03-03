using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class MonthRepo : BaseRepo
	{
		internal static Month GetByPeriod(int period) => DB.Months
					.SingleOrDefault(m => m.Period == period);

		internal static void Save(Month month)
		{
			if (!DB.Months.Select(m => m.Period).Contains(month.Period))
				DB.Months.Add(month);

			DB.SaveChanges();
		}
	}
}