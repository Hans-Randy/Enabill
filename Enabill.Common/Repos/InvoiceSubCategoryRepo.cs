using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class InvoiceSubCategoryRepo : BaseRepo
	{
		#region INVOICE SUBCATEGORY

		public static InvoiceSubCategory GetByID(int invoiceSubCategoryID) => DB.InvoiceSubCategories
					.SingleOrDefault(i => i.InvoiceSubCategoryID == invoiceSubCategoryID);

		public static InvoiceSubCategory GetByRefCode(string refCode) => DB.InvoiceSubCategories
					.FirstOrDefault(c => c.RefCode == refCode);

		public static IEnumerable<InvoiceSubCategory> GetAll() => DB.InvoiceSubCategories;

		public static IEnumerable<InvoiceSubCategory> GetByCategoryID(int? invoiceCategoryID) => DB.InvoiceSubCategories
					.Where(isc => isc.InvoiceCategoryID == invoiceCategoryID);

		public static Dictionary<int, string> GetInvoiceSubCategoriesExtendedNames()
		{
			var model = new Dictionary<int, string>();

			foreach (var ic in InvoiceCategoryRepo.GetAll().ToList())
			{
				GetByCategoryID(ic.InvoiceCategoryID)
						.ToList()
						.ForEach(isc =>
									model.Add(isc.InvoiceSubCategoryID, ic.CategoryName + ": " + isc.RefCode + " - " + isc.SubCategoryName)
				);
			}

			return model;
		}

		public static string GetInvoiceSubCategoryExtendedName(int invoiceID) => (from i in DB.Invoices
																				  join isc in DB.InvoiceSubCategories on i.InvoiceSubCategoryID equals isc.InvoiceSubCategoryID
																				  join ic in DB.InvoiceCategories on isc.InvoiceCategoryID equals ic.InvoiceCategoryID
																				  where i.InvoiceID == invoiceID
																				  select ic.CategoryName + ": " + isc.RefCode + " - " + isc.SubCategoryName
				   )
				   .SingleOrDefault();

		internal static void Save(InvoiceSubCategory invoiceSubCategory)
		{
			if (invoiceSubCategory.InvoiceSubCategoryID == 0)
				DB.InvoiceSubCategories.Add(invoiceSubCategory);

			DB.SaveChanges();
		}

		#endregion INVOICE SUBCATEGORY
	}
}