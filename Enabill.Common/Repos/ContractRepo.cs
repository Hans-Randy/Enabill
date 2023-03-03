using System;
using System.Collections.Generic;
using System.Linq;
using Enabill.Models;
using Enabill.Models.Dto;

namespace Enabill.Repos
{
	public class ContractRepo : BaseRepo
	{
		#region ATTACHMENTS

		public static ContractAttachment GetAttachmentByFileName(string fileName) => DB.ContractAttachments
					.SingleOrDefault(e => e.FileName == fileName);

		public static List<ContractAttachment> GetListOfFilesByProjectID(int projectID) => DB.ContractAttachments
					.Where(e => e.ProjectID == projectID).ToList();

		public static void SaveAttachment(ContractAttachment contractAttachment)
		{
			if (contractAttachment.ContractAttachmentID == 0)
				DB.ContractAttachments.Add(contractAttachment);

			DB.SaveChanges();
		}

		public static void DeleteAttachment(ContractAttachment contractAttachment)
		{
			if (contractAttachment.ContractAttachmentID != 0)
				DB.ContractAttachments.Remove(contractAttachment);

			DB.SaveChanges();
		}

		#endregion ATTACHMENTS
	}
}