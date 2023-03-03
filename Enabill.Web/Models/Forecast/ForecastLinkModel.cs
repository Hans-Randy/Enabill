using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class ForecastLinkModel
	{
		#region INITIALIZATION

		public ForecastLinkModel(ForecastHeader forecastHeader, int period)
		{
			this.ForecastHeader = forecastHeader;
			this.Invoices = ForecastInvoiceLinkRepo.GetForecastInvoiceLinkExtendedModel(forecastHeader.ForecastHeaderID, period, forecastHeader.BillingMethod, forecastHeader.ClientID ?? 0).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public ForecastHeader ForecastHeader { get; private set; }

		public List<ForecastInvoiceLinkExtendedModel> Invoices { get; private set; }

		#endregion PROPERTIES
	}
}