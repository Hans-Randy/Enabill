using System.Collections.Generic;

namespace Enabill.Models.Dto
{
	public class InvoiceRuleExtendedDetail
	{
		#region PROPERTIES

		public string BillingMethodName { get; internal set; }
		public string ClientName { get; internal set; }
		public string InvoiceCategoryName { get; internal set; }
		public string InvoiceSubCategoryName { get; internal set; }

		public InvoiceRule InvoiceRule { get; internal set; }

		public List<string> Activities { get; internal set; }

		#endregion PROPERTIES
	}
}