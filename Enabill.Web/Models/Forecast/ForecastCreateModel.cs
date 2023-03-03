using System;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class ForecastCreateModel
	{
		#region INITIALIZATION

		public ForecastCreateModel(ForecastHeader forecastHeader, int period)
		{
			this.ForecastHeader = forecastHeader;
			this.TMClass = (BillingMethodType)forecastHeader.BillingMethod == BillingMethodType.TimeMaterial ? "show" : "hide";
			this.NonTMClass = (BillingMethodType)forecastHeader.BillingMethod == BillingMethodType.TimeMaterial ? "hide" : "show";
			this.NrOfWorkDays = WorkDayRepo.GetNumberOfWorkableDays(true, new DateTime(period.GetYear(), period.GetMonth(), 01).ToFirstDayOfMonth(), new DateTime(period.GetYear(), period.GetMonth(), 01).ToLastDayOfMonth());
			this.DefaultReference = ForecastReferenceDefaultRepo.GetByDetailEntryDate(DateTime.Today) == null ? "" : ForecastReferenceDefaultRepo.GetByDetailEntryDate(DateTime.Today).Reference;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int NrOfWorkDays { get; private set; }

		public string DefaultReference { get; private set; }
		public string NonTMClass { get; private set; }
		public string TMClass { get; private set; }

		public ForecastHeader ForecastHeader { get; private set; }

		#endregion PROPERTIES
	}
}