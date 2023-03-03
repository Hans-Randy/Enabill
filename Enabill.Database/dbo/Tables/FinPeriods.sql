CREATE TABLE [dbo].[FinPeriods] (
    [FinPeriodID] INT      NOT NULL,
    [DateFrom]    DATETIME NOT NULL,
    [DateTo]      DATETIME NOT NULL,
    [IsCurrent]   BIT      NOT NULL,
    CONSTRAINT [PK_FinPeriods] PRIMARY KEY CLUSTERED ([FinPeriodID] ASC)
);

