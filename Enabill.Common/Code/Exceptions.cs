using System;
using System.Runtime.Serialization;

namespace Enabill
{
	[Serializable]
	public class EnabillException : Exception
	{
		public EnabillException()
		{
		}

		public EnabillException(string message) : base(message)
		{
		}

		public EnabillException(string message, Exception inner) : base(message, inner)
		{
		}

		protected EnabillException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class EnabillConsumerException : EnabillException
	{
		public EnabillConsumerException()
		{
		}

		public EnabillConsumerException(string message) : base(message)
		{
		}

		public EnabillConsumerException(string message, Exception inner) : base(message, inner)
		{
		}

		protected EnabillConsumerException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class EnabillDomainException : EnabillException
	{
		public EnabillDomainException()
		{
		}

		public EnabillDomainException(string message) : base(message)
		{
		}

		public EnabillDomainException(string message, Exception inner) : base(message, inner)
		{
		}

		protected EnabillDomainException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class EnabillCommunicationException : EnabillException
	{
		public EnabillCommunicationException()
		{
		}

		public EnabillCommunicationException(string message) : base(message)
		{
		}

		public EnabillCommunicationException(string message, Exception inner) : base(message, inner)
		{
		}

		protected EnabillCommunicationException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class EnabillArchiveException : EnabillDomainException
	{
		public EnabillArchiveException()
		{
		}

		public EnabillArchiveException(string message) : base(message)
		{
		}

		public EnabillArchiveException(string message, Exception inner) : base(message, inner)
		{
		}

		protected EnabillArchiveException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class ActivityAdminException : EnabillDomainException
	{
		public ActivityAdminException()
		{
		}

		public ActivityAdminException(string message) : base(message)
		{
		}

		public ActivityAdminException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ActivityAdminException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class ClientContactException : EnabillDomainException
	{
		public ClientContactException()
		{
		}

		public ClientContactException(string message) : base(message)
		{
		}

		public ClientContactException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ClientContactException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class ClientManagementException : EnabillDomainException
	{
		public ClientManagementException()
		{
		}

		public ClientManagementException(string message) : base(message)
		{
		}

		public ClientManagementException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ClientManagementException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class FeedbackException : EnabillDomainException
	{
		public FeedbackException()
		{
		}

		public FeedbackException(string message) : base(message)
		{
		}

		public FeedbackException(string message, Exception inner) : base(message, inner)
		{
		}

		protected FeedbackException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class FlexiBalanceException : EnabillDomainException
	{
		public FlexiBalanceException()
		{
		}

		public FlexiBalanceException(string message) : base(message)
		{
		}

		public FlexiBalanceException(string message, Exception inner) : base(message, inner)
		{
		}

		protected FlexiBalanceException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class FlexiBalanceAdjustmentException : EnabillDomainException
	{
		public FlexiBalanceAdjustmentException()
		{
		}

		public FlexiBalanceAdjustmentException(string message) : base(message)
		{
		}

		public FlexiBalanceAdjustmentException(string message, Exception inner) : base(message, inner)
		{
		}

		protected FlexiBalanceAdjustmentException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class FlexiDayException : EnabillDomainException
	{
		public FlexiDayException()
		{
		}

		public FlexiDayException(string message) : base(message)
		{
		}

		public FlexiDayException(string message, Exception inner) : base(message, inner)
		{
		}

		protected FlexiDayException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class InvoiceException : EnabillDomainException
	{
		public InvoiceException()
		{
		}

		public InvoiceException(string message) : base(message)
		{
		}

		public InvoiceException(string message, Exception inner) : base(message, inner)
		{
		}

		protected InvoiceException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class InvoiceRuleException : EnabillException
	{
		public InvoiceRuleException()
		{
		}

		public InvoiceRuleException(string message) : base(message)
		{
		}

		public InvoiceRuleException(string message, Exception inner) : base(message, inner)
		{
		}

		protected InvoiceRuleException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class LeaveException : EnabillDomainException
	{
		public LeaveException()
		{
		}

		public LeaveException(string message) : base(message)
		{
		}

		public LeaveException(string message, Exception inner) : base(message, inner)
		{
		}

		protected LeaveException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class LeaveManagementException : EnabillDomainException
	{
		public LeaveManagementException()
		{
		}

		public LeaveManagementException(string message) : base(message)
		{
		}

		public LeaveManagementException(string message, Exception inner) : base(message, inner)
		{
		}

		protected LeaveManagementException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class NoteException : EnabillDomainException
	{
		public NoteException()
		{
		}

		public NoteException(string message) : base(message)
		{
		}

		public NoteException(string message, Exception inner) : base(message, inner)
		{
		}

		protected NoteException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class ProjectManagementException : EnabillDomainException
	{
		public ProjectManagementException()
		{
		}

		public ProjectManagementException(string message) : base(message)
		{
		}

		public ProjectManagementException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ProjectManagementException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class EnabillPassphraseException : EnabillDomainException
	{
		public EnabillPassphraseException()
		{
		}

		public EnabillPassphraseException(string message) : base(message)
		{
		}

		public EnabillPassphraseException(string message, Exception inner) : base(message, inner)
		{
		}

		protected EnabillPassphraseException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class EnabillSettingsException : EnabillDomainException
	{
		public EnabillSettingsException()
		{
		}

		public EnabillSettingsException(string message) : base(message)
		{
		}

		public EnabillSettingsException(string message, Exception inner) : base(message, inner)
		{
		}

		protected EnabillSettingsException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class TimesheetApprovalException : EnabillDomainException
	{
		public TimesheetApprovalException()
		{
		}

		public TimesheetApprovalException(string message) : base(message)
		{
		}

		public TimesheetApprovalException(string message, Exception inner) : base(message, inner)
		{
		}

		protected TimesheetApprovalException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class ExpenseApprovalException : EnabillDomainException
	{
		public ExpenseApprovalException()
		{
		}

		public ExpenseApprovalException(string message) : base(message)
		{
		}

		public ExpenseApprovalException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ExpenseApprovalException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class ExpenseAttachmentException : EnabillDomainException
	{
		public ExpenseAttachmentException()
		{
		}

		public ExpenseAttachmentException(string message) : base(message)
		{
		}

		public ExpenseAttachmentException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ExpenseAttachmentException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class ContractAttachmentException : EnabillDomainException
	{
		public ContractAttachmentException()
		{
		}

		public ContractAttachmentException(string message) : base(message)
		{
		}

		public ContractAttachmentException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ContractAttachmentException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class UserManagementException : EnabillDomainException
	{
		public UserManagementException()
		{
		}

		public UserManagementException(string message) : base(message)
		{
		}

		public UserManagementException(string message, Exception inner) : base(message, inner)
		{
		}

		protected UserManagementException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class UserPreferenceException : EnabillDomainException
	{
		public UserPreferenceException()
		{
		}

		public UserPreferenceException(string message) : base(message)
		{
		}

		public UserPreferenceException(string message, Exception inner) : base(message, inner)
		{
		}

		protected UserPreferenceException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class UserRoleException : EnabillDomainException
	{
		public UserRoleException()
		{
		}

		public UserRoleException(string message) : base(message)
		{
		}

		public UserRoleException(string message, Exception inner) : base(message, inner)
		{
		}

		protected UserRoleException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class UserWorkSessionException : EnabillDomainException
	{
		public UserWorkSessionException()
		{
		}

		public UserWorkSessionException(string message) : base(message)
		{
		}

		public UserWorkSessionException(string message, Exception inner) : base(message, inner)
		{
		}

		protected UserWorkSessionException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class WorkAllocationException : EnabillDomainException
	{
		public WorkAllocationException()
		{
		}

		public WorkAllocationException(string message) : base(message)
		{
		}

		public WorkAllocationException(string message, Exception inner) : base(message, inner)
		{
		}

		protected WorkAllocationException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class WorkDayException : EnabillDomainException
	{
		public WorkDayException()
		{
		}

		public WorkDayException(string message) : base(message)
		{
		}

		public WorkDayException(string message, Exception inner) : base(message, inner)
		{
		}

		protected WorkDayException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class ExpenseException : EnabillDomainException
	{
		public ExpenseException()
		{
		}

		public ExpenseException(string message) : base(message)
		{
		}

		public ExpenseException(string message, Exception inner) : base(message, inner)
		{
		}

		protected ExpenseException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}

	[Serializable]
	public class BillingException : EnabillDomainException
	{
		public BillingException()
		{
		}

		public BillingException(string message) : base(message)
		{
		}

		public BillingException(string message, Exception inner) : base(message, inner)
		{
		}

		protected BillingException(
		  SerializationInfo info,
		  StreamingContext context)
			: base(info, context) { }
	}
}