namespace Enabill.Comms
{
	public enum MailingMethod
	{
		/// <summary>
		/// Use AppSettings["MailingMethod"]
		/// </summary>
		UseConfig = 1,

		/// <summary>
		/// Send directly from Thread.
		/// </summary>
		Direct = 2,

		/// <summary>
		/// Send via queue
		/// </summary>
		Queue = 3
	}
}