CREATE TABLE [dbo].[Tickets] (
    [TicketID]        INT           IDENTITY (1, 1) NOT NULL,
    [TicketSubject]   VARCHAR (512) NOT NULL,
    [ClientID]        INT           NOT NULL,
    [ProjectID]       INT           CONSTRAINT [DF_Tickets_ProjectID] DEFAULT ((0)) NULL,
    [TicketStatus]    INT           NOT NULL,
    [FromAddress]     VARCHAR (128) NOT NULL,
    [DateCreated]     DATETIME      NOT NULL,
    [UserCreated]     INT           NOT NULL,
    [DateModified]    DATETIME      NULL,
    [UserModified]    INT           NULL,
    [UserAssigned]    INT           NULL,
    [TicketReference] VARCHAR (20)  NOT NULL,
    [TicketType]      INT           CONSTRAINT [DF_Tickets_TicketType] DEFAULT ((1)) NULL,
    [Priority]        INT           CONSTRAINT [DF_Tickets_Priority] DEFAULT ((0)) NULL,
    [Effort]          INT           CONSTRAINT [DF_Tickets_Effort] DEFAULT ((0)) NULL,
    [TimeSpent]       FLOAT (53)    CONSTRAINT [DF_Tickets_TimeSpent] DEFAULT ((0)) NULL,
    [IsDeleted]       BIT           CONSTRAINT [DF_Tickets_IsDeleted] DEFAULT ((0)) NOT NULL,
    [TicketDetails]   VARCHAR (512) NULL,
    CONSTRAINT [PK_Tickets] PRIMARY KEY CLUSTERED ([TicketID] ASC),
    CONSTRAINT [FK_Tickets_TicketStatus] FOREIGN KEY ([TicketStatus]) REFERENCES [dbo].[TicketStatus] ([TicketStatusID]),
    CONSTRAINT [FK_Tickets_UserAssigned_Users] FOREIGN KEY ([UserAssigned]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_Tickets_UserCreated_Users] FOREIGN KEY ([UserCreated]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_Tickets_UserModified_Users] FOREIGN KEY ([UserModified]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_UserAssigned]
    ON [dbo].[Tickets]([UserAssigned] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TicketStatus]
    ON [dbo].[Tickets]([TicketStatus] ASC);

