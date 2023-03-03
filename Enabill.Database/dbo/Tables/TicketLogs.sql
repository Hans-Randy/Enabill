CREATE TABLE [dbo].[TicketLogs] (
    [TicketLogID]     INT      IDENTITY (1, 1) NOT NULL,
    [TicketID]        INT      NOT NULL,
    [TicketLogTypeID] INT      NOT NULL,
    [Source]          INT      NOT NULL,
    [DateCreated]     DATETIME NOT NULL,
    CONSTRAINT [PK_TicketLogs] PRIMARY KEY CLUSTERED ([TicketLogID] ASC),
    CONSTRAINT [FK_TicketLogs_TicketLogTypes] FOREIGN KEY ([TicketLogTypeID]) REFERENCES [dbo].[TicketLogTypes] ([TicketLogTypeID]),
    CONSTRAINT [FK_TicketLogs_Tickets] FOREIGN KEY ([TicketID]) REFERENCES [dbo].[Tickets] ([TicketID])
);

