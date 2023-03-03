using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;
using NLog;

namespace Enabill.Models
{
	[Table("Expenses")]
	public class Expense
	{
		#region LOGGER

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		#endregion LOGGER

		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ExpenseID { get; set; }

		public bool Billable { get; set; }
		public bool Locked { get; set; }

		[Required]
		public int ClientID { get; set; }

		[Required]
		public int ExpenseCategoryTypeID { get; set; }

		[Required]
		public int ProjectID { get; set; }

		[Required]
		public int UserID { get; set; }

		[Required]
		public double Amount { get; set; }

		public double? Mileage { get; set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[MaxLength(128)]
		public string ManagedBy { get; set; }

		public string Notes { get; set; }

		[Required]
		public DateTime ExpenseDate { get; set; }

		public DateTime? DateManaged { get; internal set; }

	
		#endregion PROPERTIES

		#region EXPENSE

		public static List<Expense> GetAll() => ExpenseRepo.GetAll()
			.OrderBy(e => e.ExpenseDate)
			.ToList();

		public static Expense GetNew(int userID) => new Expense()
		{
			ExpenseDate = DateTime.Today,
			UserID = userID,
			Amount = 0,
			Billable = true,
			Locked = false,
			LastModifiedBy = "Sys"
		};

		public void Save(User userSaving)
		{
			var dt = DateTime.Now.ToCentralAfricanTime();

			if (this.ExpenseDate == default)
				throw new ExpenseException("Please select a date.");

			if (this.ClientID < 1)
				throw new ExpenseException("Please select a client.");

			if (this.ProjectID < 1)
				throw new ExpenseException("Please select a project.");

			if (this.ExpenseCategoryTypeID < 1)
				throw new ExpenseException("Please select an Expense Category.");

			if (this.Amount == 0)
				throw new ExpenseException("Please enter an Amount.");

			if (this.ExpenseID == 0)
			{
				this.UserID = userSaving.UserID;
				this.ManagedBy = null;
				this.DateManaged = null;
			}
			else
			{
				this.ManagedBy = userSaving == null ? "System" : userSaving.FullName;
				this.DateManaged = DateTime.Now.ToCentralAfricanTime();
			}

			this.LastModifiedBy = userSaving == null ? "System" : userSaving.FullName;

			ExpenseRepo.Save(this);
		}

		#endregion EXPENSE

		#region USER

		internal User GetUser() => UserRepo.GetByID(this.UserID);

		#endregion USER
	}
}