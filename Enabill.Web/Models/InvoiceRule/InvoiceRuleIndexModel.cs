using System.Collections.Generic;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class InvoiceRuleIndexModel
	{
		#region INITIALIZATION

		public InvoiceRuleIndexModel(int clientID)
		{
			this.TimeMaterial = InputHistory.Get(HistoryItemType.InvRuleTimeMaterial, true);
			this.FixedCost = InputHistory.Get(HistoryItemType.InvRuleFixedCost, true);
			this.MonthlyFixedCost = InputHistory.Get(HistoryItemType.InvRuleMonthlyFixedCost, true);
			this.ActivityFixedCost = InputHistory.Get(HistoryItemType.InvRuleActivityFixedCost, true);
			this.SLA = InputHistory.Get(HistoryItemType.InvRuleSLA, true);
			this.Travel = InputHistory.Get(HistoryItemType.InvRuleTravel, true);
			this.ClientHasRule = InputHistory.Get(HistoryItemType.InvRuleClientHasRule, true);
			this.Status = InputHistory.Get(HistoryItemType.InvRuleStatus, true);
			this.InvoiceRuleClients = this.LoadClientModels(clientID, this.Status);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool ActivityFixedCost { get; private set; }
		public bool ClientHasRule { get; private set; }
		public bool FixedCost { get; private set; }
		public bool MonthlyFixedCost { get; private set; }
		public bool SLA { get; private set; }
		public bool Status { get; private set; }
		public bool TimeMaterial { get; private set; }
		public bool Travel { get; private set; }

		public List<InvoiceRuleClientModel> InvoiceRuleClients { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<InvoiceRuleClientModel> LoadClientModels(int clientID, bool status)
		{
			var model = new List<InvoiceRuleClientModel>();

			int? selectedClientID = clientID;

			if (selectedClientID.HasValue && clientID != 0)
			{
				var client = ClientRepo.GetByID(selectedClientID.Value);

				if (client != null)
				{
					model.Add(new InvoiceRuleClientModel(client, status, this.TimeMaterial, this.FixedCost, this.MonthlyFixedCost, this.ActivityFixedCost, this.SLA, this.Travel));
				}
			}
			else
			{
				if (this.ClientHasRule)
				{
					Client.GetAllWithInvoiceRules(status).ForEach(c => model.Add(new InvoiceRuleClientModel(c, status, this.TimeMaterial, this.FixedCost, this.MonthlyFixedCost, this.ActivityFixedCost, this.SLA, this.Travel)));
				}
				else
				{
					Client.GetAllActiveClients().ForEach(c => model.Add(new InvoiceRuleClientModel(c, status, this.TimeMaterial, this.FixedCost, this.MonthlyFixedCost, this.ActivityFixedCost, this.SLA, this.Travel)));
				}
			}

			return model;
		}

		#endregion FUNCTIONS
	}
}