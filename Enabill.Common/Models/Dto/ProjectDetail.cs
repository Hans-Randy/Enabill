namespace Enabill.Models.Dto
{
	public class ProjectDetail
	{
		#region INITIALIZATION

		public ProjectDetail(Project p)
		{
			this.ProjectID = p.ProjectID;
			this.ProjectName = p.ProjectName;
			this.MustHaveRemarks = p.MustHaveRemarks;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public bool MustHaveRemarks { get; set; }

		public int ProjectID { get; set; }

		public string ProjectName { get; set; }

		#endregion PROPERTIES
	}
}