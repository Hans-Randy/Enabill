using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Enabill.Models;
using Enabill.Models.Dto;
using Enabill.Repos;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Enabill.Print
{
	internal class InvoiceLine
	{
		public string ItemCode { get; set; }
		public string Description { get; set; }
		public double Hours { get; set; }
		public double Rate { get; set; }
		public double Amount { get; set; }
		public double Credits { get; set; }

		public List<string> Items { get; set; }

		public InvoiceLine()
		{
			this.Items = new List<string>();
		}
	}

	public class PrintInvoicePDF
	{
		#region PROPERTIES

		private Invoice invoice;
		private Font arialHeading;
		private Font arialBold;
		private Font arial;
		private Font arialSmall;
		private PdfPCell emptyNoBorderCell;
		private PdfPCell emptyTopBorderCell;
		private PdfPCell emptyBottomBorderCell;
		private List<ClientDepartmentCode> clientDepartmentCodeID;
		private List<GLAccount> glAccountID;
		private string clientDepartmentCode = string.Empty;
		private string gLAccountCode = string.Empty;

		#endregion PROPERTIES

		public PrintInvoicePDF(Invoice invoice)
		{
			this.invoice = invoice;
		}

		#region PRINT METHODS

		public byte[] Print(User userRequesting)
		{
			if (this.invoice == null)
				return null;

			string filename = Path.GetTempFileName();
			var fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);

			var invoiceDoc = new Document(PageSize.A4, 10, 10, 10, 10);
			var baseFont = BaseFont.CreateFont("c:/Windows/Fonts/Arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

			this.arialHeading = new Font(baseFont, 12, Font.BOLD);
			this.arialBold = new Font(baseFont, 10, Font.BOLD);
			this.arial = new Font(baseFont, 10, Font.NORMAL);
			this.arialSmall = new Font(baseFont, 8, Font.NORMAL);

			this.emptyNoBorderCell = new PdfPCell
			{
				Border = PdfPCell.NO_BORDER
			};

			this.emptyTopBorderCell = new PdfPCell
			{
				Border = PdfPCell.TOP_BORDER
			};

			this.emptyBottomBorderCell = new PdfPCell
			{
				Border = PdfPCell.BOTTOM_BORDER
			};

			var writer = PdfWriter.GetInstance(invoiceDoc, fs);
			invoiceDoc.Open();
			var content = writer.DirectContent;

			invoiceDoc.Add(this.GetAddressHeader());
			invoiceDoc.Add(this.GetInvoiceHeader());
			invoiceDoc.Add(this.GetDescription());
			invoiceDoc.Add(this.GetInvoiceLineDetails());
			invoiceDoc.Add(this.GetTotalLine());
			invoiceDoc.Add(this.GetFooter());
			invoiceDoc.NewPage();

			if (this.invoice.PrintTimeSheet)
				invoiceDoc.Add(this.GetTimesheet());

			//finally closet the document
			invoiceDoc.Close();

			fs = new FileStream(filename, FileMode.Open);
			byte[] report = new byte[fs.Length];
			fs.Position = 0;
			fs.Read(report, 0, (int)fs.Length);
			fs.Close();

			return report;
		}

		public byte[] PrintTimeSheet(User userRequesting)
		{
			if (this.invoice == null)
				return null;

			string filename = Path.GetTempFileName();
			var fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);

			var invoiceDoc = new Document(PageSize.A4.Rotate()); //x lower left, y lower left, x height, y width
			invoiceDoc.SetMargins(-80f, -80f, 18f, 18f); // left, right, top, bottom

			var baseFont = BaseFont.CreateFont("c:/Windows/Fonts/Arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

			this.arialHeading = new Font(baseFont, 12, Font.BOLD);
			this.arialBold = new Font(baseFont, 9, Font.BOLD);
			this.arial = new Font(baseFont, 9, Font.NORMAL);
			this.arialSmall = new Font(baseFont, 8, Font.NORMAL);

			this.emptyNoBorderCell = new PdfPCell
			{
				Border = PdfPCell.NO_BORDER
			};

			this.emptyTopBorderCell = new PdfPCell
			{
				Border = PdfPCell.TOP_BORDER
			};

			this.emptyBottomBorderCell = new PdfPCell
			{
				Border = PdfPCell.BOTTOM_BORDER
			};

			var writer = PdfWriter.GetInstance(invoiceDoc, fs);
			invoiceDoc.Open();
			var content = writer.DirectContent;

			invoiceDoc.Add(this.GetTimesheet());
			invoiceDoc.Close();

			fs = new FileStream(filename, FileMode.Open);
			byte[] report = new byte[fs.Length];
			fs.Position = 0;
			fs.Read(report, 0, (int)fs.Length);
			fs.Close();

			return report;
		}

		private PdfPTable GetAddressHeader()
		{
			var headerTable = new PdfPTable(2);
			int[] widths = new int[] { 50, 50 };
			headerTable.SetWidths(widths);

			this.GetType().GetTypeInfo().Assembly.GetManifestResourceNames();

			////Logo
			var logo = Image.GetInstance(Assembly.GetExecutingAssembly().GetManifestResourceStream("Enabill.Print.Resources.Company.png"));
			//logo.ScaleToFit(152, 44);

			var headerLogoCell = new PdfPCell(logo)
			{
				HorizontalAlignment = PdfPCell.ALIGN_LEFT,
				Border = PdfPCell.NO_BORDER
			};

			////Saratoga Name Bold
			var blr = new StringBuilder();
			blr.AppendLine()
				.AppendLine(Code.Constants.COMPANYNAME);
			var headerCompanyNamePhrase = new Phrase(blr.ToString(), this.arialHeading);

			var headerCompanyNameCell = new PdfPCell(headerCompanyNamePhrase)
			{
				Border = PdfPCell.NO_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_LEFT
			};

			////Saratoga Address non bold
			blr.Clear()
				.AppendLine(Code.Constants.HEADOFFICEADDRESS1)
				.AppendLine(Code.Constants.HEADOFFICEADDRESS2)
				.AppendLine(Code.Constants.HEADOFFICEADDRESS3)
				.AppendLine(Code.Constants.HEADOFFICEADDRESS4)
				.AppendLine(Code.Constants.HEADOFFICEPOSTCODE);
			var headerCompanyAddressPhrase = new Phrase(blr.ToString(), this.arial);

			var headerCompanyAddressCell = new PdfPCell(headerCompanyAddressPhrase)
			{
				Border = PdfPCell.NO_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_LEFT
			};

			////Company Registration non bold
			///: looks out of alignment but had to do that for it to be inlign on the pdf
			blr.Clear()
				.Append("Company Reg.No.   : ").AppendLine(Code.Constants.COMPANYREGISTRATIONNO)
				.AppendLine("VAT Reg.No             : ").AppendLine(Code.Constants.COMPANYVATNO)
				.AppendLine("E-mail                       : ").AppendLine(Code.Constants.EMAILADDRESSACCOUNTS)
				.AppendLine("Telephone                : ").AppendLine(Code.Constants.HEADOFFICETELNO)
				.AppendLine("Fax                           : ").AppendLine(Code.Constants.HEADOFFICEFAXNO)
				.AppendLine();
			var headerCompanyRegistrationPhrase = new Phrase(blr.ToString(), this.arial);

			var headerCompanyRegistrationCell = new PdfPCell(headerCompanyRegistrationPhrase)
			{
				Border = PdfPCell.NO_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_LEFT
			};

			////Client Name Bold
			blr.Clear()
				.AppendLine(this.invoice.Client.ClientName);
			var headerClientNamePhrase = new Phrase(blr.ToString(), this.arialHeading);

			var headerClientNameCell = new PdfPCell(headerClientNamePhrase)
			{
				Border = PdfPCell.NO_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_LEFT
			};

			////Client Address Non-Bold
			blr.Clear()
				.AppendLine(this.invoice.Client.PostalAddress1)
				.AppendLine(this.invoice.Client.PostalAddress2)
				.AppendLine(this.invoice.Client.PostalAddress3)
				.AppendLine(this.invoice.Client.PostalCode)
				.AppendLine();
			var headerClientAddressPhrase = new Phrase(blr.ToString(), this.arial);

			var headerClientAddressCell = new PdfPCell(headerClientAddressPhrase)
			{
				Border = PdfPCell.NO_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_LEFT
			};

			////TAX INVOICE BOLD
			blr.Clear()
				.AppendLine("TAX INVOICE")
				.AppendLine();
			var headerTaxInvoicePhrase = new Phrase(blr.ToString(), this.arialHeading);

			var headerTaxInvoiceCell = new PdfPCell(headerTaxInvoicePhrase)
			{
				Border = PdfPCell.NO_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_LEFT
			};

			//Add MainHeader Cells
			headerTable.AddCell(headerLogoCell);
			headerTable.AddCell(this.emptyNoBorderCell);
			headerTable.AddCell(this.emptyNoBorderCell);
			headerTable.AddCell(headerCompanyNameCell);
			headerTable.AddCell(this.emptyNoBorderCell);
			headerTable.AddCell(headerCompanyAddressCell);
			headerTable.AddCell(headerClientNameCell);
			headerTable.AddCell(this.emptyNoBorderCell);
			headerTable.AddCell(headerClientAddressCell);
			headerTable.AddCell(headerCompanyRegistrationCell);
			headerTable.AddCell(this.emptyTopBorderCell);
			headerTable.AddCell(this.emptyTopBorderCell);
			headerTable.AddCell(headerTaxInvoiceCell);
			headerTable.AddCell(this.emptyNoBorderCell);

			return headerTable;
		}

		private PdfPTable GetInvoiceHeader()
		{
			var invoiceHeaderTable = new PdfPTable(4);
			int[] widths = new int[] { 20, 30, 20, 30 };
			invoiceHeaderTable.SetWidths(widths);

			//basic spacing
			var blr = new StringBuilder();
			this.clientDepartmentCodeID = ClientDepartmentCodeRepo.GetAll();
			this.glAccountID = GLAccount.GetAll();
			string departmentCode = string.Empty;
			string glCode = string.Empty;
			string exNumber = string.Empty;
			string invoiceNumber = string.Empty;

			if (this.invoice.ClientDepartmentCodeID != null)
			{
				foreach (var c in this.clientDepartmentCodeID)
				{
					if (c.ClientDepartmentCodeID == this.invoice.ClientDepartmentCodeID)
					{
						this.clientDepartmentCode = this.clientDepartmentCodeID.SingleOrDefault(s => s.ClientDepartmentCodeID == this.invoice.ClientDepartmentCodeID)?.DepartmentCode;
					}
				}
			}

			if (this.invoice.GLAccountID != null)
			{
				foreach (var b in this.glAccountID)
				{
					if (b.GLAccountID == this.invoice.GLAccountID)
					{
						this.gLAccountCode = this.glAccountID.SingleOrDefault(c => c.GLAccountID == this.invoice.GLAccountID)?.GLAccountCode;
					}
				}
			}

			if (!string.IsNullOrEmpty(this.clientDepartmentCode))
			{
				departmentCode = this.clientDepartmentCode;
			}

			if (!string.IsNullOrEmpty(this.gLAccountCode))
				glCode = this.gLAccountCode;

			//invoice left cell headings
			blr.Clear()
				.AppendLine("Invoice No")
				.AppendLine("Invoice Date")
				.AppendLine("Due Date")
				.AppendLine("Account No")
				.AppendLine("Department Code")
				.AppendLine("GL Account Code")
				.AppendLine("Period No")
				.AppendLine("Our Reference")
				.AppendLine("Order No");

			var invoiceLeftHeadingsPhrase = new Phrase(blr.ToString(), this.arialBold);

			var invoiceLeftHeadingsCell = new PdfPCell(invoiceLeftHeadingsPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_LEFT,
				Border = PdfPCell.LEFT_BORDER + PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			//invoice left side details
			blr.Clear();

			if (this.invoice.ExternalInvoiceNo != null)
				blr.Append(": ").AppendLine(this.invoice.ExternalInvoiceNo);
			else if (this.invoice.InvoiceID != 0)
				blr.Append(": ").AppendLine(this.invoice.InvoiceID.ToString());

			blr.Append(": ").AppendLine((this.invoice.InvoiceDate ?? DateTime.Now).ToShortDateString())
				.Append(": ").AppendLine((this.invoice.InvoiceDate ?? DateTime.Now).AddDays(14).ToShortDateString())
				.Append(": ").AppendLine(this.invoice.ClientAccountCode)
				.Append(": ").AppendLine(departmentCode)
				.Append(": ").AppendLine(glCode)
				.Append(": ").Append(this.invoice.Period).AppendLine()
				.Append(": ").AppendLine(this.invoice.OurRef)
				.Append(": ").AppendLine(this.invoice.OrderNo);

			var invoiceLeftDetailsPhrase = new Phrase(blr.ToString(), this.arial);

			var invoiceLeftDetailsCell = new PdfPCell(invoiceLeftDetailsPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_LEFT,
				Border = PdfPCell.NO_BORDER + PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			//invoice right side heading
			blr.Clear()
				.AppendLine("VAT Reg");

			if (!string.IsNullOrEmpty(this.invoice.InvoiceContactName))
			{
				if (this.invoice.InvoiceContactName.Length >= 27)
				{
					blr.AppendLine("Your Contact")
						.AppendLine("");
				}
				else
				{
					blr.AppendLine("Your Contact");
				}
			}
			else
			{
				blr.AppendLine("Your Contact");
			}

			blr.AppendLine("Your Reference")
				.AppendLine(" ")
				.AppendLine(" ")
				.AppendLine(" ");

			var invoiceRightHeadingsPhrase = new Phrase(blr.ToString(), this.arialBold);

			var invoiceRightHeadingsCell = new PdfPCell(invoiceRightHeadingsPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_LEFT,
				Border = PdfPCell.LEFT_BORDER + PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			//invoice right side details
			blr.Clear()
				.Append(": ").AppendLine(this.invoice.Client.VATNo)
				.Append(": ").AppendLine(this.invoice.InvoiceContactName)
				.Append(": ").AppendLine(this.invoice.CustomerRef)
				.AppendLine(" ")
				.AppendLine(" ")
				.AppendLine(" ");

			var invoiceRightDetailsPhrase = new Phrase(blr.ToString(), this.arial);

			var invoiceRightDetailsCell = new PdfPCell(invoiceRightDetailsPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_LEFT,
				Border = PdfPCell.RIGHT_BORDER + PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			//add the cells
			invoiceHeaderTable.AddCell(invoiceLeftHeadingsCell);
			invoiceHeaderTable.AddCell(invoiceLeftDetailsCell);
			invoiceHeaderTable.AddCell(invoiceRightHeadingsCell);
			invoiceHeaderTable.AddCell(invoiceRightDetailsCell);
			//insert a blank row
			invoiceHeaderTable.AddCell(this.emptyNoBorderCell);
			invoiceHeaderTable.AddCell(this.emptyNoBorderCell);
			invoiceHeaderTable.AddCell(this.emptyNoBorderCell);
			invoiceHeaderTable.AddCell(this.emptyNoBorderCell);
			invoiceHeaderTable.AddCell(this.emptyNoBorderCell);
			invoiceHeaderTable.AddCell(this.emptyNoBorderCell);

			return invoiceHeaderTable;
		}

		private PdfPTable GetDescription()
		{
			var descriptionTable = new PdfPTable(1);

			if (this.invoice.PrintOptionTypeID == (int)PrintOptionType.PrintDescriptionOnly)
				return descriptionTable;

			string description = this.invoice.Description;

			var descriptionPhrase = new Phrase(description, this.arial);

			var descriptionCell = new PdfPCell(descriptionPhrase)
			{
				Border = PdfPCell.NO_BORDER,
				Padding = 1.5F
			};

			descriptionTable.AddCell(descriptionCell);

			return descriptionTable;
		}

		private PdfPTable GetInvoiceLineDetails()
		{
			if (this.invoice.IsActivityFixedCost)
				return this.GetInvoiceLineDetailsActivityFixedCost();
			else if (this.invoice.IsSLA)
				return this.GetInvoiceLineDetailsSLA();

			if (this.invoice.PrintOptionTypeID == (int)PrintOptionType.PrintDescriptionOnly)
				return this.GetInvoiceLineDescriptionOnly();
			else
				return this.GetInvoiceLineDetailsGeneric();
		}

		private PdfPTable GetInvoiceLineDetailsActivityFixedCost()
		{
			var lines = new List<InvoiceLine>();
			InvoiceLine line;

			/*
			switch (invoice.PrintOptionTypeID)
			{
				case (int)PrintOptionType.PrintByActivity:
					foreach (ActivityPrintModel activity in _invoice.GetActivitiesPrintModel())
					{
						line = new InvoiceLine();
						line.ItemCode = activity.ProjectCode;
						line.Description = activity.Activity.ActivityName;
						line.Hours = activity.Hours;
						line.Rate = activity.Rate;
						line.Amount = activity.ExclVATAmount;
						lines.Add(line);
					}
					break;

				case (int)PrintOptionType.PrintByPerson:*/

			foreach (var item in this.invoice.GetUsersPrintModel())
			{
				line = new InvoiceLine
				{
					ItemCode = "",
					Description = item.User.FullName,
					Hours = item.Hours,
					Rate = item.Total / item.Hours,
					Amount = item.Total
				};
				lines.Add(line);
			}

			/*
					break;
			}
			*/

			var detailTable = new PdfPTable(2);
			int[] widths = new int[] { 60, 40 };
			detailTable.SetWidths(widths);

			//Invoice Lines Column Headings
			var invoiceLineHeadingPhrase = new Phrase("Description", this.arialBold);

			var invoiceLineHeadingCell = new PdfPCell(invoiceLineHeadingPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_LEFT,
				Border = PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};
			detailTable.AddCell(invoiceLineHeadingCell);

			invoiceLineHeadingPhrase = new Phrase("Amount", this.arialBold);

			invoiceLineHeadingCell = new PdfPCell(invoiceLineHeadingPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
				Border = PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};
			detailTable.AddCell(invoiceLineHeadingCell);

			var detailColumnPhrase = new Phrase("", this.arial);
			var detailColumnCell = new PdfPCell(detailColumnPhrase);

			foreach (var printLine in lines)
			{
				//add the detail lines
				detailColumnPhrase = new Phrase(printLine.Description, this.arial);
				detailColumnCell = new PdfPCell(detailColumnPhrase)
				{
					Border = PdfPCell.NO_BORDER,
					HorizontalAlignment = PdfPCell.ALIGN_LEFT
				};
				detailTable.AddCell(detailColumnCell);

				detailColumnPhrase = new Phrase(printLine.Rate.ToString("c"), this.arial);
				detailColumnCell = new PdfPCell(detailColumnPhrase)
				{
					Border = PdfPCell.NO_BORDER,
					HorizontalAlignment = PdfPCell.ALIGN_RIGHT
				};

				detailTable.AddCell(detailColumnCell);
			}

			return detailTable;
		}

		private PdfPTable GetInvoiceLineDetailsSLA()
		{
			var detailTable = new PdfPTable(3);
			int[] widths = new int[] { 50, 25, 25 };
			detailTable.SetWidths(widths);

			//Invoice Lines Column Headings
			var invoiceLineHeadingPhrase = new Phrase("Description", this.arialBold);

			var invoiceLineHeadingCell = new PdfPCell(invoiceLineHeadingPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_LEFT,
				Border = PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			detailTable.AddCell(invoiceLineHeadingCell);

			invoiceLineHeadingPhrase = new Phrase("Hours", this.arialBold);

			invoiceLineHeadingCell = new PdfPCell(invoiceLineHeadingPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
				Border = PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			detailTable.AddCell(invoiceLineHeadingCell);

			invoiceLineHeadingPhrase = new Phrase("Amount", this.arialBold);

			invoiceLineHeadingCell = new PdfPCell(invoiceLineHeadingPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
				Border = PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			detailTable.AddCell(invoiceLineHeadingCell);

			var detailColumnPhrase = new Phrase("", this.arial);
			var detailColumnCell = new PdfPCell(detailColumnPhrase);

			//add the detail lines
			detailColumnPhrase = new Phrase(this.invoice.Description, this.arial);

			detailColumnCell = new PdfPCell(detailColumnPhrase)
			{
				Border = PdfPCell.NO_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_LEFT
			};

			detailTable.AddCell(detailColumnCell);

			detailColumnPhrase = new Phrase(this.invoice.HoursPaidFor.Value.ToString("N"), this.arial);

			detailColumnCell = new PdfPCell(detailColumnPhrase)
			{
				Border = PdfPCell.NO_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT
			};

			detailTable.AddCell(detailColumnCell);

			detailColumnPhrase = new Phrase(this.invoice.InvoiceAmountExclVAT.ToString("c"), this.arial);

			detailColumnCell = new PdfPCell(detailColumnPhrase)
			{
				Border = PdfPCell.NO_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT
			};

			detailTable.AddCell(detailColumnCell);

			return detailTable;
		}

		private PdfPTable GetInvoiceLineDescriptionOnly()
		{
			var detailTable = new PdfPTable(2);
			int[] widths = new int[] { 75, 25 };
			detailTable.SetWidths(widths);

			//Invoice Lines Column Headings
			var invoiceLineHeadingPhrase = new Phrase("Description", this.arialBold);

			var invoiceLineHeadingCell = new PdfPCell(invoiceLineHeadingPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_LEFT,
				Border = PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			detailTable.AddCell(invoiceLineHeadingCell);

			invoiceLineHeadingPhrase = new Phrase("Amount", this.arialBold);

			invoiceLineHeadingCell = new PdfPCell(invoiceLineHeadingPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
				Border = PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			detailTable.AddCell(invoiceLineHeadingCell);

			var detailColumnPhrase = new Phrase("", this.arial);
			var detailColumnCell = new PdfPCell(detailColumnPhrase);

			//add the detail lines
			detailColumnPhrase = new Phrase(this.invoice.Description, this.arial);

			detailColumnCell = new PdfPCell(detailColumnPhrase)
			{
				Border = PdfPCell.NO_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_LEFT
			};

			detailTable.AddCell(detailColumnCell);

			detailColumnPhrase = new Phrase("(" + this.invoice.Client.GetCurrency(this.invoice.Client.CurrencyTypeID).CurrencyISO +") " +this.invoice.InvoiceAmountExclVAT.ToString(), this.arial);

			detailColumnCell = new PdfPCell(detailColumnPhrase)
			{
				Border = PdfPCell.NO_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT
			};

			detailTable.AddCell(detailColumnCell);

			return detailTable;
		}

		private PdfPTable GetInvoiceLineDetailsGeneric()
		{
			var lines = new List<InvoiceLine>();
			InvoiceLine line;

			switch (this.invoice.PrintOptionTypeID)
			{
				case (int)PrintOptionType.PrintByActivity:
					foreach (var activity in this.invoice.GetActivitiesPrintModel())
					{
						line = new InvoiceLine
						{
							ItemCode = activity.ProjectCode,
							Description = activity.Activity.ActivityName,
							Hours = activity.Hours,
							Rate = activity.Rate,
							Amount = activity.ExclVATAmount,
							Credits = activity.Credits
						};
						lines.Add(line);
					}
					break;

				case (int)PrintOptionType.PrintByPerson:
					foreach (var item in this.invoice.GetUsersPrintModel())
					{
						line = new InvoiceLine
						{
							ItemCode = "",
							Description = item.User.FullName,
							Hours = item.Hours,
							Rate = (item.Total - item.Credits) / item.Hours,
							Amount = item.Total,
							Credits = item.Credits
						};
						lines.Add(line);
					}
					break;
			}

			var detailTable = new PdfPTable(5);
			int[] widths = new int[] { 15, 45, 10, 10, 20 };
			detailTable.SetWidths(widths);

			//Invoice Lines Column Headings
			var invoiceLineHeadingPhrase = new Phrase("Item Code", this.arialBold);

			var invoiceLineHeadingCell = new PdfPCell(invoiceLineHeadingPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_LEFT,
				Border = PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			detailTable.AddCell(invoiceLineHeadingCell);

			invoiceLineHeadingPhrase = new Phrase("Description", this.arialBold);

			invoiceLineHeadingCell = new PdfPCell(invoiceLineHeadingPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_LEFT,
				Border = PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			detailTable.AddCell(invoiceLineHeadingCell);

			invoiceLineHeadingPhrase = new Phrase("Hours", this.arialBold);

			invoiceLineHeadingCell = new PdfPCell(invoiceLineHeadingPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
				Border = PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			detailTable.AddCell(invoiceLineHeadingCell);

			invoiceLineHeadingPhrase = new Phrase("Rate(" + this.invoice.Client.GetCurrency(this.invoice.Client.CurrencyTypeID).CurrencyISO.ToString() +")", this.arialBold);

			invoiceLineHeadingCell = new PdfPCell(invoiceLineHeadingPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
				Border = PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			detailTable.AddCell(invoiceLineHeadingCell);

			invoiceLineHeadingPhrase = new Phrase("Amount", this.arialBold);

			invoiceLineHeadingCell = new PdfPCell(invoiceLineHeadingPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
				Border = PdfPCell.TOP_BORDER + PdfPCell.BOTTOM_BORDER
			};

			detailTable.AddCell(invoiceLineHeadingCell);

			var detailColumnPhrase = new Phrase("", this.arial);
			var detailColumnCell = new PdfPCell(detailColumnPhrase);

			foreach (var printLine in lines)
			{
				//add the detail lines
				detailColumnPhrase = new Phrase(printLine.ItemCode, this.arial);

				detailColumnCell = new PdfPCell(detailColumnPhrase)
				{
					Border = PdfPCell.NO_BORDER,
					HorizontalAlignment = PdfPCell.ALIGN_LEFT
				};

				detailTable.AddCell(detailColumnCell);

				detailColumnPhrase = new Phrase(printLine.Description, this.arial);

				detailColumnCell = new PdfPCell(detailColumnPhrase)
				{
					Border = PdfPCell.NO_BORDER,
					HorizontalAlignment = PdfPCell.ALIGN_LEFT
				};

				detailTable.AddCell(detailColumnCell);

				detailColumnPhrase = new Phrase(printLine.Hours.ToString("N2"), this.arial);

				detailColumnCell = new PdfPCell(detailColumnPhrase)
				{
					Border = PdfPCell.NO_BORDER,
					HorizontalAlignment = PdfPCell.ALIGN_RIGHT
				};

				detailTable.AddCell(detailColumnCell);

				detailColumnPhrase = new Phrase(printLine.Rate.ToString("N2"), this.arial);

				detailColumnCell = new PdfPCell(detailColumnPhrase)
				{
					Border = PdfPCell.NO_BORDER,
					HorizontalAlignment = PdfPCell.ALIGN_RIGHT
				};

				detailTable.AddCell(detailColumnCell);

				string amount = string.Empty;

				if (this.invoice.PrintCredits)
					amount = ("(" + this.invoice.Client.GetCurrency(this.invoice.Client.CurrencyTypeID).CurrencyISO) +") " + printLine.Amount.ToString();
				else
					amount = ("(" + this.invoice.Client.GetCurrency(this.invoice.Client.CurrencyTypeID).CurrencyISO) + ") " + (printLine.Amount - printLine.Credits).ToString();

				detailColumnPhrase = new Phrase(amount, this.arial);

				detailColumnCell = new PdfPCell(detailColumnPhrase)
				{
					Border = PdfPCell.NO_BORDER,
					HorizontalAlignment = PdfPCell.ALIGN_RIGHT
				};

				detailTable.AddCell(detailColumnCell);

				if (this.invoice.PrintCredits && printLine.Credits > 0.0D)
				{
					detailTable.AddCell(this.emptyNoBorderCell);

					detailColumnPhrase = new Phrase("   Credit", this.arial);

					detailColumnCell = new PdfPCell(detailColumnPhrase)
					{
						Border = PdfPCell.NO_BORDER,
						HorizontalAlignment = PdfPCell.ALIGN_LEFT
					};

					detailTable.AddCell(detailColumnCell);

					detailTable.AddCell(this.emptyNoBorderCell);
					detailTable.AddCell(this.emptyNoBorderCell);

					detailColumnPhrase = new Phrase(("(" + this.invoice.Client.GetCurrency(this.invoice.Client.CurrencyTypeID).CurrencyISO + ") " + printLine.Credits * -1.0D).ToString(), this.arial);

					detailColumnCell = new PdfPCell(detailColumnPhrase)
					{
						Border = PdfPCell.NO_BORDER,
						HorizontalAlignment = PdfPCell.ALIGN_RIGHT
					};

					detailTable.AddCell(detailColumnCell);
				}
			}

			if (this.invoice.PrintCredits && this.invoice.InvoiceCreditAmount > 0)
			{
				detailTable.AddCell(this.emptyTopBorderCell);
				detailTable.AddCell(this.emptyTopBorderCell);
				detailTable.AddCell(this.emptyTopBorderCell);
				detailTable.AddCell(this.emptyTopBorderCell);
				detailTable.AddCell(this.emptyTopBorderCell);
				detailTable.AddCell(this.emptyNoBorderCell);
				detailColumnPhrase = new Phrase("Invoice Credit", this.arial);

				detailColumnCell = new PdfPCell(detailColumnPhrase)
				{
					Border = PdfPCell.NO_BORDER,
					HorizontalAlignment = PdfPCell.ALIGN_LEFT
				};

				detailTable.AddCell(detailColumnCell);
				detailTable.AddCell(this.emptyNoBorderCell);
				detailTable.AddCell(this.emptyNoBorderCell);
				detailColumnPhrase = new Phrase(( "(" + this.invoice.Client.GetCurrency(this.invoice.Client.CurrencyTypeID).CurrencyISO + ") " + this.invoice.InvoiceCreditAmount * -1.0D).ToString(), this.arial);

				detailColumnCell = new PdfPCell(detailColumnPhrase)
				{
					Border = PdfPCell.NO_BORDER,
					HorizontalAlignment = PdfPCell.ALIGN_RIGHT
				};

				detailTable.AddCell(detailColumnCell);
			}

			return detailTable;
		}

		private PdfPTable GetTotalLine()
		{
			var totalLineTable = new PdfPTable(5);
			int[] widths = new int[] { 15, 45, 10, 10, 20 };
			totalLineTable.SetWidths(widths);

			//Excl Vat Amount
			var totalExclPhrase = new Phrase("Sub Total", this.arial);

			var totalExclCell = new PdfPCell(totalExclPhrase)
			{
				Border = PdfPCell.TOP_BORDER
			};

			var totalExclAmountPhrase = new Phrase(("("+ this.invoice.Client.GetCurrency(this.invoice.Client.CurrencyTypeID).CurrencyISO )+") "+ this.invoice.InvoiceAmountExclVAT.ToString(),this.arial);

			var totalExclAmountCell = new PdfPCell(totalExclAmountPhrase)
			{
				Border = PdfPCell.TOP_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT
			};

			totalLineTable.AddCell(this.emptyTopBorderCell);
			totalLineTable.AddCell(totalExclCell);
			totalLineTable.AddCell(this.emptyTopBorderCell);
			totalLineTable.AddCell(this.emptyTopBorderCell);
			totalLineTable.AddCell(totalExclAmountCell);

			//VAT amount
			var vatPhrase = new Phrase("VAT @ " + this.invoice.VATRate.ToString(), this.arial);

			var vatCell = new PdfPCell(vatPhrase)
			{
				Border = PdfPCell.BOTTOM_BORDER
			};

			var vatAmountPhrase = new Phrase("(" + this.invoice.Client.GetCurrency(this.invoice.Client.CurrencyTypeID).CurrencyISO + ") " + this.invoice.VATAmount.ToString(), this.arial);

			var vatAmountCell = new PdfPCell(vatAmountPhrase)
			{
				Border = PdfPCell.BOTTOM_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT
			};

			totalLineTable.AddCell(this.emptyBottomBorderCell);
			totalLineTable.AddCell(vatCell);
			totalLineTable.AddCell(this.emptyBottomBorderCell);
			totalLineTable.AddCell(this.emptyBottomBorderCell);
			totalLineTable.AddCell(vatAmountCell);

			//Incl VAT
			var totalInclPhrase = new Phrase("Grand Total", this.arialBold);

			var totalInclCell = new PdfPCell(totalInclPhrase)
			{
				Border = PdfPCell.BOTTOM_BORDER
			};

			var totalInclAmountPhrase = new Phrase("(" + this.invoice.Client.GetCurrency(this.invoice.Client.CurrencyTypeID).CurrencyISO + ") " + this.invoice.InvoiceAmountInclVAT.ToString(), this.arialBold);

			var totalInclAmountCell = new PdfPCell(totalInclAmountPhrase)
			{
				Border = PdfPCell.BOTTOM_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT
			};

			totalLineTable.AddCell(this.emptyBottomBorderCell);
			totalLineTable.AddCell(totalInclCell);
			totalLineTable.AddCell(this.emptyBottomBorderCell);
			totalLineTable.AddCell(this.emptyBottomBorderCell);
			totalLineTable.AddCell(totalInclAmountCell);

			return totalLineTable;
		}

		private PdfPTable GetFooter()
		{
			var blr = new StringBuilder();
			blr.Append("AccountName : ").Append(Code.Constants.COMPANYNAME).AppendLine(" Software")
				.Append("Bank: ").Append(Code.Constants.BANK).Append("  Branch: ").Append(Code.Constants.BANKBRANCH).Append("  BranchCode: ").Append(Code.Constants.BANKBRANCHCODE).Append("  AccountCode: ").Append(Code.Constants.BANKACCOUNTNO);
			var footerTable = new PdfPTable(1);
			var footerPhrase = new Phrase(blr.ToString(), this.arialSmall);

			var footerCell = new PdfPCell(footerPhrase)
			{
				Border = PdfPCell.NO_BORDER,
				HorizontalAlignment = PdfPCell.ALIGN_CENTER,
				VerticalAlignment = PdfPCell.ALIGN_BOTTOM
			};

			footerTable.AddCell(footerCell);

			return footerTable;
		}

		private PdfPTable GetTimesheet()
		{
			if (this.invoice.PrintLayoutTypeID == (int)PrintLayoutType.AsTimeTable)
				return this.GetTimesheetTable();
			else if (this.invoice.PrintLayoutTypeID == (int)PrintLayoutType.AsDetails)
				return this.GetTimesheetDetails();
			else
				return new PdfPTable(1);
		}

		private PdfPTable GetTimesheetTable()
		{
			var model = this.invoice.GetTimesheetTable();

			// Do the heading
			int nrCols = 9;
			int[] widths = new int[] { 11, 6, 6, 25, 52, 70, 8, 7, 11 };

			if ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID == PrintTicketRemarkType.TicketAndRemark)
			{
				nrCols = 10;
				widths = new int[] { 11, 6, 6, 25, 55, 70, 13, 8, 7, 11 };
			}

			var timesheetTable = new PdfPTable(nrCols);

			timesheetTable.SetWidths(widths);

			var workDayPhrase = new Phrase("Date", this.arialBold);

			var workDayCell = new PdfPCell(workDayPhrase)
			{
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			var startTimePhrase = new Phrase("Start", this.arialBold);

			var startTimeCell = new PdfPCell(startTimePhrase)
			{
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			var endTimePhrase = new Phrase("End", this.arialBold);

			var endTimeCell = new PdfPCell(endTimePhrase)
			{
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			var userPhrase = new Phrase("Person", this.arialBold);

			var userCell = new PdfPCell(userPhrase)
			{
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			var projectPhrase = new Phrase("Project", this.arialBold);

			var projectCell = new PdfPCell(projectPhrase)
			{
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			var remarkPhrase = new Phrase("Remark", this.arialBold);

			var remarkCell = new PdfPCell(remarkPhrase)
			{
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			var ticketPhrase = new Phrase("Ticket Reference", this.arialBold);

			var ticketCell = new PdfPCell(ticketPhrase)
			{
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			var hoursPhrase = new Phrase("Hours", this.arialBold);

			var hoursCell = new PdfPCell(hoursPhrase)
			{
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			var hourlyRatePhrase = new Phrase("Rate", this.arialBold);

			var hourlyRateCell = new PdfPCell(hourlyRatePhrase)
			{
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			var billableAmountPhrase = new Phrase("Amount", this.arialBold);

			var billableAmountCell = new PdfPCell(billableAmountPhrase)
			{
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			timesheetTable.AddCell(workDayCell);
			timesheetTable.AddCell(startTimeCell);
			timesheetTable.AddCell(endTimeCell);
			timesheetTable.AddCell(userCell);
			timesheetTable.AddCell(projectCell);

			switch ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID)
			{
				case PrintTicketRemarkType.RemarkOnly:
					timesheetTable.AddCell(remarkCell);
					break;

				case PrintTicketRemarkType.TicketReferenceOnly:
					timesheetTable.AddCell(ticketCell);
					break;

				case PrintTicketRemarkType.TicketAndRemark:
					timesheetTable.AddCell(remarkCell);
					timesheetTable.AddCell(ticketCell);
					break;
			}

			timesheetTable.AddCell(hoursCell);
			timesheetTable.AddCell(hourlyRateCell);
			timesheetTable.AddCell(billableAmountCell);

			User user = null;
			double userTotal = 0.0D;
			double hourlyRate = 0.0D;
			double billableAmount = 0.0D;
			double billableAmountTotal = 0.0D;
			double billableAmountGrandTotal = 0.0D;

			// Do the rows
			foreach (var item in model.WorkItems)
			{
				if (user == null)
				{
					user = item.User;
				}
				else if (user.UserID != item.User.UserID)
				{
					remarkPhrase = new Phrase("Sub Total", this.arialSmall);

					remarkCell = new PdfPCell(remarkPhrase)
					{
						HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LIGHT_GRAY
					};

					hoursPhrase = new Phrase(userTotal.ToString(), this.arialSmall);

					hoursCell = new PdfPCell(hoursPhrase)
					{
						HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LIGHT_GRAY
					};

					hourlyRatePhrase = new Phrase(hourlyRate.ToString(), this.arialSmall);

					hourlyRateCell = new PdfPCell(hourlyRatePhrase)
					{
						HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LIGHT_GRAY
					};

					billableAmountPhrase = new Phrase(billableAmountTotal.ToString(), this.arialSmall);

					billableAmountCell = new PdfPCell(billableAmountPhrase)
					{
						HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
						BackgroundColor = BaseColor.LIGHT_GRAY
					};

					//make the sub-total line
					timesheetTable.AddCell(this.emptyNoBorderCell);
					timesheetTable.AddCell(this.emptyNoBorderCell);
					timesheetTable.AddCell(this.emptyNoBorderCell);
					timesheetTable.AddCell(this.emptyNoBorderCell);
					timesheetTable.AddCell(this.emptyNoBorderCell);

					if ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID == PrintTicketRemarkType.TicketAndRemark)
						timesheetTable.AddCell(this.emptyNoBorderCell);

					timesheetTable.AddCell(remarkCell);
					timesheetTable.AddCell(hoursCell);
					timesheetTable.AddCell(this.emptyNoBorderCell);
					timesheetTable.AddCell(billableAmountCell);

					// Make a blank row between the different users
					timesheetTable.AddCell(this.emptyNoBorderCell);
					timesheetTable.AddCell(this.emptyNoBorderCell);
					timesheetTable.AddCell(this.emptyNoBorderCell);
					timesheetTable.AddCell(this.emptyNoBorderCell);
					timesheetTable.AddCell(this.emptyNoBorderCell);
					timesheetTable.AddCell(this.emptyNoBorderCell);
					timesheetTable.AddCell(this.emptyNoBorderCell);
					timesheetTable.AddCell(this.emptyNoBorderCell);
					timesheetTable.AddCell(this.emptyNoBorderCell);

					if ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID == PrintTicketRemarkType.TicketAndRemark)
						timesheetTable.AddCell(this.emptyNoBorderCell);

					billableAmountGrandTotal += billableAmountTotal;
					userTotal = 0.0D;
					hourlyRate = 0.0D;
					billableAmount = 0.0D;
					billableAmountTotal = 0.0D;
					user = item.User;
				}

				var workDayStart = item.WorkSessions.Min(s => s.StartTime);
				var workDayEnd = item.WorkSessions.Max(s => s.EndTime);

				foreach (var wa in item.WorkAllocations)
				{
					// Date
					workDayPhrase = new Phrase(item.DayWorked.ToShortDateString(), this.arialSmall);
					workDayCell = new PdfPCell(workDayPhrase);

					// Start
					startTimePhrase = new Phrase(string.Format($"{workDayStart.Hour}:{workDayStart.Minute.ToString("D2")}"), this.arialSmall);
					startTimeCell = new PdfPCell(startTimePhrase);

					// End
					endTimePhrase = new Phrase(string.Format($"{workDayEnd.Hour}:{workDayEnd.Minute.ToString("D2")}"), this.arialSmall);
					endTimeCell = new PdfPCell(endTimePhrase);

					// Person
					userPhrase = new Phrase(item.User.FullName, this.arialSmall);
					userCell = new PdfPCell(userPhrase);

					// Project Name
					string projectName = wa.GetProject().ProjectName;
					projectPhrase = new Phrase(projectName, this.arialSmall);
					projectCell = new PdfPCell(projectPhrase);

					// Remark
					remarkPhrase = new Phrase(wa.Remark, this.arialSmall);
					remarkCell = new PdfPCell(remarkPhrase);

					// Ticket Reference
					string ticketReference = wa.TicketReference ?? "";
					ticketPhrase = new Phrase(ticketReference, this.arialSmall);
					ticketCell = new PdfPCell(ticketPhrase);

					// Hours
					userTotal += (wa.HoursBilled ?? wa.HoursWorked);
					hoursPhrase = new Phrase((wa.HoursBilled ?? wa.HoursWorked).ToString(), this.arialSmall);

					// Hourly Rate
					hourlyRate = (wa.HourlyRate ?? 0);
					hourlyRatePhrase = new Phrase((hourlyRate).ToString(), this.arialSmall);

					// Billable Amount
					billableAmount = ((wa.HoursBilled ?? wa.HoursWorked) * hourlyRate);
					billableAmountTotal += billableAmount;
					billableAmountPhrase = new Phrase((billableAmount).ToString(), this.arialSmall);

					hoursCell = new PdfPCell(hoursPhrase)
					{
						HorizontalAlignment = PdfPCell.ALIGN_RIGHT
					};

					hourlyRateCell = new PdfPCell(hourlyRatePhrase)
					{
						HorizontalAlignment = PdfPCell.ALIGN_RIGHT
					};

					billableAmountCell = new PdfPCell(billableAmountPhrase)
					{
						HorizontalAlignment = PdfPCell.ALIGN_RIGHT
					};

					timesheetTable.AddCell(workDayCell);
					timesheetTable.AddCell(startTimeCell);
					timesheetTable.AddCell(endTimeCell);
					timesheetTable.AddCell(userCell);
					timesheetTable.AddCell(projectCell);

					switch ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID)
					{
						case PrintTicketRemarkType.RemarkOnly:
							timesheetTable.AddCell(remarkCell);
							break;

						case PrintTicketRemarkType.TicketReferenceOnly:
							timesheetTable.AddCell(ticketCell);
							break;

						case PrintTicketRemarkType.TicketAndRemark:
							timesheetTable.AddCell(remarkCell);
							timesheetTable.AddCell(ticketCell);
							break;
					}

					timesheetTable.AddCell(hoursCell);
					timesheetTable.AddCell(hourlyRateCell);
					timesheetTable.AddCell(billableAmountCell);
				}
			}

			// Make the final sub-total row
			remarkPhrase = new Phrase("Sub Total", this.arialSmall);

			remarkCell = new PdfPCell(remarkPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			hoursPhrase = new Phrase(userTotal.ToString(), this.arialSmall);

			hoursCell = new PdfPCell(hoursPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			hourlyRatePhrase = new Phrase(hourlyRate.ToString(), this.arialSmall);

			hourlyRateCell = new PdfPCell(hourlyRatePhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			billableAmountPhrase = new Phrase(billableAmountTotal.ToString(), this.arialSmall);

			billableAmountCell = new PdfPCell(billableAmountPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			// Make the sub-total line
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);

			if ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID == PrintTicketRemarkType.TicketAndRemark)
				timesheetTable.AddCell(this.emptyNoBorderCell);

			timesheetTable.AddCell(remarkCell);
			timesheetTable.AddCell(hoursCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(billableAmountCell);

			// Make a blank row before total row
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);

			if ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID == PrintTicketRemarkType.TicketAndRemark)
				timesheetTable.AddCell(this.emptyNoBorderCell);

			// Make the total row
			remarkPhrase = new Phrase("Totals", this.arialBold);

			remarkCell = new PdfPCell(remarkPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT
			};

			hoursPhrase = new Phrase(model.TotalWorkHours.ToString(), this.arialBold);

			hoursCell = new PdfPCell(hoursPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT
			};

			billableAmountGrandTotal += billableAmountTotal;

			billableAmountPhrase = new Phrase(billableAmountGrandTotal.ToString(), this.arialBold);

			billableAmountCell = new PdfPCell(billableAmountPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT
			};

			// Make the total line
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);

			if ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID == PrintTicketRemarkType.TicketAndRemark)
				timesheetTable.AddCell(this.emptyNoBorderCell);

			timesheetTable.AddCell(remarkCell);
			timesheetTable.AddCell(hoursCell);
			timesheetTable.AddCell(this.emptyNoBorderCell);
			timesheetTable.AddCell(billableAmountCell);

			return timesheetTable;
		}

		private PdfPTable GetTimesheetDetails()
		{
			var model = this.invoice.GetTimesheetDetails();

			// Do the heading
			int nrCols = 7;
			int[] widths = new int[] { 11, 25, 52, 70, 8, 7, 11 };

			if ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID == PrintTicketRemarkType.TicketAndRemark)
			{
				nrCols = 8;
				widths = new int[] { 11, 25, 52, 70, 13, 8, 7, 11 };
			}

			var detailsTable = new PdfPTable(nrCols);

			detailsTable.SetWidths(widths);

			Phrase workDayPhrase;
			PdfPCell workDayCell;
			Phrase userPhrase;
			PdfPCell userCell;
			Phrase remarkPhrase;
			PdfPCell remarkCell;
			Phrase projectPhrase;
			PdfPCell projectCell;
			Phrase ticketPhrase;
			PdfPCell ticketCell;
			Phrase hoursPhrase;
			PdfPCell hoursCell;
			Phrase hourlyRatePhrase;
			PdfPCell hourlyRateCell;
			Phrase billableAmountPhrase;
			PdfPCell billableAmountCell;

			ActivityDetail activity = null;
			double userTotal = 0.0D;
			double hourlyRate = 0.0D;
			double billableAmount = 0.0D;
			double billableAmountTotal = 0.0D;
			double billableAmountGrandTotal = 0.0D;

			foreach (var item in model.WorkItems)
			{
				if ((activity == null) || (activity.ActivityID != item.Activity.ActivityID))
				{
					if (activity != null)
					{
						// Do a sub total row first...
						remarkPhrase = new Phrase("Sub Total", this.arialSmall);

						remarkCell = new PdfPCell(remarkPhrase)
						{
							HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
							BackgroundColor = BaseColor.LIGHT_GRAY
						};

						hoursPhrase = new Phrase(userTotal.ToString(), this.arialSmall);

						hoursCell = new PdfPCell(hoursPhrase)
						{
							HorizontalAlignment = PdfPCell.ALIGN_RIGHT
						};

						hourlyRatePhrase = new Phrase(hourlyRate.ToString(), this.arialSmall);

						hourlyRateCell = new PdfPCell(hourlyRatePhrase)
						{
							HorizontalAlignment = PdfPCell.ALIGN_RIGHT
						};

						billableAmountPhrase = new Phrase(billableAmountTotal.ToString(), this.arialSmall);

						billableAmountCell = new PdfPCell(billableAmountPhrase)
						{
							HorizontalAlignment = PdfPCell.ALIGN_RIGHT
						};

						detailsTable.AddCell(this.emptyNoBorderCell);
						detailsTable.AddCell(this.emptyNoBorderCell);
						detailsTable.AddCell(this.emptyNoBorderCell);

						if ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID == PrintTicketRemarkType.TicketAndRemark)
							detailsTable.AddCell(this.emptyNoBorderCell);

						detailsTable.AddCell(remarkCell);
						detailsTable.AddCell(hoursCell);
						detailsTable.AddCell(this.emptyNoBorderCell);
						detailsTable.AddCell(billableAmountCell);

						// Insert a blank row first
						detailsTable.AddCell(this.emptyNoBorderCell);
						detailsTable.AddCell(this.emptyNoBorderCell);
						detailsTable.AddCell(this.emptyNoBorderCell);
						detailsTable.AddCell(this.emptyNoBorderCell);
						detailsTable.AddCell(this.emptyNoBorderCell);
						detailsTable.AddCell(this.emptyNoBorderCell);
						detailsTable.AddCell(this.emptyNoBorderCell);

						if ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID == PrintTicketRemarkType.TicketAndRemark)
							detailsTable.AddCell(this.emptyNoBorderCell);

						billableAmountGrandTotal += billableAmountTotal;
						userTotal = 0.0D;
						hourlyRate = 0.0D;
						billableAmount = 0.0D;
						billableAmountTotal = 0.0D;
					}

					// Then we switch to the new activity
					activity = item.Activity;
					userTotal = 0.0D;

					// Do the activity heading
					var activityHeadingPhrase = new Phrase("Activity", this.arialBold);

					var activityHeadingCell = new PdfPCell(activityHeadingPhrase)
					{
						Colspan = 2,
						BackgroundColor = BaseColor.LIGHT_GRAY
					};

					var activityNamePhrase = new Phrase(activity.ActivityName, this.arialBold);

					var activityNameCell = new PdfPCell(activityNamePhrase)
					{
						Colspan = (PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID == PrintTicketRemarkType.TicketAndRemark ? 6 : 5
					};

					detailsTable.AddCell(activityHeadingCell);
					detailsTable.AddCell(activityNameCell);

					// Do the header row for the details
					workDayPhrase = new Phrase("Date", this.arialBold);

					workDayCell = new PdfPCell(workDayPhrase)
					{
						BackgroundColor = BaseColor.LIGHT_GRAY
					};

					userPhrase = new Phrase("Person", this.arialBold);

					userCell = new PdfPCell(userPhrase)
					{
						BackgroundColor = BaseColor.LIGHT_GRAY
					};

					projectPhrase = new Phrase("Project", this.arialBold);

					projectCell = new PdfPCell(projectPhrase)
					{
						BackgroundColor = BaseColor.LIGHT_GRAY
					};

					remarkPhrase = new Phrase("Remark", this.arialBold);

					remarkCell = new PdfPCell(remarkPhrase)
					{
						BackgroundColor = BaseColor.LIGHT_GRAY
					};

					ticketPhrase = new Phrase("Ticket Reference", this.arialBold);

					ticketCell = new PdfPCell(ticketPhrase)
					{
						BackgroundColor = BaseColor.LIGHT_GRAY
					};

					hoursPhrase = new Phrase("Hours", this.arialBold);

					hoursCell = new PdfPCell(hoursPhrase)
					{
						BackgroundColor = BaseColor.LIGHT_GRAY
					};

					hourlyRatePhrase = new Phrase("Rate", this.arialBold);

					hourlyRateCell = new PdfPCell(hourlyRatePhrase)
					{
						BackgroundColor = BaseColor.LIGHT_GRAY
					};

					billableAmountPhrase = new Phrase("Amount", this.arialBold);

					billableAmountCell = new PdfPCell(billableAmountPhrase)
					{
						BackgroundColor = BaseColor.LIGHT_GRAY
					};

					detailsTable.AddCell(workDayCell);
					detailsTable.AddCell(userCell);
					detailsTable.AddCell(projectCell);

					switch ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID)
					{
						case PrintTicketRemarkType.RemarkOnly:
							detailsTable.AddCell(remarkCell);
							break;

						case PrintTicketRemarkType.TicketReferenceOnly:
							detailsTable.AddCell(ticketCell);
							break;

						case PrintTicketRemarkType.TicketAndRemark:
							detailsTable.AddCell(remarkCell);
							detailsTable.AddCell(ticketCell);
							break;
					}

					detailsTable.AddCell(hoursCell);
					detailsTable.AddCell(hourlyRateCell);
					detailsTable.AddCell(billableAmountCell);
				}

				// Date
				workDayPhrase = new Phrase(item.WorkAllocation.DayWorked.ToShortDateString(), this.arialSmall);
				workDayCell = new PdfPCell(workDayPhrase);

				// Person
				userPhrase = new Phrase(item.User.FullName, this.arialSmall);
				userCell = new PdfPCell(userPhrase);

				// Project Name
				string projectName = item.WorkAllocation.GetProject().ProjectName;
				projectPhrase = new Phrase(projectName, this.arialSmall);
				projectCell = new PdfPCell(projectPhrase);

				// Remark
				string remark = string.Empty;

				if (string.IsNullOrEmpty(item.NoteText))
				{
					remark = item.WorkAllocation.Remark;
				}
				else
				{
					//remark = PrintUtils.HTMLToText(item.NoteText);
					remark = item.WorkAllocation.Remark + " - " + PrintUtils.HTMLToText(item.NoteText);
				}

				remarkPhrase = new Phrase(remark, this.arialSmall);
				remarkCell = new PdfPCell(remarkPhrase);

				// Ticket
				string ticketReference = item.WorkAllocation.TicketReference ?? "";
				ticketPhrase = new Phrase(ticketReference, this.arialSmall);
				ticketCell = new PdfPCell(ticketPhrase);

				// Hours
				userTotal += item.WorkAllocation.HoursBilled ?? item.WorkAllocation.HoursWorked;
				hoursPhrase = new Phrase((item.WorkAllocation.HoursBilled ?? item.WorkAllocation.HoursWorked).ToString(), this.arialSmall);

				// Hourly Rate
				hourlyRate = (item.WorkAllocation.HourlyRate ?? 0);
				hourlyRatePhrase = new Phrase((hourlyRate).ToString(), this.arialSmall);

				// Billable Amount
				billableAmount = ((item.WorkAllocation.HoursBilled ?? item.WorkAllocation.HoursWorked) * hourlyRate);
				billableAmountTotal += billableAmount;
				billableAmountPhrase = new Phrase((billableAmount).ToString(), this.arialSmall);

				hoursCell = new PdfPCell(hoursPhrase)
				{
					HorizontalAlignment = PdfPCell.ALIGN_RIGHT
				};

				hourlyRateCell = new PdfPCell(hourlyRatePhrase)
				{
					HorizontalAlignment = PdfPCell.ALIGN_RIGHT
				};

				billableAmountCell = new PdfPCell(billableAmountPhrase)
				{
					HorizontalAlignment = PdfPCell.ALIGN_RIGHT
				};

				detailsTable.AddCell(workDayCell);
				detailsTable.AddCell(userCell);
				detailsTable.AddCell(projectCell);

				switch ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID)
				{
					case PrintTicketRemarkType.RemarkOnly:
						detailsTable.AddCell(remarkCell);
						break;

					case PrintTicketRemarkType.TicketReferenceOnly:
						detailsTable.AddCell(ticketCell);
						break;

					case PrintTicketRemarkType.TicketAndRemark:
						detailsTable.AddCell(remarkCell);
						detailsTable.AddCell(ticketCell);
						break;
				}

				detailsTable.AddCell(hoursCell);
				detailsTable.AddCell(hourlyRateCell);
				detailsTable.AddCell(billableAmountCell);
			}

			// Make the final sub total row
			remarkPhrase = new Phrase("Sub Total", this.arialSmall);

			remarkCell = new PdfPCell(remarkPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			hoursPhrase = new Phrase(userTotal.ToString(), this.arialSmall);

			hoursCell = new PdfPCell(hoursPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT
			};

			billableAmountGrandTotal += billableAmountTotal;

			billableAmountPhrase = new Phrase(billableAmountTotal.ToString(), this.arialSmall);

			billableAmountCell = new PdfPCell(billableAmountPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT
			};

			// Make the sub-total line
			detailsTable.AddCell(this.emptyNoBorderCell);
			detailsTable.AddCell(this.emptyNoBorderCell);
			detailsTable.AddCell(this.emptyNoBorderCell);

			if ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID == PrintTicketRemarkType.TicketAndRemark)
				detailsTable.AddCell(this.emptyNoBorderCell);

			detailsTable.AddCell(remarkCell);
			detailsTable.AddCell(hoursCell);
			detailsTable.AddCell(this.emptyNoBorderCell);
			detailsTable.AddCell(billableAmountCell);

			// Make a blank row before total row
			detailsTable.AddCell(this.emptyNoBorderCell);
			detailsTable.AddCell(this.emptyNoBorderCell);
			detailsTable.AddCell(this.emptyNoBorderCell);
			detailsTable.AddCell(this.emptyNoBorderCell);
			detailsTable.AddCell(this.emptyNoBorderCell);
			detailsTable.AddCell(this.emptyNoBorderCell);
			detailsTable.AddCell(this.emptyNoBorderCell);

			if ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID == PrintTicketRemarkType.TicketAndRemark)
				detailsTable.AddCell(this.emptyNoBorderCell);

			// Make the total row
			remarkPhrase = new Phrase("Totals", this.arialBold);

			remarkCell = new PdfPCell(remarkPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT,
				BackgroundColor = BaseColor.LIGHT_GRAY
			};

			hoursPhrase = new Phrase(model.TotalWorkHours.ToString(), this.arialBold);

			hoursCell = new PdfPCell(hoursPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT
			};

			billableAmountPhrase = new Phrase(billableAmountGrandTotal.ToString(), this.arialBold);

			billableAmountCell = new PdfPCell(billableAmountPhrase)
			{
				HorizontalAlignment = PdfPCell.ALIGN_RIGHT
			};

			// Make the total line
			detailsTable.AddCell(this.emptyNoBorderCell);
			detailsTable.AddCell(this.emptyNoBorderCell);
			detailsTable.AddCell(this.emptyNoBorderCell);

			if ((PrintTicketRemarkType)this.invoice.PrintTicketRemarkOptionID == PrintTicketRemarkType.TicketAndRemark)
				detailsTable.AddCell(this.emptyNoBorderCell);

			detailsTable.AddCell(remarkCell);
			detailsTable.AddCell(hoursCell);
			detailsTable.AddCell(this.emptyNoBorderCell);
			detailsTable.AddCell(billableAmountCell);

			return detailsTable;
		}

		#endregion PRINT METHODS
	}
}