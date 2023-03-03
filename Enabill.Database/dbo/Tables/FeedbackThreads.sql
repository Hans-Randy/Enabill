CREATE TABLE [dbo].[FeedbackThreads] (
    [FeedbackThreadID]      INT            IDENTITY (1, 1) NOT NULL,
    [FeedbackTypeID]        INT            NOT NULL,
    [FeedbackUrgencyTypeID] INT            NOT NULL,
    [FeedbackSubject]       NVARCHAR (512) NOT NULL,
    [UserClosed]            INT            NULL,
    [DateClosed]            DATETIME       NULL,
    [Source]                VARCHAR (128)  NULL,
    [ClientID]              INT            NULL,
    [ProjectID]             INT            NULL,
    [AssignedUser]          INT            NULL,
    CONSTRAINT [PK_FeedbackThreads] PRIMARY KEY CLUSTERED ([FeedbackThreadID] ASC),
    CONSTRAINT [FK_FeedbackThreads_FeedbackTypes] FOREIGN KEY ([FeedbackTypeID]) REFERENCES [dbo].[FeedbackTypes] ([FeedbackTypeID]),
    CONSTRAINT [FK_FeedbackThreads_FeedbackUrgencyTypes] FOREIGN KEY ([FeedbackUrgencyTypeID]) REFERENCES [dbo].[FeedbackUrgencyTypes] ([FeedbackUrgencyTypeID])
);

