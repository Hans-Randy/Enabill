using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class InvoiceRuleLineRepo : BaseRepo
	{
		#region INVOICE RULE LINE SPECIFIC

		internal static void SaveAll(List<InvoiceRuleLine> lines)
		{
			foreach (var line in lines)
			{
				if (line.InvoiceRuleLineID == 0)
					DB.InvoiceRuleLines.Add(line);
			}

			DB.SaveChanges();
		}

		internal static void Save(InvoiceRuleLine line)
		{
			if (line.InvoiceRuleLineID == 0)
				DB.InvoiceRuleLines.Add(line);

			DB.SaveChanges();
		}

		internal static void Delete(InvoiceRuleLine line)
		{
			DB.InvoiceRuleLines.Remove(line);
			DB.SaveChanges();
		}

		internal static void DeleteRange(List<InvoiceRuleLine> lines)
		{
			foreach (var line in lines)
			{
				Delete(line);
			}
		}

		#endregion INVOICE RULE LINE SPECIFIC

		#region INVOICE RULE

		internal static IEnumerable<InvoiceRuleLine> FetchByInvoiceRule(int invoiceRuleID) => DB.InvoiceRuleLines.Where(l => l.InvoiceRuleID == invoiceRuleID).ToList();

		internal static InvoiceRuleLine FetchByInvoiceRulePeriod(int invoiceRuleID, int period) => DB.InvoiceRuleLines.SingleOrDefault(l => l.InvoiceRuleID == invoiceRuleID && l.Period == period);

		internal static InvoiceRuleLine FetchByInvoice(int invoiceID) => DB.InvoiceRuleLines.SingleOrDefault(l => l.InvoiceID == invoiceID);

		#endregion INVOICE RULE
	}
}