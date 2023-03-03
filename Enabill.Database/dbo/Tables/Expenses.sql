CREATE TABLE [dbo].[Expenses] (
    [ExpenseID]             INT            IDENTITY (1, 1) NOT NULL,
    [UserID]                INT            NOT NULL,
    [ClientID]              INT            NOT NULL,
    [ProjectID]             INT            NOT NULL,
    [ExpenseCategoryTypeID] INT            NOT NULL,
    [ExpenseDate]           DATE           NOT NULL,
    [Amount]                FLOAT (53)     NOT NULL,
    [Notes]                 NVARCHAR (500) NULL,
    [AttachmentCode]        NVARCHAR (20)  NULL,
    [Billable]              BIT            CONSTRAINT [DF_Expenses_Billable] DEFAULT ((1)) NOT NULL,
    [Locked]                BIT            CONSTRAINT [DF_Expenses_Archived] DEFAULT ((0)) NOT NULL,
    [ManagedBy]             VARCHAR (128)  NULL,
    [DateManaged]           DATETIME       NULL,
    [LastModifiedBy]        VARCHAR (128)  NOT NULL,
    [Mileage]               FLOAT (53)     NULL,
    CONSTRAINT [PK_Expense] PRIMARY KEY CLUSTERED ([ExpenseID] ASC),
    CONSTRAINT [FK_Expenses_Client] FOREIGN KEY ([ClientID]) REFERENCES [dbo].[Clients] ([ClientID]),
    CONSTRAINT [FK_Expenses_ExpenseCategoryTypes] FOREIGN KEY ([ExpenseCategoryTypeID]) REFERENCES [dbo].[ExpenseCategoryTypes] ([ExpenseCategoryTypeID]),
    CONSTRAINT [FK_Expenses_Project] FOREIGN KEY ([ProjectID]) REFERENCES [dbo].[Projects] ([ProjectID]),
    CONSTRAINT [FK_Expenses_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);

