using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("FeedbackUrgencyTypes")]
	public class FeedbackUrgencyType
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int FeedbackUrgencyTypeID { get; private set; }

		[Required, MaxLength(64)]
		public string FeedbackUrgencyTypeName { get; private set; }

		#endregion PROPERTIES

		#region FEEDBACK URGENCY TYPES

		public static FeedbackUrgencyType GetByID(int feedbackUrgencyTypeID) => FeedbackUrgencyTypeRepo.GetByID(feedbackUrgencyTypeID);

		public static List<FeedbackUrgencyType> GetAll() => FeedbackUrgencyTypeRepo.GetAll()
					.ToList();

		#endregion FEEDBACK URGENCY TYPES
	}
}