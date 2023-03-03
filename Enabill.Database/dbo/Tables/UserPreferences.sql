CREATE TABLE [dbo].[UserPreferences] (
    [UserPreferenceID]            INT        IDENTITY (1, 1) NOT NULL,
    [UserID]                      INT        NOT NULL,
    [DefaultWorkSessionStartTime] DATETIME   NULL,
    [DefaultWorkSessionEndTime]   DATETIME   NULL,
    [DefaultLunchDuration]        FLOAT (53) NOT NULL,
    [InvoiceIndexDateSelector]    INT        CONSTRAINT [DefaultValue] DEFAULT ((1)) NOT NULL,
    [CollapseMyTimesheet]         BIT        CONSTRAINT [DF_UserPreferences_CollapseMyTimesheet] DEFAULT ((0)) NOT NULL,
    [CollapseMyFlexTimeBalance]   BIT        CONSTRAINT [DF_UserPreferences_CollapseMyFlexTimeBalance] DEFAULT ((0)) NOT NULL,
    [CollapseMyLeaveBalance]      BIT        CONSTRAINT [DF_UserPreferences_CollapseMyLeaveBalance] DEFAULT ((0)) NOT NULL,
    [CollapseMyUpcomingLeave]     BIT        CONSTRAINT [DF_UserPreferences_CollapseMyUpcomingLeave] DEFAULT ((0)) NOT NULL,
    [DayView]                     BIT        CONSTRAINT [DF_UserPreferences_DayView] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_UserPreferences] PRIMARY KEY CLUSTERED ([UserPreferenceID] ASC),
    CONSTRAINT [FK_UserPreferences_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserID]
    ON [dbo].[UserPreferences]([UserID] ASC);

