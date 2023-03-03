using System.Web.Mvc;
using Enabill.Repository.Interfaces;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class LogController : Controller
	{
		// GET: Log
		private ILogRepository logRepository;

		public LogController()
		{
		}

		public LogController(ILogRepository logRepository)
		{
			this.logRepository = logRepository;
		}

		public ActionResult Index()
		{
			this.logRepository.GetLogEntries();

			return this.View(this.logRepository.LogItems);
		}
	}
}