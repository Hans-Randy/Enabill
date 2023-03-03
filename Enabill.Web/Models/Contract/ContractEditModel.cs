using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Enabill.Models;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class ContractEditModel
	{
		#region INITIALIZATION

		public ContractEditModel(Client client, Project project, string fileName = "", string filePath = "")
		{
			this.ClientID = client.ClientID;
			this.ProjectID = project.ProjectID;
			this.FileName = fileName;
			this.FilePath = filePath;
			this.CreatedDate = project.CreatedDate;
			this.ListOfFiles = ContractRepo.GetListOfFilesByProjectID(project.ProjectID);
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		[Required]
		public int ContractAttachmentID { get; set; }

		[Required]
		public int ClientID { get; set; }

		[Required]
		public int ProjectID { get; set; }

		public string FileName { get; set; }
		public string FilePath { get; set; }

		public DateTime CreatedDate { get; set; }

		public List<ContractAttachment> ListOfFiles { get; set; }

		#endregion PROPERTIES
	}
}