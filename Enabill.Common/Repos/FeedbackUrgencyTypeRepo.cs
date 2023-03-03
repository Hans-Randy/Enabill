using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class FeedbackUrgencyTypeRepo : BaseRepo
	{
		#region FEEDBACK URGENCY SPECIFIC

		internal static FeedbackUrgencyType GetByID(int feedbackUrgencyTypeID) => DB.FeedbackUrgencyTypes
					.SingleOrDefault(x => x.FeedbackUrgencyTypeID == feedbackUrgencyTypeID);

		internal static IEnumerable<FeedbackUrgencyType> GetAll() => DB.FeedbackUrgencyTypes;

		#endregion FEEDBACK URGENCY SPECIFIC
	}
}