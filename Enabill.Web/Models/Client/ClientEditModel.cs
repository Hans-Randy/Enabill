using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Web.Models
{
	public class ClientEditModel
	{
		#region INITIALIZATION

		public ClientEditModel(Client client)
		{
			this.Client = client;
			this.Contacts = client.GetContacts();
			this.Contacts = this.CheckContacts(this.Contacts);
			this.ClientDepartmentCodes = client.GetClientDepartmentCodes();
		}

		public ClientEditModel(Client client, Contact contact)
		{
			this.Client = client;
			this.Contact = contact;
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public Client Client { get; set; }
		public Contact Contact { get; set; }

		public List<ClientDepartmentCode> ClientDepartmentCodes { get; private set; }
		public List<Contact> Contacts { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		internal List<Contact> CheckContacts(List<Contact> contacts)
		{
			foreach (var contact in contacts)
			{
				var clientContact = this.CheckContact(contact);
			}

			contacts.Sort((x, y) => x.ContactName.CompareTo(y.ContactName));

			return contacts;
		}

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