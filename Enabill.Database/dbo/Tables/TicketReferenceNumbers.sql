CREATE TABLE [dbo].[TicketReferenceNumbers] (
    [TicketReferenceNumberID] INT           IDENTITY (1, 1) NOT NULL,
    [TicketSubject]           VARCHAR (512) NOT NULL,
    CONSTRAINT [PK_TicketReferenceNumbers] PRIMARY KEY CLUSTERED ([TicketReferenceNumberID] ASC)
);

