using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("FeedbackTypes")]
	public class FeedbackType
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int FeedbackTypeID { get; private set; }

		[Required, MaxLength(64)]
		public string FeedbackTypeName { get; internal set; }

		#endregion PROPERTIES

		#region FEEDBACK TYPE

		public static FeedbackType GetByID(int feedbackTypeID) => FeedbackTypeRepo.GetByID(feedbackTypeID);

		public static List<FeedbackType> GetAll() => FeedbackTypeRepo.GetAll()
					.OrderBy(f => f.FeedbackTypeName)
					.ToList();

		#endregion FEEDBACK TYPE
	}
}