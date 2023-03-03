CREATE TABLE [dbo].[LeaveCycleBalances] (
    [LeaveCycleBalanceID] INT        IDENTITY (1, 1) NOT NULL,
    [UserID]              INT        NOT NULL,
    [LeaveTypeID]         INT        NOT NULL,
    [StartDate]           DATETIME   NOT NULL,
    [EndDate]             DATETIME   NOT NULL,
    [OpeningBalance]      FLOAT (53) NOT NULL,
    [Taken]               FLOAT (53) NOT NULL,
    [ClosingBalance]      FLOAT (53) NOT NULL,
    [Active]              INT        NOT NULL,
    [LastUpdatedDate]     DATETIME   NOT NULL,
    [ManualAdjustment]    FLOAT (53) CONSTRAINT [DF_LeaveCycleBalances_ManualAdjustment] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_LeaveCycleBalance] PRIMARY KEY CLUSTERED ([LeaveCycleBalanceID] ASC),
    CONSTRAINT [FK_LeaveCycleBalances_LeaveTypes] FOREIGN KEY ([LeaveTypeID]) REFERENCES [dbo].[LeaveTypes] ([LeaveTypeID]),
    CONSTRAINT [FK_LeaveCycleBalances_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);

