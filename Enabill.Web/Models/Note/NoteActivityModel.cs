using System.Collections.Generic;
using System.Linq;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class NoteActivityModel
	{
		#region INITIALIZATION

		public NoteActivityModel(User user)
		{
			this.ActivityModel = CommonModelFunctions.PopulateNoteActivitySearchModelForTimeCapturer(user);

			this.ClientDistinctList = this.ActivityModel.Distinct(new NoteActivityModelClientComparer()).ToList();
			this.ProjectDistinctList = this.ActivityModel.Distinct(new NoteActivityModelProjectComparer()).ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<ActivitySearchModel> ActivityModel { get; private set; }
		public List<ActivitySearchModel> ClientDistinctList { get; private set; }
		public List<ActivitySearchModel> ProjectDistinctList { get; private set; }

		#endregion PROPERTIES

		#region IEQUALITY COMPARERS

		private class NoteActivityModelClientComparer : IEqualityComparer<ActivitySearchModel>
		{
			// Projects are equal if their names and projectID's are equal.
			public bool Equals(ActivitySearchModel x, ActivitySearchModel y)
			{
				//Check whether any of the compared objects is null.
				if (x is null || y is null)
					return false;

				//Check whether the products' properties are equal.
				return x.ClientID == y.ClientID;
			}

			// If Equals() returns true for a pair of objects
			// then GetHashCode() must return the same value for these objects.

			public int GetHashCode(ActivitySearchModel model)
			{
				//Check whether the object is null
				if (model is null)
					return 0;

				//Get hash code for the Name field if it is not null.
				int hashClientName = model.ClientName.GetHashCode();

				//Get hash code for the Code field.
				int hashClientID = model.ClientID.GetHashCode();

				//Calculate the hash code for the product.
				return hashClientName ^ hashClientID;
			}
		}

		private class NoteActivityModelProjectComparer : IEqualityComparer<ActivitySearchModel>
		{
			// Projects are equal if their names and projectID's are equal.
			public bool Equals(ActivitySearchModel x, ActivitySearchModel y)
			{
				//Check whether any of the compared objects is null.
				if (x is null || y is null)
					return false;

				//Check whether the products' properties are equal.
				return x.ClientID == y.ClientID && x.ProjectID == y.ProjectID;
			}

			// If Equals() returns true for a pair of objects
			// then GetHashCode() must return the same value for these objects.

			public int GetHashCode(ActivitySearchModel model)
			{
				//Check whether the object is null
				if (model is null)
					return 0;

				//Get hash code for the Name field if it is not null.
				int hashProjectName = model.ProjectName.GetHashCode();

				//Get hash code for the Code field.
				int hashProjectID = model.ProjectID.GetHashCode();

				//Calculate the hash code for the product.
				return hashProjectName ^ hashProjectID;
			}
		}

		#endregion IEQUALITY COMPARERS
	}
}