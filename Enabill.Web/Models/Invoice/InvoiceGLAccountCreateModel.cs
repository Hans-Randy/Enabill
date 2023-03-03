using Enabill.Models;

namespace Enabill.Web.Models
{
	public class InvoiceGLAccountCreateModel
	{
		#region INITIALIZATION

		public InvoiceGLAccountCreateModel()
		{
			//GLAccount = GLAccountID == 0 ? null : GLAccount.GetGLAccountByID(GLAccountID);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public GLAccount GLAccount { get; internal set; }

		#endregion PROPERTIES
	}
}