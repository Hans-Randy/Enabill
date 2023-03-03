CREATE TABLE [dbo].[InvoiceContacts] (
    [InvoiceContactID] INT IDENTITY (1, 1) NOT NULL,
    [InvoiceID]        INT NOT NULL,
    [ContactID]        INT NOT NULL,
    CONSTRAINT [PK_InvoiceContacts] PRIMARY KEY CLUSTERED ([InvoiceContactID] ASC),
    CONSTRAINT [FK_InvoiceContacts_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_InvoiceContacts_Invoices] FOREIGN KEY ([InvoiceID]) REFERENCES [dbo].[Invoices] ([InvoiceID]),
    CONSTRAINT [IX_InvoiceContacts] UNIQUE NONCLUSTERED ([ContactID] ASC, [InvoiceID] ASC)
);

