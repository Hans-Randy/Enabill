using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class ForecastWithOutInvoiceRepo : BaseRepo
	{
		#region FORECASTWITHOUTINVOICES SPECIFIC

		public static IEnumerable<ForecastWithOutInvoice> GetAllByPeriod(int period) => DB.ForecastWithOutInvoices
				   .Where(m => m.Period == period);

		public static IEnumerable<ForecastWithOutInvoice> GetAllByPeriodRegion(int period, int regionID) => DB.ForecastWithOutInvoices
				   .Where(m => m.Period == period && m.RegionID == regionID);

		public static IEnumerable<ForecastWithOutInvoice> GetAllByPeriodDivision(int period, int divisionID) => DB.ForecastWithOutInvoices
				   .Where(m => m.Period == period && m.DivisionID == divisionID);

		public static IEnumerable<ForecastWithOutInvoice> GetAllByPeriodRegionDivision(int period, int regionID, int divisionID) => DB.ForecastWithOutInvoices
				   .Where(m => m.Period == period && m.RegionID == regionID && m.DivisionID == divisionID);

		#endregion FORECASTWITHOUTINVOICES SPECIFIC
	}
}