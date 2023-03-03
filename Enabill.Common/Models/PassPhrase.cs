using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("PassPhrases")]
	public class PassPhrase
	{
		#region PROPERTIES

		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int PassPhraseID { get; set; }

		[Required]
		public int ModifiedBy { get; set; }

		[Required, MaxLength(100)]
		public string PassPhraseName { get; set; }

		[Required]
		public DateTime ModifiedDate { get; set; }

		#endregion PROPERTIES

		#region METHODS

		public void Save() => PassPhraseRepo.Save(this);

		public static List<PassPhrase> GetAll() => PassPhraseRepo.GetAll().ToList();

		public static PassPhrase GetPassPhrase() => PassPhraseRepo.GetPassPhrase();

		#endregion METHODS
	}
}