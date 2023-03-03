using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("ClientDepartmentCode")]
	public class ClientDepartmentCode
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ClientDepartmentCodeID { get; internal set; }

		[Required]
		public bool IsActive { get; set; }

		[Required]
		public int ClientID { get; set; }

		[Required, MaxLength(20)]
		public string DepartmentCode { get; set; }

		public static List<ClientDepartmentCode> GetByClientID(int clientID) => ClientDepartmentCodeRepo.GetByClientID(clientID).ToList();

		#endregion PROPERTIES

		#region FUNCTIONS

		public void ValidateSave()
		{
			if (string.IsNullOrEmpty(this.DepartmentCode))
				throw new EnabillException("Please enter a valid Department Code.");
		}

		#endregion FUNCTIONS
	}
}