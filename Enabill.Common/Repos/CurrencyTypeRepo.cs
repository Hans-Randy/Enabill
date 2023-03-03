using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class CurrencyTypeRepo :BaseRepo
	{
			#region CURRENCY TYPE SPECIFIC

			public static CurrencyType GetByID(int currencyID) => DB.CurrencyType
						.SingleOrDefault(d => d.CurrencyTypeID == currencyID);

			public static CurrencyType GetByName(string currencyName) => DB.CurrencyType
						.SingleOrDefault(d => d.CurrencyName == currencyName);


		public static CurrencyType GetByCode(string currencyCode) => DB.CurrencyType
					.SingleOrDefault(d => d.CurrencyISO == currencyCode);
		public static IEnumerable<CurrencyType> GetAll() => DB.CurrencyType;

		#endregion CURRENCY TYPE SPECIFIC

	}
}
