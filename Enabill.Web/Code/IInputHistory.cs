using System;

namespace Enabill.Web
{
	public interface IInputHistory
	{
		DateTime? GetDateTime(HistoryItemType historyItem, DateTime? defaultInput);
	}
}