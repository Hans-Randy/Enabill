using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("LoginLogs")]
	public class LoginLog
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int LoginLogID { get; private set; }

		[MaxLength(128)]
		public string ResolvedUserName { get; internal set; }

		[Required, MaxLength(128)]
		public string UserName { get; internal set; }

		[Required]
		public DateTime LoginDate { get; internal set; }

		#endregion PROPERTIES

		#region LOGIN LOG

		public static void CaptureNewLogin(string userName, string resolvedUserName)
		{
			var newLogin = new LoginLog()
			{
				UserName = userName,
				LoginDate = DateTime.Now.ToCentralAfricanTime(),
				ResolvedUserName = resolvedUserName
			};

			if (string.IsNullOrEmpty(resolvedUserName))
				newLogin.ResolvedUserName = null;

			LoginLogRepo.Save(newLogin);
		}

		#endregion LOGIN LOG
	}
}