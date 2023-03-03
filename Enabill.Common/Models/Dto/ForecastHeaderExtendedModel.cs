namespace Enabill.Models.Dto
{
	public class ForecastHeaderExtendedModel
	{
		#region PROPERTIES

		public string HasInvoicesLink;

		public ForecastHeader ForecastHeader;
		public ForecastHeaderLastPeriodDetail LastForecastPeriodDetail;
		public ForecastHeaderMostRecentDetailLine MostRecentForecastDetail;

		#endregion PROPERTIES
	}
}