using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("Contacts")]
	public class Contact
	{
		#region PROPERTIES

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ContactID { get; set; }

		[Required]
		public bool IsActive { get; set; }

		[Required]
		public int ClientID { get; set; }

		[MaxLength(128)]
		public string CellphoneNo { get; set; }

		[Required, MaxLength(128)]
		public string ContactName { get; set; }

		[MaxLength(128)]
		public string Email { get; set; }

		[MaxLength(50)]
		public string JobTitle { get; set; }

		[Required, MinLength(3), MaxLength(128)]
		public string LastModifiedBy { get; internal set; }

		[MaxLength(128)]
		public string TelephoneNo { get; set; }

		#endregion PROPERTIES

		#region CLIENT

		public Client GetClient() => ClientRepo.GetByID(this.ClientID);

		#endregion CLIENT

		#region INVOICE RULE

		public bool IsLinkedToAnInvoiceRule() => this.GetInvoiceRulesForContact().Count > 0;

		public bool IsLinkedToAnInvoiceRuleByCC() => this.GetInvoiceRulesWhereContactisLinkedByCC().Count > 0;

		public List<InvoiceRule> GetInvoiceRulesForContact(User userRequesting)
		{
			if (!userRequesting.HasRole(UserRoleType.SystemAdministrator))
				throw new UserRoleException("You do not have the required permissions to view invoice rules. Action cancelled.");

			return this.GetInvoiceRulesForContact();
		}

		internal List<InvoiceRule> GetInvoiceRulesForContact() => ContactRepo.GetAllInvoiceRulesForWhichContactIsLinkedAsDefaultContact(this.ContactID)
				.ToList();

		public List<InvoiceRule> GetInvoiceRulesWhereContactisLinkedByCC(User userRequesting)
		{
			if (!userRequesting.HasRole(UserRoleType.SystemAdministrator))
				throw new UserRoleException("You do not have the required permissions to view invoice rules. Action cancelled.");

			return this.GetInvoiceRulesWhereContactisLinkedByCC();
		}

		internal List<InvoiceRule> GetInvoiceRulesWhereContactisLinkedByCC() => ContactRepo.GetAllInvoiceRulesForWhichContactIsLinkedByCC(this.ContactID)
				.ToList();

		#endregion INVOICE RULE

		#region INVOICE

		#region CONTACTS LINKED TO INVOICE BY BEING DEFAULT CONTACT

		public bool IsLinkedToAnInvoice(User userRequesting)
		{
			if (!userRequesting.CanManage(this))
				throw new UserRoleException("You do not have the required permissions to manage this contact. Action cancelled.");

			return this.IsLinkedToAnInvoice();
		}

		internal bool IsLinkedToAnInvoice()
		{
			if (this.GetLinkedInvoices().Count > 0)
				return true;

			if (this.GetLinkedInvoicesCC().Count > 0)
				return true;

			return false;
		}

		public List<Invoice> GetLinkedInvoices(User userRequesting)
		{
			if (!userRequesting.CanManage(this))
				throw new ClientContactException("You do not have the required permissions to view the requested detail. Action cancelled.");

			return this.GetLinkedInvoices();
		}

		private List<Invoice> GetLinkedInvoices() => ContactRepo.GetAllInvoicesForwhichContactIsLinked(this.ContactID).ToList();

		#endregion CONTACTS LINKED TO INVOICE BY BEING DEFAULT CONTACT

		#region INVOICE WHERE CONTACT IS LINKED BY CC STATUS

		internal bool IsLinkedToAnInvoiceByCC() => this.GetLinkedInvoicesCC().Count > 0;

		public List<Invoice> GetLinkedInvoicesCC() => ContactRepo.GetAllInvoicesForWhichContactIsLinkedByCC(this.ContactID)
						.ToList();

		#endregion INVOICE WHERE CONTACT IS LINKED BY CC STATUS

		#endregion INVOICE
	}
}