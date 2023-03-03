using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class InvoiceGLAccountsModel
	{
		#region INITIALIZATION

		public InvoiceGLAccountsModel()
		{
			this.GLAccounts = GLAccountRepo.GetAll().OrderBy(g => g.GLAccountCode).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<GLAccount> GLAccounts { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public static GLAccount GetByID(int gLAccountID) => GLAccountRepo.GetByID(gLAccountID);

		#endregion FUNCTIONS
	}
}