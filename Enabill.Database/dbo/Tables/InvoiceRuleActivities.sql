CREATE TABLE [dbo].[InvoiceRuleActivities] (
    [InvoiceRuleActivityID] INT IDENTITY (1, 1) NOT NULL,
    [InvoiceRuleID]         INT NOT NULL,
    [ActivityID]            INT NOT NULL,
    CONSTRAINT [PK_InvoiceRuleActivities] PRIMARY KEY CLUSTERED ([InvoiceRuleActivityID] ASC),
    CONSTRAINT [FK_InvoiceRuleActivities_Activities] FOREIGN KEY ([ActivityID]) REFERENCES [dbo].[Activities] ([ActivityID]),
    CONSTRAINT [FK_InvoiceRuleActivities_InvoiceRules] FOREIGN KEY ([InvoiceRuleID]) REFERENCES [dbo].[InvoiceRules] ([InvoiceRuleID]),
    CONSTRAINT [IX_InvoiceRuleActivities] UNIQUE NONCLUSTERED ([InvoiceRuleID] ASC, [ActivityID] ASC)
);

