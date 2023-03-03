CREATE TABLE [dbo].[BillableIndicators] (
    [BillableIndicatorID]   INT          NOT NULL,
    [BillableIndicatorName] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_BillableIndicators] PRIMARY KEY CLUSTERED ([BillableIndicatorID] ASC)
);

