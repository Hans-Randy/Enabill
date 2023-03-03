using System.Collections.Generic;

namespace Enabill.Models
{
	public interface IActivity
	{
		#region FUNCTIONS

		List<string> GetDistinctActivityNames();

		List<string> GetDistinctActivityNamesForClientID(int clientID);

		List<string> GetDistinctActivityNamesForProjectName(string projectName);

		#endregion FUNCTIONS
	}
}