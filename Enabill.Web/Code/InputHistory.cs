using System;
using System.Web;

namespace Enabill.Web
{
	public static class InputHistory
	{
		public static DateTime Get(HistoryItemType historyItem)
		{
			var res = HttpContext.Current.Session[historyItem.ToString()] as DateTime?;

			return res ?? DateTime.Now;
		}

		#region GET/SET STRING VALUES

		public static string Get(HistoryItemType historyItem, string defaultInput = "") => Get(null, historyItem, defaultInput);

		public static string Get(string firstOption, HistoryItemType historyItem, string defaultInput = "")
		{
			if (firstOption != null)
				return firstOption;

			string res = HttpContext.Current.Session[historyItem.ToString()] as string;

			if (!string.IsNullOrWhiteSpace(res))
				return res;

			return defaultInput;
		}

		public static void Set(HistoryItemType historyItem, string value) => HttpContext.Current.Session[historyItem.ToString()] = value;

		#endregion GET/SET STRING VALUES

		#region GET/SET DATETIME VALUES

		public static DateTime? GetDateTime(HistoryItemType historyItem, DateTime? defaultInput) => GetDateTime(null, historyItem, defaultInput);

		public static DateTime? GetDateTime(DateTime? firstOption, HistoryItemType historyItem, DateTime? defaultInput)
		{
			if (firstOption.HasValue)
				return firstOption.Value;

			var res = HttpContext.Current.Session[historyItem.ToString()] as DateTime?;

			return res ?? defaultInput;
		}

		public static void Set(HistoryItemType historyItem, DateTime value) => HttpContext.Current.Session[historyItem.ToString()] = value;

		#endregion GET/SET DATETIME VALUES

		#region GET/SET INT VALUES

		public static int Get(HistoryItemType historyItem, int? defaultInput)
		{
			if (defaultInput == null)
				defaultInput = 10000;

			return Get(null, historyItem, defaultInput.Value);
		}

		public static int Get(int? firstOption, HistoryItemType historyItem, int defaultInput = 10000)
		{
			if (firstOption.HasValue)
				return firstOption.Value;

			int? res = HttpContext.Current.Session[historyItem.ToString()] as int?;

			if (res.HasValue && res >= 0)
				return res.Value;

			return defaultInput;
		}

		public static void Set(HistoryItemType historyItem, int value) => HttpContext.Current.Session[historyItem.ToString()] = value;

		#endregion GET/SET INT VALUES

		#region GET/SET BOOL VALUES

		internal static bool Get(HistoryItemType historyItem, bool defaultValue = false) => Get(null, historyItem, defaultValue);

		public static bool Get(bool? firstOption, HistoryItemType historyItem, bool defaultInput = false)
		{
			if (firstOption.HasValue)
				return firstOption.Value;

			bool? res = HttpContext.Current.Session[historyItem.ToString()] as bool?;

			return res ?? defaultInput;
		}

		internal static void Set(HistoryItemType historyItem, bool value) => HttpContext.Current.Session[historyItem.ToString()] = value;

		#endregion GET/SET BOOL VALUES
	}

	public enum HistoryItemType
	{
		ClientFilterBy,
		UserFilterBy,
		ProjectFilterBy,
		ExpenseFilterBy,
		ProjectSearchCriteria,
		ClientSearchCriteria,
		ExpenseSearchCriteria,
		ClientID,
		TicketID,
		ContactSearchCriteria,
		UserSearchCriteria,
		WorkDay,
		FlexiDay,                   //Used for flexibalance view search criteria
		ExpenseDate,
		ExpenseOriginalDate,        //This is used to store the original date of an expense. Needed when the date is changed to rename attachments and change file path
		NoteSearchListAmount,       //This is used to store the amount of notes in the list that the user wants to see
		NoteSearchActivityList,     //This is used to store the id's of the activities the user wants to search by
		NoteSearchUserList,         //stores a list of userIDs to search by..
		NoteSearchDateFrom,
		NoteSearchDateTo,
		NoteSearchKeyWord,          //Note controller is not inheriting from SearchableController, so we need a new holder
		TimeStartDate,
		TimeEndDate,
		LeaveStartDate,
		LeaveEndDate,

		InvRuleIndexClient,         //InvoiceRule/Index .. client dropdownlist history item
		InvRuleTimeMaterial,
		InvRuleFixedCost,
		InvRuleSLA,
		InvRuleTravel,
		InvRuleMonthlyFixedCost,
		InvRuleActivityFixedCost,
		InvRuleClientHasRule,
		InvRuleStatus,
		StatusFilter,

		InvoiceIndexClient,
		InvoiceDateFrom,
		InvoiceDateTo,
		InvoicePeriod,

		InvoiceMyClients,
		InvoiceShowMyClientsCheckBox,
		InvoiceTimeMaterial,
		InvoiceFixedCost,
		InvoiceSLA,
		InvoiceTravel,
		InvoiceAdHoc,
		InvoiceMonthlyFixedCost,
		InvoiceActivityFixedCost,

		InvoiceStatusOpen,
		InvoiceStatusInProgress,
		InvoiceStatusReady,
		InvoiceStatusComplete,

		TimeApprovalUser,
		TimeApprovalStatus,

		StatusID,

		ApprovalDateFrom,
		ApprovalDateTo,
		ApprovalStatus,
		ApprovalManager,

		ActivityReportDateFrom,
		ActivityReportDateTo,
		ActivityReportDivisionID,
		ActivityReportManagerID,
		ActivityReportUserID,
		ActivityReportClientID,
		ActivityReportProjectName,
		ActivityReportProjectID,
		ActivityReportActivityName,
		ActivityReportActivityID,
		ActivityReportEmploymentType,
		ActivityReportEmploymentTypeID,

		TrainingReportDateFrom,
		TrainingReportDateTo,

		ProjectManagerActivityReportDateFrom,
		ProjectManagerActivityReportDateTo,
		ProjectManagerActivityReportProjectManager,

		TimesheetReportDateFrom,
		TimesheetReportDateTo,

		DSActivityReportDateFrom,
		DSActivityReportDateTo,

		ExpenseReportDateFrom,
		ExpenseReportDateTo,
		ExpenseReportActiveStatus,
		ExpenseReportEmploymentTypeID,
		ExpenseReportUserID,
		ExpenseReportClientID,
		ExpenseReportProjectID,
		ExpenseReportExpenseCategoryTypeID,
		ExpenseReportApprovalStatus,
		ExpenseReportBillableStatus,

		TicketTimeAllocationReportDateFrom,
		TicketTimeAllocationReportDateTo,

		LeaveBalanceReportLeaveType,
		LeaveBalanceReportMonth,

		WorkAllocationExceptionDateFrom,
		WorkAllocationExceptionDateTo,
		WorkAllocationExceptionDivision,

		EncentivizeDateFrom,
		EncentivizeDateTo,

		LeaveRequestsPendingDateFrom,
		LeaveRequestsPendingDateTo,

		MISAllocatedTimeDateFrom,
		MISAllocatedTimeDateTo,
		MISAllocatedTimeDivision,
		MISAllocatedTimeDepartment,

		//Forecast Index view
		ForecastRegion,

		ForecastDivision,
		ForecastClient,
		ForecastPeriodFrom,
		ForecastPeriodTo,
		ForecastProbability,
		ForecastReferences,

		//Forecast Create view
		ForecastCreateMonthList,

		ForecastCreateRegion,
		ForecastCreateDivision,
		ForecastCreateBillingMethod,
		ForecastCreateInvoiceCategory,
		ForecastCreateClient,
		ForecastCreateProject,
		ForecastCreateProbability,
		ForecastCreateRemark,
		FinPeriodID,

		//Forecast MTD Report view
		ForecastReportRegion,

		ForecastReportDivision,
		ForecastReportPeriod,

		//Ticket search criteria fields
		TicketDateFrom,

		TicketDateTo,
		TicketTypeFilter,
		TicketFilterBy,

		FlexiBalanceReportSelectedMonth,

		BalanceAuditTrailReportBalanceType,
		BalanceAuditTrailReportMonth,
		BalanceAuditTrailReportUserID,

		//Rates report filter fields
		RatesReportUserID,

		RatesReportClientID,

		//Leave Future report filter fields
		LeaveGeneralReportDateFrom,

		LeaveGeneralReportDateTo,
		LeaveGeneralReportUserID,
		LeaveGeneralReportEmploymentTypeID,
		LeaveGeneralReportManagerID,
		LeaveGeneralReportLeaveTypeID,
		LeaveGeneralReportApprovalStatusID,

		UserCostToCompanyPeriod, // Used to store the period when viewing and updating user cost to company

		RegionFilterBy,
		RegionSearchCriteria,

	}
}