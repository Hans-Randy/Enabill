using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class InvoiceStatusRepo : BaseRepo
	{
		#region INVOICE STATUS SPECIFIC

		public InvoiceStatus GetByID(int invStatusID) => DB.InvoiceStatus
					.SingleOrDefault(i => i.InvoiceStatusID == invStatusID);

		#endregion INVOICE STATUS SPECIFIC
	}
}