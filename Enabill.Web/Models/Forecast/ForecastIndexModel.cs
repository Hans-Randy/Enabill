using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;
using Model = Enabill.Models;

namespace Enabill.Web.Models
{
	public class ForecastIndexModel
	{
		#region INITIALIZATION

		public ForecastIndexModel()
		{
			this.SnapShotDate = DateTime.Today;
			this.References = "All";
			this.ReferenceList = new List<string>();
			this.ReferenceList.AddRange(this.References.Split(','));
			this.Clients = "All";
			this.ClientList = new List<string>();
			this.ClientList.AddRange(this.Clients.Split(','));
			this.ForecastHeaders = ForecastHeaderRepo.GetAll(this.SnapShotDate).ToList();
			this.MostRecentDetailLines = ForecastHeaderRepo.GetMostRecentDetailLineForCurrentPeriod(this.SnapShotDate, this.ClientList).ToList();
			this.PeriodStringList = new List<string>
			{
				DateTime.Today.Year + " " + DateTime.Today.Month.ToMonthName()
			};
			this.PeriodIntList = new List<int>
			{
				DateTime.Today.ToPeriod()
			};
			this.ForecastHeaderAllForecastDetails = this.GetForecastDetailExtendedModels(DateTime.Today.ToPeriod(), DateTime.Today.ToPeriod(), 0.00, 0, 0, this.PeriodIntList, this.SnapShotDate, this.References, this.ReferenceList, this.Clients, this.ClientList).ToList();
			this.DistinctRegionList = this.GetDistinctRegions(this.ForecastHeaders);
			this.DistinctDivisionList = this.GetDistinctDivisions(this.ForecastHeaders);
		}

		public ForecastIndexModel(int regionID, int divisionID, int periodFrom, int periodTo, double probability, DateTime snapShotDate, string references, string clients)
		{
			this.PeriodStringList = new List<string>();
			this.PeriodIntList = new List<int>();
			this.SnapShotDate = snapShotDate;
			this.References = references;
			this.ReferenceList = new List<string>();
			references = references ?? "All";
			this.ReferenceList.AddRange(references.Split(','));
			this.Clients = clients;
			this.ClientList = new List<string>();
			this.ClientList.AddRange(this.Clients.Split(','));

			int currentPeriod = periodFrom;

			while (currentPeriod <= periodTo)
			{
				this.PeriodStringList.Add(currentPeriod.GetYear() + " " + currentPeriod.GetMonth().ToMonthName());
				this.PeriodIntList.Add(currentPeriod);

				if (currentPeriod.GetMonth() < 12)
					currentPeriod++;
				else
					currentPeriod = ((currentPeriod.GetYear() + 01) * 100) + 01;
			}

			this.TypeOfSearch = regionID == 0 && divisionID == 0 && references == "All" ? "All" : regionID == 0 && divisionID != 0 && references == "All" ? "Division" : regionID != 0 && divisionID == 0 && references == "All" ? "Region" : regionID != 0 && divisionID != 0 && references == "All" ? "RegionDivision" : regionID == 0 && divisionID == 0 && references != "All" ? "AllReference" : regionID == 0 && divisionID != 0 && references != "All" ? "DivisionReference" : regionID != 0 && divisionID == 0 && references != "All" ? "RegionReference" : regionID != 0 && divisionID != 0 && references != "All" ? "RegionDivisionReference" : "All";

			switch (this.TypeOfSearch)
			{
				case "All":
					this.ForecastHeaders = ForecastHeaderRepo.GetAll(periodFrom, periodTo, probability, this.SnapShotDate, this.ClientList).ToList();
					this.MostRecentDetailLines = ForecastHeaderRepo.GetMostRecentDetailLinesByPeriodProbability(periodFrom, periodTo, probability, this.SnapShotDate, this.ClientList).ToList();
					this.ForecastHeaderAllForecastDetails = this.GetForecastDetailExtendedModels(periodFrom, periodTo, probability, divisionID, regionID, this.PeriodIntList, this.SnapShotDate, references, this.ReferenceList, this.Clients, this.ClientList).OrderBy(d => d.ForecastDetailUniqueKey).ThenBy(d => d.PeriodPosition).ToList();
					break;

				case "Division":
					this.ForecastHeaders = ForecastHeaderRepo.GetAllByDivision(periodFrom, periodTo, probability, divisionID, this.SnapShotDate, this.ClientList).ToList();
					this.MostRecentDetailLines = ForecastHeaderRepo.GetMostRecentDetailLinesByDivisionPeriodProbability(divisionID, periodFrom, periodTo, probability, this.SnapShotDate, this.ClientList).ToList();
					this.ForecastHeaderAllForecastDetails = this.GetForecastDetailExtendedModels(periodFrom, periodTo, probability, divisionID, regionID, this.PeriodIntList, this.SnapShotDate, references, this.ReferenceList, this.Clients, this.ClientList).OrderBy(d => d.ForecastDetailUniqueKey).ThenBy(d => d.PeriodPosition).ToList();
					break;

				case "Region":
					this.ForecastHeaders = ForecastHeaderRepo.GetAllByRegion(periodFrom, periodTo, probability, regionID, this.SnapShotDate, this.ClientList).ToList();
					this.MostRecentDetailLines = ForecastHeaderRepo.GetMostRecentDetailLinesByRegionPeriodProbability(regionID, periodFrom, periodTo, probability, this.SnapShotDate, this.ClientList).ToList();
					this.ForecastHeaderAllForecastDetails = this.GetForecastDetailExtendedModels(periodFrom, periodTo, probability, divisionID, regionID, this.PeriodIntList, this.SnapShotDate, references, this.ReferenceList, this.Clients, this.ClientList).OrderBy(d => d.ForecastDetailUniqueKey).ThenBy(d => d.PeriodPosition).ToList();
					break;

				case "RegionDivision":
					this.ForecastHeaders = ForecastHeaderRepo.GetAllByRegionDivision(periodFrom, periodTo, probability, regionID, divisionID, this.SnapShotDate, this.ClientList).ToList();
					this.MostRecentDetailLines = ForecastHeaderRepo.GetMostRecentDetailLinesByRegionDivisionPeriodProbability(regionID, divisionID, periodFrom, periodTo, probability, this.SnapShotDate, this.ClientList).ToList();
					this.ForecastHeaderAllForecastDetails = this.GetForecastDetailExtendedModels(periodFrom, periodTo, probability, divisionID, regionID, this.PeriodIntList, this.SnapShotDate, references, this.ReferenceList, this.Clients, this.ClientList).OrderBy(d => d.ForecastDetailUniqueKey).ThenBy(d => d.PeriodPosition).ToList();
					break;

				case "AllReference":
					this.ForecastHeaders = ForecastHeaderRepo.GetAllByReference(periodFrom, periodTo, probability, this.SnapShotDate, this.ReferenceList, this.ClientList).ToList();
					this.MostRecentDetailLines = ForecastHeaderRepo.GetMostRecentDetailLinesByPeriodProbabilityReference(periodFrom, periodTo, probability, this.SnapShotDate, this.ReferenceList, this.ClientList).ToList();
					this.ForecastHeaderAllForecastDetails = this.GetForecastDetailExtendedModels(periodFrom, periodTo, probability, divisionID, regionID, this.PeriodIntList, this.SnapShotDate, references, this.ReferenceList, this.Clients, this.ClientList).OrderBy(d => d.ForecastDetailUniqueKey).ThenBy(d => d.PeriodPosition).ToList();
					break;

				case "DivisionReference":
					this.ForecastHeaders = ForecastHeaderRepo.GetAllByDivisionReference(periodFrom, periodTo, probability, divisionID, this.SnapShotDate, this.ReferenceList, this.ClientList).ToList();
					this.MostRecentDetailLines = ForecastHeaderRepo.GetMostRecentDetailLinesByDivisionPeriodProbabilityReference(divisionID, periodFrom, periodTo, probability, this.SnapShotDate, this.ReferenceList, this.ClientList).ToList();
					this.ForecastHeaderAllForecastDetails = this.GetForecastDetailExtendedModels(periodFrom, periodTo, probability, divisionID, regionID, this.PeriodIntList, this.SnapShotDate, references, this.ReferenceList, this.Clients, this.ClientList).OrderBy(d => d.ForecastDetailUniqueKey).ThenBy(d => d.PeriodPosition).ToList();
					break;

				case "RegionReference":
					this.ForecastHeaders = ForecastHeaderRepo.GetAllByRegionReference(periodFrom, periodTo, probability, regionID, this.SnapShotDate, this.ReferenceList, this.ClientList).ToList();
					this.MostRecentDetailLines = ForecastHeaderRepo.GetMostRecentDetailLinesByRegionPeriodProbabilityReference(regionID, periodFrom, periodTo, probability, this.SnapShotDate, this.ReferenceList, this.ClientList).ToList();
					this.ForecastHeaderAllForecastDetails = this.GetForecastDetailExtendedModels(periodFrom, periodTo, probability, divisionID, regionID, this.PeriodIntList, this.SnapShotDate, references, this.ReferenceList, this.Clients, this.ClientList).OrderBy(d => d.ForecastDetailUniqueKey).ThenBy(d => d.PeriodPosition).ToList();
					break;

				case "RegionDivisionReference":
					this.ForecastHeaders = ForecastHeaderRepo.GetAllByRegionDivisionReference(periodFrom, periodTo, probability, regionID, divisionID, this.SnapShotDate, this.ReferenceList, this.ClientList).ToList();
					this.MostRecentDetailLines = ForecastHeaderRepo.GetMostRecentDetailLinesByRegionDivisionPeriodProbabilityReference(regionID, divisionID, periodFrom, periodTo, probability, this.SnapShotDate, this.ReferenceList, this.ClientList).ToList();
					this.ForecastHeaderAllForecastDetails = this.GetForecastDetailExtendedModels(periodFrom, periodTo, probability, divisionID, regionID, this.PeriodIntList, this.SnapShotDate, references, this.ReferenceList, this.Clients, this.ClientList).OrderBy(d => d.ForecastDetailUniqueKey).ThenBy(d => d.PeriodPosition).ToList();
					break;
			}

			this.DistinctRegionList = this.GetDistinctRegions(this.ForecastHeaders);
			this.DistinctDivisionList = this.GetDistinctDivisions(this.ForecastHeaders);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public string Clients { get; private set; }
		public string LinkedInvoices { get; private set; }
		public string References { get; private set; }
		public string TypeOfSearch { get; private set; }

		public DateTime SnapShotDate { get; private set; }

		public List<int> PeriodIntList { get; private set; }
		public List<string> ClientList { get; private set; }
		public List<string> PeriodStringList { get; private set; }
		public List<string> ReferenceList { get; private set; }
		public List<string> Resources { get; private set; }
		public List<Division> DistinctDivisionList { get; private set; }
		public List<ForecastDefaultReferenceExtendedModel> ForecastDefaultReferenceExtendedModel { get; private set; }
		public List<ForecastDetail> ForecastDetails { get; private set; }
		public List<ForecastDetailExtendedModel> ForecastHeaderAllForecastDetails { get; private set; }
		public List<ForecastHeaderExtendedModel> ForecastHeaders { get; private set; }
		public List<ForecastHeaderExtendedModel> MostRecentDetailLines { get; private set; }
		public List<ForecastHeaderMostRecentResourceAssignment> MostRecentResourceAssignments { get; private set; }
		public List<Model.Region> DistinctRegionList { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public List<ForecastDetailExtendedModel> GetForecastDetailExtendedModels(int periodFrom, int periodTo, double probability, int divisionID, int regionID, List<int> periodList, DateTime snapShotDate, string references, List<string> referenceList, string client, List<string> clientList)
		{
			this.TypeOfSearch = regionID == 0 && divisionID == 0 && references == "All" ? "All" : regionID == 0 && divisionID != 0 && references == "All" ? "Division" : regionID != 0 && divisionID == 0 && references == "All" ? "Region" : regionID != 0 && divisionID != 0 && references == "All" ? "RegionDivision" : regionID == 0 && divisionID == 0 && references != "All" ? "AllReference" : regionID == 0 && divisionID != 0 && references != "All" ? "DivisionReference" : regionID != 0 && divisionID == 0 && references != "All" ? "RegionReference" : regionID != 0 && divisionID != 0 && references != "All" ? "RegionDivisionReference" : "All";

			switch (this.TypeOfSearch)
			{
				case "All":
					this.ForecastDetails = ForecastDetailRepo.GetAllByHeaderByPeriodProbability(periodFrom, periodTo, probability, snapShotDate, clientList).OrderBy(d => d.ForecastDetailID).ThenBy(d => d.Period).ToList();
					break;

				case "Division":
					this.ForecastDetails = ForecastDetailRepo.GetAllByHeaderByPeriodProbabilityDivision(periodFrom, periodTo, probability, divisionID, snapShotDate, clientList).OrderBy(d => d.ForecastDetailID).ThenBy(d => d.Period).ToList();
					break;

				case "Region":
					this.ForecastDetails = ForecastDetailRepo.GetAllByHeaderByPeriodProbabilityRegion(periodFrom, periodTo, probability, regionID, snapShotDate, clientList).OrderBy(d => d.ForecastDetailID).ThenBy(d => d.Period).ToList();
					break;

				case "RegionDivision":
					this.ForecastDetails = ForecastDetailRepo.GetAllByHeaderByPeriodProbabilityRegionDivision(periodFrom, periodTo, probability, regionID, divisionID, snapShotDate, clientList).OrderBy(d => d.ForecastDetailID).ThenBy(d => d.Period).ToList();
					break;

				case "AllReference":
					this.ForecastDetails = ForecastDetailRepo.GetAllByHeaderByPeriodProbabilityReference(periodFrom, periodTo, probability, snapShotDate, referenceList, clientList).OrderBy(d => d.ForecastDetailID).ThenBy(d => d.Period).ToList();
					break;

				case "DivisionReference":
					this.ForecastDetails = ForecastDetailRepo.GetAllByHeaderByPeriodProbabilityDivisionReference(periodFrom, periodTo, probability, divisionID, snapShotDate, referenceList, clientList).OrderBy(d => d.ForecastDetailID).ThenBy(d => d.Period).ToList();
					break;

				case "RegionReference":
					this.ForecastDetails = ForecastDetailRepo.GetAllByHeaderByPeriodProbabilityRegionReference(periodFrom, periodTo, probability, regionID, snapShotDate, referenceList, clientList).OrderBy(d => d.ForecastDetailID).ThenBy(d => d.Period).ToList();
					break;

				case "RegionDivisionReference":
					this.ForecastDetails = ForecastDetailRepo.GetAllByHeaderByPeriodProbabilityRegionDivisionReference(periodFrom, periodTo, probability, regionID, divisionID, snapShotDate, referenceList, clientList).OrderBy(d => d.ForecastDetailID).ThenBy(d => d.Period).ToList();
					break;
			}

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

				int periodPosition = 0;

				foreach (int period in periodList)
				{
					periodPosition++;

					if (period == forecastDetail.Period)
					{
						break;
					}
				}

				//build the extended model

				try
				{
					forecastDetailExtendedModels.Add(new ForecastDetailExtendedModel
					{
						ForecastDetail = forecastDetail,
						Resources = resources,
						ModifiedBy = UserRepo.GetByID(forecastDetail.ModifiedByUserID).FullName,
						ForecastDetailUniqueKey = forecastDetail.EntryDate.ToDisplayString() + "_" + forecastDetail.ForecastHeaderID + "_" + forecastDetail.Remark + "_" + forecastDetail.Reference + "_" + resources + "_" + UserRepo.GetByID(forecastDetail.ModifiedByUserID).FullName,
						PeriodPosition = periodPosition
					});
				}
				catch (Exception ex)
				{
				}
			}

			return forecastDetailExtendedModels;
		}

		public List<Enabill.Models.Region> GetDistinctRegions(List<ForecastHeaderExtendedModel> forecastHeaders)
		{
			var distinctRegionList = new List<Enabill.Models.Region>();

			foreach (var forecastHeader in forecastHeaders)
			{
				var region = RegionRepo.GetByID(forecastHeader.ForecastHeader.RegionID.Value);

				if (!distinctRegionList.Contains(region))
					distinctRegionList.Add(region);
			}

			return distinctRegionList;
		}

		public List<Division> GetDistinctDivisions(List<ForecastHeaderExtendedModel> forecastHeaders)
		{
			var distinctDivisionList = new List<Division>();

			foreach (var forecastHeader in forecastHeaders)
			{
				var division = DivisionRepo.GetByID(forecastHeader.ForecastHeader.DivisionID.Value);

				if (!distinctDivisionList.Contains(division))
					distinctDivisionList.Add(division);
			}

			return distinctDivisionList;
		}

		#endregion FUNCTIONS
	}
}