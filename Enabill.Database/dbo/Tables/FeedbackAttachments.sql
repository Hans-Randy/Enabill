CREATE TABLE [dbo].[FeedbackAttachments] (
    [FeedbackAttachmentID] INT             IDENTITY (1, 1) NOT NULL,
    [FeedbackPostID]       INT             NOT NULL,
    [AttachmentName]       VARCHAR (512)   NOT NULL,
    [Attachment]           VARBINARY (MAX) NOT NULL,
    CONSTRAINT [PK_FeedbackAttachments] PRIMARY KEY CLUSTERED ([FeedbackAttachmentID] ASC),
    CONSTRAINT [FK_FeedbackAttachments_FeedbackPosts] FOREIGN KEY ([FeedbackPostID]) REFERENCES [dbo].[FeedbackPosts] ([FeedbackPostID])
);


GO
GRANT UPDATE
    ON OBJECT::[dbo].[FeedbackAttachments] TO PUBLIC
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[FeedbackAttachments] TO PUBLIC
    AS [dbo];


GO
GRANT INSERT
    ON OBJECT::[dbo].[FeedbackAttachments] TO PUBLIC
    AS [dbo];


GO
GRANT DELETE
    ON OBJECT::[dbo].[FeedbackAttachments] TO PUBLIC
    AS [dbo];

