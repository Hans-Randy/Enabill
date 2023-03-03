CREATE TABLE [dbo].[TicketPriorities] (
    [TicketPriorityID]   INT          NOT NULL,
    [TicketPriorityName] VARCHAR (50) NULL,
    CONSTRAINT [PK_TicketPriorities] PRIMARY KEY CLUSTERED ([TicketPriorityID] ASC)
);

