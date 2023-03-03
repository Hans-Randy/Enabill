using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using Enabill.Repos;
using NLog;

namespace Enabill.Models
{
	[Table("ContractAttachments")]
	public class ContractAttachment
	{
		#region LOGGER

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		#endregion LOGGER

		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ContractAttachmentID { get; set; }

		[Required]
		public int ClientID { get; set; }

		[Required]
		public int ProjectID { get; set; }

		[Required]
		[MinLength(10), MaxLength(30)]
		public string FileName { get; set; }

		[Required]
		[MinLength(2), MaxLength(100)]
		public string FilePath { get; set; }

		[Required]
		public string MimeType { get; set; }

		[NotMapped]
		public HttpPostedFileBase AttachmentFile { get; set; }

		#endregion PROPERTIES

		#region EXPENSE ATTACHMENT

		public static ContractAttachment GetContractAttachmentByFileName(string fileName) => ContractRepo.GetAttachmentByFileName(fileName);

		public static ContractAttachment GetNew(int clientID, int projectID, string filePath, string fileName, string mimeType) => new ContractAttachment()
		{
			ClientID = clientID,
			ProjectID = projectID,
			FilePath = filePath,
			FileName = fileName,
			MimeType = mimeType
		};

		public void Save() => ContractRepo.SaveAttachment(this);

		public void Delete() => ContractRepo.DeleteAttachment(this);

		#endregion EXPENSE ATTACHMENT
	}
}