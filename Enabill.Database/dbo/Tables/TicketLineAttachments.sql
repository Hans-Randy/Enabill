CREATE TABLE [dbo].[TicketLineAttachments] (
    [TicketLineAttachmentID] INT             IDENTITY (1, 1) NOT NULL,
    [TicketLineID]           INT             NOT NULL,
    [AttachmentName]         VARCHAR (512)   NOT NULL,
    [Attachment]             VARBINARY (MAX) NOT NULL,
    CONSTRAINT [PK_TicketLineAttachments] PRIMARY KEY CLUSTERED ([TicketLineAttachmentID] ASC),
    CONSTRAINT [FK_TicketLineAttachments_TicketLines] FOREIGN KEY ([TicketLineID]) REFERENCES [dbo].[TicketLines] ([TicketLineID])
);


GO
CREATE NONCLUSTERED INDEX [ix_TicketLineAttachments_TicketLineID_includes]
    ON [dbo].[TicketLineAttachments]([TicketLineID] ASC)
    INCLUDE([TicketLineAttachmentID], [AttachmentName], [Attachment]);

