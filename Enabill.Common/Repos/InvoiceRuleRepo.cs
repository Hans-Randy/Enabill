using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class InvoiceRuleRepo : BaseRepo
	{
		#region INVOICE RULE SPECIFIC

		internal static void Save(InvoiceRule invoiceRule)
		{
			if (invoiceRule.InvoiceRuleID == 0)
				DB.InvoiceRules.Add(invoiceRule);

			DB.SaveChanges();
		}

		internal static void Delete(InvoiceRule invoiceRule)
		{
			var lines = GetInvoiceRuleLines(invoiceRule.InvoiceRuleID).ToList();

			for (int i = 0; i < lines.Count; i++)
			{
				DB.InvoiceRuleLines.Remove(lines[0]);
			}

			var activities = GetActivities(invoiceRule.InvoiceRuleID).ToList();
			RemoveActivities(invoiceRule.InvoiceRuleID, activities.Select(a => a.ActivityID).ToList());
			DB.SaveChanges();

			DB.InvoiceRules.Remove(invoiceRule);
			DB.SaveChanges();
		}

		internal static InvoiceRule GetByID(int irID) => DB.InvoiceRules
					.SingleOrDefault(ir => ir.InvoiceRuleID == irID);

		internal static IEnumerable<InvoiceRule> GetAll(DateTime refDate) => DB.InvoiceRules
					.Where(ir => (ir.DateTo == null || ir.DateTo >= refDate.Date) && (ir.DateFrom <= refDate.Date));

		internal static IEnumerable<InvoiceRule> GetAll(int financePeriod)
		{
			int year = int.Parse(financePeriod.ToString().Substring(0, 4));
			int month = int.Parse(financePeriod.ToString().Substring(4, 2));
			var period = new DateTime(year, month, 1);

			return DB.InvoiceRules
					.Where(ir => ir.DateFrom <= period && (!ir.DateTo.HasValue || ir.DateTo >= period));
		}

		internal static IEnumerable<InvoiceRule> GetForClient(int clientID) => DB.InvoiceRules
					.Where(ir => ir.ClientID == clientID);

		internal static IEnumerable<InvoiceRule> GetForClient(int clientID, int billingMethodID, bool status)
		{
			var today = DateTime.Today;

			if (status)
			{
				return DB.InvoiceRules
				   .Where(ir => ir.ClientID == clientID && (ir.DateTo > today || ir.DateTo == null) && (ir.BillingMethodID & billingMethodID) > 0);
			}
			else
			{
				return DB.InvoiceRules
				  .Where(ir => ir.ClientID == clientID && ir.DateTo != null && ir.DateTo <= today && (ir.BillingMethodID & billingMethodID) > 0);
			}
		}

		internal static IEnumerable<InvoiceRule> GetForClientAndProject(int clientID, int billingMethodID, bool status, int projectID)
		{
			var today = DateTime.Today;

			if (status)
			{
				return DB.InvoiceRules
						.Where(ir => ir.ClientID == clientID && (ir.DateTo > today || ir.DateTo == null) && (ir.BillingMethodID & billingMethodID) > 0 && ir.ProjectID == projectID);
			}
			else
			{
				return DB.InvoiceRules
						.Where(ir => ir.ClientID == clientID && ir.DateTo != null && ir.DateTo <= today && (ir.BillingMethodID & billingMethodID) > 0 && ir.ProjectID == projectID);
			}
		}

		#endregion INVOICE RULE SPECIFIC

		#region INVOICE RULE LINES

		internal static IEnumerable<InvoiceRuleLine> GetInvoiceRuleLines(int invoiceRuleID) => DB.InvoiceRuleLines
					.Where(irl => irl.InvoiceRuleID == invoiceRuleID);

		#endregion INVOICE RULE LINES

		#region INVOICE

		internal static List<Invoice> GetInvoices(int invoiceRuleID) => DB.Invoices.Where(i => i.InvoiceRuleID == invoiceRuleID).OrderBy(i => i.InvoiceDate).ToList();

		internal static Invoice GetInvoiceForDate(int invoiceRuleID, DateTime refDate) => DB.Invoices
						.Where
						(
							i =>
							i.InvoiceRuleID == invoiceRuleID
							//&& i.InvoiceStatusID == (int)InvoiceStatusType.Open  // Removed since we always want to get the invoice, and test the status ourselves
							&& i.DateFrom <= refDate
						 )
						 .OrderByDescending(i => i.DateFrom)
						 .ToList()
						 .FirstOrDefault();

		#endregion INVOICE

		#region PROJECT

		public static IEnumerable<InvoiceRule> GetByProjectID(int projectID)
		{
			var currentDate = DateTime.Today;

			return from ir in DB.InvoiceRules
				   where ir.ProjectID == projectID
				   && (ir.DateTo == null || ir.DateTo > currentDate)
				   select ir;
		}

		#endregion PROJECT

		#region ACTIVITIES

		internal static IEnumerable<Activity> GetActivities(int invoiceRuleID) => DB.InvoiceRuleActivities
				.Where(x => x.InvoiceRuleID == invoiceRuleID)
				.Select(x => x.Activity);

		internal static IEnumerable<Activity> GetActivitiesForProject(int? projectID)
		{
			if (!projectID.HasValue)
				return new List<Activity>();

			return from a in DB.Activities
				   where a.ProjectID == projectID
				   select a;
		}

		internal static void AddActivity(int invoiceRuleID, int activityID)
		{
			if (DB.InvoiceRuleActivities.Any(ia => ia.InvoiceRuleID == invoiceRuleID && ia.ActivityID == activityID))
				return;

			DB.InvoiceRuleActivities.Add(new InvoiceRuleActivity() { InvoiceRuleID = invoiceRuleID, ActivityID = activityID });
			DB.SaveChanges();
		}

		internal static void AddActivities(int invoiceRuleID, IEnumerable<Activity> activities)
		{
			foreach (var activity in activities)
			{
				if (DB.InvoiceRuleActivities.Any(ia => ia.InvoiceRuleID == invoiceRuleID && ia.ActivityID == activity.ActivityID))
					continue;

				DB.InvoiceRuleActivities.Add(new InvoiceRuleActivity() { InvoiceRuleID = invoiceRuleID, ActivityID = activity.ActivityID });
			}

			DB.SaveChanges();
		}

		internal static void RemoveActivities(int invoiceRuleID, List<int> activities)
		{
			foreach (int activityID in activities)
			{
				var invoiceRuleActivity = DB.InvoiceRuleActivities.Where(ira => ira.InvoiceRuleID == invoiceRuleID && ira.ActivityID == activityID).Single();
				DB.InvoiceRuleActivities.Remove(invoiceRuleActivity);
			}

			DB.SaveChanges();
		}

		#endregion ACTIVITIES

		#region BCC CONTACTS

		internal static IEnumerable<Contact> GetContacts(int invoiceRuleID) => DB.InvoiceRuleContacts
				.Where(x => x.InvoiceRuleID == invoiceRuleID)
				.Select(x => x.Contact);

		internal static void AddBccContact(int invoiceRuleID, int contactID)
		{
			if (DB.InvoiceRuleContacts.Any(ic => ic.InvoiceRuleID == invoiceRuleID && ic.ContactID == contactID))
				return;

			DB.InvoiceRuleContacts.Add(new InvoiceRuleContact { InvoiceRuleID = invoiceRuleID, ContactID = contactID });
			DB.SaveChanges();
		}

		internal static void AddBccContacts(int invoiceRuleID, IEnumerable<Contact> contacts)
		{
			foreach (var contact in contacts)
			{
				if (DB.InvoiceRuleContacts.Any(ic => ic.InvoiceRuleID == invoiceRuleID && ic.ContactID == contact.ContactID))
					continue;

				DB.InvoiceRuleContacts.Add(new InvoiceRuleContact { InvoiceRuleID = invoiceRuleID, ContactID = contact.ContactID });
			}

			DB.SaveChanges();
		}

		internal static void RemoveBccContacts(int invoiceRuleID, List<Contact> contacts)
		{
			foreach (var contact in contacts)
			{
				var invoiceRuleContact = DB.InvoiceRuleContacts.Where(irc => irc.InvoiceRuleID == invoiceRuleID && irc.ContactID == contact.ContactID).Single();
				DB.InvoiceRuleContacts.Remove(invoiceRuleContact);
			}

			DB.SaveChanges();
		}

		internal static void RemoveBccContacts(int invoiceRuleID, List<int> contactIDs)
		{
			foreach (int contactID in contactIDs)
			{
				var invoiceRuleContact = DB.InvoiceRuleContacts.Where(irc => irc.InvoiceRuleID == invoiceRuleID && irc.ContactID == contactID).Single();
				DB.InvoiceRuleContacts.Remove(invoiceRuleContact);
			}

			DB.SaveChanges();
		}

		#endregion BCC CONTACTS
	}
}