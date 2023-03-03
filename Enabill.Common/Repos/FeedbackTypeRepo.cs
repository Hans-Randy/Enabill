using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class FeedbackTypeRepo : BaseRepo
	{
		#region FEEDBACK TYPE SPECIFIC

		internal static FeedbackType GetByID(int feedbackTypeID) => DB.FeedbackTypes
					.SingleOrDefault(t => t.FeedbackTypeID == feedbackTypeID);

		internal static IEnumerable<FeedbackType> GetAll() => DB.FeedbackTypes;

		#endregion FEEDBACK TYPE SPECIFIC
	}
}