CREATE TABLE [dbo].[TicketTypes] (
    [TicketTypeID]   INT          NOT NULL,
    [TicketTypeName] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_TicketTypes] PRIMARY KEY CLUSTERED ([TicketTypeID] ASC)
);

