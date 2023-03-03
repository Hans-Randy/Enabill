CREATE TABLE [dbo].[GLAccounts] (
    [GLAccountID]   INT           IDENTITY (1, 1) NOT NULL,
    [GLAccountCode] VARCHAR (20)  NOT NULL,
    [GLAccountName] VARCHAR (200) NOT NULL,
    [IsActive]      BIT           NOT NULL,
    CONSTRAINT [PK_GLAccounts_GLAccountID] PRIMARY KEY CLUSTERED ([GLAccountID] ASC)
);

