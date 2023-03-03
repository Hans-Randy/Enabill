CREATE TABLE [dbo].[Activities] (
	[ActivityID]      INT           IDENTITY (1, 1) NOT NULL,
	[ProjectID]       INT           NOT NULL,
	[ActivityName]    VARCHAR (50)  NOT NULL,
	[IsActive]        BIT           CONSTRAINT [DF_Activity_IsActive] DEFAULT ((0)) NOT NULL,
	[RegionID]        INT           CONSTRAINT [DF_Activities_RegionID] DEFAULT ((1)) NOT NULL,
	[DepartmentID]    INT           CONSTRAINT [DF_Activities_DepartmentID] DEFAULT ((1)) NOT NULL,
	[MustHaveRemarks] BIT           CONSTRAINT [DF_Activities_MustHaveRemarks] DEFAULT ((0)) NOT NULL,
	[CanHaveNotes]    BIT           CONSTRAINT [DF_Activities_CanHaveNotes] DEFAULT ((0)) NOT NULL,
	[LastModifiedBy]  VARCHAR (128) NOT NULL,
	[IsDefault]       BIT           CONSTRAINT [DF_Activities_IsDefault] DEFAULT ((0)) NOT NULL,
	CONSTRAINT [PK_Activity] PRIMARY KEY CLUSTERED ([ActivityID] ASC),
	CONSTRAINT [FK_Activities_Projects] FOREIGN KEY ([ProjectID]) REFERENCES [dbo].[Projects] ([ProjectID])
);




GO
CREATE NONCLUSTERED INDEX [IX_Activities_ProjectID_Include]
	ON [dbo].[Activities]([ProjectID] ASC)
	INCLUDE([ActivityName]);


GO
-- =============================================
-- Author:		Gavin van Gent
-- Create date: 2011-08-18
-- Description:	When a modification or delete is made on this table,
--				we want to archive the original record to an audit log,
--				and then save the changes
-- =============================================
CREATE TRIGGER [dbo].[ArchiveActivityTrigger]
   ON  [dbo].[Activities]
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
					@ContextID = ActivityID,
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
							@ContextID = ActivityID,
							@OldData = NULL,
							@NewData = (SELECT * FROM INSERTED FOR XML AUTO)
					FROM INSERTED
					
				END
			ELSE
				BEGIN
					-- This is an Update Record Action
					SELECT
							@ArchivedBy = LastModifiedBy,
							@ContextID = ActivityID,
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
					'Activities',
					@ContextID,
					CASE WHEN @Action = 'I' THEN 'INSERT' ELSE CASE WHEN @Action = 'U' THEN 'UPDATE' ELSE 'DELETE' END END,
					@OldData,
					@NewData
END