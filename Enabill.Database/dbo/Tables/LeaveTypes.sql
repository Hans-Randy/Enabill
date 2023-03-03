CREATE TABLE [dbo].[LeaveTypes] (
    [LeaveTypeID]   INT          NOT NULL,
    [LeaveTypeName] VARCHAR (64) NOT NULL,
    CONSTRAINT [PK_LeaveTypes] PRIMARY KEY CLUSTERED ([LeaveTypeID] ASC)
);

