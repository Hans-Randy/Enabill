using System;

namespace Enabill.Models.Dto
{
	public class InvoiceDetail
	{
		#region PROPERTIES

		public bool IsClosed;
		public bool IsTimeApproved;
		public bool PrintCredits;
		public bool PrintTimeSheet;

		public int InvoiceID;
		public int Period;
		public int? BillingMethodID;
		public int? ClientID;
		public int? HoursPaidFor;
		public int? InvoiceContactID;
		public int? InvoiceRuleID;
		public int? InvoiceStatusID;
		public int? InvoiceSubCategoryID;
		public int? PrintLayoutTypeID;
		public int? PrintOptionTypeID;
		public int? PrintTicketRemarkOptionID;
		public int? ProjectID;

		public double? AccrualExclVAT;
		public double? InvoiceAmountExclVAT;
		public double? InvoiceAmountInclVAT;
		public double? ProjectedAmountExcl;
		public double? ProvisionalAccrualAmount;
		public double? ProvisionalIncomeAmount;
		public double? VATAmount;
		public double? VATRate;

		public string ClientAccountCode;
		public string ClientDepartmentCode;
		public string ClientName;
		public string CustomerRef;
		public string Description;
		public string ExternalInvoiceNo;
		public string InvoiceContactName;
		public string OrderNo;
		public string OurRef;
		public string UserCreated;
		public string UserInvoiced;
		public string GLAccountCode;

		public DateTime? DateCreated;
		public DateTime? DateInvoiced;
		public DateTime? DateFrom;
		public DateTime? DateTo;
		public DateTime? InvoiceDate;

		#endregion PROPERTIES
	}
}