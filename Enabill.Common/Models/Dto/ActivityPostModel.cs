namespace Enabill.Models.Dto
{
	public class ActivityPostModel
	{
		#region PROPERTIES

		public int ActivityId { get; set; }
		public double Monday { get; set; }
		public double Tuesday { get; set; }
		public double Wednesday { get; set; }
		public double Thursday { get; set; }
		public double Friday { get; set; }
		public double Saturday { get; set; }
		public double Sunday { get; set; }
		public string RemarkMonday { get; set; }
		public string RemarkTuesday { get; set; }
		public string RemarkWednesday { get; set; }
		public string RemarkThursday { get; set; }
		public string RemarkFriday { get; set; }
		public string RemarkSaturday { get; set; }
		public string RemarkSunday { get; set; }

		#endregion PROPERTIES
	}
}