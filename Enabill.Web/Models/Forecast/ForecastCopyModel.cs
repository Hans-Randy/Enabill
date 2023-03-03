using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class ForecastCopyModel
	{
		#region INITIALIZATION

		public ForecastCopyModel(int forecastDetailID)
		{
			this.ForecastDetail = ForecastDetailRepo.GetByID(forecastDetailID);
			this.ForecastHeader = ForecastHeaderRepo.GetByID(this.ForecastDetail.ForecastHeaderID);
			this.PeriodFrom = this.ForecastDetail.Period;
			this.PeriodTo = this.ForecastDetail.Period.GetMonth() < 12 ? this.PeriodFrom + 01 : ((this.PeriodFrom.GetYear() + 01) * 100) + 01;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int PeriodFrom { get; private set; }
		public int PeriodTo { get; private set; }

		public ForecastDetail ForecastDetail { get; private set; }
		public ForecastHeader ForecastHeader { get; private set; }

		#endregion PROPERTIES
	}
}