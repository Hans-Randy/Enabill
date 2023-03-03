CREATE TABLE [dbo].[ExpenseCategoryTypes] (
    [ExpenseCategoryTypeID]   INT           NOT NULL,
    [ExpenseCategoryTypeName] VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_ExpenseCategoryType] PRIMARY KEY CLUSTERED ([ExpenseCategoryTypeID] ASC),
    CONSTRAINT [UC_ExpenseCategoryTypeName] UNIQUE NONCLUSTERED ([ExpenseCategoryTypeName] ASC)
);

