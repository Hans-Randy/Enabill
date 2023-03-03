CREATE TABLE [dbo].[UserHistories] (
    [UserHistoryID]   INT        IDENTITY (1, 1) NOT NULL,
    [UserID]          INT        NOT NULL,
    [WorkHoursPerDay] FLOAT (53) NOT NULL,
    [Period]          INT        NOT NULL,
    CONSTRAINT [PK_UserHistories] PRIMARY KEY CLUSTERED ([UserHistoryID] ASC),
    CONSTRAINT [FK_UserHistories_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [IX_UserHistories_UserPeriod] UNIQUE NONCLUSTERED ([UserID] ASC, [Period] ASC)
);

