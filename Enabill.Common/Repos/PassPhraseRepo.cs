using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class PassPhraseRepo : BaseRepo
	{
		public static void Save(PassPhrase passPhrase)
		{
			if (passPhrase.PassPhraseID == 0)
			{
				DB.PassPhrases.Add(passPhrase);
			}

			DB.SaveChanges();
		}

		public static IEnumerable<PassPhrase> GetAll() => DB.PassPhrases;

		public static PassPhrase GetPassPhrase() => DB.PassPhrases.Take(1).SingleOrDefault();
	}
}