namespace Enabill.Models.Dto
{
	public class ForecastWithInvoiceExtendedModel
	{
		#region PROPERTIES

		public int BillingMethod;
		public int DivisionID;
		public int InvoiceID;
		public int Period;
		public int RegionID;

		public double ActualAmount;
		public double ForecastAmount;
		public double ProjectedAmount;

		public string Client;
		public string Division;
		public string OrderNo;
		public string Region;

		#endregion PROPERTIES
	}
}