CREATE TABLE [dbo].[ELMAH_Error] (
    [ErrorId]     UNIQUEIDENTIFIER CONSTRAINT [DF_ELMAH_Error_ErrorId] DEFAULT (newid()) NOT NULL,
    [Application] NVARCHAR (120)   NOT NULL,
    [Host]        NVARCHAR (100)   NOT NULL,
    [Type]        NVARCHAR (200)   NOT NULL,
    [Source]      NVARCHAR (120)   NOT NULL,
    [Message]     NVARCHAR (1000)  NOT NULL,
    [User]        NVARCHAR (100)   NOT NULL,
    [StatusCode]  INT              NOT NULL,
    [TimeUtc]     DATETIME         NOT NULL,
    [Sequence]    INT              IDENTITY (1, 1) NOT NULL,
    [AllXml]      NTEXT            NOT NULL,
    CONSTRAINT [PK_ELMAH_Error] PRIMARY KEY CLUSTERED ([ErrorId] ASC)
);

