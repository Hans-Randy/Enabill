using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class ContactEditModel
	{
		#region INITIALIZATION

		public ContactEditModel(User userRequesting, Client client, int contactID)
		{
			this.Contact = client.GetContact(contactID);
			this.Contact = this.CheckContact(this.Contact);
			this.ContactInvoices = this.Contact.GetLinkedInvoices(userRequesting);
		}

		public ContactEditModel(User userRequesting, Client client, Contact contact)
		{
			this.Client = client;
			this.Contact = contact;
			this.Contact = this.CheckContact(this.Contact);

			this.ContactInvoices = new List<Invoice>();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public Client Client { get; set; }
		public Contact Contact { get; set; }

		public List<Invoice> ContactInvoices { get; set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		internal Contact CheckContact(Contact contact)
		{
			string passPhrase = Enabill.Code.Constants.PASSPHRASE;

			contact.ContactName = Enabill.Helpers.DecryptString(contact.ContactName, passPhrase);
			contact.Email = Enabill.Helpers.DecryptString(contact.Email, passPhrase);
			contact.TelephoneNo = Enabill.Helpers.DecryptString(contact.TelephoneNo, passPhrase);
			contact.CellphoneNo = Enabill.Helpers.DecryptString(contact.CellphoneNo, passPhrase);

			return contact;
		}

		#endregion FUNCTIONS
	}
}