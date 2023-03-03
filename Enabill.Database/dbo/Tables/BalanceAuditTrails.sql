CREATE TABLE [dbo].[BalanceAuditTrails] (
    [BalanceAuditTrailID] INT           IDENTITY (1, 1) NOT NULL,
    [UserID]              INT           NOT NULL,
    [BalanceTypeID]       INT           NOT NULL,
    [BalanceDate]         DATETIME      NOT NULL,
    [BalanceChangeTypeID] INT           NOT NULL,
    [BalanceBefore]       FLOAT (53)    NOT NULL,
    [HoursChanged]        FLOAT (53)    NOT NULL,
    [BalanceAfter]        FLOAT (53)    NOT NULL,
    [ChangedBy]           INT           NOT NULL,
    [ChangeSummary]       VARCHAR (250) NOT NULL,
    [DateChanged]         DATETIME      NOT NULL,
    CONSTRAINT [PK_BalanceAuditTrail] PRIMARY KEY CLUSTERED ([BalanceAuditTrailID] ASC),
    CONSTRAINT [FK_ChangedBy] FOREIGN KEY ([ChangedBy]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_UserID] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);

