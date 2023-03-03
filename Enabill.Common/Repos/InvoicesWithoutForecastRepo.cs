using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class InvoiceWithoutForecastRepo : BaseRepo
	{
		#region INVOICEWITHOUTFORECAST SPECIFIC

		public static IEnumerable<InvoicesWithoutForecast> GetAllByPeriod(int period) => DB.InvoicesWithoutForecasts
				   .Where(m => m.Period == period);

		public static IEnumerable<InvoicesWithoutForecast> GetAllByPeriodRegion(int period, int regionID) => DB.InvoicesWithoutForecasts
				   .Where(m => m.Period == period && m.RegionID == regionID);

		//public static IEnumerable<InvoicesWithoutForecast> GetAllByPeriodDivision(int period, int divisionID)
		//{
		//    return DB.InvoicesWithoutForecasts
		//           .Where(m => m.Period == period && m.DivisionID == divisionID);
		//}

		//public static IEnumerable<InvoicesWithoutForecast> GetAllByPeriodRegionDivision(int period, int regionID, int divisionID)
		//{
		//    return DB.InvoicesWithoutForecasts
		//           .Where(m => m.Period == period && m.RegionID == regionID &&  m.DivisionID == divisionID);
		//}

		#endregion INVOICEWITHOUTFORECAST SPECIFIC
	}
}