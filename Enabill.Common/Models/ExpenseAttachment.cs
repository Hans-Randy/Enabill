using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using Enabill.Repos;
using NLog;

namespace Enabill.Models
{
	[Table("ExpenseAttachments")]
	public class ExpenseAttachment
	{
		#region LOGGER

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		#endregion LOGGER

		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ExpenseAttachmentID { get; set; }

		[Required]
		public int ExpenseID { get; set; }

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

		public static ExpenseAttachment GetExpenseAttachmentByFileName(string fileName) => ExpenseRepo.GetAttachmentByFileName(fileName);

		public static ExpenseAttachment GetNew(int expenseID, string filePath, string fileName, string mimeType) => new ExpenseAttachment()
		{
			ExpenseID = expenseID,
			FilePath = filePath,
			FileName = fileName,
			MimeType = mimeType
		};

		public void Save() => ExpenseRepo.SaveAttachment(this);

		public void Delete() => ExpenseRepo.DeleteAttachment(this);

		#endregion EXPENSE ATTACHMENT
	}
}