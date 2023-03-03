CREATE TABLE [dbo].[BalanceChangeTypes] (
    [BalanceChangeTypeID]   INT           NOT NULL,
    [BalanceChangeTypeName] VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_BalanceChangeType] PRIMARY KEY CLUSTERED ([BalanceChangeTypeID] ASC)
);

