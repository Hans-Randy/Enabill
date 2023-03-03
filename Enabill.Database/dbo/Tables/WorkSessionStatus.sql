CREATE TABLE [dbo].[WorkSessionStatus] (
    [WorkSessionStatusID] INT          NOT NULL,
    [StatusName]          VARCHAR (15) NOT NULL,
    CONSTRAINT [PK_WorkSessionStatus] PRIMARY KEY CLUSTERED ([WorkSessionStatusID] ASC)
);

