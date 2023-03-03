using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class ForecastEditModel
	{
		#region INITIALIZATION

		public ForecastEditModel(ForecastHeader forecastHeader, ForecastDetail mostRecentForecastDetail)
		{
			this.ForecastHeader = forecastHeader;
			this.MostRecentForecastDetail = mostRecentForecastDetail;
			this.Resources = this.GetResources(mostRecentForecastDetail.ForecastDetailID);
			this.ForecastDetailExtendedModels = this.GetForecastDetailExtendedModel(forecastHeader.ForecastHeaderID, mostRecentForecastDetail.Period);
			this.NrOfWorkableDays = WorkDayRepo.GetNumberOfWorkableDays(true, new DateTime(mostRecentForecastDetail.Period.GetYear(), mostRecentForecastDetail.Period.GetMonth(), 01).ToFirstDayOfMonth(), new DateTime(mostRecentForecastDetail.Period.GetYear(), mostRecentForecastDetail.Period.GetMonth(), 01).ToLastDayOfMonth());
			this.DefaultReference = ForecastReferenceDefaultRepo.GetByDetailEntryDate(DateTime.Today) == null ? "" : ForecastReferenceDefaultRepo.GetByDetailEntryDate(DateTime.Today).Reference;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public int NrOfWorkableDays { get; private set; }

		public string DefaultReference { get; private set; }
		public string Resources { get; private set; }

		public ForecastHeader ForecastHeader { get; private set; }
		public ForecastDetail MostRecentForecastDetail { get; private set; }

		public List<ForecastDetail> ForecastDetails { get; private set; }
		public List<ForecastDetailExtendedModel> ForecastDetailExtendedModels { get; private set; }
		public List<ForecastHeaderMostRecentResourceAssignment> MostRecentResourceAssignments { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public string GetResources(int forecastDetailID)
		{
			string resources = "";
			this.MostRecentResourceAssignments = ForecastResourceAssignmentRepo.GetForecastMostRecentResourceAssignmentsByDetailID(forecastDetailID).ToList();

			if (this.MostRecentResourceAssignments != null)
			{
				foreach (var resource in this.MostRecentResourceAssignments)
				{
					resources = resources?.Length == 0 ? resource.Resource : resources + "," + resource.Resource;
				}
			}

			return resources;
		}

		public List<ForecastDetailExtendedModel> GetForecastDetailExtendedModel(int forecastHeaderID, int period)
		{
			this.ForecastDetails = ForecastDetailRepo.GetAllByHeaderIDPeriod(forecastHeaderID, period).OrderBy(d => d.ForecastDetailID).ToList();
			var forecastDetailExtendedModels = new List<ForecastDetailExtendedModel>();

			foreach (var forecastDetail in this.ForecastDetails)
			{
				//Get the resources for the detail
				string resources = "";

				var forecastResourceAssignments = ForecastResourceAssignmentRepo.GetForecastResourceAssignmentsByDetailID(forecastDetail.ForecastDetailID).ToList();

				if (forecastResourceAssignments != null)
				{
					foreach (var resource in forecastResourceAssignments)
					{
						resources = resources?.Length == 0 ? resource.Resource : resources + "," + resource.Resource;
					}
				}

				//build the extended model
				forecastDetailExtendedModels.Add(new ForecastDetailExtendedModel
				{
					ForecastDetail = forecastDetail,
					Resources = resources,
					ModifiedBy = UserRepo.GetByID(forecastDetail.ModifiedByUserID).FullName
				});
			}

			return forecastDetailExtendedModels;
		}

		#endregion FUNCTIONS
	}
}