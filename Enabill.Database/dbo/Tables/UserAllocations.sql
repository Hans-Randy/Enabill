CREATE TABLE [dbo].[UserAllocations] (
	[UserAllocationID] INT           IDENTITY (1, 1) NOT NULL,
	[UserID]           INT           NOT NULL,
	[ActivityID]       INT           NOT NULL,
	[ChargeRate]       FLOAT (53)    NOT NULL,
	[StartDate]        DATETIME      NOT NULL,
	[ScheduledEndDate] DATETIME      NULL,
	[ConfirmedEndDate] DATETIME      NULL,
	[LastModifiedBy]   VARCHAR (128) NOT NULL,
	CONSTRAINT [PK_UserAllocation] PRIMARY KEY CLUSTERED ([UserAllocationID] ASC),
	CONSTRAINT [FK_UserAllocation_Activity] FOREIGN KEY ([ActivityID]) REFERENCES [dbo].[Activities] ([ActivityID]),
	CONSTRAINT [FK_UserAllocation_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID]),
	CONSTRAINT [FK_UserAllocationArchive_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID])
);




GO
CREATE NONCLUSTERED INDEX [IX_ActivityID]
	ON [dbo].[UserAllocations]([ActivityID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_UserAllocations_UserID_Include]
	ON [dbo].[UserAllocations]([UserID] ASC)
	INCLUDE([ActivityID]);


GO
-- =============================================
-- Author:		Gavin van Gent
-- Create date: 2011-08-18
-- Description:	When a modification or delete is made on this table,
--				we want to archive the original record to an audit log,
--				and then save the changes
-- =============================================
Create TRIGGER [dbo].[ArchiveUserAllocationTrigger]
   ON  [dbo].[UserAllocations]
   FOR INSERT, UPDATE, DELETE
AS 
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @Action CHAR(1),
			@TransactionCount INT,
			@ArchivedBy VARCHAR(128),
			@ContextID INT,
			@OldData XML,
			@NewData XML
	
	-- Set Action to 'I'nsert by default. Also, determine how many actions are being done
	SELECT
				@Action = 'I',
				@TransactionCount = COUNT(*) FROM DELETED
					
	IF (@TransactionCount > 0)
		BEGIN
			-- Set Action to 'D'eleted.
			SET @Action = 'D'
			
			SELECT @TransactionCount = COUNT(*) FROM INSERTED
			
			IF (@TransactionCount > 0)
				-- Set Action to 'U'pdated.
				SET @Action = 'U'
		END
	
	
	IF (@Action = 'D')
		BEGIN
			-- This is a DELETE Record Action
			
			SELECT
					@ArchivedBy = 'Unknown',
					@ContextID = UserAllocationID,
					@OldData = (SELECT * FROM DELETED FOR XML AUTO),
					@NewData = NULL
			FROM DELETED
			
		END
	ELSE
		BEGIN
			-- Table INSERTED is common to both the INSERT, UPDATE trigger
			IF (@Action = 'I')
				BEGIN
					-- This is an Insert Record Action
					
					SELECT
							@ArchivedBy = LastModifiedBy,
							@ContextID = UserAllocationID,
							@OldData = NULL,
							@NewData = (SELECT * FROM INSERTED FOR XML AUTO)
					FROM INSERTED
					
				END
			ELSE
				BEGIN
					-- This is an Update Record Action
					SELECT
							@ArchivedBy = LastModifiedBy,
							@ContextID = UserAllocationID,
							@OldData = (SELECT * FROM DELETED FOR XML AUTO),
							@NewData = (SELECT * FROM INSERTED FOR XML AUTO)
					FROM DELETED
				END
		END
	
	INSERT INTO 
					AuditTrails
					(ArchivedBy, DateArchived, TableName, ContextID, [Action], OldData, NewData)
	SELECT
					@ArchivedBy,
					DATEADD(hh, 2, GETUTCDATE()),
					'UserAllocations',
					@ContextID,
					CASE WHEN @Action = 'I' THEN 'INSERT' ELSE CASE WHEN @Action = 'U' THEN 'UPDATE' ELSE 'DELETE' END END,
					@OldData,
					@NewData
END