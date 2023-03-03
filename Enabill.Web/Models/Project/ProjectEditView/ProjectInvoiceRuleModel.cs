using System.Collections.Generic;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class ProjectInvoiceRuleModel
	{
		#region INITIALIZATION

		public ProjectInvoiceRuleModel(Project project)
		{
			this.Project = project;
			this.InvoiceRuleClients = this.LoadClientModels(this.Project.ClientID, this.Project.ProjectID);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public Project Project { get; private set; }

		public List<InvoiceRuleClientModel> InvoiceRuleClients { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<InvoiceRuleClientModel> LoadClientModels(int clientID, int projectID)
		{
			var model = new List<InvoiceRuleClientModel>();

			int? selectedClientID = clientID;

			if (selectedClientID.HasValue && clientID != 0)
			{
				var client = ClientRepo.GetByID(selectedClientID.Value);

				if (client != null)
				{
					model.Add(new InvoiceRuleClientModel(client, true, true, true, true, true, true, true, true, projectID));
				}
			}

			return model;
		}

		#endregion FUNCTIONS
	}
}