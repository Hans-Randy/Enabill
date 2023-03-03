CREATE TABLE [dbo].[TicketAssignmentChanges] (
    [TicketAssignmentChangeID] INT            IDENTITY (1, 1) NOT NULL,
    [TicketID]                 INT            NOT NULL,
    [FromUser]                 INT            NULL,
    [ToUser]                   INT            NOT NULL,
    [Remark]                   VARCHAR (2000) NOT NULL,
    [DateCreated]              DATETIME       NOT NULL,
    [UserID]                   INT            NOT NULL,
    CONSTRAINT [PK_TicketAssignmentChanges] PRIMARY KEY CLUSTERED ([TicketAssignmentChangeID] ASC),
    CONSTRAINT [FK_TicketAssignmentChanges_FromUser_Users] FOREIGN KEY ([FromUser]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_TicketAssignmentChanges_Tickets] FOREIGN KEY ([TicketID]) REFERENCES [dbo].[Tickets] ([TicketID]),
    CONSTRAINT [FK_TicketAssignmentChanges_ToUser_Users] FOREIGN KEY ([ToUser]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_TicketAssignmentChanges_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);

