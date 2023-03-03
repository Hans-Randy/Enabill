using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class InvoiceListModel
	{
		#region INITIALIZATION

		public InvoiceListModel()
		{
			this.Invoices = Invoice.GetAll().OrderByDescending(i => i.InvoiceDate).ToList();
			this.InvoiceClients = ClientRepo.GetAll().OrderBy(c => c.ClientName).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<Client> InvoiceClients { get; private set; }
		public List<Invoice> Invoices { get; private set; }

		#endregion PROPERTIES
	}
}