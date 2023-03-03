using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class ExpenseRepo : BaseRepo
	{
		#region EXPENSE SPECIFIC

		public static IEnumerable<Expense> GetAll() => DB.Expenses;

		public static Expense GetByID(int expenseID) => DB.Expenses
					.FirstOrDefault(e => e.ExpenseID == expenseID);

		public static List<Expense> FilterByAll(string searchFilter, bool locked)
		{
			if (string.IsNullOrEmpty(searchFilter))
				searchFilter = string.Empty;

			return DB.Expenses
					.Where(u => u.Locked == locked
					//Todo
					//&& (u.ClientName.Contains(searchFilter) || u.RegisteredName.Contains(searchFilter)))
					//.OrderBy(c => c.ClientName)
					).ToList();
		}

		#endregion EXPENSE SPECIFIC

		#region EXPENSE CATEGORY TYPE

		public static List<ExpenseCategoryType> GetAllExpenseCategoryTypes() => DB.ExpenseCategoryTypes.ToList();

		#endregion EXPENSE CATEGORY TYPE

		#region DATE

		public static IEnumerable<Expense> GetByDate(DateTime date) => DB.Expenses
			.Where(e => e.ExpenseDate == date).ToList();

		public static IEnumerable<Expense> GetByDateRange(DateTime dateFrom, DateTime dateTo) => DB.Expenses
			.Where(e => e.ExpenseDate >= dateFrom && e.ExpenseDate <= dateTo).ToList();

		#endregion DATE

		#region USER

		public static IEnumerable<Expense> GetByUserID(int userID) => DB.Expenses
			.Where(e => e.UserID == userID).ToList();

		public static List<User> GetUsersWithExpenses(DateTime startDate, DateTime endDate) => (from e in DB.Expenses
																								join u in DB.Users on e.UserID equals u.UserID
																								where e.ExpenseDate >= startDate && e.ExpenseDate <= endDate
																								select u)
						.Distinct()
						.OrderBy(u => u.FirstName)
						.ThenBy(u => u.LastName)
						.ToList();

		#endregion USER

		#region USER AND DATE

		public static IEnumerable<Expense> GetByUserAndDateRange(int userID, DateTime dateFrom, DateTime dateTo) => DB.Expenses
			.Where(e => e.UserID == userID && e.ExpenseDate >= dateFrom && e.ExpenseDate <= dateTo).ToList();

		public static IEnumerable<Expense> GetByUserAndDate(int userID, DateTime date) => DB.Expenses
			.Where(e => e.UserID == userID && e.ExpenseDate == date).ToList();

		#endregion USER AND DATE

		#region CLIENT

		public static List<Client> GetClientsWithExpenses(DateTime startDate, DateTime endDate) => (from e in DB.Expenses
																									join c in DB.Clients on e.ClientID equals c.ClientID
																									where e.ExpenseDate >= startDate && e.ExpenseDate <= endDate
																									select c)
						.Distinct()
						.OrderBy(c => c.ClientName)
						.ToList();

		#endregion CLIENT

		#region PROJECT

		public static List<Project> GetProjectsWithExpenses(DateTime startDate, DateTime endDate) => (from e in DB.Expenses
																									  join p in DB.Projects on e.ProjectID equals p.ProjectID
																									  where e.ExpenseDate >= startDate && e.ExpenseDate <= endDate
																									  orderby p.ProjectName
																									  select p)
						.Distinct()
						.OrderBy(p => p.ProjectName)
						.ToList();

		public static List<int> GetProjectIDsLinkedToUser(int userID) => (from ua in DB.UserAllocations
																		  join a in DB.Activities on ua.ActivityID equals a.ActivityID
																		  join p in DB.Projects on a.ProjectID equals p.ProjectID
																		  where ua.UserID == userID
																		  select p.ProjectID
					).ToList();

		#endregion PROJECT

		#region BILLABLE

		public static bool IsBillable(int expenseID) => DB.ExpenseApprovals
			.Select(x => x.ExpenseID == expenseID)
			.FirstOrDefault();

		#endregion BILLABLE

		#region ATTACHMENTS

		public static ExpenseAttachment GetAttachmentByFileName(string fileName) => DB.ExpenseAttachments
					.SingleOrDefault(e => e.FileName == fileName);

		public static List<ExpenseAttachment> GetListOfFilesByExpenseID(int expenseID) => DB.ExpenseAttachments
					.Where(e => e.ExpenseID == expenseID).ToList();

		public static List<string> GetListOfFileNamesByExpenseID(int expenseID) => DB.ExpenseAttachments
			.Where(e => e.ExpenseID == expenseID)
			.Select(e => e.FileName).ToList();

		public static void SaveAttachment(ExpenseAttachment expenseAttachment)
		{
			if (expenseAttachment.ExpenseAttachmentID == 0)
				DB.ExpenseAttachments.Add(expenseAttachment);

			DB.SaveChanges();
		}

		public static void DeleteAttachment(ExpenseAttachment expenseAttachment)
		{
			if (expenseAttachment.ExpenseAttachmentID != 0)
				DB.ExpenseAttachments.Remove(expenseAttachment);

			DB.SaveChanges();
		}

		#endregion ATTACHMENTS

		public static void Save(Expense expense)
		{
			if (expense.ExpenseID == 0)
				DB.Expenses.Add(expense);

			DB.SaveChanges();
		}

		public static void Delete(Expense expense)
		{
			DB.Expenses.Remove(expense);
			DB.SaveChanges();
		}

		#region REPORT

		public static List<ExpenseReportModel> GetReportInfo(DateTime dateFrom, DateTime dateTo, int userID = 0, int employmentTypeID = 0, int clientID = 0, int projectID = 0, int expenseCategoryTypeID = 0, string lockedStatus = "", string billableStatus = "", string activeStatus = "")
		{
			var data = from e in DB.Expenses.AsEnumerable()
					   join u in DB.Users on e.UserID equals u.UserID
					   join et in DB.EmploymentTypes on u.EmploymentTypeID equals et.EmploymentTypeID
					   join c in DB.Clients on e.ClientID equals c.ClientID
					   join p in DB.Projects on e.ProjectID equals p.ProjectID
					   join ect in DB.ExpenseCategoryTypes on e.ExpenseCategoryTypeID equals ect.ExpenseCategoryTypeID
					   where e.ExpenseDate >= dateFrom && e.ExpenseDate <= dateTo
					   select (new ExpenseReportModel()
					   {
						   UserID = u.UserID,
						   ClientID = c.ClientID,
						   ProjectID = p.ProjectID,
						   EmploymentTypeID = et.EmploymentTypeID,
						   ExpenseCategoryTypeID = ect.ExpenseCategoryTypeID,
						   ExpenseDate = e.ExpenseDate,
						   Employee = u.FullName,
						   EmploymentType = et.EmploymentTypeName,
						   ClientName = c.ClientName,
						   ProjectName = p.ProjectName,
						   ExpenseCategoryType = ect.ExpenseCategoryTypeName,
						   Amount = e.Amount,
						   Mileage = e.Mileage,
						   Locked = e.Locked,
						   Approved = e.Locked ? "Yes" : "No",
						   ManagedBy = e.ManagedBy,
						   DateManaged = e.DateManaged,
						   Billable = e.Billable,
						   BillableStatus = e.Billable ? "Yes" : "No",
						   IsActive = u.IsActive,
						   Active = u.IsActive ? "Yes" : "No",
						   Notes = e.Notes,
						   ExpenseAmount = "(" + c.GetCurrency(c.CurrencyTypeID).CurrencyISO + ") " + e.Amount.ToString()
					   });

			if (userID != 0)
				data = data.Where(d => d.UserID == userID);

			if (employmentTypeID != 0)
				data = data.Where(d => d.EmploymentTypeID == employmentTypeID);

			if (clientID != 0)
				data = data.Where(d => d.ClientID == clientID);

			if (projectID != 0)
				data = data.Where(d => d.ProjectID == projectID);

			if (expenseCategoryTypeID != 0)
				data = data.Where(d => d.ExpenseCategoryTypeID == expenseCategoryTypeID);

			if (lockedStatus != "")
				data = data.Where(d => d.Locked != (lockedStatus == "0"));

			if (billableStatus != "")
				data = data.Where(d => d.Billable != (billableStatus == "0"));

			if (activeStatus != "")
				data = data.Where(d => d.IsActive != (activeStatus == "0"));

			return data.OrderBy(u => u.Employee).ThenBy(d => d.ExpenseDate).ToList();
		}

		#endregion REPORT
	}
}