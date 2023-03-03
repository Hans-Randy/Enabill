using System;
using System.Collections.Generic;

namespace Enabill.Models.Dto
{
	public class ExpenseDetail
	{
		#region INITIALIZATION

		public ExpenseDetail()
		{
		}

		public ExpenseDetail(Expense e, Project p, Client c, ExpenseCategoryType ec)
		{
			this.ExpenseID = e.ExpenseID;
			this.ExpenseCategoryTypeName = ec.ExpenseCategoryTypeName;
			this.ClientName = c.ClientName;
			this.ProjectName = p.ProjectName;
			this.ExpenseDate = e.ExpenseDate;
			this.Amount = e.Amount;
			this.Billable = e.Billable;
			this.Locked = e.Locked;
			this.ClientID = c.ClientID;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool Billable { get; set; }
		public bool Locked { get; set; }

		public int ExpenseID { get; set; }

		public double Amount { get; set; }

		public int ClientID { get; set; }
		public string ClientName { get; set; }
		public string ExpenseCategoryTypeName { get; set; }
		public string ProjectName { get; set; }

		public DateTime ExpenseDate { get; set; }

		#endregion PROPERTIES
	}

	internal class ExpenseDetailModelComparer : IEqualityComparer<ExpenseDetail>
	{
		// Expenses are equal if their ID's and Dates are equal.
		public bool Equals(ExpenseDetail x, ExpenseDetail y)
		{
			//Check whether the compared objects reference the same data.
			if (ReferenceEquals(x, y))
				return true;

			//Check whether any of the compared objects is null.
			if (x is null || y is null)
				return false;

			//Check whether the expenses properties are equal.
			return x.ExpenseID == y.ExpenseID;
		}

		//If Equals() returns true for a pair of objects
		// then GetHashCode() must return the same value for these objects.

		public int GetHashCode(ExpenseDetail obj)
		{
			//Check whether the object is null
			if (obj is null)
				return 0;

			//Get hash code for the Code field.
			//Calculate the hash code for the product.
			return obj.ExpenseID.GetHashCode();
		}
	}
}