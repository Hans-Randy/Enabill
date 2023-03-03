CREATE TABLE [dbo].[LeaveBalances] (
    [LeaveBalanceID]   INT        IDENTITY (1, 1) NOT NULL,
    [UserID]           INT        NOT NULL,
    [LeaveType]        INT        NOT NULL,
    [LeaveTaken]       FLOAT (53) NOT NULL,
    [LeaveCredited]    FLOAT (53) NOT NULL,
    [Balance]          FLOAT (53) NOT NULL,
    [BalanceDate]      DATETIME   NOT NULL,
    [ManualAdjustment] FLOAT (53) NULL,
    CONSTRAINT [PK_LeaveBalances] PRIMARY KEY CLUSTERED ([LeaveBalanceID] ASC),
    CONSTRAINT [FK_LeaveBalances_LeaveTypes] FOREIGN KEY ([LeaveType]) REFERENCES [dbo].[LeaveTypes] ([LeaveTypeID]),
    CONSTRAINT [FK_LeaveBalances_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE NONCLUSTERED INDEX [IX_LeaveBalances_UserID_LeaveType_BalanceDate_includes]
    ON [dbo].[LeaveBalances]([UserID] ASC, [LeaveType] ASC, [BalanceDate] ASC)
    INCLUDE([LeaveBalanceID], [LeaveTaken], [LeaveCredited], [Balance], [ManualAdjustment]) WITH (FILLFACTOR = 100);


GO
CREATE NONCLUSTERED INDEX [IX_LeaveType]
    ON [dbo].[LeaveBalances]([LeaveType] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_LeaveBalances_UserID_Include]
    ON [dbo].[LeaveBalances]([UserID] ASC)
    INCLUDE([LeaveType]);

