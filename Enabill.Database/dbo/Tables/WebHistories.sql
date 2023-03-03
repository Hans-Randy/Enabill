CREATE TABLE [dbo].[WebHistories] (
    [WebHistoryID] INT           IDENTITY (1, 1) NOT NULL,
    [UserID]       INT           NULL,
    [RequestDate]  DATETIME      NOT NULL,
    [RequestUrl]   VARCHAR (512) NOT NULL,
    [UserAgent]    VARCHAR (512) NOT NULL,
    [IPAddress]    VARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_WebHistories] PRIMARY KEY CLUSTERED ([WebHistoryID] ASC)
);

