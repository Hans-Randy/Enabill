using Enabill.Models;

namespace Enabill.Repos
{
	public class WebHistoryRepo : BaseRepo
	{
		#region WEB HISTORY SPECIFIC

		internal static void Save(WebHistory webHistory)
		{
			if (webHistory.WebHistoryID == 0)
				DB.WebHistories.Add(webHistory);

			DB.SaveChanges();
		}

		#endregion WEB HISTORY SPECIFIC
	}
}