CREATE TABLE [dbo].[TicketStatusChanges] (
    [TicketStatusChangeID] INT            IDENTITY (1, 1) NOT NULL,
    [TicketID]             INT            NOT NULL,
    [FromStatus]           INT            NOT NULL,
    [ToStatus]             INT            NOT NULL,
    [Remark]               VARCHAR (2000) NOT NULL,
    [DateCreated]          DATETIME       NOT NULL,
    [UserID]               INT            NOT NULL,
    CONSTRAINT [PK_TicketStatusChanges] PRIMARY KEY CLUSTERED ([TicketStatusChangeID] ASC),
    CONSTRAINT [FK_TicketStatusChanges_FromStatus_TicketStatus] FOREIGN KEY ([FromStatus]) REFERENCES [dbo].[TicketStatus] ([TicketStatusID]),
    CONSTRAINT [FK_TicketStatusChanges_Tickets] FOREIGN KEY ([TicketID]) REFERENCES [dbo].[Tickets] ([TicketID]),
    CONSTRAINT [FK_TicketStatusChanges_ToStatus_TicketStatus] FOREIGN KEY ([ToStatus]) REFERENCES [dbo].[TicketStatus] ([TicketStatusID]),
    CONSTRAINT [FK_TicketStatusChanges_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);

