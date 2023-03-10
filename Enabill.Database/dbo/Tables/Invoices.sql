CREATE TABLE [dbo].[Invoices] (
    [InvoiceID]                 INT           IDENTITY (1, 1) NOT NULL,
    [ExternalInvoiceNo]         VARCHAR (30)  NULL,
    [InvoiceRuleID]             INT           NULL,
    [UserCreated]               VARCHAR (50)  NOT NULL,
    [DateCreated]               DATETIME      NOT NULL,
    [UserInvoiced]              VARCHAR (50)  NULL,
    [DateInvoiced]              DATETIME      NULL,
    [InvoiceStatusID]           INT           NOT NULL,
    [InvoiceSubCategoryID]      INT           NOT NULL,
    [BillingMethodID]           INT           NOT NULL,
    [ClientID]                  INT           NOT NULL,
    [ClientName]                VARCHAR (100) NULL,
    [InvoiceContactID]          INT           NULL,
    [CustomerRef]               VARCHAR (50)  NULL,
    [OurRef]                    VARCHAR (50)  NULL,
    [OrderNo]                   VARCHAR (50)  NULL,
    [DateFrom]                  DATETIME      NOT NULL,
    [DateTo]                    DATETIME      NOT NULL,
    [InvoiceDate]               DATETIME      NULL,
    [Period]                    INT           NOT NULL,
    [HoursPaidFor]              INT           NULL,
    [InvoiceAmountExclVAT]      FLOAT (53)    NOT NULL,
    [AccrualExclVAT]            FLOAT (53)    NOT NULL,
    [InvoiceAmountInclVAT]      FLOAT (53)    NOT NULL,
    [VATRate]                   FLOAT (53)    NOT NULL,
    [VATAmount]                 FLOAT (53)    NOT NULL,
    [ProjectID]                 INT           NULL,
    [ProjectedAmountExcl]       FLOAT (53)    NULL,
    [InvoiceContactName]        VARCHAR (128) NULL,
    [ProvisionalAccrualAmount]  FLOAT (53)    NULL,
    [ProvisionalIncomeAmount]   FLOAT (53)    NULL,
    [ClientAccountCode]         VARCHAR (50)  NULL,
    [Description]               VARCHAR (500) NULL,
    [PrintOptionTypeID]         INT           NULL,
    [PrintCredits]              BIT           DEFAULT ((0)) NOT NULL,
    [IsTimeApproved]            BIT           DEFAULT ((0)) NOT NULL,
    [PrintLayoutTypeID]         INT           NULL,
    [PrintTimeSheet]            BIT           DEFAULT ((1)) NOT NULL,
    [PrintTicketRemarkOptionID] INT           DEFAULT ((1)) NOT NULL,
    [ClientDepartmentCodeID]    INT           NULL,
    [GLAccountID]               INT           NULL,
    [IsInternal]                BIT           CONSTRAINT [DF_Invoices_IsInternal] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Invoice2] PRIMARY KEY CLUSTERED ([InvoiceID] ASC),
    CONSTRAINT [FK_Invoice_BillingMethods] FOREIGN KEY ([BillingMethodID]) REFERENCES [dbo].[BillingMethods] ([BillingMethodID]),
    CONSTRAINT [FK_Invoice_InvoiceRules] FOREIGN KEY ([InvoiceRuleID]) REFERENCES [dbo].[InvoiceRules] ([InvoiceRuleID]),
    CONSTRAINT [FK_Invoice_InvoiceSubCategories] FOREIGN KEY ([InvoiceSubCategoryID]) REFERENCES [dbo].[InvoiceSubCategories] ([InvoiceSubCategoryID]),
    CONSTRAINT [FK_Invoices_ClientDepartmentCode] FOREIGN KEY ([ClientDepartmentCodeID]) REFERENCES [dbo].[ClientDepartmentCode] ([ClientDepartmentCodeID]),
    CONSTRAINT [FK_Invoices_GLAccounts] FOREIGN KEY ([GLAccountID]) REFERENCES [dbo].[GLAccounts] ([GLAccountID]),
    CONSTRAINT [FK_Invoices_InvoiceStatus] FOREIGN KEY ([InvoiceStatusID]) REFERENCES [dbo].[InvoiceStatus] ([InvoiceStatusID])
);


GO
CREATE NONCLUSTERED INDEX [IX_Invoices_ClientID_Multiple]
    ON [dbo].[Invoices]([ClientID] ASC, [InvoiceRuleID] ASC, [Period] ASC, [BillingMethodID] ASC, [DateFrom] ASC);

