CREATE TABLE [dbo].[NonWorkSessions] (
    [NonWorkSessionID] INT           IDENTITY (1, 1) NOT NULL,
    [UserID]           INT           NOT NULL,
    [Date]             DATE          NOT NULL,
    [LastModifiedBy]   VARCHAR (128) NOT NULL,
    CONSTRAINT [PK_NonWorkSessions] PRIMARY KEY CLUSTERED ([NonWorkSessionID] ASC)
);

