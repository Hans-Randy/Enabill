using System.Collections.Generic;
using System.Linq;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public abstract class RatesRepo : BaseRepo
	{
		#region RATES

		public static List<RatesModel> GetRates(int userID = 0, int clientID = 0)
		{
			var data = from u in DB.Users
					   join wa in DB.WorkAllocations on u.UserID equals wa.UserID
					   join ua in DB.UserAllocations on u.UserID equals ua.UserID
					   join a in DB.Activities on ua.ActivityID equals a.ActivityID
					   join p in DB.Projects on a.ProjectID equals p.ProjectID
					   join c in DB.Clients on p.ClientID equals c.ClientID
					   join cr in DB.CurrencyType on c.CurrencyTypeID equals cr.CurrencyTypeID
					   where u.IsActive
					   select (new RatesModel()
					   {
						   UserID = u.UserID,
						   ClientID = c.ClientID,
						   ActivityID = a.ActivityID,
						   ProjectID = p.ProjectID,
						   FullName = u.FullName,
						   ClientName = c.ClientName,
						   ProjectName = p.ProjectName,
						   ActivityName = a.ActivityName,
						   ScheduledEndDate = ua.ScheduledEndDate,
						   ConfirmedEndDate = ua.ConfirmedEndDate,
						   HourlyRate = wa.HourlyRate,
						   ChargeRate = ua.ChargeRate,
						   Currency = cr.CurrencyISO
					   }
					   );

			if (userID != 0)
				data = data.Where(d => d.UserID == userID);

			if (clientID != 0)
				data = data.Where(d => d.ClientID == clientID);

			return data.Distinct().OrderBy(u => u.FullName).ThenBy(c => c.ClientName).ThenBy(p => p.ProjectName).ThenBy(a => a.ActivityName).ToList();
		}

		#endregion RATES

		#region JSON LOOKUPS

		//public static List<JsonLookup> AutoComplete(string client, string partialProject, int topCount)
		//{
		//    if (client == null || client == "")
		//    {
		//        return (from p in DB.Projects
		//                where p.ProjectName.StartsWith(partialProject)
		//                select new JsonLookup
		//                {
		//                    id = p.ProjectID,
		//                    label = p.ProjectName,
		//                    value = p.ProjectName
		//                }).Distinct()
		//                .OrderBy(p => p.label)
		//                .Take(topCount)
		//                .ToList();
		//    }
		//    else
		//    {
		//        return (from p in DB.Projects
		//                join c in DB.Clients on p.ClientID equals c.ClientID
		//                where c.ClientName == client &&
		//                p.ProjectName.StartsWith(partialProject)
		//                select new JsonLookup
		//                {
		//                    id = p.ProjectID,
		//                    label = p.ProjectName,
		//                    value = p.ProjectName
		//                }).Distinct()
		//                .OrderBy(p => p.label)
		//                .Take(topCount)
		//                .ToList();
		//    }
		//}

		#endregion JSON LOOKUPS
	}
}