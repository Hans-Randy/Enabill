using System;

namespace Enabill.Web
{
	public class InputHistoryWrapper : IInputHistory
	{
		public DateTime? GetDateTime(HistoryItemType historyItem, DateTime? defaultInput) => InputHistory.GetDateTime(historyItem, defaultInput);
	}
}