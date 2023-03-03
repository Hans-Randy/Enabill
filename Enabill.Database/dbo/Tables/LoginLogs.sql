CREATE TABLE [dbo].[LoginLogs] (
    [LoginLogID]       INT           IDENTITY (1, 1) NOT NULL,
    [UserName]         VARCHAR (128) NOT NULL,
    [ResolvedUserName] VARCHAR (128) NULL,
    [LoginDate]        DATETIME      NOT NULL,
    CONSTRAINT [PK_LoginLogs] PRIMARY KEY CLUSTERED ([LoginLogID] ASC)
);

