using System.Collections.Generic;
using System.Linq;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class ForecastWithInvoiceSummaryRepo : BaseRepo
	{
		#region FORECASTWITHINVOICESSUMMARY SPECIFIC

		public static List<ForecastWithInvoiceSummaryExtendedModel> GetAllByPeriod(int period)
		{
			var list = (from s in DB.ForecastWithInvoices
						where s.Period == period
						group s by new { s.BillingMethod, s.Client, s.Region, s.Division, s.RegionID, s.DivisionID, s.Period } into iGroup
						orderby iGroup.Key.BillingMethod, iGroup.Key.Client, iGroup.Key.Region, iGroup.Key.Division, iGroup.Key.RegionID, iGroup.Key.DivisionID, iGroup.Key.Period

						select new ForecastWithInvoiceSummaryExtendedModel()
						{
							BillingMethod = iGroup.Key.BillingMethod,
							Client = iGroup.Key.Client,
							Region = iGroup.Key.Region,
							Division = iGroup.Key.Division,
							RegionID = iGroup.Key.RegionID,
							DivisionID = iGroup.Key.DivisionID,
							Period = iGroup.Key.Period,
							ForecastAmount = iGroup.Select(i => i.ForecastAmount).Max(),
							ProjectedAmount = iGroup.Sum(i => i.ProjectedAmount),
							ActualAmount = iGroup.Sum(i => i.ActualAmount)
						}
					   ).Distinct();

			return list.ToList();
		}

		public static IEnumerable<ForecastWithInvoiceSummaryExtendedModel> GetAllByPeriodRegion(int period, int regionID)
		{
			var list = (from s in DB.ForecastWithInvoices
						where s.Period == period
						&& s.RegionID == regionID
						group s by new { s.BillingMethod, s.Client, s.Region, s.Division, s.RegionID, s.DivisionID, s.Period } into iGroup
						orderby iGroup.Key.BillingMethod, iGroup.Key.Client, iGroup.Key.Region, iGroup.Key.Division, iGroup.Key.RegionID, iGroup.Key.DivisionID, iGroup.Key.Period

						select new ForecastWithInvoiceSummaryExtendedModel()
						{
							BillingMethod = iGroup.Key.BillingMethod,
							Client = iGroup.Key.Client,
							Region = iGroup.Key.Region,
							Division = iGroup.Key.Division,
							RegionID = iGroup.Key.RegionID,
							DivisionID = iGroup.Key.DivisionID,
							Period = iGroup.Key.Period,
							ForecastAmount = iGroup.Select(i => i.ForecastAmount).Max(),
							ProjectedAmount = iGroup.Sum(i => i.ProjectedAmount),
							ActualAmount = iGroup.Sum(i => i.ActualAmount),
						}
					  ).Distinct();

			return list.ToList();
		}

		public static IEnumerable<ForecastWithInvoiceSummaryExtendedModel> GetAllByPeriodDivision(int period, int divisionID)
		{
			var list = (from s in DB.ForecastWithInvoiceSummaries
						where s.Period == period
						&& s.DivisionID == divisionID
						group s by new { s.BillingMethod, s.Client, s.Region, s.Division, s.RegionID, s.DivisionID, s.Period } into iGroup
						orderby iGroup.Key.BillingMethod, iGroup.Key.Client, iGroup.Key.Region, iGroup.Key.Division, iGroup.Key.RegionID, iGroup.Key.DivisionID, iGroup.Key.Period

						select new ForecastWithInvoiceSummaryExtendedModel()
						{
							BillingMethod = iGroup.Key.BillingMethod,
							Client = iGroup.Key.Client,
							Region = iGroup.Key.Region,
							Division = iGroup.Key.Division,
							RegionID = iGroup.Key.RegionID,
							DivisionID = iGroup.Key.DivisionID,
							Period = iGroup.Key.Period,
							ForecastAmount = iGroup.Select(i => i.ForecastAmount).Max(),
							ProjectedAmount = iGroup.Sum(i => i.ProjectedAmount),
							ActualAmount = iGroup.Sum(i => i.ActualAmount)
						}
					  ).Distinct();

			return list.ToList();
		}

		public static IEnumerable<ForecastWithInvoiceSummaryExtendedModel> GetAllByPeriodRegionDivision(int period, int regionID, int divisionID)
		{
			var list = from s in DB.ForecastWithInvoiceSummaries
					   where s.Period == period
					   && s.RegionID == regionID
					   && s.DivisionID == divisionID
					   group s by new { s.BillingMethod, s.Client, s.Region, s.Division, s.RegionID, s.DivisionID, s.Period } into iGroup
					   orderby iGroup.Key.BillingMethod, iGroup.Key.Client, iGroup.Key.Region, iGroup.Key.Division, iGroup.Key.RegionID, iGroup.Key.DivisionID, iGroup.Key.Period

					   select new ForecastWithInvoiceSummaryExtendedModel()
					   {
						   BillingMethod = iGroup.Key.BillingMethod,
						   Client = iGroup.Key.Client,
						   Region = iGroup.Key.Region,
						   Division = iGroup.Key.Division,
						   RegionID = iGroup.Key.RegionID,
						   DivisionID = iGroup.Key.DivisionID,
						   Period = iGroup.Key.Period,
						   ForecastAmount = iGroup.Select(i => i.ForecastAmount).Max(),
						   ProjectedAmount = iGroup.Sum(i => i.ProjectedAmount),
						   ActualAmount = iGroup.Sum(i => i.ActualAmount),
					   }
					  ;

			return list.ToList();
		}

		#endregion FORECASTWITHINVOICESSUMMARY SPECIFIC
	}
}