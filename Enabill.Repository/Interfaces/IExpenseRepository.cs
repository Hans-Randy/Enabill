using System.Collections.Generic;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repository.Interfaces
{
	public interface IExpenseRepository : IBaseRepository
	{
		IList<Expense> Expenses { get; }
		Expense Expense { get; set; }

		void GetExpenses();

		void GetExpense(int expenseId);

		//ToDo
		void SearchExpenses(ExpenseFilterModel expenseFilterModel);
	}
}