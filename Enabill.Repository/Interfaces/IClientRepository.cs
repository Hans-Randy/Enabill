using System.Collections.Generic;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repository.Interfaces
{
	public interface IClientRepository : IBaseRepository
	{
		IList<Client> Clients { get; }
		Client Client { get; set; }

		void GetClients();

		void GetClient(int clientId);

		void SearchClients(ClientFilterModel clientFilterModel);
	}
}