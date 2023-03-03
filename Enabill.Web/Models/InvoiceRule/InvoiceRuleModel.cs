using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Web.Models
{
	public class InvoiceRuleModel
	{
		#region INITIALIZATION

		public InvoiceRuleModel(InvoiceRule invRule, Client client, Project project)
		{
			this.InvoiceRule = invRule;
			this.Client = client;
			this.Project = project;
			this.InvoiceRuleLines = this.SetupInvoiceRuleLines(invRule);
			this.InvoiceRuleContacts = this.SetupInvoiceRuleContacts(invRule, client);

			this.SetPropertiesForInvoiceRule(invRule, client);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public Client Client { get; private set; }
		public Project Project { get; private set; }
		public InvoiceRule InvoiceRule { get; private set; }

		public List<ActivityDetail> ActivityFixedCostActivities { get; private set; }
		public List<ActivityDetail> SlaActivities { get; private set; }
		public List<ActivityDetail> TMActivities { get; private set; }
		public List<ContactSelectModel> InvoiceRuleContacts { get; private set; }
		public List<InvoiceRuleLine> FixedCostAccrualBreakdown { get; private set; }
		public List<InvoiceRuleLine> InvoiceRuleLines { get; private set; }

		#endregion PROPERTIES

		#region FUNCTION

		private List<InvoiceRuleLine> SetupInvoiceRuleLines(InvoiceRule invRule)
		{
			var model = new List<InvoiceRuleLine>();

			if (!invRule.IsFixedCost)
				return model;

			return model;
		}

		private List<ContactSelectModel> SetupInvoiceRuleContacts(InvoiceRule invRule, Client client)
		{
			var model = new List<ContactSelectModel>();

			foreach (var contact in client.GetContacts())
			{
				contact.ContactName = Enabill.Helpers.DecryptString(contact.ContactName, Enabill.Code.Constants.PASSPHRASE);

				if (contact.ContactID == invRule.DefaultContactID)
					continue;

				model.Add(new ContactSelectModel { Contact = contact, IsSelected = false });
			}

			foreach (var contact in invRule.Contacts)
			{
				contact.ContactName = Enabill.Helpers.DecryptString(contact.ContactName, Enabill.Code.Constants.PASSPHRASE);

				if (contact.ContactID == invRule.DefaultContactID)
					continue;

				model.Single(c => c.Contact == contact).IsSelected = true;
			}

			return model;
		}

		private void SetPropertiesForInvoiceRule(InvoiceRule invRule, Client client)
		{
			this.TMActivities = this.SlaActivities = new List<ActivityDetail>();
			this.FixedCostAccrualBreakdown = new List<InvoiceRuleLine>();
			this.ActivityFixedCostActivities = new List<ActivityDetail>();

			switch ((BillingMethodType)invRule.BillingMethodID)
			{
				case BillingMethodType.TimeMaterial:
					this.SetUpTMInvoiceRule(invRule, client);
					break;

				case BillingMethodType.FixedCost:
					this.SetUpFixedCostInvoiceRule(invRule, client);
					break;

				case BillingMethodType.MonthlyFixedCost:
					this.SetUpMonthlyFixedCostInvoiceRule(invRule, client);
					break;

				case BillingMethodType.ActivityFixedCost:
					this.SetUpActivityFixedCostInvoiceRule(invRule, client);
					break;

				case BillingMethodType.SLA:
					this.SetUpSLAInvoiceRule(invRule, client);
					break;

				default:
					throw new NullReferenceException("TODO");
			}
		}

		private void SetUpTMInvoiceRule(InvoiceRule invRule, Client client)
		{
			client.GetActivities(BillingMethodType.TimeMaterial, null)
					.ForEach(a => this.TMActivities.Add(a));

			invRule.GetDetailedActivities()
					.ForEach(a => this.TMActivities.Add(a));
		}

		private void SetUpFixedCostInvoiceRule(InvoiceRule invRule, Client client) => this.FixedCostAccrualBreakdown.AddRange(invRule.InvoiceRuleLines);

		private void SetUpMonthlyFixedCostInvoiceRule(InvoiceRule invRule, Client client)
		{
			//TODO - what should happen here?
		}

		private void SetUpActivityFixedCostInvoiceRule(InvoiceRule invRule, Client client)
		{
			client.GetActivities(BillingMethodType.ActivityFixedCost, null)
					.ForEach(a => this.ActivityFixedCostActivities.Add(a));

			invRule.GetDetailedActivities()
					.ForEach(a => this.ActivityFixedCostActivities.Add(a));
		}

		private void SetUpSLAInvoiceRule(InvoiceRule invRule, Client client)
		{
			var model = new List<ActivityDetail>();

			client.GetActivities(BillingMethodType.SLA, null)
					.ForEach(a => model.Add(a));

			invRule.GetDetailedActivities()
					.ForEach(a => model.Add(a));

			this.SlaActivities = model;
		}

		#endregion FUNCTION
	}
}