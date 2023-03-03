using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class ForecastInvoiceLinkRepo : BaseRepo
	{
		#region FORECASTINVOICELINK SPECIFIC

		public static IEnumerable<ForecastInvoiceLink> GetAll() => DB.ForecastInvoiceLinks;

		public static IEnumerable<ForecastInvoiceLink> GetAllByForecastDetailID(int forecastDetailID) => DB.ForecastInvoiceLinks.Where(i => i.ForecastDetailID == forecastDetailID);

		public static ForecastInvoiceLink GetAllByKey(int forecastDetailID, int invoiceID) => DB.ForecastInvoiceLinks.Where(i => i.ForecastDetailID == forecastDetailID && i.InvoiceID == invoiceID).SingleOrDefault();

		public static IEnumerable<ForecastInvoiceLink> GetAllByInvoiceID(int invoiceID) => DB.ForecastInvoiceLinks.Where(i => i.InvoiceID == invoiceID);

		public static IEnumerable<ForecastInvoiceLink> GetAllByForecastHeaderID(int forecastHeaderID) => from i in DB.ForecastInvoiceLinks
																										 join d in DB.ForecastDetails on i.ForecastDetailID equals d.ForecastDetailID
																										 where d.ForecastHeaderID == forecastHeaderID
																										 select i;

		public static void Save(ForecastInvoiceLink forecastInvoiceLink)
		{
			var existingForecastInvoiceLink = GetAllByKey(forecastInvoiceLink.ForecastDetailID, forecastInvoiceLink.InvoiceID);
			if (existingForecastInvoiceLink != null)
				return;

			if (forecastInvoiceLink.ForecastInvoiceLinkID == 0)
				DB.ForecastInvoiceLinks.Add(forecastInvoiceLink);

			DB.SaveChanges();
		}

		public static void Delete(ForecastInvoiceLink forecastInvoiceLink)
		{
			if (forecastInvoiceLink == null)
				return;

			DB.ForecastInvoiceLinks.Remove(forecastInvoiceLink);
			DB.SaveChanges();
		}

		public static IEnumerable<ForecastInvoiceLinkExtendedModel> GetForecastInvoiceLinkExtendedModel(int forecastHeaderID, int period, int billingMethod, int clientID) => (from i in DB.Invoices
																																											   join l in DB.ForecastInvoiceLinks on i.InvoiceID equals l.InvoiceID into il
																																											   from l in il.DefaultIfEmpty()
																																											   where i.Period == period && i.BillingMethodID == billingMethod && i.ClientID == clientID
																																											   select new ForecastInvoiceLinkExtendedModel
																																											   {
																																												   Invoice = i,
																																												   ForecastHeaderID = l.ForecastHeaderID == null ? 0 : l.ForecastHeaderID,
																																												   ForecastInvoiceLinkID = l.InvoiceID == null ? 0 : l.InvoiceID
																																											   })
																																												.OrderBy(i => i.Invoice.OrderNo)
																																												.Distinct();

		#endregion FORECASTINVOICELINK SPECIFIC
	}
}