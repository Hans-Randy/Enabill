using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

//using System.Data.Objects;

namespace Enabill.Repos
{
	public abstract class GLAccountRepo : BaseRepo
	{
		#region GL ACCOUNT SPECIFIC

		public static List<GLAccount> GetAll() => DB.GLAccounts.ToList();

		public static GLAccount GetByID(int gLAccountID) => DB.GLAccounts
					.SingleOrDefault(i => i.GLAccountID == gLAccountID);

		public static void Save(GLAccount gLAccount)
		{
			if (gLAccount.GLAccountID <= 0)
				DB.GLAccounts.Add(gLAccount);

			DB.SaveChanges();
		}

		#endregion GL ACCOUNT SPECIFIC
	}
}