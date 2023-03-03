CREATE TABLE [dbo].[TicketLines] (
    [TicketLineID]           INT            IDENTITY (1, 1) NOT NULL,
    [TicketID]               INT            NOT NULL,
    [TicketLineSourceTypeID] INT            NOT NULL,
    [FromAddress]            VARCHAR (128)  NOT NULL,
    [ToAddress]              VARCHAR (5000) NOT NULL,
    [Body]                   VARCHAR (MAX)  NOT NULL,
    [UserCreated]            INT            NOT NULL,
    [DateCreated]            DATETIME       NOT NULL,
    CONSTRAINT [PK_TicketLines] PRIMARY KEY CLUSTERED ([TicketLineID] ASC),
    CONSTRAINT [FK_TicketLines_TicketLineSourceTypes] FOREIGN KEY ([TicketLineSourceTypeID]) REFERENCES [dbo].[TicketLineSourceTypes] ([TicketLineSourceTypeID]),
    CONSTRAINT [FK_TicketLines_Tickets] FOREIGN KEY ([TicketID]) REFERENCES [dbo].[Tickets] ([TicketID]),
    CONSTRAINT [FK_TicketLines_UserCreated_Users] FOREIGN KEY ([UserCreated]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TicketLines_Ticket]
    ON [dbo].[TicketLines]([TicketID] ASC);

