using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class InvoiceCategoryRepo : BaseRepo
	{
		#region INVOICE CATEGORY

		//internal static InvoiceCategory GetByID(int invoiceCategoryID) => DB.InvoiceCategories
		//			.SingleOrDefault(ic => ic.InvoiceCategoryID == invoiceCategoryID);

		public static InvoiceCategory GetByID(int invoiceCategoryID) => DB.InvoiceCategories
					.SingleOrDefault(ic => ic.InvoiceCategoryID == invoiceCategoryID);

		public static IEnumerable<InvoiceCategory> GetAll() => DB.InvoiceCategories;

		internal static void Save(InvoiceCategory invoiceCategory)
		{
			if (!DB.InvoiceCategories.Any(ic => ic.InvoiceCategoryID == invoiceCategory.InvoiceCategoryID))
				DB.InvoiceCategories.Add(invoiceCategory);

			DB.SaveChanges();
		}

		#endregion INVOICE CATEGORY

		#region INVOICE SUB CATEGORY

		internal static InvoiceSubCategory GetSubCatByID(int invSubCatID) => DB.InvoiceSubCategories.SingleOrDefault(i => i.InvoiceSubCategoryID == invSubCatID);

		#endregion INVOICE SUB CATEGORY
	}
}