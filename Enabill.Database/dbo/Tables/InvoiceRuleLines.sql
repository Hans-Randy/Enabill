CREATE TABLE [dbo].[InvoiceRuleLines] (
    [InvoiceRuleLineID]  INT           IDENTITY (1, 1) NOT NULL,
    [InvoiceRuleID]      INT           NOT NULL,
    [DefaultDescription] VARCHAR (500) NULL,
    [Period]             INT           NOT NULL,
    [CustomerAmount]     FLOAT (53)    NOT NULL,
    [AccrualAmount]      FLOAT (53)    NOT NULL,
    [InvoiceID]          INT           NULL,
    CONSTRAINT [PK_InvoiceRuleLines] PRIMARY KEY CLUSTERED ([InvoiceRuleLineID] ASC),
    CONSTRAINT [FK_InvoiceRuleLines_InvoiceRules] FOREIGN KEY ([InvoiceRuleID]) REFERENCES [dbo].[InvoiceRules] ([InvoiceRuleID]),
    CONSTRAINT [FK_InvoiceRuleLines_Invoices] FOREIGN KEY ([InvoiceID]) REFERENCES [dbo].[Invoices] ([InvoiceID])
);


GO
CREATE NONCLUSTERED INDEX [IX_InvoiceRuleID]
    ON [dbo].[InvoiceRuleLines]([InvoiceRuleID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_InvoiceID]
    ON [dbo].[InvoiceRuleLines]([InvoiceID] ASC);

