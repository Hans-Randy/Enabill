using System.Collections.Generic;
using System.Web.Mvc;
using Enabill.Models;
using Enabill.Properties;
using Enabill.Repos;
using Enabill.Web.Models;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class ContactController : SearchableController
	{
		protected override string GetSearchLabelText() => "contacts";

		public override ActionResult Index(string q, bool? isActive)
		{
			if (!this.CurrentUser.HasRole(UserRoleType.SystemAdministrator))
				return this.ErrorView(new ErrorModel(Resources.ERR_NoPermissionToViewPage_Message, true));

			this.ViewBag.Contact_q = InputHistory.Get(q, HistoryItemType.ContactSearchCriteria);
			List<Contact> model = ContactRepo.FilterByName(this.ViewBag.Contact_q);

			string passPhrase = Enabill.Code.Constants.PASSPHRASE;

			foreach (var contact in model)
			{
				contact.ContactName = Enabill.Helpers.DecryptString(contact.ContactName, passPhrase);
				contact.Email = Enabill.Helpers.DecryptString(contact.Email, passPhrase);
				contact.TelephoneNo = Enabill.Helpers.DecryptString(contact.TelephoneNo, passPhrase);
				contact.CellphoneNo = Enabill.Helpers.DecryptString(contact.CellphoneNo, passPhrase);
			}

			SaveSearchInput(HistoryItemType.ContactSearchCriteria, this.ViewBag.Contact_q);

			model.Sort((x, y) => x.ContactName.CompareTo(y.ContactName));

			return this.View(model);
		}
	}
}