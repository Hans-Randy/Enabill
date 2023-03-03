using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class InvoiceIndexFilterModel
	{
		public User UserRequesting { get; set; }
		public DateTime? DateFrom { get; set; }
		public DateTime? DateTo { get; set; }
		public int? InvoicePeriod { get; set; }
		public bool MyClients { get; set; }
		public bool ShowMyClientsCheckBox { get; set; }
		public bool TimeMaterial { get; set; }
		public bool FixedCost { get; set; }
		public bool SLA { get; set; }
		public bool Travel { get; set; }
		public bool AdHoc { get; set; }
		public bool MonthlyFixedCost { get; set; }
		public bool ActivityFixedCost { get; set; }
		public bool Open { get; set; }
		public bool InProgress { get; set; }
		public bool Ready { get; set; }
		public bool Complete { get; set; }
		public int? ClientId { get; set; }
	}
}