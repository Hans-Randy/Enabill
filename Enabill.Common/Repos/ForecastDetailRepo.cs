using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class ForecastDetailRepo : BaseRepo
	{
		#region FORECASTDETAIL SPECIFIC

		public static IEnumerable<ForecastDetail> GetAll() => DB.ForecastDetails;

		public static ForecastDetail GetByID(int forecastDetailID) => DB.ForecastDetails
				   .SingleOrDefault(f => f.ForecastDetailID == forecastDetailID);

		public static ForecastHeaderLastPeriodDetail GetLastPeriodDetailByHeaderID(int forecastHeaderID) => DB.ForecastHeaderLastPeriodDetails
				   .SingleOrDefault(f => f.ForecastHeaderID == forecastHeaderID);

		public static IEnumerable<ForecastDetail> GetAllByHeaderIDPeriod(int forecastHeaderID, int period) => DB.ForecastDetails
				.Where(d => d.ForecastHeaderID == forecastHeaderID && d.Period == period);

		public static IEnumerable<ForecastDetail> GetAllByHeaderByPeriodProbability(int periodFrom, int periodTo, double probability, DateTime snapShotDate, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;
			var data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && d.EntryDate < snapShotDateOnly
					   select d;

			if (!client.Contains("All"))
			{
				data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && d.EntryDate < snapShotDateOnly
					   && client.Contains(h.Client)
					   select d;
			}

			return data;
		}

		public static IEnumerable<ForecastDetail> GetAllByHeaderByPeriodProbabilityReference(int periodFrom, int periodTo, double probability, DateTime snapShotDate, List<string> referenceList, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && d.EntryDate < snapShotDateOnly
					   && referenceList.Contains(d.Reference)
					   select d;

			if (!client.Contains("All"))
			{
				data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && d.EntryDate < snapShotDateOnly
					   && referenceList.Contains(d.Reference)
					   && client.Contains(h.Client)
					   select d;
			}

			return data;
		}

		public static IEnumerable<ForecastDetail> GetAllByHeaderByPeriodProbabilityDivision(int periodFrom, int periodTo, double probability, int division, DateTime snapShotDate, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && h.DivisionID == division
					   && d.EntryDate < snapShotDateOnly
					   select d;

			if (!client.Contains("All"))
			{
				data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && h.DivisionID == division
					   && d.EntryDate < snapShotDateOnly
					   && client.Contains(h.Client)
					   select d;
			}

			return data;
		}

		public static IEnumerable<ForecastDetail> GetAllByHeaderByPeriodProbabilityDivisionReference(int periodFrom, int periodTo, double probability, int division, DateTime snapShotDate, List<string> referenceList, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && h.DivisionID == division
					   && d.EntryDate < snapShotDateOnly
					   && referenceList.Contains(d.Reference)
					   select d;

			if (!client.Contains("All"))
			{
				data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && h.DivisionID == division
					   && d.EntryDate < snapShotDateOnly
					   && referenceList.Contains(d.Reference)
					   && client.Contains(h.Client)
					   select d;
			}

			return data;
		}

		public static IEnumerable<ForecastDetail> GetAllByHeaderByPeriodProbabilityRegion(int periodFrom, int periodTo, double probability, int region, DateTime snapShotDate, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && h.RegionID == region
					   && d.EntryDate < snapShotDateOnly
					   select d;

			if (!client.Contains("All"))
			{
				data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && h.RegionID == region
					   && d.EntryDate < snapShotDateOnly
					   && client.Contains(h.Client)
					   select d;
			}

			return data;
		}

		public static IEnumerable<ForecastDetail> GetAllByHeaderByPeriodProbabilityRegionReference(int periodFrom, int periodTo, double probability, int region, DateTime snapShotDate, List<string> referenceList, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && h.RegionID == region
					   && referenceList.Contains(d.Reference)
					   && d.EntryDate < snapShotDateOnly
					   select d;

			if (!client.Contains("All"))
			{
				data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && h.RegionID == region
					   && referenceList.Contains(d.Reference)
					   && d.EntryDate < snapShotDateOnly
					   && client.Contains(h.Client)
					   select d;
			}

			return data;
		}

		public static IEnumerable<ForecastDetail> GetAllByHeaderByPeriodProbabilityRegionDivision(int periodFrom, int periodTo, double probability, int region, int division, DateTime snapShotDate, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && h.RegionID == region
					   && h.DivisionID == division
					   && d.EntryDate < snapShotDateOnly
					   select d;

			if (!client.Contains("All"))
			{
				data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && h.RegionID == region
					   && h.DivisionID == division
					   && d.EntryDate < snapShotDateOnly
					   && client.Contains(h.Client)
					   select d;
			}

			return data;
		}

		public static IEnumerable<ForecastDetail> GetAllByHeaderByPeriodProbabilityRegionDivisionReference(int periodFrom, int periodTo, double probability, int region, int division, DateTime snapShotDate, List<string> referenceList, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && h.RegionID == region
					   && h.DivisionID == division
					   && d.EntryDate < snapShotDateOnly
					   && referenceList.Contains(d.Reference)
					   select d;

			if (!client.Contains("All"))
			{
				data = from d in DB.ForecastDetails
					   join h in DB.ForecastHeaders on d.ForecastHeaderID equals h.ForecastHeaderID
					   where d.Period >= periodFrom && d.Period <= periodTo
					   && h.Probability >= probability
					   && h.RegionID == region
					   && h.DivisionID == division
					   && d.EntryDate < snapShotDateOnly
					   && referenceList.Contains(d.Reference)
					   && client.Contains(h.Client)
					   select d;
			}

			return data;
		}

		public static ForecastDetail GetLastDetailEntryForHeader(int forecastHeaderID, int period)
		{
			if ((from d in DB.ForecastDetails
				 where d.ForecastHeaderID == forecastHeaderID
				 && d.Period == period
				 select d.ForecastDetailID).Count() == 0)
			{
				return null;
			}

			int lastDetailID = (from d in DB.ForecastDetails
								where d.ForecastHeaderID == forecastHeaderID
								&& d.Period == period
								select d.ForecastDetailID).
								Max();

			return DB.ForecastDetails
				.Where(d => d.ForecastHeaderID == forecastHeaderID && d.ForecastDetailID == lastDetailID)
				.SingleOrDefault();
		}

		public static IEnumerable<string> GetDistinctReferences() => DB.ForecastDetails
				   .Where(d => !string.IsNullOrEmpty(d.Reference))
				   .Select(d => d.Reference)
				   .Distinct();

		public static void Save(ForecastDetail forecastDetail)
		{
			if (forecastDetail.ForecastDetailID == 0)
				DB.ForecastDetails.Add(forecastDetail);

			DB.SaveChanges();
		}

		internal static void Delete(ForecastDetail forecastDetail)
		{
			if (forecastDetail == null)
				return;

			DB.ForecastDetails.Remove(forecastDetail);
			DB.SaveChanges();
		}

		#endregion FORECASTDETAIL SPECIFIC
	}
}