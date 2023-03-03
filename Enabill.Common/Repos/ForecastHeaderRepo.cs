using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class ForecastHeaderRepo : BaseRepo
	{
		#region FORECASTHEADER SPECIFIC

		public static IEnumerable<ForecastHeaderExtendedModel> GetAll(DateTime snapShotDate)
		{
			int currentPeriod = DateTime.Today.ToPeriod();
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period == currentPeriod
						&& d.EntryDate < snapShotDateOnly
						select new { forecastHeader = h, lastPeriodDetail = l })
						.Distinct();

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period == currentPeriod
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return data.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				LastForecastPeriodDetail = f.lastPeriodDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetAllByReference(DateTime snapShotDate, string references, List<string> client)
		{
			int currentPeriod = DateTime.Today.ToPeriod();
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period == currentPeriod
						&& d.EntryDate < snapShotDateOnly
						&& references.Contains(d.Reference)
						select new { forecastHeader = h, lastPeriodDetail = l })
						.Distinct();

			if (!client.Contains("All"))
			{
				data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period == currentPeriod
						&& d.EntryDate < snapShotDateOnly
						&& references.Contains(d.Reference)
						&& client.Contains(h.Client)
						select new { forecastHeader = h, lastPeriodDetail = l })
						.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= currentPeriod
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return data.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				LastForecastPeriodDetail = f.lastPeriodDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetAll(int periodFrom, int periodTo, double probability, DateTime snapShotDate, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;
			var data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& d.EntryDate < snapShotDateOnly
						select new { forecastHeader = h, lastperiodDetail = l })
						 .Distinct();

			if (!client.Contains("All"))
			{
				data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						select new { forecastHeader = h, lastperiodDetail = l })
						.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return data.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				LastForecastPeriodDetail = f.lastperiodDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetAllByReference(int periodFrom, int periodTo, double probability, DateTime snapShotDate, List<string> referenceList, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& d.EntryDate < snapShotDateOnly
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, lastperiodDetail = l })
						.Distinct();

			if (!client.Contains("All"))
			{
				data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, lastperiodDetail = l })
						.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return data.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				LastForecastPeriodDetail = f.lastperiodDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetAllByRegion(int periodFrom, int periodTo, double probability, int regionID, DateTime snapShotDate, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& h.RegionID == regionID
						&& d.EntryDate < snapShotDateOnly
						select new { forecastHeader = h, lastperiodDetail = l })
				.Distinct();

			if (!client.Contains("All"))
			{
				data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& h.RegionID == regionID
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						select new { forecastHeader = h, lastperiodDetail = l })
				.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return data.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				LastForecastPeriodDetail = f.lastperiodDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetAllByRegionReference(int periodFrom, int periodTo, double probability, int regionID, DateTime snapShotDate, List<string> referenceList, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& h.RegionID == regionID
						&& d.EntryDate < snapShotDateOnly
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, lastperiodDetail = l })
				.Distinct();

			if (!client.Contains("All"))
			{
				data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& h.RegionID == regionID
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, lastperiodDetail = l })
				.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return data.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				LastForecastPeriodDetail = f.lastperiodDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetAllByDivision(int periodFrom, int periodTo, double probability, int divisionID, DateTime snapShotDate, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& h.DivisionID == divisionID
						&& d.EntryDate < snapShotDateOnly
						select new { forecastHeader = h, lastperiodDetail = l })
				.Distinct();

			if (!client.Contains("All"))
			{
				data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& h.DivisionID == divisionID
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						select new { forecastHeader = h, lastperiodDetail = l })
			   .Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return data.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				LastForecastPeriodDetail = f.lastperiodDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetAllByDivisionReference(int periodFrom, int periodTo, double probability, int divisionID, DateTime snapShotDate, List<string> referenceList, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& h.DivisionID == divisionID
						&& d.EntryDate < snapShotDateOnly
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, lastperiodDetail = l })
				.Distinct();

			if (!client.Contains("All"))
			{
				data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& h.DivisionID == divisionID
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, lastperiodDetail = l })
				.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return data.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				LastForecastPeriodDetail = f.lastperiodDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetAllByRegionDivision(int periodFrom, int periodTo, double probability, int regionID, int divisionID, DateTime snapShotDate, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& h.RegionID == regionID
						&& h.DivisionID == divisionID
						&& d.EntryDate < snapShotDateOnly
						select new { forecastHeader = h, lastperiodDetail = l })
				.Distinct();

			if (!client.Contains("All"))
			{
				data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& h.RegionID == regionID
						&& h.DivisionID == divisionID
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						select new { forecastHeader = h, lastperiodDetail = l })
				.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return data.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				LastForecastPeriodDetail = f.lastperiodDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetAllByRegionDivisionReference(int periodFrom, int periodTo, double probability, int regionID, int divisionID, DateTime snapShotDate, List<string> referenceList, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& h.RegionID == regionID
						&& h.DivisionID == divisionID
						&& d.EntryDate < snapShotDateOnly
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, lastperiodDetail = l })
				.Distinct();

			if (!client.Contains("All"))
			{
				data = (from h in DB.ForecastHeaders
						join l in DB.ForecastHeaderLastPeriodDetails on h.ForecastHeaderID equals l.ForecastHeaderID
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period >= periodFrom && d.Period <= periodTo
						&& h.Probability >= probability
						&& h.RegionID == regionID
						&& h.DivisionID == divisionID
						&& d.EntryDate < snapShotDateOnly
						&& referenceList.Contains(d.Reference)
						&& client.Contains(h.Client)
						select new { forecastHeader = h, lastperiodDetail = l })
				.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return data.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				LastForecastPeriodDetail = f.lastperiodDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetMostRecentDetailLineForCurrentPeriod(DateTime snapShotDate, List<string> client)
		{
			int currentPeriod = DateTime.Today.ToPeriod();
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period == currentPeriod
						&& d.EntryDate < snapShotDateOnly
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();

			if (!client.Contains("All"))
			{
				list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period == currentPeriod
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= currentPeriod
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return list.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				MostRecentForecastDetail = f.forecastDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetMostRecentDetailLineForCurrentPeriodReference(DateTime snapShotDate, List<string> referenceList, List<string> client)
		{
			int currentPeriod = DateTime.Today.ToPeriod();
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where d.Period == currentPeriod
						&& d.EntryDate < snapShotDateOnly
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();

			if (!client.Contains("All"))
			{
				list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						join z in DB.DoesForecastHeaderHaveInvoiceLinks on h.ForecastHeaderID equals z.ForecastHeaderID
						where d.Period == currentPeriod
						&& d.EntryDate < snapShotDateOnly
						&& referenceList.Contains(d.Reference)
						&& client.Contains(h.Client)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= currentPeriod
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return list.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				MostRecentForecastDetail = f.forecastDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetMostRecentDetailLinesByRegionDivisionPeriodProbability(int regionID, int divisionID, int periodFrom, int periodTo, double probability, DateTime snapShotDate, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.RegionID == regionID && h.DivisionID == divisionID && h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();

			if (!client.Contains("All"))
			{
				list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.RegionID == regionID && h.DivisionID == divisionID && h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return list.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				MostRecentForecastDetail = f.forecastDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetMostRecentDetailLinesByRegionDivisionPeriodProbabilityReference(int regionID, int divisionID, int periodFrom, int periodTo, double probability, DateTime snapShotDate, List<string> referenceList, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.RegionID == regionID && h.DivisionID == divisionID && h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();

			if (!client.Contains("All"))
			{
				list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.RegionID == regionID && h.DivisionID == divisionID && h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						&& referenceList.Contains(d.Reference)
						&& client.Contains(h.Client)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return list.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				MostRecentForecastDetail = f.forecastDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetMostRecentDetailLinesByDivisionPeriodProbability(int divisionID, int periodFrom, int periodTo, double probability, DateTime snapShotDate, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.DivisionID == divisionID && h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();

			if (!client.Contains("All"))
			{
				list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						join z in DB.DoesForecastHeaderHaveInvoiceLinks on h.ForecastHeaderID equals z.ForecastHeaderID
						where h.DivisionID == divisionID && h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						select new { forecastHeader = h, forecastDetail = d })
					   .OrderBy(d => d.forecastDetail.ForecastHeaderID)
					   .OrderBy(d => d.forecastDetail.Period)
					   .Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return list.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				MostRecentForecastDetail = f.forecastDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetMostRecentDetailLinesByDivisionPeriodProbabilityReference(int divisionID, int periodFrom, int periodTo, double probability, DateTime snapShotDate, List<string> referenceList, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.DivisionID == divisionID && h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();

			if (!client.Contains("All"))
			{
				list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						join z in DB.DoesForecastHeaderHaveInvoiceLinks on h.ForecastHeaderID equals z.ForecastHeaderID
						where h.DivisionID == divisionID && h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return list.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				MostRecentForecastDetail = f.forecastDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetMostRecentDetailLinesByRegionPeriodProbability(int regionID, int periodFrom, int periodTo, double probability, DateTime snapShotDate, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.RegionID == regionID && h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();

			if (!client.Contains("All"))
			{
				list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.RegionID == regionID && h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return list.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				MostRecentForecastDetail = f.forecastDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetMostRecentDetailLinesByRegionPeriodProbabilityReference(int regionID, int periodFrom, int periodTo, double probability, DateTime snapShotDate, List<string> referenceList, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.RegionID == regionID && h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();

			if (!client.Contains("All"))
			{
				list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.RegionID == regionID && h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return list.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				MostRecentForecastDetail = f.forecastDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetMostRecentDetailLinesByPeriodProbability(int periodFrom, int periodTo, double probability, DateTime snapShotDate, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();

			if (!client.Contains("All"))
			{
				list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						&& client.Contains(h.Client)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return list.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				MostRecentForecastDetail = f.forecastDetail
			});
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetMostRecentDetailLinesByPeriodProbabilityReference(int periodFrom, int periodTo, double probability, DateTime snapShotDate, List<string> referenceList, List<string> client)
		{
			var snapShotDateOnly = snapShotDate.AddDays(1).Date;

			var list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						&& referenceList.Contains(d.Reference)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();

			if (!client.Contains("All"))
			{
				list = (from h in DB.ForecastHeaders
						join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
						where h.Probability >= probability
						&& d.Period >= periodFrom && d.Period <= periodTo
						&& d.EntryDate < snapShotDateOnly
						&& referenceList.Contains(d.Reference)
						&& client.Contains(h.Client)
						select new { forecastHeader = h, forecastDetail = d })
						.OrderBy(d => d.forecastDetail.ForecastHeaderID)
						.OrderBy(d => d.forecastDetail.Period)
						.Distinct();
			}

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						where l.Period >= periodFrom && l.Period <= periodTo
						&& l.HasInvoicesLinked == "true"
						select l.ForecastHeaderID;

			return list.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.ForecastHeaderID) ? "true" : "false",
				MostRecentForecastDetail = f.forecastDetail
			});
		}

		public static ForecastHeader GetByID(int forecastHeaderID) => DB.ForecastHeaders
				   .SingleOrDefault(f => f.ForecastHeaderID == forecastHeaderID);

		public static ForecastHeader GetHeaderByUniqueKey(int regionID, int divisionID, int billingMethodID, int invoiceCategoryID, string clientName, string projectName, string resource) => DB.ForecastHeaders
				   .SingleOrDefault(f => f.RegionID == regionID && f.DivisionID == divisionID && f.BillingMethod == billingMethodID && f.InvoiceCategoryID == invoiceCategoryID && f.Client == clientName && f.Project == projectName && f.Resource == resource);

		public static IEnumerable<string> GetDistinctClients() => DB.ForecastHeaders
				   .Select(h => h.Client)
				   .Distinct();

		public static IEnumerable<int> GetDistinctPeriodsByHeaderID(int headerID) => DB.ForecastHeaderMostRecentDetailLines
				   .Where(h => h.ForecastHeaderID == headerID)
				   .Select(h => h.Period)
				   .Distinct();

		public static void Save(ForecastHeader forecastHeader)
		{
			if (forecastHeader.ForecastHeaderID == 0)
				DB.ForecastHeaders.Add(forecastHeader);
			DB.SaveChanges();
		}

		public static IEnumerable<ForecastHeaderExtendedModel> GetReportForecastAmount(int period)
		{
			var data = from h in DB.ForecastHeaders
					   join d in DB.ForecastHeaderMostRecentDetailLines on h.ForecastHeaderID equals d.ForecastHeaderID
					   join b in DB.BillingMethods on h.BillingMethod equals b.BillingMethodID
					   where d.Period == period
					   select new { forecastHeader = h, forecastDetail = d, billingMethod = b }
						;

			var links = from l in DB.DoesForecastHeaderHaveInvoiceLinks
						join h in DB.ForecastHeaders on l.ForecastHeaderID equals h.ForecastHeaderID
						join b in DB.BillingMethods on h.BillingMethod equals b.BillingMethodID
						where l.Period == period
						&& l.HasInvoicesLinked == "true"
						select h.Client + "_" + b.BillingMethodName;

			return data.Select(f => new ForecastHeaderExtendedModel()
			{
				ForecastHeader = f.forecastHeader,
				HasInvoicesLink = links.Contains(f.forecastHeader.Client + "_" + f.billingMethod.BillingMethodName) ? "true" : "false",
				MostRecentForecastDetail = f.forecastDetail
			});
		}

		public static double GetTotalForecastAmountByKey(int billingMethod, string client, int regionId, int divisionID, int period)
		{
			var data = from h in DB.ForecastHeaders
					   join d in DB.ForecastHeaderMostRecentDetailLines
					   on h.ForecastHeaderID equals d.ForecastHeaderID
					   where h.BillingMethod == billingMethod
							 && h.Client == client
							 && h.RegionID == regionId
							 && h.DivisionID == divisionID
							 && d.Period == period
					   select d
					;

			if (data.Count() > 0)
				return data.Sum(d => d.Amount);
			else
				return 0;
		}

		internal static void Delete(ForecastHeader forecastHeader)
		{
			if (forecastHeader == null)
				return;

			DB.ForecastHeaders.Remove(forecastHeader);
			DB.SaveChanges();
		}

		#endregion FORECASTHEADER SPECIFIC
	}
}