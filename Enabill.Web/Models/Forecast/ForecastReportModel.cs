using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class ForecastReportModel
	{
		#region INITIALIZATION

		public ForecastReportModel(int period, int region, int division)
		{
			switch (region == 0 && division == 0 ? "All" : region != 0 && division == 0 ? "Region" : region == 0 && division != 0 ? "Division" : "RegionDivision")
			{
				case "All":
					this.ForecastWithInvoices = ForecastWithInvoiceSummaryRepo.GetAllByPeriod(period).ToList();
					this.ForecastWithInvoiceDetails = ForecastWithInvoiceRepo.GetAllByPeriod(period);
					this.ForecastWithOutInvoices = ForecastWithOutInvoiceRepo.GetAllByPeriod(period).ToList();
					this.InvoicesWithoutForecasts = InvoiceWithoutForecastRepo.GetAllByPeriod(period).ToList();
					break;

				case "Region":
					this.ForecastWithInvoices = ForecastWithInvoiceSummaryRepo.GetAllByPeriodRegion(period, region).ToList();
					this.ForecastWithInvoiceDetails = ForecastWithInvoiceRepo.GetAllByPeriodRegion(period, region);
					this.ForecastWithOutInvoices = ForecastWithOutInvoiceRepo.GetAllByPeriodRegion(period, region).ToList();
					this.InvoicesWithoutForecasts = InvoiceWithoutForecastRepo.GetAllByPeriodRegion(period, region).ToList();
					break;

				case "Division":
					this.ForecastWithInvoices = ForecastWithInvoiceSummaryRepo.GetAllByPeriodDivision(period, division).ToList();
					this.ForecastWithInvoiceDetails = ForecastWithInvoiceRepo.GetAllByPeriodDivision(period, division);
					this.ForecastWithOutInvoices = ForecastWithOutInvoiceRepo.GetAllByPeriodDivision(period, division).ToList();
					this.InvoicesWithoutForecasts = InvoiceWithoutForecastRepo.GetAllByPeriod(period).ToList();
					break;

				case "RegionDivision":
					this.ForecastWithInvoices = ForecastWithInvoiceSummaryRepo.GetAllByPeriodRegionDivision(period, region, division).ToList();
					this.ForecastWithInvoiceDetails = ForecastWithInvoiceRepo.GetAllByPeriodRegionDivision(period, region, division);
					this.ForecastWithOutInvoices = ForecastWithOutInvoiceRepo.GetAllByPeriodRegionDivision(period, region, division).ToList();
					this.InvoicesWithoutForecasts = InvoiceWithoutForecastRepo.GetAllByPeriodRegion(period, region).ToList();
					break;
			}

			this.ForecastAmounts = ForecastHeaderRepo.GetReportForecastAmount(period).ToList();

			this.ForecastWithInvoiceDistinctRegionList = this.GetForecastWithInvoicesDistinctRegions(this.ForecastWithInvoices);
			this.ForecastWithInvoiceDistinctDivisionList = this.GetForecastWithInvoicesDistinctDivisions(this.ForecastWithInvoices);

			this.ForecastWithOutInvoiceDistinctRegionList = this.GetForecastWithOutInvoicesDistinctRegions(this.ForecastWithOutInvoices);
			this.ForecastWithOutInvoiceDistinctDivisionList = this.GetForecastWithOutInvoicesDistinctDivisions(this.ForecastWithOutInvoices);

			this.InvoicesWithoutForecastsDistinctRegionList = this.GetInvoicesWithoutForecastsDistinctRegions(this.InvoicesWithoutForecasts);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<Division> ForecastWithInvoiceDistinctDivisionList { get; private set; }
		public List<Division> ForecastWithOutInvoiceDistinctDivisionList { get; private set; }
		public List<ForecastHeaderExtendedModel> ForecastAmounts { get; set; }
		public List<ForecastWithInvoiceExtendedModel> ForecastWithInvoiceDetails { get; set; }
		public List<ForecastWithInvoiceSummaryExtendedModel> ForecastWithInvoices { get; set; }
		public List<ForecastWithOutInvoice> ForecastWithOutInvoices { get; set; }
		public List<InvoicesWithoutForecast> InvoicesWithoutForecasts { get; set; }
		public List<Enabill.Models.Region> ForecastWithInvoiceDistinctRegionList { get; private set; }
		public List<Enabill.Models.Region> ForecastWithOutInvoiceDistinctRegionList { get; private set; }
		public List<Enabill.Models.Region> InvoicesWithoutForecastsDistinctRegionList { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public List<Enabill.Models.Region> GetForecastWithInvoicesDistinctRegions(List<ForecastWithInvoiceSummaryExtendedModel> forecastWithInvoices)
		{
			var distinctRegionList = new List<Enabill.Models.Region>();

			foreach (var forecastWithInvoice in forecastWithInvoices)
			{
				var region = RegionRepo.GetByID(forecastWithInvoice.RegionID);

				if (!distinctRegionList.Contains(region))
					distinctRegionList.Add(region);
			}

			return distinctRegionList;
		}

		public List<Division> GetForecastWithInvoicesDistinctDivisions(List<ForecastWithInvoiceSummaryExtendedModel> forecastWithInvoices)
		{
			var distinctDivisionList = new List<Division>();

			foreach (var forecastWithInvoice in forecastWithInvoices)
			{
				var division = DivisionRepo.GetByID(forecastWithInvoice.DivisionID);

				if (!distinctDivisionList.Contains(division))
					distinctDivisionList.Add(division);
			}

			return distinctDivisionList;
		}

		public List<Enabill.Models.Region> GetForecastWithOutInvoicesDistinctRegions(List<ForecastWithOutInvoice> forecastWithOutInvoices)
		{
			var distinctRegionList = new List<Enabill.Models.Region>();

			foreach (var forecastWithOutInvoice in forecastWithOutInvoices)
			{
				var region = RegionRepo.GetByID(forecastWithOutInvoice.RegionID);

				if (!distinctRegionList.Contains(region))
					distinctRegionList.Add(region);
			}

			return distinctRegionList;
		}

		public List<Division> GetForecastWithOutInvoicesDistinctDivisions(List<ForecastWithOutInvoice> forecastWithOutInvoices)
		{
			var distinctDivisionList = new List<Division>();

			foreach (var forecastWithOutInvoice in forecastWithOutInvoices)
			{
				var division = DivisionRepo.GetByID(forecastWithOutInvoice.DivisionID);

				if (!distinctDivisionList.Contains(division))
					distinctDivisionList.Add(division);
			}

			return distinctDivisionList;
		}

		public List<Enabill.Models.Region> GetInvoicesWithoutForecastsDistinctRegions(List<InvoicesWithoutForecast> invoicesWithoutForecasts)
		{
			var distinctRegionList = new List<Enabill.Models.Region>();

			foreach (var invoiceWithoutForecasts in invoicesWithoutForecasts)
			{
				var region = RegionRepo.GetByID(invoiceWithoutForecasts.RegionID);

				if (!distinctRegionList.Contains(region))
					distinctRegionList.Add(region);
			}

			return distinctRegionList;
		}

		#endregion FUNCTIONS
	}
}