using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using Enabill.Models;
using Enabill.Repos;

//using System.Data.EntityClient;
//using System.Data.Objects;
//using System.Data.Objects.DataClasses;

namespace Enabill.DB
{
	public class EnabillContext : DbContext
	{
		public DbSet<Activity> Activities { get; set; }
		public DbSet<ApprovalStatus> ApprovalStatus { get; set; }
		public DbSet<BillableIndicator> BillableIndicators { get; set; }
		public DbSet<BillingMethod> BillingMethods { get; set; }
		public DbSet<Client> Clients { get; set; }
		public DbSet<Contact> Contacts { get; set; }
		public DbSet<Department> Departments { get; set; }
		public DbSet<Division> Divisions { get; set; }
		public DbSet<EmploymentType> EmploymentTypes { get; set; }
		public DbSet<FeedbackPost> FeedbackPosts { get; set; }
		public DbSet<FeedbackAttachment> FeedbackAttachments { get; set; }
		public DbSet<FeedbackThread> FeedbackThreads { get; set; }
		//tulisa
		public DbSet<CurrencyType> CurrencyType { get; set; }

		//public DbSet<ClientProjectFeedback> ClientProjectFeedbacks { get; set; }
		//public DbSet<FeedbackPost> FeedbackPosts { get; set; }
		//public DbSet<FeedbackThread> FeedbackThreads { get; set; }
		public DbSet<FeedbackType> FeedbackTypes { get; set; }

		public DbSet<FeedbackUrgencyType> FeedbackUrgencyTypes { get; set; }
		public DbSet<FlexiBalance> FlexiBalances { get; set; }
		public DbSet<FlexiBalanceAdjustment> FlexiBalanceAdjustments { get; set; }
		public DbSet<FlexiDay> FlexiDays { get; set; }
		public DbSet<FinPeriod> FinPeriods { get; set; }
		public DbSet<Invoice> Invoices { get; set; }
		public DbSet<InvoiceWorkAllocationException> InvoiceWorkAllocationExceptions { get; set; }
		public DbSet<InvoiceCredit> InvoiceCredits { get; set; }
		public DbSet<InvoiceContact> InvoiceContacts { get; set; }
		public DbSet<InvoiceRuleActivity> InvoiceRuleActivities { get; set; }
		public DbSet<InvoiceRuleContact> InvoiceRuleContacts { get; set; }
		public DbSet<InvoiceCategory> InvoiceCategories { get; set; }
		public DbSet<InvoiceRule> InvoiceRules { get; set; }
		public DbSet<InvoiceRuleLine> InvoiceRuleLines { get; set; }
		public DbSet<InvoiceStatus> InvoiceStatus { get; set; }
		public DbSet<InvoiceSubCategory> InvoiceSubCategories { get; set; }
		public DbSet<Leave> Leaves { get; set; }
		public DbSet<LeaveBalance> LeaveBalances { get; set; }
		public DbSet<LeaveType> LeaveTypes { get; set; }
		public DbSet<LeaveTypeHistory> LeaveTypeHistories { get; set; }
		public DbSet<LoginLog> LoginLogs { get; set; }
		public DbSet<Menu> Menus { get; set; }
		public DbSet<Month> Months { get; set; }
		public DbSet<Note> Notes { get; set; }
		public DbSet<Project> Projects { get; set; }
		public DbSet<PrintOption> PrintOptions { get; set; }
		public DbSet<Region> Regions { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<SecondaryManagerAllocation> SecondaryManagerAllocations { get; set; }
		public DbSet<TimesheetApproval> TimesheetApprovals { get; set; }
		public DbSet<UserPreference> UserPreferences { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<UserAllocation> UserAllocations { get; set; }
		public DbSet<UserCostToCompany> UserCostToCompanies { get; set; }
		public DbSet<UserHistory> UserHistories { get; set; }
		public DbSet<UserRole> UserRoles { get; set; }
		public DbSet<VATHistory> VATHistories { get; set; }
		public DbSet<WebHistory> WebHistories { get; set; }
		public DbSet<WorkAllocation> WorkAllocations { get; set; }
		public DbSet<WorkAllocationsWithLeave> WorkAllocationsWithLeave { get; set; }
		public DbSet<WorkDay> WorkDays { get; set; }
		public DbSet<Holiday> Holidays { get; set; }
		public DbSet<WorkSession> WorkSessions { get; set; }
		public DbSet<NonWorkSession> NonWorkSessions { get; set; }
		public DbSet<WorkSessionOverview> WorkSessionOverviews { get; set; }
		public DbSet<WorkSessionStatus> WorkSessionStatus { get; set; }
		public DbSet<TrainingCategory> TrainingCategories { get; set; }
		public DbSet<Report> Reports { get; set; }
		public DbSet<ReportEmail> ReportEmails { get; set; }
		public DbSet<Frequency> Frequencies { get; set; }
		public DbSet<LeaveManualAdjustment> LeaveManualAdjustments { get; set; }
		public DbSet<LeaveCycleBalance> LeaveCycleBalances { get; set; }
		public DbSet<IndividualLeaveDay> IndividualLeaveDays { get; set; }
		public DbSet<UserLastWorkSession> UserLastWorkSessions { get; set; }
		public DbSet<ForecastHeader> ForecastHeaders { get; set; }
		public DbSet<ForecastDetail> ForecastDetails { get; set; }
		public DbSet<ForecastResourceAssignment> ForecastResourceAssignments { get; set; }
		public DbSet<ForecastHeaderMostRecentDetailLine> ForecastHeaderMostRecentDetailLines { get; set; }
		public DbSet<ForecastHeaderLastPeriodDetail> ForecastHeaderLastPeriodDetails { get; set; }
		public DbSet<ForecastHeaderMostRecentResourceAssignment> ForecastHeaderMostRecentResourceAssignments { get; set; }
		public DbSet<ForecastReferenceDefault> ForecastReferenceDefaults { get; set; }
		public DbSet<ForecastInvoiceLink> ForecastInvoiceLinks { get; set; }
		public DbSet<ForecastWithInvoice> ForecastWithInvoices { get; set; }
		public DbSet<ForecastWithOutInvoice> ForecastWithOutInvoices { get; set; }
		public DbSet<InvoicesWithoutForecast> InvoicesWithoutForecasts { get; set; }
		public DbSet<DoesForecastHeaderHaveInvoiceLinks> DoesForecastHeaderHaveInvoiceLinks { get; set; }
		public DbSet<ForecastWithInvoiceSummary> ForecastWithInvoiceSummaries { get; set; }
		public DbSet<TicketLineAttachment> TicketLineAttachments { get; set; }
		public DbSet<Ticket> Tickets { get; set; }
		public DbSet<TicketLine> TicketLines { get; set; }
		public DbSet<TicketFilter> TicketFilters { get; set; }
		public DbSet<TicketStatus> TicketStatus { get; set; }
		public DbSet<ClientProjectTicket> ClientProjectTickets { get; set; }
		public DbSet<TicketLog> TicketLogs { get; set; }
		public DbSet<TicketReferenceNumber> TicketReferenceNumbers { get; set; }
		public DbSet<TicketAssignmentChange> TicketAssignmentChanges { get; set; }
		public DbSet<TicketStatusChange> TicketStatusChanges { get; set; }
		public DbSet<TicketType> TicketTypes { get; set; }
		public DbSet<TicketPriority> TicketPriorities { get; set; }
		public DbSet<SupportEmail> SupportEmails { get; set; }
		public DbSet<WorkableDaysPerPeriod> WorkableDaysPerPeriods { get; set; }
		public DbSet<PassPhrase> PassPhrases { get; set; }
		public DbSet<BalanceAuditTrail> BalanceAuditTrails { get; set; }
		public DbSet<BalanceChangeType> BalanceChangeTypes { get; set; }
		public DbSet<BalanceType> BalanceTypes { get; set; }
		public DbSet<UserTimeSplit> UserTimeSplits { get; set; }
		public DbSet<TicketTimeAllocation> TicketTimeAllocations { get; set; }
		public DbSet<UserTimeSplitTotalHoursPerPeriod> UserTimeSplitTotalHours { get; set; }
		public DbSet<PrintTicketRemarkOption> PrintTicketRemarkOptions { get; set; }
		public DbSet<GLAccount> GLAccounts { get; set; }
		public DbSet<ClientDepartmentCode> ClientDepartmentCode { get; set; }
		public DbSet<UserWorkAllocationException> UserWorkAllocationExceptions { get; set; }
		public DbSet<Expense> Expenses { get; set; }
		public DbSet<ExpenseCategoryType> ExpenseCategoryTypes { get; set; }
		public DbSet<ExpenseApproval> ExpenseApprovals { get; set; }
		public DbSet<ExpenseAttachment> ExpenseAttachments { get; set; }
		public DbSet<ContractAttachment> ContractAttachments { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder) =>
			//Fixed the below in the actual entity definitions

			//modelBuilder.Entity<UserAllocation>().HasKey(x => new { UserID = x.UserID, ActivityID = x.ActivityID });
			//modelBuilder.Entity<UserRole>().HasKey(x => new { UserID = x.UserID, RoleID = x.RoleID });
			//modelBuilder.Entity<InvoiceActivity>().HasKey(x => new { InvoiceID = x.InvoiceID, ActivityID = x.ActivityID });
			//modelBuilder.Entity<InvoiceContact>().HasKey(x => new { InvoiceID = x.InvoiceID, ContactID = x.ContactID });
			//modelBuilder.Entity<InvoiceRuleActivity>().HasKey(x => new { InvoiceRuleID = x.InvoiceRuleID, ActivityID = x.ActivityID });
			//modelBuilder.Entity<InvoiceRuleContact>().HasKey(x => new { InvoiceRuleID = x.InvoiceRuleID, ContactID = x.ContactID });

			/////Ignores
			//modelBuilder.Entity<InvoiceRule>().Ignore(x => x.InvoiceCategory);
			//modelBuilder.Entity<Project>().Ignore(x => x.IsActive);
			//modelBuilder.Entity<WorkAllocation>().Ignore(x => x.AmountExcl);
			base.OnModelCreating(modelBuilder);
	}

	public class EnabillContextSQL : DbContext
	{
		public DbSet<Activity> Activities { get; set; }
		public DbSet<BillableIndicator> BillableIndicators { get; set; }
		public DbSet<BillingMethod> BillingMethods { get; set; }
		public DbSet<Client> Clients { get; set; }
		public DbSet<Contact> Contacts { get; set; }
		public DbSet<Department> Departments { get; set; }
		public DbSet<Division> Divisions { get; set; }
		public DbSet<EmploymentType> EmploymentTypes { get; set; }
		public DbSet<FeedbackPost> FeedbackPosts { get; set; }
		public DbSet<TicketLineAttachment> FeedbackAttachments { get; set; }
		public DbSet<FeedbackThread> FeedbackThreads { get; set; }
		public DbSet<FeedbackType> FeedbackTypes { get; set; }
		public DbSet<FeedbackUrgencyType> FeedbackUrgencyTypes { get; set; }
		public DbSet<FlexiBalance> FlexiBalances { get; set; }
		public DbSet<FlexiBalanceAdjustment> FlexiBalanceAdjustments { get; set; }
		public DbSet<FlexiDay> FlexiDays { get; set; }
		public DbSet<FinPeriod> FinPeriods { get; set; }
		public DbSet<Invoice> Invoices { get; set; }
		public DbSet<InvoiceWorkAllocationException> InvoiceWorkAllocationExceptions { get; set; }
		public DbSet<InvoiceCredit> InvoiceCredits { get; set; }
		public DbSet<InvoiceContact> InvoiceContacts { get; set; }
		public DbSet<InvoiceRuleActivity> InvoiceRuleActivities { get; set; }
		public DbSet<InvoiceRuleContact> InvoiceRuleContacts { get; set; }
		public DbSet<InvoiceCategory> InvoiceCategories { get; set; }
		public DbSet<InvoiceRule> InvoiceRules { get; set; }
		public DbSet<InvoiceRuleLine> InvoiceRuleLines { get; set; }
		public DbSet<InvoiceStatus> InvoiceStatus { get; set; }
		public DbSet<InvoiceSubCategory> InvoiceSubCategories { get; set; }
		public DbSet<Leave> Leaves { get; set; }
		public DbSet<LeaveBalance> LeaveBalances { get; set; }
		public DbSet<LeaveTypeHistory> LeaveTypeHistories { get; set; }
		public DbSet<LoginLog> LoginLogs { get; set; }
		public DbSet<Menu> Menus { get; set; }
		public DbSet<Month> Months { get; set; }
		public DbSet<NonWorkSession> NonWorkSessions { get; set; }
		public DbSet<Note> Notes { get; set; }
		public DbSet<Project> Projects { get; set; }
		public DbSet<PrintOption> PrintOptions { get; set; }
		public DbSet<Region> Regions { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<SecondaryManagerAllocation> SecondaryManagerAllocations { get; set; }
		public DbSet<TimesheetApproval> TimesheetApprovals { get; set; }
		public DbSet<UserPreference> UserPreferences { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<UserAllocation> UserAllocations { get; set; }
		public DbSet<UserCostToCompany> UserCostToCompanies { get; set; }
		public DbSet<UserHistory> UserHistories { get; set; }
		public DbSet<UserRole> UserRoles { get; set; }
		public DbSet<VATHistory> VATHistories { get; set; }
		public DbSet<WebHistory> WebHistories { get; set; }
		public DbSet<WorkAllocation> WorkAllocations { get; set; }
		public DbSet<WorkAllocationsWithLeave> WorkAllocationsWithLeave { get; set; }
		public DbSet<WorkDay> WorkDays { get; set; }
		public DbSet<Holiday> Holidays { get; set; }
		public DbSet<WorkSession> WorkSessions { get; set; }
		public DbSet<WorkSessionOverview> WorkSessionOverviews { get; set; }
		public DbSet<WorkSessionStatus> WorkSessionStatus { get; set; }
		public DbSet<TrainingCategory> TrainingCategories { get; set; }
		public DbSet<Report> Reports { get; set; }
		public DbSet<ReportEmail> ReportEmails { get; set; }
		public DbSet<Frequency> Frequencies { get; set; }
		public DbSet<LeaveManualAdjustment> LeaveManualAdjustments { get; set; }
		public DbSet<LeaveCycleBalance> LeaveCycleBalances { get; set; }
		public DbSet<IndividualLeaveDay> IndividualLeaveDays { get; set; }
		public DbSet<UserLastWorkSession> UserLastWorkSessions { get; set; }
		public DbSet<ForecastHeader> ForecastHeaders { get; set; }
		public DbSet<ForecastDetail> ForecastDetails { get; set; }
		public DbSet<ForecastResourceAssignment> ForecastResourceAssignments { get; set; }
		public DbSet<ForecastHeaderMostRecentDetailLine> ForecastHeaderMostRecentDetailLines { get; set; }
		public DbSet<ForecastHeaderLastPeriodDetail> ForecastHeaderLastPeriodDetails { get; set; }
		public DbSet<ForecastHeaderMostRecentResourceAssignment> ForecastHeaderMostRecentResourceAssignments { get; set; }
		public DbSet<ForecastReferenceDefault> ForecastReferenceDefaults { get; set; }
		public DbSet<ForecastInvoiceLink> ForecastInvoiceLinks { get; set; }
		public DbSet<ForecastWithInvoice> ForecastWithInvoices { get; set; }
		public DbSet<ForecastWithOutInvoice> ForecastWithOutInvoices { get; set; }
		public DbSet<InvoicesWithoutForecast> InvoicesWithoutForecast { get; set; }
		public DbSet<DoesForecastHeaderHaveInvoiceLinks> DoesForecastHeaderHaveInvoiceLinks { get; set; }
		public DbSet<ForecastWithInvoiceSummary> ForecastWithInvoiceSummaries { get; set; }
		public DbSet<TicketAssignmentChange> TicketAssignmentChanges { get; set; }
		public DbSet<TicketStatusChange> TicketStatusChanges { get; set; }
		public DbSet<TicketType> TicketTypes { get; set; }
		public DbSet<TicketPriority> TicketPriorities { get; set; }
		public DbSet<SupportEmail> SupportEmails { get; set; }
		public DbSet<WorkableDaysPerPeriod> WorkableDaysPerPeriods { get; set; }
		public DbSet<PassPhrase> PassPhrases { get; set; }
		public DbSet<BalanceAuditTrail> BalanceAuditTrails { get; set; }
		public DbSet<BalanceChangeType> BalanceChangeTypes { get; set; }
		public DbSet<BalanceType> BalanceTypes { get; set; }
		public DbSet<UserTimeSplit> UserTimeSplits { get; set; }
		public DbSet<TicketTimeAllocation> TicketTimeAllocations { get; set; }
		public DbSet<UserTimeSplitTotalHoursPerPeriod> UserTimeSplitTotalHours { get; set; }
		public DbSet<PrintTicketRemarkOption> PrintTicketRemarkOptions { get; set; }
		public DbSet<ExpenseApproval> ExpenseApprovals { get; set; }

		//tulisa change 2021/05/28
		public DbSet<CurrencyType> CurrencyType { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			//Fixed the below in the actual entity definitions

			//modelBuilder.Entity<UserAllocation>().HasKey(x => new { UserID = x.UserID, ActivityID = x.ActivityID });
			//modelBuilder.Entity<UserRole>().HasKey(x => new { UserID = x.UserID, RoleID = x.RoleID });
			//modelBuilder.Entity<InvoiceActivity>().HasKey(x => new { InvoiceID = x.InvoiceID, ActivityID = x.ActivityID });
			//modelBuilder.Entity<InvoiceContact>().HasKey(x => new { InvoiceID = x.InvoiceID, ContactID = x.ContactID });
			//modelBuilder.Entity<InvoiceRuleActivity>().HasKey(x => new { InvoiceRuleID = x.InvoiceRuleID, ActivityID = x.ActivityID });
			//modelBuilder.Entity<InvoiceRuleContact>().HasKey(x => new { InvoiceRuleID = x.InvoiceRuleID, ContactID = x.ContactID });

			/////Ignores
			//modelBuilder.Entity<InvoiceRule>().Ignore(x => x.InvoiceCategory);
			//modelBuilder.Entity<Project>().Ignore(x => x.IsActive);
			//modelBuilder.Entity<WorkAllocation>().Ignore(x => x.AmountExcl);

			modelBuilder.Entity<Project>().HasRequired(c => c.Department).WithMany().WillCascadeOnDelete(false);

			base.OnModelCreating(modelBuilder);
		}
	}

	public static class TestContextHelper
	{
		public static EnabillContext DB => EnabillSettings.DB;
		public static EnabillContextSQL DBSQL => EnabillSettings.DBSQL;

		public static void SetupContextForModelChangeRecreate() =>
			//Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
			Database.SetInitializer(new DropCreateDatabaseIfModelChanges<EnabillContext>());

		public static void SetupContextWithNewDBAlways() =>
			//Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
			Database.SetInitializer(new DropCreateDatabaseAlways<EnabillContext>());

		public static void CreateAdminUser(string email)
		{
			var u = new User()
			{
				Email = email,
				IsActive = true,
				FirstName = "Admin",
				LastName = "User",
				EmployStartDate = DateTime.Parse("2011-01-01"),
				DivisionID = 1,
				RegionID = 1,
				ManagerID = 250,
				BillableIndicatorID = (int)BillableIndicatorType.Yes,
				EmploymentTypeID = (int)EmploymentTypeEnum.Permanent,
				IsSystemUser = false,

				LastModifiedBy = "Sys generated for TDD"
			};

			if (UserRepo.DB.Users.Where(us => us.UserName == u.UserName).Count() == 0)
			{
				UserRepo.Save(u);
				UserRepo.AssignRoleToUser(new UserRole() { UserID = u.UserID, RoleID = (int)UserRoleType.SystemAdministrator, DateCreated = DateTime.Now.ToCentralAfricanTime(), LastModifiedBy = "Sys generated for TDD" });
			}
		}

		public static void PopulateTestDB()
		{
			var userIDs = new Dictionary<int, int>();

			foreach (var u in DBSQL.Users)
			{
				int oldUserID = u.UserID;
				u.UserID = 0;
				u.LastModifiedBy = "Sys generated for TDD";
				UserRepo.Save(u);

				userIDs.Add(oldUserID, u.UserID);
			}

			foreach (var r in DBSQL.Roles)
			{
				RoleRepo.Save(r);
			}

			foreach (var ur in DBSQL.UserRoles)
			{
				int newUserID = userIDs.Single(u => u.Key == ur.UserID).Value;
				UserRepo.AssignRoleToUser(new UserRole() { UserID = newUserID, RoleID = (int)UserRoleType.SystemAdministrator, DateCreated = DateTime.Now.ToCentralAfricanTime(), LastModifiedBy = "Sys generated for TDD" });
			}

			var adminUser = UserRepo.GetByRoleBW((int)UserRoleType.SystemAdministrator).FirstOrDefault();

			foreach (var div in DBSQL.Divisions)
				DB.Divisions.Add(div);
			DB.SaveChanges();

			foreach (var reg in DBSQL.Regions)
				DB.Regions.Add(reg);
			DB.SaveChanges();

			foreach (var billI in DBSQL.BillableIndicators)
				DB.BillableIndicators.Add(billI);
			DB.SaveChanges();

			foreach (var b in DBSQL.BillingMethods)
				DB.BillingMethods.Add(b);
			DB.SaveChanges();

			foreach (var c in DBSQL.Clients)
			{
				int oldClientID = c.ClientID;
				c.ClientID = 0;
				c.LastModifiedBy = "Sys Generated for TDD";
				ClientRepo.Save(c);

				foreach (var contact in DBSQL.Contacts.Where(p => p.ClientID == oldClientID))
				{
					contact.ContactID = 0;
					contact.ClientID = oldClientID;
					ClientRepo.SaveContact(contact);
				}

				foreach (var p in DBSQL.Projects.Where(p => p.ClientID == oldClientID))
				{
					int oldProjectID = p.ProjectID;
					p.ProjectID = 0;
					p.ClientID = c.ClientID;
					p.LastModifiedBy = "Sys Generated for TDD";
					ProjectRepo.Save(p);

					foreach (var act in DBSQL.Activities.Where(a => a.ProjectID == oldProjectID))
					{
						int oldActivityID = act.ActivityID;

						//manually create new instance to save
						var newact = new Activity
						{
							ActivityID = 0,
							ActivityName = act.ActivityName,
							CanHaveNotes = act.CanHaveNotes,
							DepartmentID = act.DepartmentID,
							IsActive = act.IsActive,
							MustHaveRemarks = act.MustHaveRemarks,
							ProjectID = p.ProjectID,
							RegionID = act.RegionID
						};

						p.SaveActivity(adminUser, newact);

						foreach (var wa in DBSQL.WorkAllocations.Where(wa => wa.ActivityID == oldActivityID))
						{
							//wa.WorkAllocationID = 0;
							//wa.ActivityID = act.ActivityID;
							//wa.UserID = userIDs.Where(u => u.Key == wa.UserID).Single().Value;

							var newwa = new WorkAllocation
							{
								WorkAllocationID = 0,
								ActivityID = newact.ActivityID,
								DateCreated = wa.DateCreated,
								DateModified = wa.DateModified,
								DayWorked = wa.DayWorked,
								//newwa.HourRate = wa.HourRate;
								HoursWorked = wa.HoursWorked,
								InvoiceID = wa.InvoiceID,
								Period = wa.Period,
								Remark = wa.Remark,
								UserID = userIDs.Single(u => u.Key == wa.UserID).Value,
								UserCreated = wa.UserCreated,
								UserModified = wa.UserModified,

								LastModifiedBy = "Sys generated for TDD"
							};
							UserRepo.SaveWorkAllocation(newwa);
						}
					}
				}
			}

			PopulateWorkDayTable();

			DB.SaveChanges();

			//foreach (var ws in DBSQL.WorkSessions)
			//{
			//    ws.WorkSessionID = 0;
			//    ws.UserID = userIDs.Where(u => u.Key == ws.UserID).Single().Value;
			//    UserRepo.SaveWorkSession(ws);
			//}
		}

		public static void PopulateWorkDayTable()
		{
			foreach (var wd in DBSQL.WorkDays)
			{
				DB.WorkDays.Add(wd);
			}
		}
	}
}