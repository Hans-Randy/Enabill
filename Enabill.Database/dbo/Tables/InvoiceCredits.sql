CREATE TABLE [dbo].[InvoiceCredits] (
    [InvoiceCreditID]  INT           IDENTITY (1, 1) NOT NULL,
    [InvoiceID]        INT           NOT NULL,
    [WorkAllocationID] INT           NULL,
    [CreditAmount]     FLOAT (53)    NOT NULL,
    [CreatedBy]        VARCHAR (128) NULL,
    [LastModifiedBy]   VARCHAR (128) NOT NULL,
    CONSTRAINT [PK_InvoiceCredits] PRIMARY KEY CLUSTERED ([InvoiceCreditID] ASC),
    CONSTRAINT [FK_InvoiceCredits_Invoices] FOREIGN KEY ([InvoiceID]) REFERENCES [dbo].[Invoices] ([InvoiceID]),
    CONSTRAINT [FK_InvoiceCredits_WorkAllocations] FOREIGN KEY ([WorkAllocationID]) REFERENCES [dbo].[WorkAllocations] ([WorkAllocationID]),
    CONSTRAINT [IX_InvoiceCredits_Invoice_WorkAllocation] UNIQUE NONCLUSTERED ([InvoiceID] ASC, [WorkAllocationID] ASC)
);

