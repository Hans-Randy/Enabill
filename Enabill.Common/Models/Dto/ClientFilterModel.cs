using System.Collections.Generic;

namespace Enabill.Models.Dto
{
	public class ClientFilterModel
	{
		#region PROPERTIES

		public bool IsActive { get; set; }

		public string FilterText { get; set; }

		public IList<Client> ClientList { get; set; }

		#endregion PROPERTIES
	}
}