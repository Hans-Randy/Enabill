using System;

namespace Enabill.Models.Dto
{
	public class RatesModel
	{
		#region PROPERTIES

		public int ActivityID { get; set; }
		public int ClientID { get; set; }
		public int ProjectID { get; set; }
		public int UserID { get; set; }

		public double? ChargeRate { get; set; }
		public double? HourlyRate { get; set; }

		public string ActivityName { get; set; }
		public string ClientName { get; set; }
		public string FullName { get; set; }
		public string ProjectName { get; set; }

		public DateTime? ConfirmedEndDate { get; set; }
		public DateTime? ScheduledEndDate { get; set; }

		public string Currency { get;  internal set; }
		#endregion PROPERTIES
	}
}