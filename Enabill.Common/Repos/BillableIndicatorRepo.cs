using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class BillableIndicatorRepo : BaseRepo
	{
		#region BILLIBALE INDICATOR SPECIFIC

		public static IEnumerable<BillableIndicator> GetAll() => DB.BillableIndicators;

		public static BillableIndicator GetByID(int billableIndicatorID) => DB.BillableIndicators
					.SingleOrDefault(b => b.BillableIndicatorID == billableIndicatorID);

		#endregion BILLIBALE INDICATOR SPECIFIC
	}
}