CREATE TABLE [dbo].[FlexiBalances] (
    [FlexiBalanceID]       INT           IDENTITY (1, 1) NOT NULL,
    [UserID]               INT           NOT NULL,
    [BalanceDate]          DATETIME      NOT NULL,
    [CalculatedAdjustment] FLOAT (53)    NOT NULL,
    [ManualAdjustment]     FLOAT (53)    NOT NULL,
    [UpdatedBy]            VARCHAR (128) NULL,
    [DateUpdated]          DATETIME      NULL,
    [FinalBalance]         FLOAT (53)    NULL,
    CONSTRAINT [PK_FlexiBalances] PRIMARY KEY CLUSTERED ([FlexiBalanceID] ASC),
    CONSTRAINT [FK_FlexiBalances_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_FlexiBalances_UserID_Include]
    ON [dbo].[FlexiBalances]([UserID] ASC)
    INCLUDE([BalanceDate]);

