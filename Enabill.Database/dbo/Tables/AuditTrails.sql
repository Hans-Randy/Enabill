CREATE TABLE [dbo].[AuditTrails] (
    [AuditID]      INT           IDENTITY (1, 1) NOT NULL,
    [ArchivedBy]   VARCHAR (128) NOT NULL,
    [DateArchived] DATETIME      NOT NULL,
    [TableName]    VARCHAR (128) NOT NULL,
    [ContextID]    INT           NOT NULL,
    [Action]       VARCHAR (128) NOT NULL,
    [OldData]      XML           NULL,
    [NewData]      XML           NULL,
    CONSTRAINT [PK_AuditTrails] PRIMARY KEY CLUSTERED ([AuditID] ASC)
);

