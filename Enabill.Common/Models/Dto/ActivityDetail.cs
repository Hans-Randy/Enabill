using System.Collections.Generic;

namespace Enabill.Models.Dto
{
	public class ActivityDetail
	{
		#region INITIALIZATION

		public ActivityDetail()
		{
		}

		public ActivityDetail(Activity a, Project p, Client c, bool isSelected)
		{
			this.ActivityID = a.ActivityID;
			this.ActivityName = a.ActivityName;
			this.ProjectID = p.ProjectID;
			this.ProjectName = p.ProjectName;
			this.ClientID = c.ClientID;
			this.ClientName = c.ClientName;
			this.CanHaveNotes = a.CanHaveNotes;
			this.IsSelected = isSelected;
			this.MustHaveRemarks = a.MustHaveRemarks;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool CanHaveNotes { get; set; }
		public bool IsSelected { get; set; }
		public bool MustHaveRemarks { get; set; }

		public int ActivityID { get; set; }
		public int ClientID { get; set; }
		public int ProjectID { get; set; }

		public string ActivityName { get; set; }
		public string ClientName { get; set; }
		public string ProjectName { get; set; }

		#endregion PROPERTIES
	}

	#region FUNCTIONS

	internal class ActivityDetailModelComparer : IEqualityComparer<ActivityDetail>
	{
		// Activities are equal if their names and activityID's are equal.
		public bool Equals(ActivityDetail x, ActivityDetail y)
		{
			//Check whether the compared objects reference the same data.
			if (ReferenceEquals(x, y))
				return true;

			//Check whether any of the compared objects is null.
			if (x is null || y is null)
				return false;

			//Check whether the products' properties are equal.
			return x.ActivityID == y.ActivityID && x.ActivityName == y.ActivityName;
		}

		// If Equals() returns true for a pair of objects
		// then GetHashCode() must return the same value for these objects.

		public int GetHashCode(ActivityDetail obj)
		{
			//Check whether the object is null
			if (obj is null)
				return 0;

			//Get hash code for the Name field if it is not null.
			int hashActivityName = obj.ActivityName?.GetHashCode() ?? 0;

			//Get hash code for the Code field.
			int hashActivityID = obj.ActivityID.GetHashCode();

			//Calculate the hash code for the product.
			return hashActivityName ^ hashActivityID;
		}
	}

	#endregion FUNCTIONS
}