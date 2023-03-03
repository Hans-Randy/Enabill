using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class BillingMethodRepo : BaseRepo
	{
		#region BILLING METHOD SPECIFIC

		public static BillingMethod GetByID(int billingMethodID) => DB.BillingMethods
					.SingleOrDefault(b => b.BillingMethodID == billingMethodID);

		public static IEnumerable<BillingMethod> GetListByID(int iDs) => DB.BillingMethods
					.Where(b => (b.BillingMethodID & iDs) > 0);

		public static BillingMethod GetByName(string billingMethod) => DB.BillingMethods
					.SingleOrDefault(r => r.BillingMethodName == billingMethod);

		public static IEnumerable<BillingMethod> GetAll() => DB.BillingMethods;

		#endregion BILLING METHOD SPECIFIC
	}
}