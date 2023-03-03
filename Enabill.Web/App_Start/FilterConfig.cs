using System.Web.Mvc;

namespace Enabill.Web
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters) => filters.Add(new HandleErrorAttribute());
	}
}