using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class ContactRepo : BaseRepo
	{
		#region CONTACT SPECIFIC

		public static Contact GetByID(int contactID) => DB.Contacts
					.SingleOrDefault(c => c.ContactID == contactID);

		internal static Contact GetByName(string contactName) => DB.Contacts
					.SingleOrDefault(r => r.ContactName == contactName);

		internal static IEnumerable<Contact> GetAll() => DB.Contacts;

		public static List<Contact> FilterByName(string nameFilter)
		{
			if (nameFilter == null)
				nameFilter = string.Empty;

			return DB.Contacts
					.Where(c => c.ContactName.Contains(nameFilter))
					.OrderBy(c => c.ContactName)
					.ToList();
		}

		public static void DeleteContact(Contact contact)
		{
			DB.Contacts.Remove(contact);
			DB.SaveChanges();
		}

		#endregion CONTACT SPECIFIC

		#region INVOICE

		internal static IEnumerable<Invoice> GetAllInvoicesForwhichContactIsLinked(int contactID) => DB.Invoices
					.Where(i => i.InvoiceContactID == contactID);

		internal static IEnumerable<Invoice> GetAllInvoicesForWhichContactIsLinkedByCC(int contactID) => from ic in DB.InvoiceContacts
																										 join i in DB.Invoices on ic.InvoiceID equals i.InvoiceID
																										 where ic.ContactID == contactID
																										 select i;

		#endregion INVOICE

		#region INVOICE RULE

		internal static IEnumerable<InvoiceRule> GetAllInvoiceRulesForWhichContactIsLinkedAsDefaultContact(int contactID) => from ir in DB.InvoiceRules
																															 where ir.DefaultContactID == contactID
																															 select ir;

		internal static IEnumerable<InvoiceRule> GetAllInvoiceRulesForWhichContactIsLinkedByCC(int contactID) => from irc in DB.InvoiceRuleContacts
																												 join ir in DB.InvoiceRules on irc.InvoiceRuleID equals ir.InvoiceRuleID
																												 where irc.ContactID == contactID
																												 select ir;

		#endregion INVOICE RULE
	}
}