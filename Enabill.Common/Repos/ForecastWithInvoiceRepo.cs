using System.Collections.Generic;
using System.Linq;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class ForecastWithInvoiceRepo : BaseRepo
	{
		#region FORECASTWITHINVOICES SPECIFIC

		public static List<ForecastWithInvoiceExtendedModel> GetAllByPeriod(int period)
		{
			var list = from h in DB.ForecastHeaders
					   join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
					   join l in DB.ForecastInvoiceLinks on d.ForecastDetailID equals l.ForecastDetailID
					   join i in DB.Invoices on l.InvoiceID equals i.InvoiceID
					   where d.Period == period
					   group i by new { h.BillingMethod, h.Client, h.Region, h.Division, h.RegionID, h.DivisionID, i.Period, i.InvoiceID, i.OrderNo } into iGroup
					   orderby iGroup.Key.BillingMethod, iGroup.Key.Client, iGroup.Key.Region, iGroup.Key.Division, iGroup.Key.RegionID, iGroup.Key.DivisionID, iGroup.Key.Period, iGroup.Key.InvoiceID, iGroup.Key.OrderNo

					   select new ForecastWithInvoiceExtendedModel()
					   {
						   BillingMethod = iGroup.Key.BillingMethod,
						   Client = iGroup.Key.Client,
						   Region = iGroup.Key.Region,
						   Division = iGroup.Key.Division,
						   RegionID = iGroup.Key.RegionID.Value,
						   DivisionID = iGroup.Key.DivisionID.Value,
						   Period = iGroup.Key.Period,
						   InvoiceID = iGroup.Key.InvoiceID,
						   OrderNo = iGroup.Key.OrderNo,
						   ForecastAmount = 0,
						   ProjectedAmount = iGroup.Select(i => i.ProvisionalAccrualAmount ?? 0).Max(),
						   ActualAmount = iGroup.Select(i => i.AccrualExclVAT).Max(),
					   }
					   ;

			return list.ToList();
		}

		public static List<ForecastWithInvoiceExtendedModel> GetAllByPeriodRegion(int period, int regionID)
		{
			var list = from h in DB.ForecastHeaders
					   join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
					   join l in DB.ForecastInvoiceLinks on d.ForecastDetailID equals l.ForecastDetailID
					   join i in DB.Invoices on l.InvoiceID equals i.InvoiceID
					   where d.Period == period
					   && h.RegionID == regionID
					   group i by new { h.BillingMethod, h.Client, h.Region, h.Division, h.RegionID, h.DivisionID, i.Period, i.InvoiceID, i.OrderNo } into iGroup
					   orderby iGroup.Key.BillingMethod, iGroup.Key.Client, iGroup.Key.Region, iGroup.Key.Division, iGroup.Key.RegionID, iGroup.Key.DivisionID, iGroup.Key.Period, iGroup.Key.InvoiceID, iGroup.Key.OrderNo

					   select new ForecastWithInvoiceExtendedModel()
					   {
						   BillingMethod = iGroup.Key.BillingMethod,
						   Client = iGroup.Key.Client,
						   Region = iGroup.Key.Region,
						   Division = iGroup.Key.Division,
						   RegionID = iGroup.Key.RegionID.Value,
						   DivisionID = iGroup.Key.DivisionID.Value,
						   Period = iGroup.Key.Period,
						   InvoiceID = iGroup.Key.InvoiceID,
						   OrderNo = iGroup.Key.OrderNo,
						   ForecastAmount = 0,
						   ProjectedAmount = iGroup.Select(i => i.ProvisionalAccrualAmount ?? 0).Max(),
						   ActualAmount = iGroup.Select(i => i.AccrualExclVAT).Max(),
					   }
					   ;

			return list.ToList();
		}

		public static List<ForecastWithInvoiceExtendedModel> GetAllByPeriodDivision(int period, int divisionID)
		{
			var list = from h in DB.ForecastHeaders
					   join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
					   join l in DB.ForecastInvoiceLinks on d.ForecastDetailID equals l.ForecastDetailID
					   join i in DB.Invoices on l.InvoiceID equals i.InvoiceID
					   where d.Period == period
					   && h.DivisionID == divisionID
					   group i by new { h.BillingMethod, h.Client, h.Region, h.Division, h.RegionID, h.DivisionID, i.Period, i.InvoiceID, i.OrderNo } into iGroup
					   orderby iGroup.Key.BillingMethod, iGroup.Key.Client, iGroup.Key.Region, iGroup.Key.Division, iGroup.Key.RegionID, iGroup.Key.DivisionID, iGroup.Key.Period, iGroup.Key.InvoiceID, iGroup.Key.OrderNo

					   select new ForecastWithInvoiceExtendedModel()
					   {
						   BillingMethod = iGroup.Key.BillingMethod,
						   Client = iGroup.Key.Client,
						   Region = iGroup.Key.Region,
						   Division = iGroup.Key.Division,
						   RegionID = iGroup.Key.RegionID.Value,
						   DivisionID = iGroup.Key.DivisionID.Value,
						   Period = iGroup.Key.Period,
						   InvoiceID = iGroup.Key.InvoiceID,
						   OrderNo = iGroup.Key.OrderNo,
						   ForecastAmount = 0,
						   ProjectedAmount = iGroup.Select(i => i.ProvisionalAccrualAmount ?? 0).Max(),
						   ActualAmount = iGroup.Select(i => i.AccrualExclVAT).Max(),
					   }
					   ;

			return list.ToList();
		}

		public static List<ForecastWithInvoiceExtendedModel> GetAllByPeriodRegionDivision(int period, int regionID, int divisionID)
		{
			var list = from h in DB.ForecastHeaders
					   join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
					   join l in DB.ForecastInvoiceLinks on d.ForecastDetailID equals l.ForecastDetailID
					   join i in DB.Invoices on l.InvoiceID equals i.InvoiceID
					   where d.Period == period
					   && h.RegionID == regionID
					   && h.DivisionID == divisionID
					   group i by new { h.BillingMethod, h.Client, h.Region, h.Division, h.RegionID, h.DivisionID, i.Period, i.InvoiceID, i.OrderNo } into iGroup
					   orderby iGroup.Key.BillingMethod, iGroup.Key.Client, iGroup.Key.Region, iGroup.Key.Division, iGroup.Key.RegionID, iGroup.Key.DivisionID, iGroup.Key.Period, iGroup.Key.InvoiceID, iGroup.Key.OrderNo

					   select new ForecastWithInvoiceExtendedModel()
					   {
						   BillingMethod = iGroup.Key.BillingMethod,
						   Client = iGroup.Key.Client,
						   Region = iGroup.Key.Region,
						   Division = iGroup.Key.Division,
						   RegionID = iGroup.Key.RegionID.Value,
						   DivisionID = iGroup.Key.DivisionID.Value,
						   Period = iGroup.Key.Period,
						   InvoiceID = iGroup.Key.InvoiceID,
						   OrderNo = iGroup.Key.OrderNo,
						   ForecastAmount = 0,
						   ProjectedAmount = iGroup.Select(i => i.ProvisionalAccrualAmount ?? 0).Max(),
						   ActualAmount = iGroup.Select(i => i.AccrualExclVAT).Max(),
					   }
					   ;

			return list.ToList();
		}

		#endregion FORECASTWITHINVOICES SPECIFIC
	}
}