CREATE TABLE [dbo].[FeedbackPosts] (
    [FeedbackPostID]   INT           IDENTITY (1, 1) NOT NULL,
    [FeedbackThreadID] INT           NOT NULL,
    [UserID]           INT           NOT NULL,
    [DateAdded]        DATETIME      NOT NULL,
    [PostText]         VARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_FeedbackPosts] PRIMARY KEY CLUSTERED ([FeedbackPostID] ASC),
    CONSTRAINT [FK_FeedbackPosts_FeedbackThreads] FOREIGN KEY ([FeedbackThreadID]) REFERENCES [dbo].[FeedbackThreads] ([FeedbackThreadID]),
    CONSTRAINT [FK_FeedbackPosts_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_FeedbackThreadID]
    ON [dbo].[FeedbackPosts]([FeedbackThreadID] ASC);

