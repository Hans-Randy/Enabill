using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public abstract class ActivityRepo : BaseRepo
	{
		#region ACTIVITY SPECIFIC

		public static List<string> GetDistinctActivityNames() => (from p in DB.Activities
																  select p.ActivityName)
					 .Distinct()
					 .ToList();

		public static List<string> GetDistinctActivityNamesForClientID(int clientID) => (from a in DB.Activities
																						 join p in DB.Projects on a.ProjectID equals p.ProjectID
																						 where p.ClientID == clientID
																						 select a.ActivityName)
					 .Distinct()
					 .ToList();

		public static List<string> GetDistinctActivityNamesForProjectName(string projectName) => (from a in DB.Activities
																								  join p in DB.Projects on a.ProjectID equals p.ProjectID
																								  where p.ProjectName == projectName
																								  select a.ActivityName)
					 .Distinct()
					 .ToList();

		public static Activity GetByID(int activityID) => DB.Activities
					.SingleOrDefault(a => a.ActivityID == activityID);

		public static IEnumerable<Activity> GetAll() => DB.Activities;

		public static Activity GetByName(string activityName) => DB.Activities
					.SingleOrDefault(a => a.ActivityName == activityName);

		public static ActivityDetail GetFullDetail(int activityID) => (from a in DB.Activities
																	   join p in DB.Projects on a.ProjectID equals p.ProjectID
																	   join c in DB.Clients on p.ClientID equals c.ClientID
																	   where a.ActivityID == activityID
																	   select new
																	   {
																		   activity = a,
																		   project = p,
																		   client = c
																	   })
					.Select(m => new ActivityDetail(m.activity, m.project, m.client, false))
					.SingleOrDefault();

		#endregion ACTIVITY SPECIFIC

		#region JSON LOOKUPS

		public static List<JsonLookupDescription> AutoComplete(string partialActivity, int topCount) => (from a in DB.Activities
																										 where a.IsActive && (a.ActivityName.Contains(partialActivity))
																										 select new JsonLookupDescription
																										 {
																											 value = a.ActivityName
																										 }).Distinct()
					.OrderBy(u => u.value)
					.Take(topCount)
					.ToList();

		#endregion JSON LOOKUPS

		#region INVOICE RULE

		internal static bool IsActivityAssignedToAnInvoiceRule(int activityID) => DB.InvoiceRuleActivities
					.Select(ira => ira.ActivityID)
					.Contains(activityID);

		internal static bool IsActivityAssignedToThisInvoiceRule(int invRuleID, int activityID) => DB.InvoiceRuleActivities
					.SingleOrDefault(ira => ira.InvoiceRuleID == invRuleID && ira.ActivityID == activityID) != null;

		#endregion INVOICE RULE
	}
}