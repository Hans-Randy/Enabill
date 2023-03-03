using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class InvoiceRuleActivityRepo : BaseRepo
	{
		#region INVOICE RULE ACTIVITY SPECIFIC

		internal static void Save(int invoiceRuleID, int activityID)
		{
			if (DB.InvoiceRuleActivities.SingleOrDefault(ir => ir.InvoiceRuleID == invoiceRuleID && ir.ActivityID == activityID) == null)
				return;

			var ira = new InvoiceRuleActivity
			{
				InvoiceRuleID = invoiceRuleID,
				ActivityID = activityID
			};

			DB.InvoiceRuleActivities.Add(ira);
			DB.SaveChanges();
		}

		internal static IEnumerable<InvoiceRuleActivity> GetByActivityID(int activityID) => DB.InvoiceRuleActivities
					.Where(ia => ia.ActivityID == activityID);

		#endregion INVOICE RULE ACTIVITY SPECIFIC
	}
}