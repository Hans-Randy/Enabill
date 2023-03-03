CREATE TABLE [dbo].[FeedbackTypes] (
    [FeedbackTypeID]   INT          NOT NULL,
    [FeedbackTypeName] VARCHAR (64) NOT NULL,
    CONSTRAINT [PK_FeedbackTypes] PRIMARY KEY CLUSTERED ([FeedbackTypeID] ASC)
);

