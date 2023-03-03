CREATE TABLE [dbo].[TicketLineSourceTypes] (
    [TicketLineSourceTypeID]   INT          NOT NULL,
    [TicketLineSourceTypeName] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_TicketLineSourceTypes] PRIMARY KEY CLUSTERED ([TicketLineSourceTypeID] ASC)
);

