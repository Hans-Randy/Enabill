using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class InvoiceRuleContactRepo : BaseRepo
	{
		#region INVOIECE RULE CONTACT SPECIFIC

		internal static void Save(int invoiceRuleID, int contactID)
		{
			if (DB.InvoiceRuleContacts.SingleOrDefault(ir => ir.InvoiceRuleID == invoiceRuleID && ir.ContactID == contactID) == null)
				return;

			var irc = new InvoiceRuleContact
			{
				InvoiceRuleID = invoiceRuleID,
				ContactID = contactID
			};

			DB.InvoiceRuleContacts.Add(irc);
			DB.SaveChanges();
		}

		#endregion INVOIECE RULE CONTACT SPECIFIC
	}
}