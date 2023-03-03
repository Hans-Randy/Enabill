using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class ForecastReferenceDefaultRepo : BaseRepo
	{
		#region FORECASTREFERENCEDEFAULT SPECIFIC

		public static IEnumerable<ForecastReferenceDefault> GetAll() => DB.ForecastReferenceDefaults;

		public static IEnumerable<ForecastDefaultReferenceExtendedModel> GetAllDefaultReferenceExtendedModels()
		{
			var data = (from r in DB.ForecastReferenceDefaults
						join u in DB.Users on r.ModifiedByUserID equals u.UserID
						select new { forecastReferenceDefault = r, modifiedUser = u })
						.OrderByDescending(r => r.forecastReferenceDefault.EffectiveDate)
						.Distinct();

			return data.Select(r => new ForecastDefaultReferenceExtendedModel()
			{
				ForecastReferenceDefault = r.forecastReferenceDefault,
				ModifiedBy = r.modifiedUser.UserName
			});
		}

		public static ForecastReferenceDefault GetByDetailEntryDate(DateTime detailEntryDate)
		{
			if ((from r in DB.ForecastReferenceDefaults
				 where r.EffectiveDate <= detailEntryDate
				 select r.ForecastReferenceDefaultID).Count() == 0)
			{
				return null;
			}

			int lastID = (from r in DB.ForecastReferenceDefaults
						  where r.EffectiveDate <= detailEntryDate
						  select r.ForecastReferenceDefaultID).
								Max();

			return DB.ForecastReferenceDefaults
				.Where(r => r.ForecastReferenceDefaultID == lastID)
				.SingleOrDefault();
		}

		public static void Save(ForecastReferenceDefault forecastReferenceDefault)
		{
			if (forecastReferenceDefault.ForecastReferenceDefaultID == 0)
				DB.ForecastReferenceDefaults.Add(forecastReferenceDefault);

			DB.SaveChanges();
		}

		public static void Delete(ForecastReferenceDefault forecastReferenceDefault)
		{
			if (forecastReferenceDefault == null)
				return;

			DB.ForecastReferenceDefaults.Remove(forecastReferenceDefault);
			DB.SaveChanges();
		}

		#endregion FORECASTREFERENCEDEFAULT SPECIFIC
	}
}