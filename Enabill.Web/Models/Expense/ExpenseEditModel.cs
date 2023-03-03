using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class ExpenseEditModel
	{
		#region INITIALIZATION

		public ExpenseEditModel(User user, Expense expense, List<ExpenseAttachment> listOfFiles = null, string fileName = "", string filePath = "", string callingPage = "")
		{
			this.ExpenseID = expense.ExpenseID;
			this.UserID = user.UserID;
			this.ExpenseCategoryTypeID = expense.ExpenseCategoryTypeID;
			this.ClientID = expense.ClientID;
			this.ProjectID = expense.ProjectID;
			this.ExpenseDate = expense.ExpenseDate;
			this.Amount = expense.Amount;
			this.Mileage = expense.Mileage;
			this.Notes = expense.Notes;
			this.Billable = expense.Billable;
			this.Locked = expense.Locked;
			this.FileName = fileName;
			this.FilePath = filePath;
			this.CallingPage = callingPage;
			this.User = user;
			this.Expense = expense;
			this.ListOfFiles = listOfFiles;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool Billable { get; set; }
		public bool Locked { get; set; }

		[Required]
		public int ClientID { get; set; }

		[Required]
		public int ExpenseID { get; set; }

		[Required]
		public int ExpenseCategoryTypeID { get; set; }

		[Required]
		public int ProjectID { get; set; }

		[Required]
		public int UserID { get; set; }

		[DisplayFormat(DataFormatString = "{0:C0}")]
		[RegularExpression(@"^\s*(?=.*[1-9])\d*(?:\.\d{1,2})?\s*$", ErrorMessage = "Please enter a valid number")]
		public double Amount { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Please enter a value larger than zero")]
		public double? Mileage { get; set; }

		public string CallingPage { get; set; }
		public string FileName { get; set; }
		public string FilePath { get; set; }

		[MinLength(2), MaxLength(500)]
		public string Notes { get; set; }

		[Required]
		public DateTime ExpenseDate { get; set; }

		public User User { get; set; }
		public Expense Expense { get; set; }

		public List<ExpenseAttachment> ListOfFiles { get; set; }

		#endregion PROPERTIES
	}
}