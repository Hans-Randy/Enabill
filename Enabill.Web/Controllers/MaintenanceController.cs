using System;
using System.Web.Mvc;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class MaintenanceController : BaseController
	{
		[HttpGet]
		public ActionResult Index() => this.View();
	}
}