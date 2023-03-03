using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Amazon;
using Amazon.SimpleEmail.Model;

namespace Enabill.Comms
{
	public static class Mailer
	{
		/// <summary>
		/// AppSettings["SendEmails"] == 0 to prevent mails from being sent.
		/// </summary>
		public static bool SendEmails
		{
			get
			{
				string s = Code.Constants.EMAILSERVERSEND;

				return (s ?? "1") == "1";
			}
		}

		/// <summary>
		/// Sends the specified message according to the method set at ConfigurationManager.AppSettings["MailingMethod"]
		/// </summary>
		/// <param name="from">The from address for this e-mail message. Defaults to Enabill.Code.Constants.EMAILADDRESSDEFAULTFROM if empty.</param>
		/// <param name="replyTo">The ReplyTo addresses for the mail message. Defaults to Enabill.Code.Constants.EMAILADDRESSDEFAULTREPLYTO if empty.</param>
		/// <param name="to">The recipients of this e-mail message.</param>
		/// <param name="subject">The subject line for this e-mail message.</param>
		/// <param name="body">The HTML message body for this e-mail message.</param>
		/// <param name="attachments">The attachments for this e-mail message.</param>
		public static bool Send(string from,
								string replyTo,
								List<string> to,
								string subject,
								string body,
								params string[] attachments) => Send(MailingMethod.UseConfig, from, replyTo, to, subject, body, attachments);

		/// <summary>
		/// Sends the specified message acorrding to the MailingMethod parmater.
		/// </summary>
		/// <param name="mailingMethod"></param>
		/// <param name="from">The from address for this e-mail message. Defaults to Enabill.Code.Constants.EMAILADDRESSDEFAULTFROM if empty.</param>
		/// <param name="replyTo">The ReplyTo addresses for the mail message. Defaults to Enabill.Code.Constants.EMAILADDRESSDEFAULTREPLYTO if empty.</param>
		/// <param name="to">The recipients of this e-mail message.</param>
		/// <param name="subject">The subject line for this e-mail message.</param>
		/// <param name="body">The HTML message body for this e-mail message.</param>
		/// <param name="attachments">The attachments for this e-mail message.</param>
		public static bool Send(MailingMethod mailingMethod,
								string from,
								string replyTo,
								List<string> to,
								string subject,
								string body,
								params string[] attachments)
		{
			// check if mailing is enable for the system.
			if (!SendEmails)
				return false;

			// build the mail message
			var message = new MailMessage();

			// set the from Address
			if (string.IsNullOrEmpty(from))
				from = Code.Constants.EMAILADDRESSDEFAULTFROM;

			message.From = new MailAddress(from);

			// set the replyTo Addresses
			if (string.IsNullOrEmpty(replyTo))
				replyTo = Code.Constants.EMAILADDRESSDEFAULTREPLYTO;

			message.ReplyToList.Add(new MailAddress(replyTo));

			// set the To Addresses
			foreach (string addr in to)
			{
				Console.WriteLine("Adding to recipients:" + addr);
				message.To.Add(OverrideEmailTo(addr));
			}

			message.Subject = subject;
			message.IsBodyHtml = true;
			message.Body = body;

			if (attachments?.Count() > 0)
			{
				foreach (string item in attachments)
					message.Attachments.Add(new Attachment(item));
			}

			return DeliverMail(mailingMethod, message);
		}

		private static bool DeliverMail(MailingMethod mailingMethod, MailMessage message)
		{
			// Direct send
			if ((mailingMethod == MailingMethod.Direct)
				|| string.IsNullOrEmpty(Code.Constants.EMAILSERVERMAILINGMETHOD)
				|| (mailingMethod == MailingMethod.UseConfig & Code.Constants.EMAILSERVERMAILINGMETHOD == "Direct"))
			{
				var credentials = new NetworkCredential
				(
					((System.Collections.Specialized.NameValueCollection)ConfigurationManager.GetSection("sendGrid"))["SendGridUserName"],
					((System.Collections.Specialized.NameValueCollection)ConfigurationManager.GetSection("sendGrid"))["SendGridPassword"]
				);

				// Create a Web transport for sending email.

				var client = new SmtpClient(Code.Constants.EMAILSERVERSMTP, Convert.ToInt32(2525))
				{
					//SmtpClient client = new SmtpClient();
					Credentials = credentials
				};

				string host = Code.Constants.EMAILSERVERSMTP;

				if (string.IsNullOrEmpty(host))
					host = "localhost";

				client.Host = host;

				try
				{
					client.Send(message);

					return true;
				}
				catch (SmtpException ex)
				{
					if (ex.InnerException != null)
					{
						Console.WriteLine(ex.InnerException);
					}

					throw new EnabillCommunicationException("Could not deliver mail. " + ex.Message, ex);
				}
			}

			// AWS SES Queue
			if ((mailingMethod == MailingMethod.Queue)
				|| (mailingMethod == MailingMethod.UseConfig && Code.Constants.EMAILSERVERMAILINGMETHOD == "Queue"))
			{
				try
				{
					var ses = AWSClientFactory.CreateAmazonSimpleEmailServiceClient(ConfigurationManager.AppSettings["AWSAccessKeyId"], ConfigurationManager.AppSettings["AWSSecretKey"]);
					var request = new SendEmailRequest
					{
						// set the FROM address
						Source = message.From.Address
					};

					// add the TO addessses
					var des = new Destination();

					foreach (var t in message.To)
						des.ToAddresses.Add(t.Address);

					request.Destination = des;

					// set the REPLY-TO address
					foreach (var t in message.ReplyToList)
						request.ReplyToAddresses.Add(t.Address);

					// add the MESSAGE
					var msg = new Message
					{
						Subject = new Content(message.Subject)
					};

					var msgBody = new Body();
					msgBody.WithHtml(new Content(message.Body));
					msg.Body = msgBody;

					// send the message
					request.Message = msg;
					ses.SendEmail(request);

					return true;
				}
				catch (Exception ex)
				{
					//This is where we will log any emails that were not sent, for queuing and sending at a later stage.
					throw new EnabillCommunicationException("Could not deliver mail. " + ex.Message);
				}
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Check if outgoing email must be send to an override. Used for testing to prevent confusion
		/// </summary>
		/// <param name="originalEmailTo"></param>
		private static string OverrideEmailTo(string originalEmailTo)
		{
			string s = Code.Constants.EMAILADDRESSOVERRIDETO;

			if (string.IsNullOrEmpty(s))
				return originalEmailTo;
			else
				return s;
		}
	}
}