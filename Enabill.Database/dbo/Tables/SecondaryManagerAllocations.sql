CREATE TABLE [dbo].[SecondaryManagerAllocations] (
    [StaffManagerAllocationID] INT IDENTITY (1, 1) NOT NULL,
    [ManagerID]                INT NOT NULL,
    [UserID]                   INT NOT NULL,
    CONSTRAINT [PK_StaffManagerAllocations] PRIMARY KEY CLUSTERED ([StaffManagerAllocationID] ASC),
    CONSTRAINT [FK_StaffManagerAllocations_Users] FOREIGN KEY ([ManagerID]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [FK_StaffManagerAllocations_Users1] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [IX_StaffManagerAllocations] UNIQUE NONCLUSTERED ([ManagerID] ASC, [UserID] ASC)
);

