using System.Collections.Generic;

namespace Enabill.Models
{
	public interface IProject
	{
		#region FUNCTIONS

		List<string> GetDistinctProjectNames();

		List<string> GetDistinctProjectNamesForClientID(int clientID);

		#endregion FUNCTIONS
	}
}