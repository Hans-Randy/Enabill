CREATE TABLE [dbo].[FeedbackUrgencyTypes] (
    [FeedbackUrgencyTypeID]   INT          NOT NULL,
    [FeedbackUrgencyTypeName] VARCHAR (64) NOT NULL,
    CONSTRAINT [PK_FeedbackUrgencyTypes] PRIMARY KEY CLUSTERED ([FeedbackUrgencyTypeID] ASC)
);

