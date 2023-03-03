using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class InvoiceClientModel
	{
		#region INITIALIZATION

		public InvoiceClientModel(User userRequesting, Client client, bool myClients)
		{
			this.Client = client;
			this.Invoices = this.LoadInvoices(userRequesting, client, myClients);
			this.LoadTotals();
		}

		public InvoiceClientModel(InvoiceIndexFilterModel filterModel, Client client)
		{
			this.Client = client;
			this.Invoices = this.LoadInvoices(filterModel, client);
			this.LoadTotals();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public double AccrualExclTotal { get; private set; }
		public double EstimateTotal { get; private set; }
		public double InvoiceExclTotal { get; private set; }
		public double InvoiceInclTotal { get; private set; }
		public double ProvisionalAccrualTotal { get; private set; }
		public double ProvisionalIncomeTotal { get; private set; }
		public double ProvisionalNettAmount { get; private set; }

		public Client Client { get; private set; }

		public List<Invoice> Invoices { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		private List<Invoice> LoadInvoices(User userLoading, Client client, bool myClients)
		{
			var dateFrom = InputHistory.GetDateTime(HistoryItemType.InvoiceDateFrom, DateTime.Today.ToFirstDayOfMonth().AddMonths(-1)).Value;
			var dateTo = InputHistory.GetDateTime(HistoryItemType.InvoiceDateTo, DateTime.Today.ToLastDayOfMonth()).Value;
			int invoicePeriod = InputHistory.Get(HistoryItemType.InvoicePeriod, DateTime.Today.ToPeriod());

			bool TM = InputHistory.Get(HistoryItemType.InvoiceTimeMaterial, true);
			bool fixedCost = InputHistory.Get(HistoryItemType.InvoiceFixedCost, true);
			bool monthlyFixedCost = InputHistory.Get(HistoryItemType.InvoiceMonthlyFixedCost, true);
			bool activityFixedCost = InputHistory.Get(HistoryItemType.InvoiceActivityFixedCost, true);
			bool SLA = InputHistory.Get(HistoryItemType.InvoiceSLA, true);
			bool travel = InputHistory.Get(HistoryItemType.InvoiceTravel, true);
			bool adHoc = InputHistory.Get(HistoryItemType.InvoiceAdHoc, true);

			int billingMethodBWTotal = (TM ? (int)BillingMethodType.TimeMaterial : 0) + (fixedCost ? (int)BillingMethodType.FixedCost : 0) + (monthlyFixedCost ? (int)BillingMethodType.MonthlyFixedCost : 0) + (SLA ? (int)BillingMethodType.SLA : 0) + (travel ? (int)BillingMethodType.Travel : 0) + (adHoc ? (int)BillingMethodType.AdHoc : 0) + (activityFixedCost ? (int)BillingMethodType.ActivityFixedCost : 0);

			bool open = InputHistory.Get(HistoryItemType.InvoiceStatusOpen, true);
			bool inProgress = InputHistory.Get(HistoryItemType.InvoiceStatusInProgress, true);
			bool ready = InputHistory.Get(HistoryItemType.InvoiceStatusReady, true);
			bool complete = InputHistory.Get(HistoryItemType.InvoiceStatusComplete, true);
			int statusBWTotal = (open ? (int)InvoiceStatusType.Open : 0) + (inProgress ? (int)InvoiceStatusType.InProgress : 0) + (ready ? (int)InvoiceStatusType.Ready : 0) + (complete ? (int)InvoiceStatusType.Complete : 0);

			List<Invoice> Invoices;

			if (userLoading.GetUserPreference().InvoiceIndexDateSelector == 0) // dateRange
			{
				if (myClients)
					Invoices = client.GetInvoicesForUser(userLoading, dateFrom, dateTo, billingMethodBWTotal, statusBWTotal);
				else
					Invoices = client.GetInvoices(dateFrom, dateTo, billingMethodBWTotal, statusBWTotal);
			}
			else
				if (!invoicePeriod.IsValidPeriod())
			{
				return new List<Invoice>();
			}
			else
			{
				if (myClients)
					Invoices = client.GetInvoicesForUser(userLoading, invoicePeriod, billingMethodBWTotal, statusBWTotal);
				else
					Invoices = client.GetInvoices(invoicePeriod, billingMethodBWTotal, statusBWTotal);
			}

			//update approval status
			foreach (var i in Invoices)
			{
				var invoiceFromDate = i.DateFrom;
				var invoiceToDate = i.DateTo;

				if (invoiceFromDate == DateTime.MinValue)
					invoiceFromDate = i.Period.FirstDayOfPeriod();

				if (invoiceToDate == DateTime.MinValue)
					invoiceToDate = i.Period.LastDayOfPeriod();

				var activityIDs = i.GetActivitiesFromRule().Select(a => a.ActivityID).ToList();
				var userExceptionDetails = new List<WorkAllocationExceptionModel>();

				if (activityIDs.Count > 0)
				{
					var unApprovedWorkSessions = new List<WorkSession>();
					unApprovedWorkSessions.AddRange(InvoiceRepo.GetUnApprovedWorkSessionsLinkedToInvoicePeriod(activityIDs, invoiceFromDate, invoiceToDate));

					var allWorkAllocationExceptionList = i.GetTimeCaptureExceptions();

					i.IsTimeApproved = unApprovedWorkSessions.Count <= 0 && allWorkAllocationExceptionList.Count <= 0;

					/*
					if (unApprovedWorkSessions.Count > 0)
					{
						//The invoice is unapproved if there is unapproved\exception worksessions
						i.IsTimeApproved = false;
					}
					else
					{
						var users = new List<User>();
						users.AddRange(InvoiceRepo.GetUsersLinkedToInvoicePeriod(activityIDs, invoiceFromDate, dateTo));

						foreach (var u in users)
						{
							u.IsTimeCaptured(invoiceFromDate, invoiceToDate, Settings.Current.CurrentUser, out userExceptionDetails);

							foreach (var wae in userExceptionDetails)
							{
								if (wae.ExceptionDetail == "Worksession not captured.")
								{
									i.IsTimeApproved = false;
									break;
								}
							}
						}
					}
					*/

					i.Save(Settings.Current.CurrentUser);
				}			
			}

			return Invoices;
		}

		private List<Invoice> LoadInvoices(InvoiceIndexFilterModel filterModel, Client client)
		{
			var dateFrom = filterModel.DateFrom ?? DateTime.Today.ToFirstDayOfMonth().AddMonths(-1);
			var dateTo = filterModel.DateTo ?? DateTime.Today.ToLastDayOfMonth();

			int invoicePeriod = filterModel.InvoicePeriod ?? 0;

			bool TM = filterModel.TimeMaterial;
			bool fixedCost = filterModel.FixedCost;
			bool monthlyFixedCost = filterModel.MonthlyFixedCost;
			bool activityFixedCost = filterModel.ActivityFixedCost;
			bool SLA = filterModel.SLA;
			bool travel = filterModel.Travel;
			bool adHoc = filterModel.AdHoc;

			int billingMethodBWTotal = (TM ? (int)BillingMethodType.TimeMaterial : 0) + (fixedCost ? (int)BillingMethodType.FixedCost : 0) + (monthlyFixedCost ? (int)BillingMethodType.MonthlyFixedCost : 0) + (SLA ? (int)BillingMethodType.SLA : 0) + (travel ? (int)BillingMethodType.Travel : 0) + (adHoc ? (int)BillingMethodType.AdHoc : 0) + (activityFixedCost ? (int)BillingMethodType.ActivityFixedCost : 0);

			bool open = filterModel.Open;
			bool inProgress = filterModel.InProgress;
			bool ready = filterModel.Ready;
			bool complete = filterModel.Complete;
			int statusBWTotal = (open ? (int)InvoiceStatusType.Open : 0) + (inProgress ? (int)InvoiceStatusType.InProgress : 0) + (ready ? (int)InvoiceStatusType.Ready : 0) + (complete ? (int)InvoiceStatusType.Complete : 0);

			List<Invoice> Invoices;
			
			if (filterModel.UserRequesting.GetUserPreference().InvoiceIndexDateSelector == 0) // dateRange
			{
				if (filterModel.MyClients)
					Invoices = client.GetInvoicesForUser(filterModel.UserRequesting, dateFrom, dateTo, billingMethodBWTotal, statusBWTotal);
				else
					Invoices = client.GetInvoices(dateFrom, dateTo, billingMethodBWTotal, statusBWTotal);
			}
			else
				if (!invoicePeriod.IsValidPeriod())
			{
				return new List<Invoice>();
			}
			else
			{
				if (filterModel.MyClients)
					Invoices = client.GetInvoicesForUser(filterModel.UserRequesting, invoicePeriod, billingMethodBWTotal, statusBWTotal);
				else
					Invoices = client.GetInvoices(invoicePeriod, billingMethodBWTotal, statusBWTotal);
			}

			//update approval status
			foreach (var i in Invoices)
			{
				var invoiceFromDate = i.DateFrom;
				var invoiceToDate = i.DateTo;

				if (invoiceFromDate == DateTime.MinValue)
					invoiceFromDate = i.Period.FirstDayOfPeriod();

				if (invoiceToDate == DateTime.MinValue)
					invoiceToDate = i.Period.LastDayOfPeriod();

				var activityIDs = i.GetActivitiesFromRule().Select(a => a.ActivityID).ToList();
				var userExceptionDetails = new List<WorkAllocationExceptionModel>();

				if (activityIDs.Count > 0)
				{
					var unApprovedWorkSessions = new List<WorkSession>();
					unApprovedWorkSessions.AddRange(InvoiceRepo.GetUnApprovedWorkSessionsLinkedToInvoicePeriod(activityIDs, invoiceFromDate, invoiceToDate));

					if (unApprovedWorkSessions.Count > 0)
					{
						//The invoice is unapproved if there is unapproved\exception worksessions
						i.IsTimeApproved = false;
					}
					else
					{
						var users = new List<User>();
						users.AddRange(InvoiceRepo.GetUsersLinkedToInvoicePeriod(activityIDs, invoiceFromDate, dateTo));

						foreach (var u in users)
						{
							//what is the difference between the current user and the user loading the data
							u.IsTimeCaptured(invoiceFromDate, invoiceToDate, filterModel.UserRequesting, out userExceptionDetails);

							foreach (var wae in userExceptionDetails)
							{
								if (wae.ExceptionDetail == "Worksession not captured.")
								{
									i.IsTimeApproved = false;
									break;
								}
							}
						}
					}

					i.Save(filterModel.UserRequesting);
				}
			}
		
			return Invoices;
		}

		private void LoadTotals()
		{
			this.ProvisionalAccrualTotal = 0;
			this.ProvisionalIncomeTotal = 0;
			this.ProvisionalNettAmount = 0;
			this.EstimateTotal = 0;
			this.AccrualExclTotal = 0;
			this.InvoiceExclTotal = 0;
			this.InvoiceInclTotal = 0;

			foreach (var inv in this.Invoices)
			{
				if((InvoiceStatusType)inv.InvoiceStatusID == InvoiceStatusType.Open)
				{
					inv.InvoiceAmountExclVAT = inv.ProvisionalNettAmount;
				}

				this.ProvisionalAccrualTotal += inv.ProvisionalAccrualAmount ?? 0;
				this.ProvisionalIncomeTotal += inv.ProvisionalIncomeAmount ?? 0;
				this.EstimateTotal += inv.ProjectedAmountExcl ?? 0;
				this.AccrualExclTotal += inv.AccrualExclVAT;
				this.InvoiceExclTotal += inv.InvoiceAmountExclVAT;
				this.InvoiceInclTotal += inv.InvoiceAmountInclVAT;
				this.ProvisionalNettAmount += inv.ProvisionalNettAmount;
			}
		}

		#endregion FUNCTIONS
	}
}