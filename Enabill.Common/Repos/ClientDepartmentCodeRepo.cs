using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public abstract class ClientDepartmentCodeRepo : BaseRepo
	{
		#region Client Department Code SPECIFIC

		public static List<ClientDepartmentCode> GetAll() => DB.ClientDepartmentCode
				.Where(i => i.IsActive).ToList();

		public static ClientDepartmentCode GetByID(int clientDepartmentCodeID) => DB.ClientDepartmentCode
					.SingleOrDefault(i => i.ClientDepartmentCodeID == clientDepartmentCodeID);

		public static List<ClientDepartmentCode> GetByClientID(int clientID) => DB.ClientDepartmentCode
					.Where(i => i.ClientID == clientID && i.IsActive).ToList();

		public static List<ClientDepartmentCode> GetAllByClientID(int clientID) => DB.ClientDepartmentCode
				   .Where(i => i.ClientID == clientID).ToList();

		public static void Delete(ClientDepartmentCode clientDepartmentCode)
		{
			DB.ClientDepartmentCode.Remove(clientDepartmentCode);
			DB.SaveChanges();
		}

		public static void Save(ClientDepartmentCode clientDepartmentCode)
		{
			if (clientDepartmentCode.ClientDepartmentCodeID <= 0)
				DB.ClientDepartmentCode.Add(clientDepartmentCode);

			DB.SaveChanges();
		}

		#endregion Client Department Code SPECIFIC
	}
}