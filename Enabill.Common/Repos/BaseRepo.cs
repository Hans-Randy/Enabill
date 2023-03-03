using Enabill.DB;

namespace Enabill.Repos
{
	public abstract class BaseRepo
	{
		public static EnabillContext DB => EnabillSettings.DB;
	}
}