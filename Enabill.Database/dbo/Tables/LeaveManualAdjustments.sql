CREATE TABLE [dbo].[LeaveManualAdjustments] (
    [LeaveManualAdjustmentID] INT           IDENTITY (1, 1) NOT NULL,
    [EffectiveDate]           DATETIME      NOT NULL,
    [UserID]                  INT           NOT NULL,
    [LeaveTypeID]             INT           NOT NULL,
    [ManualAdjustment]        FLOAT (53)    NOT NULL,
    [Remark]                  VARCHAR (200) NOT NULL,
    CONSTRAINT [PK_LeaveAdjustments] PRIMARY KEY CLUSTERED ([LeaveManualAdjustmentID] ASC),
    CONSTRAINT [FK_LeaveManualAdjustments_LeaveTypes] FOREIGN KEY ([LeaveTypeID]) REFERENCES [dbo].[LeaveTypes] ([LeaveTypeID]),
    CONSTRAINT [FK_LeaveManualAdjustments_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);

