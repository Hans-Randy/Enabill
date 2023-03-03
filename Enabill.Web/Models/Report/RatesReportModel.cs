using System.Collections.Generic;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models.Dto
{
	public class RatesReportModel
	{
		#region INITIALIZATION

		public RatesReportModel(int userID, int clientID)
		{
			this.RatesReport = this.LoadRatesReportModel(userID, clientID);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<RatesModel> RatesReport { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<RatesModel> LoadRatesReportModel(int userID, int clientID)
		{
			var model = RatesRepo.GetRates(userID, clientID).ToList();

			return model;
		}

		#endregion FUNCTIONS
	}
}