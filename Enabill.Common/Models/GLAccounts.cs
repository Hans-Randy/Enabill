using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("GLAccounts")]
	public class GLAccount
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int GLAccountID { get; internal set; }

		[Required]
		public bool IsActive { get; set; }

		[Required, MaxLength(20)]
		public string GLAccountCode { get; set; }

		[Required, MaxLength(200)]
		public string GLAccountName { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public static GLAccount GetGLAccountByID(int glAccountID) => GLAccountRepo.GetByID(glAccountID);

		public static List<GLAccount> GetAll() => GLAccountRepo.GetAll().Where(s => s.IsActive).ToList();

		public static GLAccount GetNew() => new GLAccount()
		{
			IsActive = true
		};

		public void ValidateSave()
		{
			if (string.IsNullOrEmpty(this.GLAccountCode) && string.IsNullOrEmpty(this.GLAccountName))
				throw new EnabillException("Please enter valid Codes.");

			if (string.IsNullOrEmpty(this.GLAccountCode))
				throw new EnabillException("Please enter a valid GL Account Code.");

			if (string.IsNullOrEmpty(this.GLAccountName))
				throw new EnabillException("Please enter a valid GL Account Name.");
		}

		public static void Save(GLAccount gLAccount) => GLAccountRepo.Save(gLAccount);

		#endregion FUNCTIONS
	}
}