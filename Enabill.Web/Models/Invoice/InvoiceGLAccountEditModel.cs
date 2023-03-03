using Enabill.Models;

namespace Enabill.Web.Models
{
	public class InvoiceGLAccountEditModel
	{
		#region INITIALIZATION

		public InvoiceGLAccountEditModel(int gLAccountID)
		{
			this.GLAccount = gLAccountID == 0 ? null : GLAccount.GetGLAccountByID(gLAccountID);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public GLAccount GLAccount { get; internal set; }

		#endregion PROPERTIES
	}
}