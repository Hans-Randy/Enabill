CREATE TABLE [dbo].[InvoiceRuleContacts] (
    [InvoiceRuleContactID] INT IDENTITY (1, 1) NOT NULL,
    [InvoiceRuleID]        INT NOT NULL,
    [ContactID]            INT NOT NULL,
    CONSTRAINT [PK_InvoiceRuleContacts] PRIMARY KEY CLUSTERED ([InvoiceRuleContactID] ASC),
    CONSTRAINT [FK_InvoiceRuleContacts_Contacts] FOREIGN KEY ([ContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
    CONSTRAINT [FK_InvoiceRuleContacts_InvoiceRules] FOREIGN KEY ([InvoiceRuleID]) REFERENCES [dbo].[InvoiceRules] ([InvoiceRuleID]),
    CONSTRAINT [IX_InvoiceRuleContacts] UNIQUE NONCLUSTERED ([ContactID] ASC, [InvoiceRuleID] ASC)
);

