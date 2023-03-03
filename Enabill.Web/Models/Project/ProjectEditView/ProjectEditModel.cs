using System.Collections.Generic;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class ProjectEditModel
	{
		#region INITIALIZATION

		public ProjectEditModel(Project project, bool isActive, string fileName = "", string filePath = "")
		{
			this.ClientName = project.GetClient().ClientName;
			this.Project = project;
			this.ProjectActivities = new ProjectActivitiesModel(project, isActive);
			this.ProjectActivityUsers = new ProjectActivityUsersModel(project);
			this.ProjectInvoices = new ProjectInvoiceModel(project);
			this.ProjectInvoiceRules = new ProjectInvoiceRuleModel(project);
			this.ProjectContractAttachments = new ContractEditModel(project.GetClient(), project, fileName, filePath);
			this.ProjectRegionName = RegionRepo.GetByID(project.RegionID).RegionName;
			this.ProjectDepartmentName = DepartmentRepo.GetByID(project.DepartmentID).DepartmentName;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public string ClientName { get; private set; }
		public string ProjectDepartmentName { get; private set; }
		public string ProjectRegionName { get; private set; }

		public Project Project { get; private set; }
		public ProjectActivitiesModel ProjectActivities { get; private set; }
		public ProjectActivityUsersModel ProjectActivityUsers { get; private set; }
		public ProjectInvoiceModel ProjectInvoices { get; private set; }
		public ProjectInvoiceRuleModel ProjectInvoiceRules { get; private set; }
		public ContractEditModel ProjectContractAttachments { get; private set; }

		#endregion PROPERTIES
	}
}