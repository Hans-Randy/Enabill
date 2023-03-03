namespace Enabill.Web.Models
{
	public class xxInvoiceLineModel
	{
		/*
		public xxInvoiceLineModel(xxInvoiceLine invoiceLine)
		{
			if (invoiceLine == null)
				throw new ArgumentNullException("Invoice line cannot be null");
			InvoiceLine = invoiceLine;

			TempID = invoiceLine.InvoiceLineID > 0 ? (Guid?)null : Guid.NewGuid();
		}

		public xxInvoiceLine InvoiceLine { get; private set; }
		public Guid? TempID { get; set; }

		public string LineID
		{
			get
			{
				if (TempID == null)
					return InvoiceLine.InvoiceLineID.ToString();
				else
					return TempID.ToString();
			}
		}

		public string LineType
		{
			get
			{
				if (TempID == null)
					return "LineID";
				else
					return "TempID";
			}
		}
		 * */
	}
}