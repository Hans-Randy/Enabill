using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class FrequencyRepo : BaseRepo
	{
		#region FREQUENCY SPECIFIC

		public Frequency GetByID(int frequencyID) => DB.Frequencies
					.SingleOrDefault(i => i.FrequencyID == frequencyID);

		public static IEnumerable<Frequency> GetAll() => DB.Frequencies.OrderBy(f => f.FrequencyName);

		#endregion FREQUENCY SPECIFIC
	}
}