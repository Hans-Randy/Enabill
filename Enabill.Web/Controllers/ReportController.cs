using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using ClosedXML.Excel;
using Enabill.Code;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;
using Enabill.Repository.Interfaces;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class ReportController : BaseController
	{
		private IUserRepository userRepository;

		private string templatePath;

		public ReportController(IUserRepository userRepository)
		{
			this.templatePath = ReportHelpers.TemplatePath();
			this.userRepository = userRepository;
		}

		[HttpGet]
		public ActionResult Index() => this.View();

		#region ACTIVITY REPORT

		[HttpPost]
		public ActionResult ShowActivityReportParameters(bool isAnalysis = false)
		{
			try
			{
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.ActivityReportDateFrom, DateTime.Today.AddMonths(-6).ToFirstDayOfMonth()).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.ActivityReportDateTo, DateTime.Today).Value;

				var activityReportDisplayModel = new ActivityReportDisplayModel(this.userRepository);

				int managerID = isAnalysis ? this.CurrentUserID : 0;

				activityReportDisplayModel.GetUsersWorkAllocations(null, null, managerID, 0, null, null, null, null, null, 300, 0, false, isAnalysis);

				return this.PartialView("ucShowActivityReportParameters", activityReportDisplayModel);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult ShowActivityFilterResults(bool? isModal, DateTime? dateFrom, DateTime? dateTo, int? divisionID, int? clientID, int? userID, int? projectID, int? activityID, int? employmentTypeID)
		{
			try
			{
				if (!dateFrom.HasValue)
					dateFrom = DateTime.Today.AddMonths(-2).ToFirstDayOfMonth();

				if (!dateTo.HasValue)
					dateTo = DateTime.Today;

				var activityReportModel = new ActivityReportDisplayModel(dateFrom, dateTo, divisionID, userID, clientID, projectID, activityID, employmentTypeID);

				if (isModal.HasValue)
				{
					if (isModal.Value)
						return this.PartialView("ucShowActivityReportParameters", activityReportModel);
					else
						return this.PartialView("ucOnScreenActivityReport", activityReportModel);
				}
				else
				{
					return this.PartialView("ucOnScreenActivityReport", activityReportModel);
				}
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult ShowOnScreenActivityReport()
		{
			var activityReportDisplayModel = new ActivityReportDisplayModel(this.userRepository);
			activityReportDisplayModel.GetUsersWorkAllocations(null, null, null, null, null, null, null, null, null, 300, 0);

			return this.PartialView("ucOnScreenActivityReport", activityReportDisplayModel);
		}

		[HttpPost]
		public ActionResult FilterActivityReport(string dateFrom, string dateTo, int? managerId, int? userId, int? divisionId, int? clientId, int? projectId, int? activityId, int? employmentTypeId, int pageSize, int pageNumber, bool includeLeave = false, bool isAnalysis = false)
		{
			var activityReportDisplayModel = new ActivityReportDisplayModel(this.userRepository);
			activityReportDisplayModel.GetUsersWorkAllocations(dateFrom, dateTo, managerId, userId, divisionId, clientId, projectId, activityId, employmentTypeId, pageSize, pageNumber, includeLeave, isAnalysis);

			return this.PartialView("ucShowActivityReportParameters", activityReportDisplayModel);
		}

		[HttpGet]
		public ActionResult PrintActivityReport(string dateFrom, string dateTo, int? managerId, int? userId, int? divisionId, int? clientId, int? projectId, int activityId, int? employmentTypeId, int pageSize, int pageNumber, bool includeLeave, bool isAnalysis = false)
		{
			var activityReportDisplayModel = new ActivityReportDisplayModel(this.userRepository);

			if (managerId == null)
				managerId = isAnalysis ? this.CurrentUserID : 0;

			activityReportDisplayModel.GetUsersWorkAllocations(dateFrom, dateTo, managerId, userId, divisionId, clientId, projectId, activityId, employmentTypeId, pageSize, pageNumber, includeLeave, isAnalysis);

			var dataTable = ToADOTable<UserWorkAllocation>(activityReportDisplayModel.UsersWorkAllocations);

			dataTable.Columns.Remove("WorkAllocationId");
			dataTable.Columns.Remove("ManagerId");
			dataTable.Columns.Remove("UserId");
			dataTable.Columns.Remove("EmploymentTypeId");
			dataTable.Columns.Remove("DivisionId");
			dataTable.Columns.Remove("ClientId");
			dataTable.Columns.Remove("ProjectId");
			dataTable.Columns.Remove("ActivityId");
			dataTable.Columns.Remove("TotalHours");
			dataTable.Columns.Remove("Capacity");
			dataTable.Columns.Remove("TrainingType");
			dataTable.Columns.Remove("TrainerName");
			dataTable.Columns.Remove("TrainingInstitute");
			dataTable.Columns.Remove("RegionId");
			dataTable.Columns.Remove("DepartmentId");
			dataTable.Columns.Remove("WorkHours");
			dataTable.Columns.Remove("UserName");

			var dv = dataTable.DefaultView;
			string fileNameSuffix = "ActivityReport";

			if (isAnalysis)
			{
				dv.Sort = "FullNameManager, FullName, DayWorked";
				fileNameSuffix = "ActivityAnalysisReport";
			}
			else
			{
				dataTable.Columns.Remove("FullNameManager");
				dv.Sort = "FullName, DayWorked";
			}

			var dtSorted = dv.ToTable();

			this.templatePath = ReportHelpers.TemplatePath(fileNameSuffix, this.templatePath);

			return this.FillClosedXml(fileNameSuffix, this.templatePath, dtSorted);
		}

		#endregion ACTIVITY REPORT

		#region BALANCE AUDIT TRAIL REPORT

		[HttpGet]
		public ActionResult BalanceAuditTrailReportIndex(int balanceTypeID = 1, int userID = 0)
		{
			this.ViewData["BalanceTypeList"] = new SelectList(Enabill.Extensions.GetEnumSelectList<BalanceTypeEnum>(), "Value", "Text", balanceTypeID);
			this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.IsInCurrentMonth() });

			var userItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Users" }
			};
			userItems.AddRange(UserRepo.GetAll().Where(u => u.IsFlexiTimeUser).Select(u => new SelectListItem() { Value = u.UserID.ToString(), Text = u.FullName, Selected = u.UserID == userID }));
			this.ViewData["UserList"] = userItems;

			InputHistory.Set(HistoryItemType.BalanceAuditTrailReportBalanceType, balanceTypeID);
			InputHistory.Set(HistoryItemType.BalanceAuditTrailReportUserID, userID);

			var model = userID == 0 ? BalanceAuditTrailRepo.GetAll(balanceTypeID, DateTime.Today.ToFirstDayOfMonth()) : BalanceAuditTrailRepo.GetAll(balanceTypeID, userID);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult BalanceAuditTrailReportIndex(FormCollection form)
		{
			try
			{
				int balanceTypeID = int.Parse(form["BalanceTypeList"]);
				InputHistory.Set(HistoryItemType.BalanceAuditTrailReportBalanceType, balanceTypeID);

				var balanceAuditTrailMonth = form["MonthList"].ToDate();
				balanceAuditTrailMonth = balanceAuditTrailMonth ?? DateTime.Today.ToFirstDayOfMonth();
				InputHistory.Set(HistoryItemType.BalanceAuditTrailReportMonth, balanceAuditTrailMonth.Value);

				int userID = int.Parse(form["UserList"]);
				InputHistory.Set(HistoryItemType.BalanceAuditTrailReportUserID, userID);

				this.ViewData["BalanceTypeList"] = new SelectList(Enabill.Extensions.GetEnumSelectList<BalanceTypeEnum>(), "Value", "Text", balanceTypeID);
				this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key == balanceAuditTrailMonth });

				var userItems = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Users" }
				};
				userItems.AddRange(UserRepo.GetAll().Where(u => u.IsFlexiTimeUser).Select(u => new SelectListItem() { Value = u.UserID.ToString(), Text = u.FullName, Selected = u.UserID == userID }));
				this.ViewData["UserList"] = userItems;

				var model = userID == 0 ? BalanceAuditTrailRepo.GetAll(balanceTypeID, balanceAuditTrailMonth.Value) : BalanceAuditTrailRepo.GetAll(balanceTypeID, userID);

				return this.View(model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion BALANCE AUDIT TRAIL REPORT

		#region DEVELOPMENT SERVICES ACTIVITY REPORT

		[HttpPost]
		public ActionResult ShowDSActivityReportParameters()
		{
			try
			{
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.DSActivityReportDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.DSActivityReportDateTo, DateTime.Today).Value;

				return this.PartialView("ucShowDSActivityReportParameters", "");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult PrintDSActivityReport(DateTime dateFrom, DateTime dateTo)
		{
			try
			{
				InputHistory.Set(HistoryItemType.DSActivityReportDateFrom, dateFrom);
				InputHistory.Set(HistoryItemType.DSActivityReportDateTo, dateTo);

				return this.RedirectToAction("PrintToExcel", new { reportName = "DS Activity Report" });
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion DEVELOPMENT SERVICES ACTIVITY REPORT

		#region ENCENTIVIZE REPORT

		[HttpPost]
		public ActionResult ShowEncentivizeReportParameters()
		{
			try
			{
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.EncentivizeDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.EncentivizeDateTo, DateTime.Today).Value;
				int divisionID = InputHistory.Get(HistoryItemType.WorkAllocationExceptionDivision, 0);

				var divisionItems = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Divisions" }
				};
				DivisionRepo.GetActive().OrderBy(d => d.DivisionName).ToList().ForEach(d => divisionItems.Add(new SelectListItem { Value = d.DivisionID.ToString(), Text = d.DivisionName, Selected = d.DivisionID == InputHistory.Get(HistoryItemType.ForecastDivision, 0) }));
				DivisionRepo.GetActive().OrderBy(d => d.DivisionName).ToList().ForEach(d => divisionItems.Add(new SelectListItem { Value = d.DivisionID.ToString(), Text = d.DivisionName, Selected = d.DivisionID == InputHistory.Get(HistoryItemType.WorkAllocationExceptionDivision, 0) }));
				this.ViewData["Division"] = divisionItems;

				return this.PartialView("ucShowEncentivizeReportParameters", "");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult EncentivizeReport(DateTime? dateFrom, DateTime? dateTo)
		{
			var reportConsoleModel = new EncentivizeReportModel(dateFrom, dateTo);

			return this.PartialView("ucOnScreenEncentivizeReport", reportConsoleModel);
		}

		[HttpGet]
		public ActionResult PrintEncentivizeReport(DateTime dateFrom, DateTime dateTo)
		{
			try
			{
				InputHistory.Set(HistoryItemType.EncentivizeDateFrom, dateFrom);
				InputHistory.Set(HistoryItemType.EncentivizeDateTo, dateTo);

				return this.RedirectToAction("PrintToExcel", new { reportName = "Encentivize Report" });
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion ENCENTIVIZE REPORT

		#region EXPENSE REPORT

		[HttpPost]
		public ActionResult ShowExpenseReportParameters()
		{
			try
			{
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.ExpenseReportDateFrom, DateTime.Today.ToFirstDayOfMonth()).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.ExpenseReportDateTo, DateTime.Today.ToLastDayOfMonth()).Value;

				//Active
				this.ViewData["ExpenseActiveUserList"] = new List<SelectListItem>
				{
					new SelectListItem() { Value = "", Text = "All Employees" },
					new SelectListItem() { Value = "0", Text = "Inactive" },
					new SelectListItem() { Value = "1", Text = "Active" }
				};

				//Employment Type
				var items2 = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Employes" }
				};

				UserRepo.GetAllEmploymentTypesList().ForEach(e => items2.Add(new SelectListItem() { Value = e.EmploymentTypeID.ToString(), Text = e.EmploymentTypeName }));
				this.ViewData["ExpenseEmploymentTypeList"] = items2;

				//Employee
				var items3 = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Employees" }
				};

				UserRepo.GetAllActiveUsers().ForEach(u => items3.Add(new SelectListItem() { Value = u.UserID.ToString(), Text = u.FullName }));
				this.ViewData["ExpenseEmployeeList"] = items3;

				//Clients
				var items4 = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Clients" }
				};

				ClientRepo.GetAllActiveClients().ToList().ForEach(c => items4.Add(new SelectListItem() { Value = c.ClientID.ToString(), Text = c.ClientName }));
				this.ViewData["ExpenseClientList"] = items4;

				//Projects
				var items5 = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Projects" }
				};

				ProjectRepo.GetAllActiveProjects().ForEach(p => items5.Add(new SelectListItem() { Value = p.ProjectID.ToString(), Text = p.ProjectName }));
				this.ViewData["ExpenseProjectList"] = items5;

				//Expense Category Type
				var items6 = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Expenses" }
				};

				ExpenseRepo.GetAllExpenseCategoryTypes().ForEach(e => items6.Add(new SelectListItem() { Value = e.ExpenseCategoryTypeID.ToString(), Text = e.ExpenseCategoryTypeName }));
				this.ViewData["ExpenseCategoryTypeList"] = items6;

				//Approval
				this.ViewData["ExpenseApprovalList"] = new List<SelectListItem>
				{
					new SelectListItem() { Value = "", Text = "All Expenses" },
					new SelectListItem() { Value = "0", Text = "Unapproved" },
					new SelectListItem() { Value = "1", Text = "Approved" }
				};

				//Billable
				this.ViewData["ExpenseBillableList"] = new List<SelectListItem>
				{
					new SelectListItem() { Value = "", Text = "All Expenses" },
					new SelectListItem() { Value = "0", Text = "Non-Billable" },
					new SelectListItem() { Value = "1", Text = "Billable" }
				};

				return this.PartialView("ucShowExpenseReportParameters", "");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult PrintExpenseReport(DateTime dateFrom, DateTime dateTo, string activeStatus = "", int employmentTypeID = 0, int userID = 0, int clientID = 0, int projectID = 0, int expenseCategoryTypeID = 0, string approvalStatus = "", string billableStatus = "")
		{
			try
			{
				InputHistory.Set(HistoryItemType.ExpenseReportDateFrom, dateFrom);
				InputHistory.Set(HistoryItemType.ExpenseReportDateTo, dateTo);
				InputHistory.Set(HistoryItemType.ExpenseReportActiveStatus, activeStatus);
				InputHistory.Set(HistoryItemType.ExpenseReportEmploymentTypeID, employmentTypeID);
				InputHistory.Set(HistoryItemType.ExpenseReportUserID, userID);
				InputHistory.Set(HistoryItemType.ExpenseReportClientID, clientID);
				InputHistory.Set(HistoryItemType.ExpenseReportProjectID, projectID);
				InputHistory.Set(HistoryItemType.ExpenseReportExpenseCategoryTypeID, expenseCategoryTypeID);
				InputHistory.Set(HistoryItemType.ExpenseReportApprovalStatus, approvalStatus);
				InputHistory.Set(HistoryItemType.ExpenseReportBillableStatus, billableStatus);

				return this.RedirectToAction("PrintToExcel", new { reportName = "Expense Report" });
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion EXPENSE REPORT

		#region FLEXIBALANCE REPORT

		[HttpGet]
		public ActionResult FlexiTimeBalanceReportIndex()
		{
			this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.IsInCurrentMonth() });
			var model = new FlexiBalanceReportModel(DateTime.Today);
			InputHistory.Set(HistoryItemType.FlexiBalanceReportSelectedMonth, DateTime.Today);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult FlexiTimeBalanceReportIndex(FormCollection form)
		{
			this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.IsInCurrentMonth() });
			var selectedMonth = form["MonthList"].ToDate();
			InputHistory.Set(HistoryItemType.FlexiBalanceReportSelectedMonth, selectedMonth.Value);
			var model = new FlexiBalanceReportModel(selectedMonth.Value);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult Month(string date)
		{
			try
			{
				this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.IsInCurrentMonth() });
				var model = new FlexiBalanceReportModel(date.ToDate().Value);
				InputHistory.Set(HistoryItemType.FlexiBalanceReportSelectedMonth, date.ToDate().Value);

				return this.PartialView("ucFlexiTimeBalanceReportIndex", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult FixFlexiOB(int userID)
		{
			var user = UserRepo.GetByID(userID);
			this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.IsInCurrentMonth() });
			var selectedMonth = InputHistory.GetDateTime(HistoryItemType.FlexiBalanceReportSelectedMonth, DateTime.Today).Value;
			int monthsToRecalc = DateTime.Today.Year != selectedMonth.Year ? 12 - selectedMonth.Month + DateTime.Today.Month : DateTime.Today.Month - 1;
			user.RecalculateLeaveAndFlexiBalances(this.CurrentUser, user, monthsToRecalc);
			var model = new FlexiBalanceReportModel(selectedMonth);

			return this.View("ucFlexiTimeBalanceReportIndex", model);
		}

		#endregion FLEXIBALANCE REPORT

		#region FORECAST REPORT

		[HttpGet]
		public ActionResult ForecastReportIndex()
		{
			InputHistory.Set(HistoryItemType.ForecastReportPeriod, DateTime.Today.ToPeriod());
			InputHistory.Set(HistoryItemType.ForecastReportRegion, 0);
			InputHistory.Set(HistoryItemType.ForecastReportDivision, 0);

			this.SetForecastReportViewData();
			var model = new ForecastReportModel(DateTime.Today.ToPeriod(), 0, 0);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult ForecastReportIndex(FormCollection form)
		{
			try
			{
				var monthYear = form["ForecastReportPeriod"].ToDate();
				int period = monthYear.Value.ToPeriod();
				InputHistory.Set(HistoryItemType.ForecastReportPeriod, period);

				int? regionID = form["Region"].ToInt();
				regionID = regionID ?? 0;
				InputHistory.Set(HistoryItemType.ForecastReportRegion, regionID.Value);

				int? divisionID = form["Division"].ToInt();
				divisionID = divisionID ?? 0;
				InputHistory.Set(HistoryItemType.ForecastReportDivision, divisionID.Value);

				this.SetForecastReportViewData();
				var model = new ForecastReportModel(period, regionID.Value, divisionID.Value);

				return this.View(model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult ShowLinkInvoicesPartialView(int forecastHeaderID, int? period)
		{
			try
			{
				period = period.HasValue && period.Value != 0 ? period.Value : DateTime.Today.AddMonths(-1).ToPeriod();
				var forecastHeader = ForecastHeaderRepo.GetByID(forecastHeaderID);
				this.SetLinkInvoiceViewDataLists(forecastHeaderID, period.Value);
				var model = new ForecastLinkModel(forecastHeader, period.Value);

				return this.PartialView("ucLinkInvoices", model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult LinkInvoices(FormCollection form, string selectedInvoices)
		{
			int? period = form["MonthList"].ToInt();
			int? forecastHeaderID = form["ForecastHeaderID"].ToInt();

			var lastForecastDetail = new ForecastDetail();
			lastForecastDetail = ForecastDetailRepo.GetLastDetailEntryForHeader(forecastHeaderID.Value, period.Value);

			foreach (string invoice in selectedInvoices.Split(','))
			{
				var forecastInvoiceLink = new ForecastInvoiceLink
				{
					ForecastDetailID = lastForecastDetail.ForecastDetailID,
					InvoiceID = invoice.ToInt().Value
				};
				ForecastInvoiceLinkRepo.Save(forecastInvoiceLink);
			}

			this.SetForecastReportViewData();
			var model = new ForecastReportModel(DateTime.Today.ToPeriod(), 0, 0);

			return this.View(model);
		}

		#endregion FORECAST REPORT

		#region LEAVE CYCLE REPORT

		[HttpGet]
		public ActionResult LeaveCycleReportIndex()
		{
			InputHistory.Set(HistoryItemType.LeaveBalanceReportLeaveType, (int)LeaveTypeEnum.Sick);
			var leaveTypeList = Enabill.Extensions.GetEnumSelectList<LeaveTypeEnum>().Where(l => l.Value != "128").ToList();
			this.ViewData["LeaveTypeList"] = new SelectList(leaveTypeList, "Value", "Text", (int)LeaveTypeEnum.Sick);
			this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.IsInCurrentMonth() });
			var model = new LeaveCycleBalanceModel(LeaveTypeEnum.Sick);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult LeaveCycleReportIndex(FormCollection form)
		{
			try
			{
				int leaveType = int.Parse(form["LeaveTypeList"]);
				InputHistory.Set(HistoryItemType.LeaveBalanceReportLeaveType, leaveType);

				var leaveBalanceMonth = form["MonthList"].ToDate();
				leaveBalanceMonth = leaveBalanceMonth ?? DateTime.Today.ToFirstDayOfMonth();
				InputHistory.Set(HistoryItemType.LeaveBalanceReportMonth, leaveBalanceMonth.Value);

				this.ViewData["LeaveTypeList"] = new SelectList(Enabill.Extensions.GetEnumSelectList<LeaveTypeEnum>(), "Value", "Text", leaveType);
				this.ViewData["MonthList"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(1)).OrderByDescending(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key == leaveBalanceMonth });

				LeaveCycleBalanceModel model = null;

				if (leaveType == (int)LeaveTypeEnum.Sick || leaveType == (int)LeaveTypeEnum.Compassionate)
					model = new LeaveCycleBalanceModel((LeaveTypeEnum)leaveType);
				else
					model = new LeaveCycleBalanceModel((LeaveTypeEnum)leaveType, leaveBalanceMonth.Value);

				return this.View(model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult LeaveWithLeaveBalanceReport(LeaveTypeEnum? leaveType)
		{
			if (!leaveType.HasValue)
				leaveType = LeaveTypeEnum.Sick;

			var model = new LeaveWithLeaveCycleBalanceModel(leaveType.Value);

			return this.View(model);
		}

		#endregion LEAVE CYCLE REPORT

		#region LEAVE GENERAL REPORT

		[HttpPost]
		public ActionResult ShowLeaveGeneralReportParameters()
		{
			try
			{
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.LeaveGeneralReportDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.LeaveGeneralReportDateTo, DateTime.Today).Value;

				var items = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Employees" }
				};

				UserRepo.GetAllActiveUsers().ForEach(u => items.Add(new SelectListItem() { Value = u.UserID.ToString(), Text = u.FullName }));
				this.ViewData["EmployeeGeneralLeaveList"] = items;

				var items2 = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Employment Types" }
				};

				UserRepo.GetAllEmploymentTypesList().ForEach(e => items2.Add(new SelectListItem() { Value = e.EmploymentTypeID.ToString(), Text = e.EmploymentTypeName }));
				this.ViewData["EmploymentTypeGeneralLeaveList"] = items2;

				var items3 = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Leave Types" }
				};

				UserLeaveGeneralRepo.GetLeaveTypes().ForEach(l => items3.Add(new SelectListItem() { Value = l.LeaveTypeID.ToString(), Text = l.LeaveTypeName }));
				this.ViewData["LeaveTypeGeneralLeaveList"] = items3;

				var items4 = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Approval Status" }
				};

				UserLeaveGeneralRepo.GetApprovalStatus().ForEach(a => items4.Add(new SelectListItem() { Value = a.ApprovalStatusID.ToString(), Text = a.ApprovalStatusName }));
				this.ViewData["ApprovalStatusGeneralLeaveList"] = items4;

				return this.PartialView("ucShowLeaveGeneralReportParameters", "");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult PrintLeaveGeneralReport(DateTime dateFrom, DateTime dateTo, int userID = 0, int employmentTypeID = 0, int leaveTypeID = 0, int approvalStatusID = 0)
		{
			try
			{
				InputHistory.Set(HistoryItemType.LeaveGeneralReportDateFrom, dateFrom);
				InputHistory.Set(HistoryItemType.LeaveGeneralReportDateTo, dateTo);
				InputHistory.Set(HistoryItemType.LeaveGeneralReportUserID, userID);
				InputHistory.Set(HistoryItemType.LeaveGeneralReportEmploymentTypeID, employmentTypeID);
				InputHistory.Set(HistoryItemType.LeaveGeneralReportLeaveTypeID, leaveTypeID);
				InputHistory.Set(HistoryItemType.LeaveGeneralReportApprovalStatusID, approvalStatusID);

				return this.RedirectToAction("PrintToExcel", new { reportName = "General Leave Report" });
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion LEAVE GENERAL REPORT

		#region LEAVE REQUESTS PENDING REPORT

		[HttpPost]
		public ActionResult LeaveRequestsPendingReport(DateTime dateFrom)
		{
			var leaveRequestsPendingModel = new LeaveRequestsPendingModel(dateFrom);

			return this.PartialView("ucOnScreenLeaveRequestsPending", leaveRequestsPendingModel);
		}

		[HttpPost]
		public ActionResult ShowLeaveRequestsPendingReportParameters()
		{
			try
			{
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.LeaveRequestsPendingDateFrom, DateTime.Today).Value;

				return this.PartialView("ucShowLeaveRequestsPendingReportParameters", "");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpPost]
		public ActionResult LeaveRequestPendingRunEmail(int managerID, DateTime? dateFrom)
		{
			if (!dateFrom.HasValue)
				dateFrom = DateTime.Today.AddDays(-5);

			try
			{
				Processes.Processes.RunEmail(dateFrom.Value, managerID, EmailType.LeaveRequestsPending);

				return this.Json("Message was sent");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion LEAVE REQUESTS PENDING REPORT

		#region MIS ALLOCATION TIME REPORT

		[HttpGet]
		public ActionResult MISAllocatedTimeReportIndex()
		{
			var dateFrom = InputHistory.GetDateTime(HistoryItemType.MISAllocatedTimeDateFrom, DateTime.Today.AddDays(-7)).Value;
			var dateTo = InputHistory.GetDateTime(HistoryItemType.MISAllocatedTimeDateTo, DateTime.Today).Value;
			int divisionID = InputHistory.Get(HistoryItemType.MISAllocatedTimeDivision, 0);
			int departmentID = InputHistory.Get(HistoryItemType.MISAllocatedTimeDepartment, 0);

			var divisionItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Divisions" }
			};
			DivisionRepo.GetAll().OrderBy(d => d.DivisionName).ToList().ForEach(d => divisionItems.Add(new SelectListItem { Value = d.DivisionID.ToString(), Text = d.DivisionName }));
			this.ViewData["Division"] = divisionItems;

			var departmentItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Department" }
			};
			DepartmentRepo.GetAll().OrderBy(d => d.DepartmentName).ToList().ForEach(d => departmentItems.Add(new SelectListItem { Value = d.DepartmentID.ToString(), Text = d.DepartmentName }));
			this.ViewData["Department"] = departmentItems;

			var model = new MISAllocatedTimeReportModel(DateTime.Today.AddDays(-7), DateTime.Today, 0, 0);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult MISAllocatedTimeReportIndex(FormCollection form)
		{
			try
			{
				int? division = form["Division"].ToInt();
				int? department = form["Department"].ToInt();
				var dateFrom = form["DateFrom"].ToDate() ?? DateTime.Today.AddDays(-7);
				var dateTo = form["DateTo"].ToDate() ?? DateTime.Today;

				InputHistory.Set(HistoryItemType.MISAllocatedTimeDivision, division.Value);
				InputHistory.Set(HistoryItemType.MISAllocatedTimeDepartment, department.Value);
				InputHistory.Set(HistoryItemType.MISAllocatedTimeDateFrom, dateFrom);
				InputHistory.Set(HistoryItemType.MISAllocatedTimeDateTo, dateTo);

				var divisionItems = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Divisions" }
				};
				DivisionRepo.GetAll().OrderBy(d => d.DivisionName).ToList().ForEach(d => divisionItems.Add(new SelectListItem { Value = d.DivisionID.ToString(), Text = d.DivisionName, Selected = d.DivisionID == (division ?? 0) }));
				this.ViewData["Division"] = divisionItems;

				var departmentItems = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Departments" }
				};
				DepartmentRepo.GetAll().OrderBy(d => d.DepartmentName).ToList().ForEach(d => departmentItems.Add(new SelectListItem { Value = d.DepartmentID.ToString(), Text = d.DepartmentName, Selected = d.DepartmentID == (department ?? 0) }));
				this.ViewData["Department"] = departmentItems;

				var model = new MISAllocatedTimeReportModel(dateFrom, dateTo, division.Value, department.Value);

				return this.View(model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion MIS ALLOCATION TIME REPORT

		#region PERCENTAGE ALLOCATION REPORT

		[HttpPost]
		public ActionResult PercentageAllocationReportParameters()
		{
			int finperiodID = InputHistory.Get(HistoryItemType.FinPeriodID, 0);
			var cItems = new List<SelectListItem>();
			FinPeriod.GetAll("lastYearFinPeriod").ForEach(c => cItems.Add(new SelectListItem() { Value = c.FinPeriodID.ToString(), Text = c.FinPeriodID.ToString(), Selected = c.FinPeriodID == finperiodID }));
			this.ViewData["FinPeriod"] = cItems;

			return this.PartialView("ucPercentageAllocationReportParameters");
		}

		#endregion PERCENTAGE ALLOCATION REPORT

		#region PROJECT MANAGER ACTIVITY REPORT

		[HttpPost]
		public ActionResult ShowProjectManagerActivityReportParameters()
		{
			try
			{
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.ProjectManagerActivityReportDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.ProjectManagerActivityReportDateTo, DateTime.Today).Value;
				var items = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Project Managers" }
				};
				ProjectRepo.GetAllProjectManagers().ForEach(p => items.Add(new SelectListItem() { Value = p.UserID.ToString(), Text = p.FullName }));
				this.ViewData["ProjectManagerList"] = items;

				return this.PartialView("ucShowProjectManagerActivityReportParameters", "");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult PrintProjectManagerActivityReport(DateTime dateFrom, DateTime dateTo, int projectManagerID)
		{
			try
			{
				InputHistory.Set(HistoryItemType.ProjectManagerActivityReportDateFrom, dateFrom);
				InputHistory.Set(HistoryItemType.ProjectManagerActivityReportDateTo, dateTo);
				InputHistory.Set(HistoryItemType.ProjectManagerActivityReportProjectManager, projectManagerID);

				return this.RedirectToAction("PrintToExcel", new { reportName = "ProjectManager Activity Report" });
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion PROJECT MANAGER ACTIVITY REPORT

		#region RATES REPORT

		[HttpPost]
		public ActionResult ShowRatesReportParameters()
		{
			try
			{
				var items = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Employees" }
				};
				UserRepo.GetAllActiveUsers().ForEach(u => items.Add(new SelectListItem() { Value = u.UserID.ToString(), Text = u.FullName }));
				this.ViewData["EmployeeRatesList"] = items;

				var items2 = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Clients" }
				};
				ClientRepo.GetAllActiveClients().ToList().ForEach(c => items2.Add(new SelectListItem() { Value = c.ClientID.ToString(), Text = c.ClientName }));
				this.ViewData["ClientRatesList"] = items2;

				return this.PartialView("ucShowRatesReportParameters", "");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult PrintRatesReport(int userID, int clientID)
		{
			try
			{
				InputHistory.Set(HistoryItemType.RatesReportUserID, userID);
				InputHistory.Set(HistoryItemType.RatesReportClientID, clientID);

				return this.RedirectToAction("PrintToExcel", new { reportName = "Rates Report" });
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion RATES REPORT

		#region TICKET TIMEALLOCATION REPORT

		[HttpPost]
		public ActionResult ShowTicketTimeAllocationReportParameters()
		{
			try
			{
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.TicketTimeAllocationReportDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.TicketTimeAllocationReportDateTo, DateTime.Today).Value;

				return this.PartialView("ucShowTicketTimeAllocationReportParameters", "");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult PrintTicketTimeAllocationReport(DateTime dateFrom, DateTime dateTo)
		{
			try
			{
				InputHistory.Set(HistoryItemType.TicketTimeAllocationReportDateFrom, dateFrom);
				InputHistory.Set(HistoryItemType.TicketTimeAllocationReportDateTo, dateTo);

				return this.RedirectToAction("PrintToExcel", new { reportName = "Ticket Time Allocation Report" });
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion TICKET TIMEALLOCATION REPORT

		#region TIMESHEET REPORT

		[HttpPost]
		public ActionResult ShowTimesheetReportParameters()
		{
			try
			{
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.TimesheetReportDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.TimesheetReportDateTo, DateTime.Today).Value;

				return this.PartialView("ucShowTimesheetReportParameters", "");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult PrintTimesheetReport(DateTime dateFrom, DateTime dateTo)
		{
			try
			{
				InputHistory.Set(HistoryItemType.TimesheetReportDateFrom, dateFrom);
				InputHistory.Set(HistoryItemType.TimesheetReportDateTo, dateTo);

				return this.RedirectToAction("PrintToExcel", new { reportName = "Timesheet Report" });
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult TimesheetReportIndex(string callingPage = "")
		{
			if (callingPage?.Length == 0)
			{
				InputHistory.Set(HistoryItemType.TimesheetReportDateFrom, DateTime.Today);
				InputHistory.Set(HistoryItemType.TimesheetReportDateTo, DateTime.Today);
			}

			var dateFrom = InputHistory.GetDateTime(HistoryItemType.TimesheetReportDateFrom, DateTime.Today).Value;
			var dateTo = InputHistory.GetDateTime(HistoryItemType.TimesheetReportDateTo, DateTime.Today).Value;

			var model = new TimesheetReportModel(dateFrom, dateTo, this.CurrentUser);

			return this.View(model);
		}

		[HttpPost]
		public ActionResult TimesheetReportIndex(FormCollection form)
		{
			try
			{
				InputHistory.Set(HistoryItemType.TimesheetReportDateFrom, form["DateFrom"].ToDate() ?? DateTime.Today.ToFirstDayOfMonth());
				InputHistory.Set(HistoryItemType.TimesheetReportDateTo, form["DateTo"].ToDate() ?? DateTime.Today.ToLastDayOfMonth());

				var dateFrom = InputHistory.GetDateTime(HistoryItemType.TimesheetReportDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.TimesheetReportDateTo, DateTime.Today).Value;

				var model = new TimesheetReportModel(dateFrom, dateTo, this.CurrentUser);

				return this.View(model);
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion TIMESHEET REPORT

		#region TRAINING REPORT

		[HttpPost]
		public ActionResult ShowTrainingReportParameters()
		{
			try
			{
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.TrainingReportDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.TrainingReportDateTo, DateTime.Today).Value;

				return this.PartialView("ucShowTrainingReportParameters", "");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult PrintTrainingReport(DateTime dateFrom, DateTime dateTo)
		{
			try
			{
				InputHistory.Set(HistoryItemType.TrainingReportDateFrom, dateFrom);
				InputHistory.Set(HistoryItemType.TrainingReportDateTo, dateTo);

				return this.RedirectToAction("PrintToExcel", new { reportName = "Training Report" });
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion TRAINING REPORT

		#region WORKALLOCATIONEXCEPTION REPORT

		[HttpPost]
		public ActionResult ShowWAExceptionReportParameters()
		{
			try
			{
				var dateFrom = InputHistory.GetDateTime(HistoryItemType.WorkAllocationExceptionDateFrom, DateTime.Today).Value;
				var dateTo = InputHistory.GetDateTime(HistoryItemType.WorkAllocationExceptionDateTo, DateTime.Today).Value;
				int divisionID = InputHistory.Get(HistoryItemType.WorkAllocationExceptionDivision, 0);

				var divisionItems = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Divisions" }
				};
				DivisionRepo.GetAll().OrderBy(d => d.DivisionName).ToList().ForEach(d => divisionItems.Add(new SelectListItem { Value = d.DivisionID.ToString(), Text = d.DivisionName, Selected = d.DivisionID == InputHistory.Get(HistoryItemType.ForecastDivision, 0) }));
				DivisionRepo.GetAll().OrderBy(d => d.DivisionName).ToList().ForEach(d => divisionItems.Add(new SelectListItem { Value = d.DivisionID.ToString(), Text = d.DivisionName, Selected = d.DivisionID == InputHistory.Get(HistoryItemType.WorkAllocationExceptionDivision, 0) }));
				this.ViewData["Division"] = divisionItems;

				return this.PartialView("ucShowWAExceptionReportParameters", "");
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		[HttpGet]
		public ActionResult PrintWAExceptionReport(DateTime dateFrom, DateTime dateTo, int division)
		{
			try
			{
				InputHistory.Set(HistoryItemType.WorkAllocationExceptionDateFrom, dateFrom);
				InputHistory.Set(HistoryItemType.WorkAllocationExceptionDateTo, dateTo);
				InputHistory.Set(HistoryItemType.WorkAllocationExceptionDivision, division);

				var divisionItems = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Divisions" }
				};
				DivisionRepo.GetAll().OrderBy(d => d.DivisionName).ToList().ForEach(d => divisionItems.Add(new SelectListItem { Value = d.DivisionID.ToString(), Text = d.DivisionName, Selected = d.DivisionID == InputHistory.Get(HistoryItemType.WorkAllocationExceptionDivision, 0) }));
				this.ViewData["Division"] = divisionItems;

				return this.RedirectToAction("PrintToExcel", new { reportName = "WorkAllocationException Report" });
			}
			catch (Exception ex)
			{
				return this.ReturnJsonException(ex);
			}
		}

		#endregion WORKALLOCATIONEXCEPTION REPORT

		#region LOOKUPS

		private void SetViewData(int clientID, string projectName, string activityName)
		{
			if (clientID == 0 && projectName == "0")
			{
				var pItems = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Projects" }
				};
				Project.GetDistinctProjectNames().ForEach(p => pItems.Add(new SelectListItem() { Value = p, Text = p, Selected = p == projectName }));
				this.ViewData["ProjectList"] = pItems.Distinct();

				var aItems = new List<SelectListItem>
				{
					new SelectListItem() { Value = "0", Text = "All Activities" }
				};
				Activity.GetDistinctActivityNames().ForEach(a => aItems.Add(new SelectListItem() { Value = a, Text = a, Selected = a == activityName }));
				this.ViewData["ActivityList"] = aItems.Distinct();
			}
			else
			{
				if (clientID != 0 && projectName == "0")
				{
					var pItems = new List<SelectListItem>
					{
						new SelectListItem() { Value = "0", Text = "All Projects" }
					};
					Project.GetDistinctProjectNamesForClientID(clientID).ForEach(p => pItems.Add(new SelectListItem() { Value = p, Text = p, Selected = p == projectName }));
					this.ViewData["ProjectList"] = pItems;

					var aItems = new List<SelectListItem>
					{
						new SelectListItem() { Value = "0", Text = "All Activities" }
					};
					Activity.GetDistinctActivityNamesForClientID(clientID).ForEach(a => aItems.Add(new SelectListItem() { Value = a, Text = a, Selected = a == activityName }));
					this.ViewData["ActivityList"] = aItems.Distinct();
				}
				else
				{
					if (clientID != 0 && projectName != "0")
					{
						var pItems = new List<SelectListItem>
						{
							new SelectListItem() { Value = "0", Text = "All Projects" }
						};
						Project.GetDistinctProjectNamesForClientID(clientID).ForEach(p => pItems.Add(new SelectListItem() { Value = p, Text = p, Selected = p == projectName }));
						this.ViewData["ProjectList"] = pItems;

						var aItems = new List<SelectListItem>
						{
							new SelectListItem() { Value = "0", Text = "All Activities" }
						};
						Activity.GetDistinctActivityNamesForProjectName(projectName).ForEach(a => aItems.Add(new SelectListItem() { Value = a, Text = a, Selected = a == activityName }));
						this.ViewData["ActivityList"] = aItems.Distinct();
					}
					else
					{
						if (clientID == 0 && projectName != "0")
						{
							var pItems = new List<SelectListItem>
							{
								new SelectListItem() { Value = "0", Text = "All Projects" }
							};
							Project.GetDistinctProjectNames().ForEach(p => pItems.Add(new SelectListItem() { Value = p, Text = p, Selected = p == projectName }));
							this.ViewData["ProjectList"] = pItems.Distinct();
							var aItems = new List<SelectListItem>
							{
								new SelectListItem() { Value = "0", Text = "All Activities" }
							};
							Activity.GetDistinctActivityNamesForProjectName(projectName).ForEach(a => aItems.Add(new SelectListItem() { Value = a, Text = a, Selected = a == activityName }));
							this.ViewData["ActivityList"] = aItems.Distinct();
						}
					}
				}
			}
		}

		private void SetForecastReportViewData()
		{
			var regionItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Regions" }
			};
			RegionRepo.GetAll().OrderBy(r => r.RegionName).ToList().ForEach(r => regionItems.Add(new SelectListItem { Value = r.RegionID.ToString(), Text = r.RegionName, Selected = r.RegionID == InputHistory.Get(HistoryItemType.ForecastReportRegion, 0) }));
			this.ViewData["Region"] = regionItems;

			var divisionItems = new List<SelectListItem>
			{
				new SelectListItem() { Value = "0", Text = "All Divisions" }
			};
			DivisionRepo.GetAll().OrderBy(d => d.DivisionName).ToList().ForEach(d => divisionItems.Add(new SelectListItem { Value = d.DivisionID.ToString(), Text = d.DivisionName, Selected = d.DivisionID == InputHistory.Get(HistoryItemType.ForecastReportDivision, 0) }));
			this.ViewData["Division"] = divisionItems;

			this.ViewData["ForecastReportPeriod"] = Helpers.GetDropDownOfMonths(Settings.SiteStartDate, DateTime.Today.ToFirstDayOfMonth().AddMonths(12)).OrderBy(x => x.Key).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value, Selected = x.Key.ToPeriod().ToString() == InputHistory.Get(HistoryItemType.ForecastReportPeriod, 0).ToString() });
		}

		private void SetLinkInvoiceViewDataLists(int forecastHeaderID, int period) => this.ViewData["MonthList"] = ForecastHeaderRepo.GetDistinctPeriodsByHeaderID(forecastHeaderID).OrderBy(p => p).Select(p => new SelectListItem { Value = p.ToString(), Text = p.GetYear().ToString() + " " + p.GetMonth().ToMonthName(), Selected = p == period });

		#endregion LOOKUPS

		#region PRINT

		public ActionResult PrintToExcel(string reportName)
		{
			bool isPartial = false;

			int activityID = 0;
			int approvalStatusID = 0;
			int clientID = 0;
			int departmentID = 0;
			int divisionID = 0;
			int employmentTypeID = 0;
			int expenseCategoryTypeID = 0;
			int leaveTypeID = 0;
			int managerID = 0;
			int projectManagerID = 0;
			int projectID = 0;
			int userID = 0;

			string approvalStatus = "";
			string activityName = "0";
			string activeStatus = "";
			string billableStatus = "";
			string fileNameSuffix = "";
			string projectName = "0";

			var dateFrom = DateTime.Today;
			var dateTo = DateTime.Today;

			ActivityReportModel model = null;
			ActivityReportDisplayModel reportDisplayModel = null;
			DSActivityReportModel dsModel = null;
			EncentivizeReportModel encModel = null;
			ExpenseReportDisplayModel expenseReportModel = null;
			MISAllocatedTimeReportModel MISModel = null;
			ProjectManagerActivityReportModel paModel = null;
			RatesReportModel ratesModel = null;
			TimesheetReportModel tsModel = null;
			TicketTimeAllocationReportModel ttaModel = null;
			UserLeaveGeneralReportModel userLeaveGeneralReportModel = null;
			WorkAllocationExceptionReportModel waeModel = null;

			List<ExpenseReportModel> expenseList = null;
			List<MISAllocatedTimeModel> resMIS = null;
			List<ProjectActivityTimeReportModel> resPa = null;
			List<RatesModel> ratesList = null;
			List<TicketTimeAllocation> resTta = null;
			List<UserTimeSplitReportModel> res = null;
			List<UserLeaveGeneralModel> userLeaveGeneralList = null;
			List<UserWorkDayExcelModel> resEnc;
			List<WorkAllocationExceptionModel> resWae = null;

			IEnumerable<BalanceAuditTrail> resBAT = null;

			DataTable dataTable = null;

			switch (reportName)
			{
				case "Activity Report":
					dateFrom = InputHistory.GetDateTime(HistoryItemType.ActivityReportDateFrom, DateTime.Today).Value;
					dateTo = InputHistory.GetDateTime(HistoryItemType.ActivityReportDateTo, DateTime.Today).Value;
					clientID = InputHistory.Get(HistoryItemType.ActivityReportClientID, 0);
					userID = InputHistory.Get(HistoryItemType.ActivityReportUserID, 0);
					projectID = InputHistory.Get(HistoryItemType.ActivityReportProjectID, 0);
					activityID = InputHistory.Get(HistoryItemType.ActivityReportActivityID, 0);
					employmentTypeID = InputHistory.Get(HistoryItemType.ActivityReportEmploymentTypeID, 0);
					fileNameSuffix = "ActivityReport";
					reportDisplayModel = new ActivityReportDisplayModel(dateFrom, dateTo, divisionID, managerID, userID, clientID, projectID, activityID, employmentTypeID);
					res = reportDisplayModel.UserTimeSplitReportModels.ToList();
					//Call Tracer Excel Export functions
					dataTable = ToADOTable<UserTimeSplitReportModel>(res);
					break;

				case "Balance AuditTrail Report":
					int balanceTypeID = InputHistory.Get(HistoryItemType.BalanceAuditTrailReportBalanceType, 1);
					userID = InputHistory.Get(HistoryItemType.BalanceAuditTrailReportUserID, 0);
					var balanceAuditTrailMonth = InputHistory.GetDateTime(HistoryItemType.BalanceAuditTrailReportMonth, DateTime.Today.ToFirstDayOfMonth()).Value;
					fileNameSuffix = "BalanceAuditTrailReport";
					resBAT = userID == 0 ? BalanceAuditTrailRepo.GetAll(balanceTypeID, balanceAuditTrailMonth) : BalanceAuditTrailRepo.GetAll(balanceTypeID, userID);
					resBAT = resBAT.OrderByDescending(b => b.BalanceDate);
					//Call Tracer Excel Export functions
					dataTable = ToADOTable<BalanceAuditTrail>(resBAT);
					break;

				case "DS Activity Report":
					dateFrom = InputHistory.GetDateTime(HistoryItemType.DSActivityReportDateFrom, DateTime.Today).Value;
					dateTo = InputHistory.GetDateTime(HistoryItemType.DSActivityReportDateTo, DateTime.Today).Value;
					fileNameSuffix = "DSActivityReport";
					dsModel = new DSActivityReportModel(dateFrom, dateTo);
					res = dsModel.DSActivityReport.ToList();
					//Call Tracer Excel Export functions
					dataTable = ToADOTable<UserTimeSplitReportModel>(res);
					break;

				case "Encentivize Report":
					dateFrom = InputHistory.GetDateTime(HistoryItemType.EncentivizeDateFrom, DateTime.Today).Value;
					dateTo = InputHistory.GetDateTime(HistoryItemType.EncentivizeDateTo, DateTime.Today).Value;
					fileNameSuffix = "EncentivizeReport";
					encModel = new EncentivizeReportModel(dateFrom, dateTo);
					resEnc = encModel.UserWorkDaysExcelModel.ToList();
					////Call Tracer Excel Export functions
					dataTable = ToADOTable<UserWorkDayExcelModel>(resEnc);
					break;

				case "Expense Report":
					dateFrom = InputHistory.GetDateTime(HistoryItemType.ExpenseReportDateFrom, DateTime.Today.ToFirstDayOfMonth()).Value;
					dateTo = InputHistory.GetDateTime(HistoryItemType.ExpenseReportDateTo, DateTime.Today.ToLastDayOfMonth()).Value;
					employmentTypeID = InputHistory.Get(HistoryItemType.ExpenseReportEmploymentTypeID, 0);
					userID = InputHistory.Get(HistoryItemType.ExpenseReportUserID, 0);
					clientID = InputHistory.Get(HistoryItemType.ExpenseReportClientID, 0);
					projectID = InputHistory.Get(HistoryItemType.ExpenseReportProjectID, 0);
					expenseCategoryTypeID = InputHistory.Get(HistoryItemType.ExpenseReportExpenseCategoryTypeID, 0);
					approvalStatus = InputHistory.Get(HistoryItemType.ExpenseReportApprovalStatus, "");
					billableStatus = InputHistory.Get(HistoryItemType.ExpenseReportBillableStatus, "");
					activeStatus = InputHistory.Get(HistoryItemType.ExpenseReportActiveStatus, "");
					fileNameSuffix = "ExpenseReport";
					expenseReportModel = new ExpenseReportDisplayModel(dateFrom, dateTo, userID, employmentTypeID, clientID, projectID, expenseCategoryTypeID, approvalStatus, billableStatus, activeStatus);
					expenseList = expenseReportModel.ExpenseReport.ToList();
					dataTable = ToADOTable<ExpenseReportModel>(expenseList);
					break;

				case "General Leave Report":
					userID = 0;
					employmentTypeID = 0;
					managerID = 0;
					leaveTypeID = 0;
					approvalStatusID = 0;
					dateFrom = InputHistory.GetDateTime(HistoryItemType.LeaveGeneralReportDateFrom, DateTime.Today).Value;
					dateTo = InputHistory.GetDateTime(HistoryItemType.LeaveGeneralReportDateTo, DateTime.Today).Value;
					employmentTypeID = InputHistory.Get(HistoryItemType.LeaveGeneralReportEmploymentTypeID, 0);
					userID = InputHistory.Get(HistoryItemType.LeaveGeneralReportUserID, 0);
					managerID = InputHistory.Get(HistoryItemType.LeaveGeneralReportManagerID, 0);
					leaveTypeID = InputHistory.Get(HistoryItemType.LeaveGeneralReportLeaveTypeID, 0);
					approvalStatusID = InputHistory.Get(HistoryItemType.LeaveGeneralReportApprovalStatusID, 0);
					fileNameSuffix = "LeaveGeneralReport";
					userLeaveGeneralReportModel = new UserLeaveGeneralReportModel(dateFrom, dateTo, userID, employmentTypeID, managerID, leaveTypeID, isPartial, approvalStatusID);
					userLeaveGeneralList = userLeaveGeneralReportModel.UserLeaveGeneralReport.ToList();
					dataTable = ToADOTable<UserLeaveGeneralModel>(userLeaveGeneralList);
					break;

				case "MIS Allocated Time Report":
					dateFrom = InputHistory.GetDateTime(HistoryItemType.MISAllocatedTimeDateFrom, DateTime.Today).Value;
					dateTo = InputHistory.GetDateTime(HistoryItemType.MISAllocatedTimeDateTo, DateTime.Today).Value;
					divisionID = InputHistory.Get(HistoryItemType.MISAllocatedTimeDivision, 0);
					departmentID = InputHistory.Get(HistoryItemType.MISAllocatedTimeDepartment, 0);
					fileNameSuffix = "MISAllocatedTimeReport";
					MISModel = new MISAllocatedTimeReportModel(dateFrom, dateTo, divisionID, departmentID);
					resMIS = MISModel.MISAllocatedTimeReport.ToList();
					//Call Tracer Excel Export functions
					dataTable = ToADOTable<MISAllocatedTimeModel>(resMIS);
					break;

				case "ProjectManager Activity Report":
					dateFrom = InputHistory.GetDateTime(HistoryItemType.ProjectManagerActivityReportDateFrom, DateTime.Today).Value;
					dateTo = InputHistory.GetDateTime(HistoryItemType.ProjectManagerActivityReportDateTo, DateTime.Today).Value;
					projectManagerID = InputHistory.Get(HistoryItemType.ProjectManagerActivityReportProjectManager, 0);
					fileNameSuffix = "PMActivityReport";
					paModel = new ProjectManagerActivityReportModel(dateFrom, dateTo, projectManagerID);
					resPa = paModel.ProjectActivityReport.ToList();
					//Call Tracer Excel Export functions
					dataTable = ToADOTable<ProjectActivityTimeReportModel>(resPa);
					break;

				case "Rates Report":
					userID = 0;
					clientID = 0;
					userID = InputHistory.Get(HistoryItemType.RatesReportUserID, 0);
					clientID = InputHistory.Get(HistoryItemType.RatesReportClientID, 0);
					fileNameSuffix = "RatesReport";
					ratesModel = new RatesReportModel(userID, clientID);
					ratesList = ratesModel.RatesReport.ToList();
					dataTable = ToADOTable<RatesModel>(ratesList);
					break;

				case "Ticket Time Allocation Report":
					dateFrom = InputHistory.GetDateTime(HistoryItemType.TicketTimeAllocationReportDateFrom, DateTime.Today).Value;
					dateTo = InputHistory.GetDateTime(HistoryItemType.TicketTimeAllocationReportDateTo, DateTime.Today).Value;
					fileNameSuffix = "TicketTimeAllocationReport";
					ttaModel = new TicketTimeAllocationReportModel(dateFrom, dateTo);
					resTta = ttaModel.TicketTimeAllocationReport.ToList();
					//Call Tracer Excel Export functions
					dataTable = ToADOTable<TicketTimeAllocation>(resTta);
					break;

				case "Timesheet Report":
					dateFrom = InputHistory.GetDateTime(HistoryItemType.TimesheetReportDateFrom, DateTime.Today).Value;
					dateTo = InputHistory.GetDateTime(HistoryItemType.TimesheetReportDateTo, DateTime.Today).Value;
					fileNameSuffix = "TimesheetReport";
					tsModel = new TimesheetReportModel(dateFrom, dateTo, this.CurrentUser);
					res = tsModel.TimesheetReport.ToList();
					//Call Tracer Excel Export functions
					dataTable = ToADOTable<UserTimeSplitReportModel>(res);
					break;

				case "Training Report":
					dateFrom = InputHistory.GetDateTime(HistoryItemType.TrainingReportDateFrom, DateTime.Today).Value;
					dateTo = InputHistory.GetDateTime(HistoryItemType.TrainingReportDateTo, DateTime.Today).Value;
					clientID = 159;
					projectName = "0";
					activityName = "Training";
					fileNameSuffix = "TrainingReport";
					model = new ActivityReportModel(dateFrom, dateTo, clientID, projectName, activityName, "0");
					res = model.ActivityReport.ToList();
					//Call Tracer Excel Export functions
					dataTable = ToADOTable<UserTimeSplitReportModel>(res);
					break;

				case "WorkAllocationException Report":
					dateFrom = InputHistory.GetDateTime(HistoryItemType.WorkAllocationExceptionDateFrom, DateTime.Today).Value;
					dateTo = InputHistory.GetDateTime(HistoryItemType.WorkAllocationExceptionDateTo, DateTime.Today).Value;
					divisionID = InputHistory.Get(HistoryItemType.WorkAllocationExceptionDivision, 0);
					fileNameSuffix = "WorkAllocationExceptionReport";
					waeModel = new WorkAllocationExceptionReportModel(dateFrom, dateTo, divisionID);
					resWae = waeModel.WorkAllocationExceptionReport.ToList();
					//Call Tracer Excel Export functions
					dataTable = ToADOTable<WorkAllocationExceptionModel>(resWae);
					break;

				default:
					break;
			}

			this.templatePath = ReportHelpers.TemplatePath(fileNameSuffix, this.templatePath);

			return this.FillClosedXml(fileNameSuffix, this.templatePath, dataTable);
		}

		public ActionResult PrintLeaveReportToExcel(LeaveTypeEnum leaveType)
		{
			string fileNameSuffix = "";
			DataTable dataTable = null;
			DateTime? leaveBalanceMonth = InputHistory.GetDateTime(HistoryItemType.LeaveBalanceReportMonth, DateTime.Today).Value;
			LeaveCycleBalanceModel model = null;
			List<LeaveCycleExtendedReportModel> resCycle = null;
			List<LeaveOtherReportModel> resOtherLeave = null;
			List<LeaveBalanceSummarisedReportModel> resAnnualLeave = null;

			model = new LeaveCycleBalanceModel(leaveType, leaveBalanceMonth.Value);

			switch (leaveType)
			{
				case LeaveTypeEnum.Annual:
					fileNameSuffix = "AnnualLeaveReport";
					resAnnualLeave = model.LeaveBalances.ToList();
					dataTable = ToADOTable<LeaveBalanceSummarisedReportModel>(resAnnualLeave);
					break;

				case LeaveTypeEnum.Sick:
				case LeaveTypeEnum.Compassionate:
					fileNameSuffix = "SickCompassionateLeaveReport";
					model = new LeaveCycleBalanceModel(leaveType);
					resCycle = model.LeaveCycleBalances.ToList();
					dataTable = ToADOTable<LeaveCycleExtendedReportModel>(resCycle);
					break;

				default:
					fileNameSuffix = "OtherLeaveReport";
					resOtherLeave = model.LeaveOtherBalances.ToList();
					dataTable = ToADOTable<LeaveOtherReportModel>(resOtherLeave);
					break;
			}

			this.templatePath = ReportHelpers.TemplatePath(fileNameSuffix, this.templatePath);

			//Call Tracer Excel Export functions
			return this.FillClosedXml(fileNameSuffix, this.templatePath, dataTable);
		}

		public ActionResult PrintFlexiReportToExcel()
		{
			string fileNameSuffix = "";
			//string templatePath = "";
			DataTable dataTable = null;

			fileNameSuffix = "FlexiTimeReport";
			this.templatePath = ReportHelpers.TemplatePath(fileNameSuffix, this.templatePath);
			var selectedMonth = InputHistory.GetDateTime(HistoryItemType.FlexiBalanceReportSelectedMonth, DateTime.Today).Value;
			var model = new FlexiBalanceReportModel(selectedMonth);
			var resFlexiLeave = model.FlexiTimeReport.ToList();
			dataTable = ToADOTable<FlexiTimeReportModel>(resFlexiLeave);
			//Call Tracer Excel Export functions

			return this.FillClosedXml(fileNameSuffix, this.templatePath, dataTable);
		}

		#endregion PRINT

		public ActionResult PrintPercentageAllocationReport(string finPeriod)
		{
			string fileNameSuffix = "";
			//string templatePath = "";
			DataTable dataTable = null;
			int year = (int)finPeriod.Substring(0, 4).ToInt();
			string manager = string.Empty;
			int month = (int)finPeriod.Remove(0, 4).ToInt();
			if (this.CurrentUser.HasRole(UserRoleType.Accountant))
				manager = "%";
			else if (this.CurrentUser.HasRole(UserRoleType.Manager))
				manager = this.CurrentUser.UserID.ToString();

			var query = PercentageAllocationReportRepo.ExecutePercentageAllocationReport(year, month, (int)finPeriod.ToInt(), manager);
			fileNameSuffix = "PercentageAllocationReport";
			this.templatePath = ReportHelpers.TemplatePath(fileNameSuffix, this.templatePath);
			dataTable = ToADOTable<PercentageAllocationModel>(query);

			return this.FillClosedXml(fileNameSuffix, this.templatePath, dataTable);
		}

		#region TRACER EXCEL EXPORT FUNCTIONS

		private static string CleanFileName(string src)
		{
			var enc = new ASCIIEncoding();
			byte[] conv = enc.GetBytes(src);
			// System.Text.ASCIIEncoding.ASCII.GetString(conv);

			return Encoding.GetEncoding("iso-8859-8").GetString(conv);
		}

		private ActionResult FillClosedXml(string filenamesuffix, string templatePath, DataTable dataTable)
		{
			// Do temp dir cleanup....
			this.DoTempDirCleanup();

			// construct the path and name of the new temp xlsx file
			string destFilename = string.Format($"{this.TempPath}/{Guid.NewGuid().ToString()}.xlsx");

			//try
			//{
			// this collection contains all the column names (as per source dataTable) and the excel column index it will be mapped to
			var colindex = new Dictionary<string, int>();

			//System.IO.File.Copy(System.Web.Hosting.HostingEnvironment.MapPath(templatePath), destFilename);
			System.IO.File.Copy(templatePath, destFilename);

			// find the template package
			var package = new XLWorkbook(destFilename);

			//TODO MEL change to correct exception
			if (package == null)
				throw new FeedbackException();//NotFoundException(String.Format($"File cannot be found: {templatePath}"));

			// find the 'Data' worksheet
			var ws = package.Worksheet("Data");

			if (ws == null)
				throw new Exception(string.Format($"Template file does not have a 'Data' worksheet : {templatePath}"));

			// find the 'datatable' table
			var table = ws.Tables.SingleOrDefault()?.Name == "Table2" ? ws.Table("Table2") : ws.Table("datatable");

			if (table == null)
				throw new Exception(string.Format($"Template file does not contain a table with reference named 'datatable' : {templatePath}"));

			// find the 'fields' range
			var fields = package.Range("fields");

			if (fields == null)
				throw new Exception(string.Format($"Template file does not contain a range/table with reference named 'fields' : {templatePath}"));

			// now prep a collection of columnname vs indexes
			for (int i = fields.FirstColumn().ColumnNumber(); i <= fields.LastColumn().ColumnNumber(); i++)
			{
				colindex.Add(ws.Cell(fields.FirstRow().RowNumber(), i).Value.ToString(), i);
			}

			// removes the fields row
			fields.FirstRow().Delete();

			// this loop will add the fields that are found in the source table (but not defined in the template) to the output
			foreach (DataColumn col in dataTable.Columns)
			{
				if (!colindex.ContainsKey(col.Caption))
				{
					table.InsertColumnsAfter(1);
					table.HeadersRow().Cell(table.HeadersRow().RangeAddress.LastAddress.ColumnNumber).Value = col.Caption;
					colindex.Add(col.Caption, table.HeadersRow().RangeAddress.LastAddress.ColumnNumber);
					//table.Cell(2, colindex[col.Caption]).DataType = ClosedXML.Excel.XLCellValues.Text;
				}
			}

			// ... ok... the meat of this routine... this is where we do the actual cell-by-cell construction of the new output
			object cellval = null;
			IXLCell cell = null;

			if (dataTable.Rows.Count > 0)
			{
				table.InsertRowsBelow(dataTable.Rows.Count - 1, true);

				for (int i = 0; i < dataTable.Rows.Count; i++)
				{
					foreach (var col in colindex)
					{
						cell = table.Cell(i + 2, col.Value);
						cellval = dataTable.Rows[i][col.Key];
						cell.Style = table.Cell(2, col.Value).Style;
						cell.DataType = table.Cell(2, col.Value).DataType;

						if (dataTable.Columns.Contains(col.Key) && cellval != null)
						{
							cell.DataType = table.Cell(2, col.Value).DataType;

							try
							{
								cell.Value = GetCellValue(cellval, dataTable.Columns[col.Key].DataType);
							}
							catch { }
						}
					}
				}
			}

			try
			{
				// Autowidth all fields
				ws.Columns().AdjustToContents();
			}
			catch
			{
			}

			// Save file to new path
			package.Save();

			package = null;

			return this.File(destFilename, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", CleanFileName(string.Format($"{this.CurrentUser.UserName}_{DateTime.Now.ToString("yyyyMMddHHmmss")}_{filenamesuffix}.xlsx")));
		}

		public ActionResult ShowOnScreenTimeSheetApproval(UserWorkDayStatus status = UserWorkDayStatus.All, string dateFrom = "", string dateTo = "", int userID = 0, TimesheetSchedule timesheetSchedule = TimesheetSchedule.LastDayOfTheMonthMinusSevenDays)
		{
			var model = new TimeApproveModel(this.userRepository);
			var dtFrom = new DateTime();
			var dtTo = new DateTime();
			bool validFromDate = false;
			bool validToDate = false;

			var enZA = new CultureInfo("en-ZA");
			validFromDate = DateTime.TryParseExact(dateFrom, "yyyy-MM-dd", enZA, DateTimeStyles.None, out dtFrom);
			validToDate = DateTime.TryParseExact(dateTo, "yyyy-MM-dd", enZA, DateTimeStyles.None, out dtTo);

			if (!validFromDate)
			{
				dtFrom = DateTime.Now.ToFirstDayOfMonth();
			}

			if (!validToDate)
			{
				if (validFromDate)
				{
					dtTo = dtFrom.ToLastDayOfMonth();
				}
				else
				{
					dtTo = DateTime.Now.ToLastDayOfMonth();
				}
			}

			model.GetUserWorkSessions(dtFrom, dtTo, userID, status, timesheetSchedule);

			return this.PartialView("ucOnScreenTimeSheetApproval", model);
		}

		public ActionResult ShowOnScreenTimeSheetUserList()
		{
			this.userRepository.GetUsers();

			return this.PartialView("ucOnScreenTimeSheetApproval", this.userRepository.Users);
		}

		[HttpPost]
		public ActionResult ValidateTimesheets(TimesheetSchedule schedule = TimesheetSchedule.LastDayOfTheMonthMinusSevenDays, string month = "")
		{
			var dtMonth = new DateTime();
			DateTime.TryParse(month, out dtMonth);

			return this.Json(new { Schedule = schedule, Month = dtMonth.ToString() });
		}

		public static DataTable ToADOTable<T>(IEnumerable<T> varlist)
		{
			var dtReturn = new DataTable();
			// Use reflection to get property names, to create table
			// column names
			var oProps = typeof(T).GetProperties();

			foreach (var pi in oProps)
			{
				var colType = pi.PropertyType;

				if (colType.IsGenericType && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
					colType = colType.GetGenericArguments()[0];

				dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
			}

			foreach (var rec in varlist)
			{
				var dr = dtReturn.NewRow();

				foreach (var pi in oProps)
					dr[pi.Name] = pi.GetValue(rec, null) ?? DBNull.Value;

				dtReturn.Rows.Add(dr);
			}

			return dtReturn;
		}

		private static object GetCellValue(object cellval, Type type)
		{
			if (cellval == null)
				return null;

			if (type == typeof(string))
			{
				string val = cellval.ToString();
				//if (cellval.ToString().Length > 255)
				//    val = cellval.ToString().Substring(0, 254);

				return RemoveInvalidXMLChars(val);
			}

			if (type == typeof(decimal))
			{
				string val = cellval.ToString();

				if (string.IsNullOrEmpty(val))
					return 0;

				return Convert.ToDecimal(val);
			}

			return cellval;
		}

		public static string RemoveInvalidXMLChars(string text)
		{
			if (string.IsNullOrEmpty(text))
				return "";

			if (invalidXMLChars.IsMatch(text))
				return invalidXMLChars.Replace(text, "");

			return text;
		}

		// filters control characters but allows only properly-formed surrogate sequences
		private static Regex invalidXMLChars = new Regex(
			@"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
			RegexOptions.Compiled);

		private void DoTempDirCleanup()
		{
			foreach (string filename in System.IO.Directory.GetFiles(this.TempPath))
			{
				try
				{
					System.IO.File.Delete(filename);
				}
				catch { } //explicit dummy catch
			}
		}

		private string TempPath => Constants.PATHTEMP.Replace("\\", "/");

		#endregion TRACER EXCEL EXPORT FUNCTIONS

		public object ParModel { get; set; }
	}
}