CREATE TABLE [dbo].[ExpenseAttachments] (
    [ExpenseAttachmentID] INT            IDENTITY (1, 1) NOT NULL,
    [ExpenseID]           INT            NOT NULL,
    [FilePath]            NVARCHAR (100) NOT NULL,
    [FileName]            NVARCHAR (30)  NOT NULL,
    [MimeType]            NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_ExpenseAttachment] PRIMARY KEY CLUSTERED ([ExpenseAttachmentID] ASC),
    CONSTRAINT [FK_ExpenseAttachments_Expenses] FOREIGN KEY ([ExpenseID]) REFERENCES [dbo].[Expenses] ([ExpenseID])
);

