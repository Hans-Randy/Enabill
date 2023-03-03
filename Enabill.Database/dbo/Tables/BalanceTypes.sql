CREATE TABLE [dbo].[BalanceTypes] (
    [BalanceTypeID]   INT           NOT NULL,
    [BalanceTypeName] VARCHAR (100) NOT NULL,
    CONSTRAINT [PK_BalanceType] PRIMARY KEY CLUSTERED ([BalanceTypeID] ASC)
);

