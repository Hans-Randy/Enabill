CREATE TABLE [dbo].[TicketStatus] (
    [TicketStatusID]   INT          NOT NULL,
    [TicketStatusName] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_TicketStatus] PRIMARY KEY CLUSTERED ([TicketStatusID] ASC)
);

