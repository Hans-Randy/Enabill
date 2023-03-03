CREATE TABLE [dbo].[TicketFilters] (
    [TicketFilterID] INT           NOT NULL,
    [ProjectID]      INT           NOT NULL,
    [FromAddress]    VARCHAR (256) NULL,
    [ToAddress]      VARCHAR (256) NULL,
    [TicketSubject]  VARCHAR (512) NULL,
    CONSTRAINT [PK_TicketFilters] PRIMARY KEY CLUSTERED ([TicketFilterID] ASC),
    CONSTRAINT [FK_TicketFilters_Projects] FOREIGN KEY ([ProjectID]) REFERENCES [dbo].[Projects] ([ProjectID])
);

