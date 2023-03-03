using System.Collections.Generic;

namespace Enabill.Models.Dto
{
	public class InvoiceExtendedDetail
	{
		#region PROPERTIES

		public string BillingMethodName { get; internal set; }
		public string InvoiceCategoryName { get; internal set; }
		public string InvoiceSubCategoryName { get; internal set; }

		public Client Client { get; internal set; }
		public Invoice Invoice { get; internal set; }

		public List<WorkAllocationExtendedModel> WorkAllocations { get; internal set; }

		#endregion PROPERTIES
	}
}