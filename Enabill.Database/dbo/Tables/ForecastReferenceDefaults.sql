CREATE TABLE [dbo].[ForecastReferenceDefaults] (
    [ForecastReferenceDefaultID] INT           IDENTITY (1, 1) NOT NULL,
    [Reference]                  VARCHAR (250) NOT NULL,
    [EffectiveDate]              DATETIME      NOT NULL,
    [ModifiedByUserID]           INT           NOT NULL,
    CONSTRAINT [PK_ForecastReferenceDefaults] PRIMARY KEY CLUSTERED ([ForecastReferenceDefaultID] ASC)
);


GO
GRANT SELECT
    ON OBJECT::[dbo].[ForecastReferenceDefaults] TO [EnabillReportRole]
    AS [dbo];

