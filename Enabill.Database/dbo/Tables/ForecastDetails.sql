CREATE TABLE [dbo].[ForecastDetails] (
    [ForecastDetailID]     INT           IDENTITY (1, 1) NOT NULL,
    [ForecastHeaderID]     INT           NOT NULL,
    [Period]               INT           NOT NULL,
    [EntryDate]            DATETIME      NOT NULL,
    [HourlyRate]           FLOAT (53)    NULL,
    [AllocationPercentage] FLOAT (53)    NULL,
    [Adjustment]           FLOAT (53)    NULL,
    [Amount]               FLOAT (53)    NOT NULL,
    [Remark]               VARCHAR (512) NULL,
    [Reference]            VARCHAR (250) NULL,
    [ModifiedByUserID]     INT           NOT NULL,
    CONSTRAINT [PK_ForecastDetails] PRIMARY KEY CLUSTERED ([ForecastDetailID] ASC),
    CONSTRAINT [FK_FCDetail_FCHeader] FOREIGN KEY ([ForecastHeaderID]) REFERENCES [dbo].[ForecastHeaders] ([ForecastHeaderID])
);


GO
GRANT SELECT
    ON OBJECT::[dbo].[ForecastDetails] TO [EnabillReportRole]
    AS [dbo];

