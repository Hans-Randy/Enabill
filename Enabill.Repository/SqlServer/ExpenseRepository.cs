using System.Collections.Generic;
using System.Linq;
using Alacrity.DataAccess;
using Dapper;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repository.Interfaces;

namespace Enabill.Repository.SqlServer
{
	public class ExpenseRepository : BaseRepository, IExpenseRepository
	{
		private IList<Expense> expenses;
		public IList<Expense> Expenses => this.expenses;
		private Expense expense;

		public Expense Expense
		{
			get => this.expense;
			set => this.expense = value;
		}

		public ExpenseRepository()
		{
		}

		public ExpenseRepository(IDbManager dbManager)
			: base(dbManager)
		{
		}

		public void GetExpenses() => this.expenses = this.DbManager.Connection.Query<Expense>("SELECT [ExpenseID],[UserID],[ClientID],[ProjectID],[ExepenseCategoryTypeID],[ExpenseDate],[Amount],[Notes],[Billable],[Locked],[ManagedBy],[DateManaged],[LastModifiedBy] FROM [dbo].[Expenses]").ToList();

		public void GetExpense(int expenseId) => this.expense = this.DbManager.Connection.Query<Expense>("SELECT [ExpenseID],[UserID],[ClientID],[ProjectID],[ExepenseCategoryTypeID],[ExpenseDate],[Amount],[Notes],[Billable],[Locked],[ManagedBy],[DateManaged],[LastModifiedBy] FROM [dbo].[Expenses] WHERE ExpenseID=@ExpenseId", new { ExpenseId = expenseId }).FirstOrDefault();

		//ToDo
		public void SearchExpenses(ExpenseFilterModel expenseFilterModel)
		{
			this.GetExpenses();
			if (!string.IsNullOrEmpty(expenseFilterModel.FilterText))
			{
				string filteredText = expenseFilterModel.FilterText.ToLower();

				//ToDo
				//if (this.expenses.Where(c => c.ClientName != null).Where(c => c.ClientName.ToLower().Contains(filteredText)).Any())
				//{
				//	this.expenses = this.expenses.Where(c => c.ClientName != null).Where(c => c.ClientName.ToLower().Contains(filteredText)).ToList();
				//}

				//if (this.expenses.Where(c => c.RegisteredName != null).Where(c => c.RegisteredName.ToLower().Contains(filteredText)).Any())
				//{
				//	this.expenses = this.expenses.Where(c => c.RegisteredName != null).Where(c => c.RegisteredName.ToLower().Contains(filteredText)).ToList();
				//}

				//if (this.expenses.Where(c => c.AccountCode != null).Where(c => c.AccountCode.ToLower().Contains(filteredText)).Any())
				//{
				//	this.expenses = this.expenses.Where(c => c.AccountCode != null).Where(c => c.AccountCode.ToLower().Contains(filteredText)).ToList();
				//}

				//if (this.expenses.Where(c => c.VATNo != null).Where(c => c.VATNo.ToLower().Contains(filteredText)).Any())
				//{
				//	this.expenses = this.expenses.Where(c => c.VATNo != null).Where(c => c.VATNo.ToLower().Contains(filteredText)).ToList();
				//}
			}

			this.expenses = this.expenses.Where(c => c.Locked == expenseFilterModel.Locked).ToList();
		}
	}
}