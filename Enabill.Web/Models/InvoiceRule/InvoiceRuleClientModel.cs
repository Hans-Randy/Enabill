using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class InvoiceRuleClientModel
	{
		#region INITIALIZATION

		public InvoiceRuleClientModel(Client client, bool status, bool tM = false, bool fixedCost = false, bool monthlyFixedCost = false, bool activityFixedCost = false, bool sLA = false, bool travel = false, bool clienthasrule = true, int projectID = 0)
		{
			int billingMethodTotal = (tM ? (int)BillingMethodType.TimeMaterial : 0) + (fixedCost ? (int)BillingMethodType.FixedCost : 0) + (monthlyFixedCost ? (int)BillingMethodType.MonthlyFixedCost : 0) + (activityFixedCost ? (int)BillingMethodType.ActivityFixedCost : 0) + (sLA ? (int)BillingMethodType.SLA : 0) + (travel ? (int)BillingMethodType.Travel : 0);

			this.Client = client;

			if (projectID > 0)
			{
				this.InvoiceRules = InvoiceRule.GetForClientAndProject(client.ClientID, billingMethodTotal, status, projectID);
			}
			else
			{
				this.InvoiceRules = InvoiceRule.GetForClient(client.ClientID, billingMethodTotal, status);
			}
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public Client Client { get; private set; }

		public List<InvoiceRule> InvoiceRules { get; private set; }

		#endregion PROPERTIES
	}
}