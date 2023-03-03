CREATE TABLE [dbo].[ForecastResourceAssignments] (
    [ForecastResourceAssignmentID] INT           IDENTITY (1, 1) NOT NULL,
    [ForecastDetailID]             INT           NOT NULL,
    [Resource]                     VARCHAR (512) NOT NULL,
    [UserID]                       INT           NULL,
    [PercentageAllocation]         FLOAT (53)    NOT NULL,
    CONSTRAINT [PK_ForecastResourceAssignments] PRIMARY KEY CLUSTERED ([ForecastResourceAssignmentID] ASC),
    CONSTRAINT [FK_FCResourceAssignment_FCDetail] FOREIGN KEY ([ForecastDetailID]) REFERENCES [dbo].[ForecastDetails] ([ForecastDetailID])
);


GO
GRANT SELECT
    ON OBJECT::[dbo].[ForecastResourceAssignments] TO [EnabillReportRole]
    AS [dbo];

