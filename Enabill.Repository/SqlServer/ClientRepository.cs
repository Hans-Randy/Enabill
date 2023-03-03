using System.Collections.Generic;
using System.Linq;
using Alacrity.DataAccess;
using Dapper;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repository.Interfaces;

namespace Enabill.Repository.SqlServer
{
	public class ClientRepository : BaseRepository, IClientRepository
	{
		private IList<Client> clients;
		public IList<Client> Clients => this.clients;
		private Client client;

		public Client Client
		{
			get => this.client;
			set => this.client = value;
		}

		public ClientRepository()
		{
		}

		public ClientRepository(IDbManager dbManager)
			: base(dbManager)
		{
		}

		public void GetClients() => this.clients = this.DbManager.Connection.Query<Client>("SELECT [ClientID],[ClientName],[IsActive],[RegisteredName],[VATNo],[VATRate],[PostalAddress1],[PostalAddress2],[PostalAddress3],[PostalCode],[LastModifiedBy],[AccountCode],[SupportEmailAddress] FROM [dbo].[Clients]").ToList();

		public void GetClient(int clientId) => this.client = this.DbManager.Connection.Query<Client>("SELECT [ClientID],[ClientName],[IsActive],[RegisteredName],[VATNo],[VATRate],[PostalAddress1],[PostalAddress2],[PostalAddress3],[PostalCode],[LastModifiedBy],[AccountCode],[SupportEmailAddress] FROM [dbo].[Clients] WHERE ClientID=@ClientId", new { ClientId = clientId }).FirstOrDefault();

		public void SearchClients(ClientFilterModel clientFilterModel)
		{
			this.GetClients();
			if (!string.IsNullOrEmpty(clientFilterModel.FilterText))
			{
				string filteredText = clientFilterModel.FilterText.ToLower();

				if (this.clients.Any(c => c.ClientName?.ToLower().Contains(filteredText) == true))
				{
					this.clients = this.clients.Where(c => c.ClientName?.ToLower().Contains(filteredText) == true).ToList();
				}

				if (this.clients.Any(c => c.RegisteredName?.ToLower().Contains(filteredText) == true))
				{
					this.clients = this.clients.Where(c => c.RegisteredName?.ToLower().Contains(filteredText) == true).ToList();
				}

				if (this.clients.Any(c => c.AccountCode?.ToLower().Contains(filteredText) == true))
				{
					this.clients = this.clients.Where(c => c.AccountCode?.ToLower().Contains(filteredText) == true).ToList();
				}

				if (this.clients.Any(c => c.VATNo?.ToLower().Contains(filteredText) == true))
				{
					this.clients = this.clients.Where(c => c.VATNo?.ToLower().Contains(filteredText) == true).ToList();
				}
			}

			this.clients = this.clients.Where(c => c.IsActive == clientFilterModel.IsActive).ToList();
		}
	}
}