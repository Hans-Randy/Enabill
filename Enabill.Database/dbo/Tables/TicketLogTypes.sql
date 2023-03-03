CREATE TABLE [dbo].[TicketLogTypes] (
    [TicketLogTypeID]   INT          NOT NULL,
    [TicketLogTypeName] VARCHAR (50) NOT NULL,
    [SourceTable]       VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_TicketLogTypes] PRIMARY KEY CLUSTERED ([TicketLogTypeID] ASC)
);

