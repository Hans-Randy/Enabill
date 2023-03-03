CREATE TABLE [dbo].[ForecastHeaders] (
    [ForecastHeaderID]  INT           IDENTITY (1, 1) NOT NULL,
    [BillingMethod]     INT           NOT NULL,
    [Client]            VARCHAR (512) NULL,
    [Project]           VARCHAR (512) NULL,
    [Activity]          VARCHAR (512) NULL,
    [Resource]          VARCHAR (512) NULL,
    [Region]            VARCHAR (512) NULL,
    [Division]          VARCHAR (512) NULL,
    [InvoiceCategory]   VARCHAR (512) NULL,
    [ClientID]          INT           NULL,
    [ProjectID]         INT           NULL,
    [ActivityID]        INT           NULL,
    [UserID]            INT           NULL,
    [RegionID]          INT           NULL,
    [DivisionID]        INT           NULL,
    [InvoiceCategoryID] INT           NULL,
    [Remark]            VARCHAR (512) NULL,
    [Probability]       FLOAT (53)    NOT NULL,
    CONSTRAINT [PK_ForecastHeaders] PRIMARY KEY CLUSTERED ([ForecastHeaderID] ASC)
);


GO
GRANT SELECT
    ON OBJECT::[dbo].[ForecastHeaders] TO [EnabillReportRole]
    AS [dbo];

