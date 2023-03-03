using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enabill.Code;
using Enabill.Comms;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Emailing
{
	public class EnabillEmails
	{
		#region SEND EMAIL

		public static void SendEmail(EnabillEmailModel model)
		{
			try
			{
				Mailer.Send(model.From, model.ReplyTo, model.Recipients, model.Subject, model.Body, model.Attachments);
			}
			catch (EnabillCommunicationException ex)
			{
				throw new EnabillException(ex.Message);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}
		}

		#endregion SEND EMAIL

		#region CLIENT MANAGEMENT

		public static void NotifyAccountantsAboutNewClient(Client client)
		{
			if (client == null)
				return;

			var reciepients = new List<string>();
			User.GetByRoleBW((int)UserRoleType.Accountant).ForEach(u => reciepients.Add(u.Email));

			const string subject = "New Client Added to Enabill";
			var body = new StringBuilder();

			body.AppendLine("<h1>Good day,</h1>")
				.AppendLine("<p>A new client has been added to the system.<p>")
				.AppendLine("The new client's credentials: <br />")
				.AppendLine("<ul>")
				.AppendFormat($"<li>Client Name: {client.ClientName}</li>").AppendLine()
				.AppendLine("<br />");

			if (!string.IsNullOrEmpty(client.RegisteredName))
			{
				body.AppendFormat($"<li>Registered Name: {client.RegisteredName}</li>").AppendLine()
					.AppendLine("<br />");
			}

			if (!string.IsNullOrEmpty(client.VATNo))
			{
				body.AppendFormat($"<li>VAT No: {client.VATNo}</li>").AppendLine()
					.AppendLine("<br />");
			}

			body.AppendLine("</ul>")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, reciepients, body.ToString());

			SendEmail(model);
		}

		#endregion CLIENT MANAGEMENT

		#region FLEXIDAYS

		public static void NotifyManagerOfBookedFlexiDay(User userTakingFlexiDay, FlexiDay flexiDay)
		{
			if (userTakingFlexiDay == null)
				return;

			var manager = userTakingFlexiDay.Manager;

			if (manager == null)
				return;

			var recipients = new List<string>
			{
				manager.Email
			};

			string subject = string.Format($"{userTakingFlexiDay.FullName} has booked a flexiday");
			var body = new StringBuilder();

			body.AppendFormat($"<h2>Good day {manager.FullName},</h2>").AppendLine()
				.AppendFormat($"<p>{userTakingFlexiDay.FullName} has booked a flexiday on {flexiDay.FlexiDate.ToExceptionDisplayString()}<p>").AppendLine()
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void NotifyManagerOfFlexiDayAuthorisation(User userTakingFlexiDay, FlexiDay flexiDay)
		{
			if (userTakingFlexiDay == null)
				return;

			var manager = userTakingFlexiDay.Manager;

			if (manager == null)
				return;

			var recipients = new List<string>
			{
				manager.Email
			};

			string subject = string.Format($"{userTakingFlexiDay.FullName} has booked a flexiday - Authorisation Required");
			var body = new StringBuilder();

			body.AppendFormat($"<h2>Good day {manager.FullName},</h2>").AppendLine()
				.AppendFormat($"<p>{userTakingFlexiDay.FullName} has booked a flexiday on {flexiDay.FlexiDate.ToExceptionDisplayString()}.This exceeds the 2 day limit per month. Please go to the FlexiTime page to authorise this request.<p>").AppendLine()
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void NotifyUserOfBookedFlexiDayByManager(User managerBookingFlexiday, User userBeingBookedFor, FlexiDay flexiDay)
		{
			if (userBeingBookedFor == null || managerBookingFlexiday == null || flexiDay == null)
				return;

			string subject = string.Format($"{managerBookingFlexiday.FullName} has booked your flexiday");
			var body = new StringBuilder();

			var recipients = new List<string>
			{
				userBeingBookedFor.Email
			};

			body.AppendFormat($"<h2>Good day {userBeingBookedFor.FullName},</h2>").AppendLine()
				.AppendFormat($"<p>{managerBookingFlexiday.FullName} has booked a flexiday for you on {flexiDay.FlexiDate.ToExceptionDisplayString()}<p>").AppendLine()
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		#endregion FLEXIDAYS

		#region INVOICE

		public static void NotifyUsersOfInvoiceExceptions(List<string> exceptionList)
		{
			var recipients = new List<string>
			{
				//Notify the Person/s looking after Invoices
				Constants.EMAILADDRESSINVOICES
			};

			string subject = string.Format("Invoices needing attention for Billing.");
			var body = new StringBuilder();

			body.AppendLine("<h1>Good day,</h1>")
				.AppendLine("<p>Please check list of Invoices below that need attention so that Billing can proceed.<p>");

			foreach (string exception in exceptionList)
			{
				body.AppendLine("<br /><br />")
					.AppendLine($"{exception}");
			}

			body.AppendLine("</p>")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		#endregion INVOICE

		#region LEAVE

		#region NOTIFY MANAGER OF LEAVE REQUEST

		public static void NotifyManagerOfLeaveRequest(User userApplyingForLeave, Leave leave)
		{
			if (userApplyingForLeave?.ManagerID.HasValue != true)
				return;

			var manager = userApplyingForLeave.Manager;

			if (manager == null)
				return;

			if (leave.DateFrom.Date == leave.DateTo.Date)
				NotifyManagerOfSingleDayLeave(userApplyingForLeave, manager, leave);
			else
				NotifyManagerOfMultipleDaysLeave(userApplyingForLeave, manager, leave);
		}

		public static void NotifyManagerOfSingleDayLeave(User userApplyingForLeave, User manager, Leave leave)
		{
			string subject = string.Format($"{userApplyingForLeave.FullName} has applied for {((LeaveTypeEnum)leave.LeaveType).GetEnumDescription()} leave");
			var body = new StringBuilder();

			var recipients = new List<string>
			{
				manager.Email
			};

			body.AppendFormat($"<h1>Good day {manager.FullName},</h1>").AppendLine()
				.AppendFormat($"<p>{userApplyingForLeave.FullName} has applied for leave. Details of the leave request:<p>").AppendLine()
				.AppendLine("<ul>")
				.AppendLine("<li>Single Leave Day</li>")
				.AppendFormat($"<li>Leave Type: {((LeaveTypeEnum)leave.LeaveType).GetEnumDescription()}</li>").AppendLine()
				.AppendFormat($"<li>Leave Date: {leave.DateFrom.ToExceptionDisplayString()}</li>").AppendLine()
				.AppendFormat($"<li>Leave Hours: {leave.GetNumberOfHours}</li>").AppendLine()
				.AppendLine("</ul>")
				.AppendLine("<br />")
				.AppendLine("<p>Please make use of the site to approve/deny leave.</p>")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void NotifyManagerOfMultipleDaysLeave(User userApplyingForLeave, User manager, Leave leave)
		{
			string subject = string.Format($"{userApplyingForLeave.FullName} has applied for {((LeaveTypeEnum)leave.LeaveType).ToString()} leave");
			var body = new StringBuilder();

			var recipients = new List<string>
			{
				manager.Email
			};

			body.AppendFormat($"<h1>Good day {manager.FullName},</h1>").AppendLine()
				.AppendFormat($"<p>{userApplyingForLeave.FullName} has applied for leave. Details of the leave request:<p>").AppendLine()
				.AppendLine("<ul>")
				.AppendLine("<li>Multiple Leave Days</li>")
				.AppendFormat($"<li>Leave Type: {((LeaveTypeEnum)leave.LeaveType).GetEnumDescription()}</li>").AppendLine()
				.AppendFormat($"<li>Leave Date From: {leave.DateFrom.ToExceptionDisplayString()}</li>").AppendLine()
				.AppendFormat($"<li>Leave Date To: {leave.DateTo.ToExceptionDisplayString()}</li>").AppendLine()
				.AppendFormat($"<li>Number of days taken as leave: {leave.NumberOfDays}</li>").AppendLine()
				.AppendLine("</ul>")
				.AppendLine("<br />")
				.AppendLine("<p>Please make use of the site to approve/deny leave.</p>")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		#endregion NOTIFY MANAGER OF LEAVE REQUEST

		#region NOTIFY USER OF LEAVE BOOKED BY MANAGER

		public static void NotifyUserOfLeaveBookedByManager(User manager, User userBookedFor, Leave leave)
		{
			if (manager == null || userBookedFor == null || leave == null)
				return;

			if (leave.DateFrom.Date == leave.DateTo.Date)
				NotifyUserOfSingleLeaveDayBookedByManager(manager, userBookedFor, leave);
			else
				NotifyUserOfMultipleDayLeaveBookedByManager(manager, userBookedFor, leave);
		}

		public static void NotifyUserOfSingleLeaveDayBookedByManager(User manager, User userBookedFor, Leave leave)
		{
			string subject = string.Format($"{manager.FullName} has booked your leave");
			var body = new StringBuilder();

			var recipients = new List<string>
			{
				userBookedFor.Email
			};

			body.AppendFormat($"<h1>Good day {userBookedFor.FullName},</h1>").AppendLine()
				.AppendFormat($"<p>{manager.FullName} has booked your {((LeaveTypeEnum)leave.LeaveType).GetEnumDescription()} leave. Details of the leave:<p>").AppendLine()
				.AppendLine("<ul>")
				.AppendLine("<li>Single Leave Day</li>")
				.AppendFormat($"<li>Leave Type: {((LeaveTypeEnum)leave.LeaveType).GetEnumDescription()}</li>").AppendLine()
				.AppendFormat($"<li>Leave Date: {leave.DateFrom.ToExceptionDisplayString()}</li>").AppendLine()
				.AppendFormat($"<li>Leave Hours: {leave.GetNumberOfHours}</li>").AppendLine()
				.AppendLine("</ul>")
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void NotifyUserOfMultipleDayLeaveBookedByManager(User manager, User userBookedFor, Leave leave)
		{
			string subject = string.Format($"{manager.FullName} has booked your {((LeaveTypeEnum)leave.LeaveType).GetEnumDescription()} leave");
			var body = new StringBuilder();

			var recipients = new List<string>
			{
				userBookedFor.Email
			};

			body.AppendFormat($"<h1>Good day {userBookedFor.FullName},</h1>").AppendLine()
				.AppendFormat($"<p>{manager.FullName} has booked your leave. Details of the leave:<p>").AppendLine()
				.AppendLine("<ul>")
				.AppendLine("<li>Multiple Leave Days</li>")
				.AppendFormat($"<li>Leave Type: {((LeaveTypeEnum)leave.LeaveType).GetEnumDescription()}</li>").AppendLine()
				.AppendFormat($"<li>Leave Date From: {leave.DateFrom.ToExceptionDisplayString()}</li>").AppendLine()
				.AppendFormat($"<li>Leave Date To: {leave.DateTo.ToExceptionDisplayString()}</li>").AppendLine()
				.AppendFormat($"<li>Number of days taken as leave: {leave.NumberOfDays}</li>").AppendLine()
				.AppendLine("</ul>")
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		#endregion NOTIFY USER OF LEAVE BOOKED BY MANAGER

		#region NOTIFY USER OF LEAVE APPROVAL

		public static void NotifyUserOfLeaveApproval(User userManaging, User ownerOfLeaveToBeNotified, Leave leave)
		{
			string leaveApprovalStatus = leave.ApprovalStatus == (int)ApprovalStatusType.Approved ? "approved" : "declined";

			string subject = string.Format($"{userManaging.FullName} has {leaveApprovalStatus} your {((LeaveTypeEnum)leave.LeaveType).GetEnumDescription()} leave");
			var body = new StringBuilder();

			var recipients = new List<string>
			{
				ownerOfLeaveToBeNotified.Email
			};

			body.AppendFormat($"<h1>Good day {ownerOfLeaveToBeNotified.FullName},</h1>").AppendLine()
				.AppendFormat($"<p>{userManaging.FullName} has {leaveApprovalStatus} your {((LeaveTypeEnum)leave.LeaveType).GetEnumDescription()} leave. Details of the leave:<p>").AppendLine()
				.AppendLine("<ul>")
				.AppendLine("<li>Multiple Leave Days</li>")
				.AppendFormat($"<li>Leave Type: {((LeaveTypeEnum)leave.LeaveType).GetEnumDescription()}</li>").AppendLine()
				.AppendFormat($"<li>Leave Date From: {leave.DateFrom.ToExceptionDisplayString()}</li>").AppendLine()
				.AppendFormat($"<li>Leave Date To: {leave.DateTo.ToExceptionDisplayString()}</li>").AppendLine()
				.AppendFormat($"<li>Number of days taken as leave: {leave.NumberOfDays}</li>").AppendLine()
				.AppendLine("</ul>")
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void NotifyUserOfUnpaidLeaveApproval(User ownerOfLeave, Leave leave)
		{
			string leaveApprovalStatus = leave.ApprovalStatus == (int)ApprovalStatusType.Approved ? "approved" : "declined";

			string subject = string.Format($"Unpaid Leave Approval Non Hourly Contractor - {ownerOfLeave.FullName}");
			var body = new StringBuilder();

			var hrUsers = new List<User>();
			hrUsers = UserRepo.GetUsersByRole((int)UserRoleType.HR).ToList();

			var recipients = new List<string>();

			foreach (var user in hrUsers)
				recipients.Add(user.Email);

			recipients.Add("payroll@capeyebss.co.za");

			body.AppendFormat("<h1>Good day,</h1>").AppendLine()
				.AppendFormat($"<p>{ownerOfLeave.FullName}'s unpaid leave request has been approved. Details of the leave:<p>").AppendLine()
				.AppendLine("<ul>")
				.AppendFormat($"<li>{((LeaveTypeEnum)leave.LeaveType).GetEnumDescription()}</li>").AppendLine()
				.AppendFormat($"<li>Leave Date From: {leave.DateFrom.ToExceptionDisplayString()}</li>").AppendLine()
				.AppendFormat($"<li>Leave Date To: {leave.DateTo.ToExceptionDisplayString()}</li>").AppendLine()
				.AppendFormat($"<li>Number of days taken as leave: {leave.NumberOfDays}</li>").AppendLine()
				.AppendLine("</ul>")
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");
			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		#endregion NOTIFY USER OF LEAVE APPROVAL

		#region NOTIFY USER OF FLEXI DAY APPROVAL

		public static void NotifyUserOfFlexiDayApproval(User userManaging, User ownerOfFlexiDayToBeNotified, FlexiDay flexiDay)
		{
			string flexiDayApprovalStatus = flexiDay.ApprovalStatusID == (int)ApprovalStatusType.Approved ? "approved" : "declined";

			string subject = string.Format($"{userManaging.FullName} has {flexiDayApprovalStatus} your flexiday");
			var body = new StringBuilder();

			var recipients = new List<string>
			{
				ownerOfFlexiDayToBeNotified.Email
			};

			body.AppendFormat($"<h1>Good day {ownerOfFlexiDayToBeNotified.FullName},</h1>").AppendLine()
				.AppendFormat($"<p>{userManaging.FullName} has {flexiDayApprovalStatus} your flexiday. Details of the flexiday:<p>").AppendLine()
				.AppendLine("<ul>")
				.AppendLine("<li>Additional Flexi Day</li>")
				.AppendFormat($"<li>Flexi Date: {flexiDay.FlexiDate.ToExceptionDisplayString()}</li>").AppendLine()
				.AppendFormat($"<li>Remark: {flexiDay.Remark}</li>").AppendLine()
				.AppendLine("</ul>")
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		#endregion NOTIFY USER OF FLEXI DAY APPROVAL

		#region LEAVE CYCLE LIMIT EXCEEDED

		public static void NotifyManagerLeaveBalanceLimitExceeded(User userApplyingForLeave, Leave leave, List<LeaveCycleBalance> leaveCycleBalanceList)
		{
			var manager = userApplyingForLeave.Manager;
			string subject = string.Format($"{userApplyingForLeave.FullName} has applied for {((LeaveTypeEnum)leave.LeaveType).GetEnumDescription()} leave -  Limit Exceeded");
			var body = new StringBuilder();

			var recipients = new List<string>
			{
				manager.Email
			};

			body.AppendFormat($"<h1>Good day {manager.FullName},</h1>").AppendLine()
				.AppendFormat($"<p>{userApplyingForLeave.FullName} has applied for leave. Details of the leave request:<p>").AppendLine()
				.AppendFormat($"<li>Type: {((LeaveTypeEnum)leave.LeaveType).GetEnumDescription()}").AppendLine(" leave </li>")
				.AppendFormat($"<li>Period: {leave.DateFrom.ToExceptionDisplayString().Replace("-", " ")} to {leave.DateTo.ToExceptionDisplayString().Replace("-", " ")} </li>").AppendLine()
				.AppendFormat($"<li>Number of days: {leave.NumberOfDays}</li>").AppendLine()
				.AppendLine("<p>Approving the above leave will result in a negative closing balance for one or more leave cycles. Please refer to the table below for more details:</p>")
				.AppendLine("<table border='1' cellpadding='0' cellspacing='0' >")
				.AppendLine("<tr style='background-color:#eeeeee'>")
				.AppendLine("<th align='center'>")
				.AppendLine("Date From")
				.AppendLine("</th>")
				.AppendLine("<th align='center'>")
				.AppendLine("Date To")
				.AppendLine("</th>")
				.AppendLine("<th align='center'>")
				.AppendLine("Closing Balance <br></br> Before")
				.AppendLine("</th>")
				.AppendLine("<th align='center'>")
				.AppendLine("Nr of Days <br></br> applying for")
				.AppendLine("</th>")
				.AppendLine("<th align='center'>")
				.AppendLine("Closing Balance <br></br>  After")
				.AppendLine("</th>")
				.AppendLine("<tr>");

			foreach (var lcb in leaveCycleBalanceList)
			{
				body.AppendLine("<tr>")
					.AppendLine("<td align='center'>")
					.AppendLine(lcb.StartDate.ToExceptionDisplayString())
					.AppendLine("</td>")
					.AppendLine("<td align='center'>")
					.AppendLine(lcb.EndDate.ToExceptionDisplayString())
					.AppendLine("</td>")
					.AppendLine("<td align='center'>")
					.AppendLine(lcb.ClosingBalance.ToString())
					.AppendLine("</td>")
					.AppendLine("<td align='center'>")
					.AppendLine(lcb.Taken.ToString())
					.AppendLine("</td>")
					.AppendLine("<td align='center'>")
					.AppendLine((lcb.ClosingBalance - lcb.Taken).ToString())
					.AppendLine("</td>")
					.AppendLine("<tr>");
			}

			body.AppendLine("</table>")
				.AppendLine("<p>Please make use of the site to approve/deny leave.</p>")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		#endregion LEAVE CYCLE LIMIT EXCEEDED

		#endregion LEAVE

		#region EXPENSE

		#region NOTIFY MANAGER OF EXPENSE

		public static void NotifyManagerOfExpense(User userSubmittingExpense)
		{
			var managerOfExpenseToBeNotified = userSubmittingExpense.Manager;
			const string subject = "Expense submitted";
			var body = new StringBuilder();

			var recipients = new List<string>
			{
				managerOfExpenseToBeNotified.Email
			};

			body.AppendFormat($"<h1>Good day {managerOfExpenseToBeNotified.FullName},</h1>").AppendLine()
				.AppendLine("<br />")
				.AppendFormat($"{userSubmittingExpense.FullName} has submitted an expense for approval.").AppendLine()
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		#endregion NOTIFY MANAGER OF EXPENSE

		#endregion EXPENSE

		#region PROJECT OWNER

		public static void NotifyProjectOwnerOfNewProjectAssignedToHimHer(Project project)
		{
			if (project == null)
				return;

			var projectOwner = UserRepo.GetByID(project.ProjectOwnerID);

			var recipients = new List<string>
			{
				projectOwner.Email
			};

			const string subject = "New project";
			var body = new StringBuilder();

			body.AppendFormat($"<h2>Good day {projectOwner.FullName},</h2>").AppendLine()
				.AppendFormat("<p>You have been allocated to the following project:<p>", project.ProjectName).AppendLine()
				.AppendLine("<ul>")
				.AppendFormat($"<li>Client: {project.GetClient().ClientName}</li>").AppendLine()
				.AppendFormat($"<li>Project: {project.ProjectName}</li>").AppendLine()
				.AppendLine("</ul>")
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		#endregion PROJECT OWNER

		#region FEEDBACK

		public static void NotifyFeedbackAdminOfNewThread(User newThreadUser, FeedbackThread thread)
		{
			if (newThreadUser == null)
				return;

			var recipients = new List<string>();
			User.GetByRoleBW((int)UserRoleType.FeedbackAdmin).ForEach(u => recipients.Add(u.Email));

			string subject = string.Format($"{newThreadUser.FullName} has started a feedback thread");
			var body = new StringBuilder();

			body.AppendLine("<h2>Good day,</h2>")
				.AppendFormat($"<p>{newThreadUser.FullName} has logged a new feedback:<p>").AppendLine()
				.AppendLine("<ul>")
				.AppendFormat($"<li>Subject: {thread.FeedbackSubject}</li>").AppendLine()
				.AppendFormat($"<li>Feedback Type: {thread.FeedBackTypeName}</li>").AppendLine()
				.AppendFormat($"<li>Urgency: {thread.FeedBackUrgencyTypeName}</li>").AppendLine()
				.AppendLine("<br />")
				.AppendFormat($"<li>Post: {thread.GetInitialFeedbackPost().PostText}</li>").AppendLine()
				.AppendLine("</ul>")
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void NotifyUsersOfClosedThread(User userClosed, FeedbackThread thread)
		{
			if (userClosed == null || thread == null)
				return;

			var recipients = new List<string>();
			thread.GetThreadUsers().ForEach(u => recipients.Add(u.Email));

			string subject = string.Format($"{userClosed.FullName} has closed a feedback thread");
			var body = new StringBuilder();

			body.AppendLine("<h2>Good day,</h2>")
				.AppendFormat($"<p>{userClosed.FullName} has closed a feedback thread: {thread.FeedbackSubject}<p>").AppendLine()
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		#endregion FEEDBACK

		#region TICKET

		public static void NotifyUserofNewTicket(User newTicketUser, Ticket ticket)
		{
			if (newTicketUser == null)
				return;

			//Only notify the user to whom the ticket was assigned
			var recipients = new List<string>
			{
				newTicketUser.Email
			};

			string subject = string.Format($"|{ticket.TicketReference}| {ticket.TicketSubject}");
			var body = new StringBuilder();

			body.AppendLine("<p style='font-family:calibri;font-size:10;'>Good day,")
				.AppendLine("<br /><br />")
				.AppendFormat($"The following ticket has been logged by {UserRepo.GetByID(ticket.UserCreated).FullName}, via Enabill:").AppendLine()
				.AppendLine("<br /><br />")
				.AppendFormat($"<b>Client:</b>{ProjectRepo.GetByID(ticket.ProjectID).GetClient().ClientName}").AppendLine()
				.AppendLine("<br />")
				.AppendFormat($"<b>Project:</b>{ProjectRepo.GetByID(ticket.ProjectID).ProjectName}").AppendLine()
				.AppendLine("<br />")
				.AppendFormat($"<b>Subject:</b>{ticket.TicketSubject}").AppendLine()
				.AppendLine("<br />")
				.AppendFormat($"<b>Date Created:</b>{ticket.DateCreated}").AppendLine()
				.AppendLine("<br />")
				.AppendFormat($"<b>From Address:</b>{ticket.FromAddress}").AppendLine()
				.AppendLine("<br />")
				.AppendLine("<br />")
				.AppendLine("<b>Notes</b>")
				.AppendLine("<br />")
				.AppendLine(TicketLineRepo.GetFirstTicketLine(ticket.TicketID).Body)
				.AppendLine("<br />")
				.AppendLine("</p>")
				.AppendLine("<p style='font-family:calibri;font-size:9;'><i>This is an automated response from Enabill. Please do not reply to this email.</i></p>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void NotifyUserofNewTicketLine(User newTicketLineUser, Ticket ticket, string postText)
		{
			if (newTicketLineUser == null)
				return;

			//Only notify support group if any post logged via enabill
			var recipients = new List<string>
			{
				newTicketLineUser.Email
			};

			string subject = string.Format($"RE: |{ticket.TicketReference}| {ticket.TicketSubject}");
			var body = new StringBuilder();

			body.AppendLine("<p style='font-family:calibri;font-size:10;'>Good day,")
				.AppendLine("<br />")
				.AppendLine("<br />")
				.AppendFormat($"The following reply has been posted by {UserRepo.GetByID(ticket.UserModified.Value).FullName} :").AppendLine()
				.AppendLine("<br />")
				.AppendLine("<br />")
				.AppendFormat($"<b>Client:</b>{ProjectRepo.GetByID(ticket.ProjectID).GetClient().ClientName}").AppendLine()
				.AppendLine("<br />")
				.AppendFormat($"<b>Project:</b>{ProjectRepo.GetByID(ticket.ProjectID).ProjectName}").AppendLine()
				.AppendLine("<br />")
				.AppendFormat($"<b>Subject:</b>{ticket.TicketSubject}").AppendLine()
				.AppendLine("<br />")
				.AppendFormat($"<b>Date Created:</b>{ticket.DateCreated}").AppendLine()
				.AppendLine("<br />")
				.AppendFormat($"<b>From Address:</b>{ticket.FromAddress}").AppendLine()
				.AppendLine("<br />")
				.AppendLine("<br />")
				.AppendLine("<b><u>Notes</u></b>")
				.AppendLine("</p>")
				.Append("<p style='font-family:calibri;font-size:10;'>").Append(postText).AppendLine("</p>")
				.AppendLine("<br />")
				.AppendLine("<p style='font-family:calibri;font-size:9;'><i>This is an automated response from Enabill. Please do not reply to this email.</i></p>");

			//if (typeOfPost != "")
			//    body.AppendLine(string.Format($"{typeOfPost}"));
			//body.AppendLine("<br />");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void NotifyUsersOfClosedTicket(User userClosed, Ticket ticket)
		{
			if (userClosed == null || ticket == null)
				return;

			var recipients = new List<string>
			{
				//Notify the Client
				ticket.FromAddress
			};

			//Add TicketManagers to email notification
			UserRepo.GetUsersByRole((int)UserRoleType.TicketManager).ToList().ForEach(u => recipients.Add(u.Email));

			//Add User that was involved\assigned to the ticket
			UserRepo.GetAssignedUsersByTicketID(ticket.TicketID).ToList().ForEach(u => recipients.Add(u.Email));

			string subject = string.Format($"|{ticket.TicketReference}| {ticket.TicketSubject} has been closed.");
			var body = new StringBuilder();

			body.AppendLine("<p style='font-family:calibri;font-size:10;'>Good day,")
				.AppendLine("<br />")
				.AppendLine("<br />")
				.AppendFormat($"{userClosed.FullName} has closed ticket <b>{ticket.TicketReference}.</b>").AppendLine()
				.AppendLine("<br />")
				.AppendLine("<br />")
				.AppendLine("<b><u>Notes</u></b><br />")
				.AppendLine(TicketLineRepo.GetLastTicketLine(ticket.TicketID).Body)
				.AppendLine("<br />")
				.AppendLine("</p>")
				.Append("<p style='font-family:calibri;font-size:9;'><i>This is an automated message from the ").Append(Code.Constants.COMPANYNAME).AppendLine(" Issue Tracker. Please do not reply to this message.</i></p>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void SendClosedTicketManualEmail(string emailAddresses, Ticket ticket)
		{
			var user = UserRepo.GetByID(ticket.UserModified.Value);

			var recipients = new List<string>();

			foreach (string email in emailAddresses.Split(','))
			{
				recipients.Add(email);
			}

			string subject = string.Format($"|{ticket.TicketReference}| {ticket.TicketSubject} has been closed.");
			var body = new StringBuilder();

			body.AppendLine("<p style='font-family:calibri;font-size:10;'>Good day,")
				.AppendLine("<br />")
				.AppendLine("<br />")
				.AppendFormat($"{user.FullName} has closed ticket <b>{ticket.TicketReference}.</b>").AppendLine()
				.AppendLine("<br />")
				.AppendLine("<br />")
				.AppendLine("<b><u>Notes</u></b><br />")
				.AppendLine(TicketLineRepo.GetLastTicketLine(ticket.TicketID).Body)
				.AppendLine("<br />")
				.AppendLine("</p>")
				.Append("<p style='font-family:calibri;font-size:9;'><i>This is an automated message from the ").Append(Code.Constants.COMPANYNAME).AppendLine(" Issue Tracker. Please do not reply to this message.</i></p>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void NotifyUserofDeletedTicket(User user, string ticketReferenceList)
		{
			if (user == null)
				return;

			//Only notify the user to whom the ticket was assigned
			var recipients = new List<string>();
			UserRepo.GetUsersByRole((int)UserRoleType.TicketManager).ToList().ForEach(u => recipients.Add(u.Email));

			string subject = string.Format($"{ticketReferenceList} Deleted");
			var body = new StringBuilder();

			body.AppendLine("<p style='font-family:calibri;font-size:10;'>Good day,")
				.AppendLine("<br /><br />")
				.AppendFormat($"Kindly note that ticket(s) <b>{ticketReferenceList}</b>, was deleted by {user.FullName}.").AppendLine()
				.AppendLine("<br /><br />")
				.AppendLine("</p>")
				.AppendLine("<p style='font-family:calibri;font-size:9;'><i>This is an automated response from Enabill. Please do not reply to this email.</i></p>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void NotifyUserofReactivatedTicket(User user, string ticketReactivateList)
		{
			if (user == null)
				return;

			//Only notify the user to whom the ticket was assigned
			var recipients = new List<string>();
			UserRepo.GetUsersByRole((int)UserRoleType.TicketManager).ToList().ForEach(u => recipients.Add(u.Email));

			string subject = string.Format($"{ticketReactivateList} Reactivated");
			var body = new StringBuilder();

			body.AppendLine("<p style='font-family:calibri;font-size:10;'>Good day,")
				.AppendLine("<br /><br />")
				.AppendFormat($"Kindly note that ticket(s) <b>{ticketReactivateList}</b>, was reactivated by {user.FullName}.").AppendLine()
				.AppendLine("<br /><br />")
				.AppendLine("</p>")
				.AppendLine("<p style='font-family:calibri;font-size:9;'><i>This is an automated response from Enabill. Please do not reply to this email.</i></p>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void SendNewTicketManualEmail(string emailAddresses, Ticket ticket)
		{
			//Only notify the user to whom the ticket was assigned
			var recipients = new List<string>();

			foreach (string email in emailAddresses.Split(','))
			{
				recipients.Add(email);
			}

			string subject = string.Format($"|{ticket.TicketReference}| {ticket.TicketSubject}");
			var body = new StringBuilder();

			body.AppendLine("<div>")
				.AppendLine("<p style='font-family:calibri;font-size:12;'> Good day, <br /> <br />")
				.AppendFormat($"Your query has been logged. Your ticket reference is <b>{ticket.TicketReference}</b>. <br /> <br />").AppendLine()
				.AppendLine("<b><u>Details</u></b> <br />")
				.AppendFormat($"<b>Date received:</b>{ticket.DateCreated}<br />").AppendLine()
				.AppendFormat($"<b>Email Subject:</b>{ticket.TicketSubject}").AppendLine()
				.AppendLine("<br /> <br />")
				.AppendLine("Our team will be responding to your query as soon as possible.</p> ")
				.AppendLine("<p style='font-family:calibri;font-size:12;'>")
				.AppendLine("Regards<br />")
				.Append(Code.Constants.COMPANYNAME).AppendLine(" Support</p>")
				.Append(" <p style='font-family:calibri;font-size:12;'><i>This is an automated message from the ").Append(Code.Constants.COMPANYNAME).AppendLine(" Issue Tracker. Please do not reply to this message.</i></p>")
				.AppendLine("</div>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void SendFeedbackToTicket(string emailfrom, string emailAddresses, string user, string feedbacksubject, string posttext)
		{
			var recipients = new List<string>();

			foreach (string email in emailAddresses.Split(','))
			{
				recipients.Add(email);
			}

			string subject = feedbacksubject;
			var body = new StringBuilder();

			body.AppendLine("<div>")
				.AppendLine("Good day Enabill Team,<br /><br />")
				.Append("Feedback Thread Subject: ").AppendLine(subject)
				.AppendLine(posttext)
				.AppendLine("  ")
				.AppendLine("Regards<br />")
				.AppendLine(user)
				.AppendLine(" <p style='font-family:calibri;font-size:12;'><i>This is an automated message from the Enabill Feedback Section.</i></p>")
				.AppendLine("</div>");

			var model = new EnabillEmailModel(emailfrom, emailfrom, subject, recipients, body.ToString());

			SendEmail(model);
		}

		#endregion TICKET

		#region ACCOUNT

		public static void NewAccountEmail(User user)
		{
			if (user == null)
				return;

			if (!user.IsActive)
				SendNotificationThatAccountIsInactiveFromPasswordRecoverProblems(user);

			if (!user.ForgottenPasswordToken.HasValue)
				return;

			string siteAddress = Code.Constants.SITEADDRESS;

			if (string.IsNullOrEmpty(siteAddress))
				return;

			string url = siteAddress + "/Account/RecoverPassword/";
			string validationCode = user.ForgottenPasswordToken.Value.ToString();

			var recipients = new List<string>
			{
				user.Email,
				Constants.EMAILADDRESSSUPPORT,
				Constants.EMAILADDRESSHR
			};

			const string subject = "Enabill - Temporary Password";

			var body = new StringBuilder();

			body.AppendFormat($"<h2>Good day {user.FullName},</h2>").AppendLine()
				.AppendFormat($"<p>Welcome to Enabill.").AppendLine()
				.AppendLine("<br />")
				.AppendFormat($"<p>Note that you will only be able to log in from your start date: {user.EmployStartDate.ToShortDateString()}").AppendLine()
				.AppendLine("<br />")
				.AppendFormat($"<p>This link <a href=\"http://{url}{validationCode}\" >Please click to change password</a> will give you access to Enabill and you will be prompted to change your password.</p>").AppendLine()
				.AppendLine("<p>When setting your password, please follow the change password guidelines.")
				.AppendLine("<br />")
				.AppendFormat($"<p>After the initial login using the link above, for future access to Enabill please use this link: {siteAddress}").AppendLine()
				//.AppendFormat($"<p>{siteAddress}</p>").AppendLine()
				.AppendLine("<br />")
				.AppendLine("<p>Your login details are:")
				.AppendLine("<br />")
				.AppendFormat($"<p>Username: {user.Email}").AppendLine()
				.AppendLine("<br />")
				.AppendLine("Password: As changed above.")
				.AppendLine("<br />")
				.Append("<p>Should you encounter any difficulty logging in to Enabill please send an email to ").Append(Code.Constants.EMAILADDRESSSUPPORT).AppendLine(".");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void SendRecoverPasswordEmail(User user)
		{
			if (user == null)
				return;

			if (!user.IsActive)
				SendNotificationThatAccountIsInactiveFromPasswordRecoverProblems(user);

			if (!user.ForgottenPasswordToken.HasValue)
				return;

			string siteAddress = Code.Constants.SITEADDRESS;

			if (string.IsNullOrEmpty(siteAddress))
				return;

			string url = siteAddress + "/Account/RecoverPassword/";
			string validationCode = user.ForgottenPasswordToken.Value.ToString();

			var recipients = new List<string>
			{
				user.Email
			};

			const string subject = "Enabill - Recover Password";

			var body = new StringBuilder();

			body.AppendFormat($"<h2>Good day {user.FullName},</h2>").AppendLine()
				.AppendLine("<p>A request was received to recover your password. If you did not request this, then please ignore this email, as your password will not be changed, removed, or given out.")
				.AppendLine("<br />")
				.AppendFormat($"<p>If you did request this email, please click the following link: <a href=\"http://{url}{validationCode}\" >Recover Password</a></p>").AppendLine()
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		public static void SendNotificationThatAccountIsInactiveFromPasswordRecoverProblems(User user)
		{
			if (user?.IsActive != false)
				return;

			var recipients = new List<string>
			{
				user.Email
			};

			const string subject = "Account Deactivated";

			var body = new StringBuilder();

			body.AppendFormat($"<h2>Good day {user.FullName},</h2>").AppendLine()
				.AppendLine("<p>A request was received to recover your password.")
				.AppendLine("<br />")
				.AppendLine("<p>Your password into Enabill cannot be given to you as this account has been deactivated. You do not have access into the site.</p>")
				.AppendLine("<p>If this is a problem, please contact {} or a system administrator to rectify the problem.</p>")
				.AppendLine("<br />")
				.AppendLine("<h4>From Enabill Automated Services</h4>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		#endregion ACCOUNT

		#region REPORTS

		public static void EmailReport(DateTime dateFrom, DateTime dateTo, List<string> recipients, string reportName, string reportPath)
		{
			if (recipients.Count > 0)
			{
				string subject = reportName + " from " + dateFrom.ToLongDateString() + " to " + dateTo.ToLongDateString();
				var body = new StringBuilder();

				body.AppendFormat("<h1>Good day All,</h1>").AppendLine()
					.AppendFormat("<p>Attached please find the " + subject + ".<p>").AppendLine()
					.AppendLine("<br />")
					.AppendLine("<h4>From Enabill Automated Services</h4>");

				string[] attachments = { reportPath };
				var model = new EnabillEmailModel(subject, recipients, body.ToString(), attachments);
				SendEmail(model);
			}
		}

		#endregion REPORTS

		#region EMAILS

		public static void RunLeaveRequestsPendingEmail(DateTime dateFrom, LeaveRequestsPendingModel leaveRequestsPendingModel)
		{
			foreach (var m in leaveRequestsPendingModel.Leaves.GroupBy(m => m.Manager))
			{
				var managerMessage = new StringBuilder();
				string managerMessageSubject = "Leave Requests Pending from " + dateFrom.ToLongDateString();

				managerMessage.AppendFormat($"<h1>Good day {m.Key.FirstName} {m.Key.LastName},</h1>")
					.Append("<p>The following users have unapproved leave outstanding:</p>");

				foreach (var u in m.GroupBy(u => u.User))
				{
					managerMessage.AppendFormat($"<p>{u.Key.FullName} has applied for leave. Details of the leave request:<p>").AppendLine();

					foreach (var l in u.GroupBy(ll => ll.Leave))
					{
						managerMessage.AppendLine("<ul>")
							.AppendFormat($"<li>Leave Type: {((LeaveTypeEnum)l.Key.LeaveType).GetEnumDescription()}</li>").AppendLine()
							.AppendFormat($"<li>Leave Date From: {l.Key.DateFrom.ToExceptionDisplayString()}</li>").AppendLine()
							.AppendFormat($"<li>Leave Date To: {l.Key.DateTo.ToExceptionDisplayString()}</li>").AppendLine()
							.AppendFormat($"<li>Number of days taken as leave: {l.Key.NumberOfDays}</li>").AppendLine()
							.AppendLine("</ul>");
					}
				}

				managerMessage.AppendLine("<br />")
					.AppendLine("<h4>From Enabill Automated Services</h4>");
				var model = new EnabillEmailModel(managerMessageSubject, new List<string> { m.Key.Email }, managerMessage.ToString(), null);

				SendEmail(model);
			}
		}

		#endregion EMAILS

		#region SCHEDULED JOBS

		public static void NotifyMonthEndJobComplete()
		{
			//Notify all the relevant people that Leave and Flexi Balance process has completed
			var recipients = new List<string>();
			User.GetByRoleBW((int)UserRoleType.Accountant).ForEach(u => recipients.Add(u.Email));
			User.GetByRoleBW((int)UserRoleType.HR).ForEach(u => recipients.Add(u.Email));
			//Enabill.Models.User.GetByRoleBW((int)UserRoleType.InvoiceAdministrator).ForEach(u => recipients.Add(u.Email));
			//Enabill.Models.User.GetByRoleBW((int)UserRoleType.SystemAdministrator).ForEach(u => recipients.Add(u.Email));

			const string subject = "Enabill MonthEnd Updates Complete";
			var body = new StringBuilder();

			body.AppendLine("<p style='font-family:calibri;font-size:10;'>Good day,")
				.AppendLine("<br /><br />")
				.AppendFormat($"Kindly note that the following monthend update processes, for the period {DateTime.Now.ToPeriod().ToString()},  have completed successfully: ").AppendLine()
				.AppendLine("- Leave and Flexi Balances<br />")
				.AppendLine("- User Cost to Company<br /><br/>")
				.AppendLine("You can access the above updated information via Enabill.<br /><br />")
				.AppendLine("</p>")
				.AppendLine("<p style='font-family:calibri;font-size:9;'><i>This is an automated response from Enabill. Please do not reply to this email.</i></p>");

			var model = new EnabillEmailModel(subject, recipients, body.ToString());

			SendEmail(model);
		}

		#endregion SCHEDULED JOBS
	}

	public class EnabillEmailModel
	{
		#region PROPERTIES

		public string From { get; private set; }
		public string ReplyTo { get; private set; }
		public string Subject { get; private set; }
		public List<string> Recipients { get; private set; }
		public string Body { get; private set; }
		public string[] Attachments { get; private set; }

		#endregion PROPERTIES

		#region FUNCTIONS

		public EnabillEmailModel(string subject, List<string> recipients, string body, string[] attachments = null)
		{
			this.From = null;
			this.ReplyTo = null;
			this.Subject = subject;
			this.Recipients = recipients;
			this.Body = body;
			this.Attachments = attachments;
		}

		public EnabillEmailModel(string from, string replyTo, string subject, List<string> recipients, string body, string[] attachments = null)
		{
			this.From = from;
			this.ReplyTo = replyTo;
			this.Subject = subject;
			this.Recipients = recipients;
			this.Body = body;
			this.Attachments = attachments;
		}

		#endregion FUNCTIONS
	}
}