using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class InvoiceIndexModel
	{
		#region INITIALIZATION

		public InvoiceIndexModel(User userRequesting)
		{
			this.MyClients = InputHistory.Get(HistoryItemType.InvoiceMyClients, true);

			this.DateFrom = InputHistory.GetDateTime(HistoryItemType.InvoiceDateFrom, DateTime.Today.ToFirstDayOfMonth().AddMonths(-1)).Value;
			this.DateTo = InputHistory.GetDateTime(HistoryItemType.InvoiceDateTo, DateTime.Today.ToLastDayOfMonth()).Value;
			this.InvoicePeriod = InputHistory.Get(HistoryItemType.InvoicePeriod, DateTime.Today.ToPeriod());

			this.ShowMyClientsCheckBox = InputHistory.Get(HistoryItemType.InvoiceShowMyClientsCheckBox, true);
			this.TimeMaterial = InputHistory.Get(HistoryItemType.InvoiceTimeMaterial, true);
			this.FixedCost = InputHistory.Get(HistoryItemType.InvoiceFixedCost, true);
			this.SLA = InputHistory.Get(HistoryItemType.InvoiceSLA, true);
			this.Travel = InputHistory.Get(HistoryItemType.InvoiceTravel, true);
			this.AdHoc = InputHistory.Get(HistoryItemType.InvoiceAdHoc, true);
			this.MonthlyFixedCost = InputHistory.Get(HistoryItemType.InvoiceMonthlyFixedCost, true);
			this.ActivityFixedCost = InputHistory.Get(HistoryItemType.InvoiceActivityFixedCost, true);

			this.Open = InputHistory.Get(HistoryItemType.InvoiceStatusOpen, true);
			this.InProgress = InputHistory.Get(HistoryItemType.InvoiceStatusInProgress, true);
			this.Ready = InputHistory.Get(HistoryItemType.InvoiceStatusReady, true);
			this.Complete = InputHistory.Get(HistoryItemType.InvoiceStatusComplete, true);

			this.ClientId = InputHistory.Get(HistoryItemType.InvoiceIndexClient, "0").ToInt();

			this.ClientInvoices = this.LoadClientInvoices(userRequesting);
			this.LoadAllowableInvoiceStates(userRequesting);
		}

		public InvoiceIndexModel(InvoiceIndexFilterModel invoiceIndexFilterModel)
		{
			if (invoiceIndexFilterModel.DateFrom != null && invoiceIndexFilterModel.DateFrom.HasValue)
			{
				this.DateFrom = invoiceIndexFilterModel.DateFrom.Value;
			}
			else
			{
				this.DateFrom = DateTime.Today.ToFirstDayOfMonth().AddMonths(-1);
			}

			if (invoiceIndexFilterModel.DateTo != null && invoiceIndexFilterModel.DateTo.HasValue)
			{
				this.DateTo = invoiceIndexFilterModel.DateTo.Value;
			}
			else
			{
				this.DateTo = DateTime.Today.ToLastDayOfMonth();
			}

			if (invoiceIndexFilterModel.InvoicePeriod != null && invoiceIndexFilterModel.InvoicePeriod.HasValue)
			{
				this.InvoicePeriod = invoiceIndexFilterModel.InvoicePeriod.Value;
			}

			this.MyClients = invoiceIndexFilterModel.MyClients;
			this.ShowMyClientsCheckBox = invoiceIndexFilterModel.ShowMyClientsCheckBox;

			this.TimeMaterial = invoiceIndexFilterModel.TimeMaterial;
			this.FixedCost = invoiceIndexFilterModel.FixedCost;
			this.SLA = invoiceIndexFilterModel.SLA;
			this.Travel = invoiceIndexFilterModel.Travel;
			this.AdHoc = invoiceIndexFilterModel.AdHoc;
			this.MonthlyFixedCost = invoiceIndexFilterModel.MonthlyFixedCost;
			this.ActivityFixedCost = invoiceIndexFilterModel.ActivityFixedCost;

			this.Open = invoiceIndexFilterModel.Open;
			this.InProgress = invoiceIndexFilterModel.InProgress;
			this.Ready = invoiceIndexFilterModel.Ready;
			this.Complete = invoiceIndexFilterModel.Complete;

			this.ClientId = invoiceIndexFilterModel.ClientId;
			this.ClientInvoices = this.LoadClientInvoices(invoiceIndexFilterModel);
			this.LoadAllowableInvoiceStates(invoiceIndexFilterModel.UserRequesting);
			this.Currency = Enabill.Models.Client.GetClientByID(this.ClientId.Value).GetCurrency(Enabill.Models.Client.GetClientByID(this.ClientId.Value).CurrencyTypeID).CurrencyISO;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool ActivityFixedCost { get; set; }
		public bool AdHoc { get; set; }
		public bool Complete { get; set; }
		public bool FixedCost { get; set; }
		public bool InProgress { get; set; }
		public bool MonthlyFixedCost { get; set; }
		public bool MyClients { get; set; }
		public int? ClientId { get; set; }
		public bool Open { get; set; }
		public bool Ready { get; set; }
		public bool ShowMyClientsCheckBox { get; set; }
		public bool SLA { get; set; }
		public bool TimeMaterial { get; set; }
		public bool Travel { get; set; }

		public int InvoicePeriod { get; set; }

		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }

		public string Currency;

		public List<ClientDepartmentCode> ClientDepartmentCode { get; set; }
		public List<GLAccount> GLAccountCode { get; set; }
		public List<InvoiceClientModel> ClientInvoices { get; set; }
		public List<InvoiceStatusType> AllowedInvoiceStatusTypes { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<InvoiceClientModel> LoadClientInvoices(User userLoading)
		{
			var model = new List<InvoiceClientModel>();
			this.ClientDepartmentCode = ClientDepartmentCodeRepo.GetAll().Where(s => s.IsActive).ToList();
			this.GLAccountCode = GLAccountRepo.GetAll().Where(s => s.IsActive).ToList();

			if (this.ClientId.HasValue)
			{
				var client = ClientRepo.GetByID(this.ClientId.Value);

				if (client != null)
				{
					model.Add(new InvoiceClientModel(userLoading, client, this.MyClients));

					return model;
				}
			}

			if (this.MyClients)
				Client.GetAllActiveClientsForUser(userLoading).ForEach(c => model.Add(new InvoiceClientModel(userLoading, c, this.MyClients)));
			else
				Client.GetAllActiveClients().ForEach(c => model.Add(new InvoiceClientModel(userLoading, c, this.MyClients)));

			return model;
		}

		private List<InvoiceClientModel> LoadClientInvoices(InvoiceIndexFilterModel invoiceIndexFilterModel)
		{
			var model = new List<InvoiceClientModel>();

			this.ClientDepartmentCode = ClientDepartmentCodeRepo.GetAll().Where(s => s.IsActive).ToList();
			this.GLAccountCode = GLAccountRepo.GetAll().Where(s => s.IsActive).ToList();

			if(this.ClientId.HasValue)
			{
				var client = ClientRepo.GetByID(this.ClientId.Value);

				if(client != null)
				{
					model.Add(new InvoiceClientModel(invoiceIndexFilterModel.UserRequesting, client, this.MyClients));

					return model;
				}
			}

			if(this.MyClients)
			{
				Client.GetAllActiveClientsForUser(invoiceIndexFilterModel.UserRequesting).ForEach(c => model.Add(new InvoiceClientModel(invoiceIndexFilterModel, c)));
			}
			else
			{
				Client.GetAllActiveClients().ForEach(c => model.Add(new InvoiceClientModel(invoiceIndexFilterModel, c)));
			}

			return model;
		}

		private void LoadAllowableInvoiceStates(User user)
		{
			this.AllowedInvoiceStatusTypes = new List<InvoiceStatusType>();
			if (!user.HasRole(UserRoleType.Accountant) && user.HasRole(UserRoleType.InvoiceAdministrator))
			{
				this.AllowedInvoiceStatusTypes.Add(InvoiceStatusType.Ready);
			}
		}

		#endregion FUNCTIONS
	}
}