CREATE TABLE [dbo].[WorkAllocations] (
	[WorkAllocationID]       INT           IDENTITY (1, 1) NOT NULL,
	[WorkAllocationType]     INT           NOT NULL,
	[UserID]                 INT           NOT NULL,
	[ActivityID]             INT           NOT NULL,
	[DayWorked]              DATETIME      NOT NULL,
	[Period]                 INT           NOT NULL,
	[HoursWorked]            FLOAT (53)    NOT NULL,
	[HoursBilled]            FLOAT (53)    NULL,
	[Remark]                 VARCHAR (200) NULL,
	[UserCreated]            VARCHAR (128) NULL,
	[DateCreated]            DATETIME      NOT NULL,
	[UserModified]           VARCHAR (128) NULL,
	[DateModified]           DATETIME      NULL,
	[ParentWorkAllocationID] INT           NULL,
	[LastModifiedBy]         VARCHAR (128) NOT NULL,
	[InvoiceID]              INT           NULL,
	[HourlyRate]             FLOAT (53)    NULL,
	[TrainingCategoryID]     INT           DEFAULT ((0)) NOT NULL,
	[TrainerName]            VARCHAR (100) NULL,
	[TrainingInstitute]      VARCHAR (250) NULL,
	[TicketReference]        VARCHAR (20)  NULL,
	CONSTRAINT [PK_WorkAllocation] PRIMARY KEY CLUSTERED ([WorkAllocationID] ASC) WITH (FILLFACTOR = 95),
	CONSTRAINT [FK_WorkAllocation_Activity] FOREIGN KEY ([ActivityID]) REFERENCES [dbo].[Activities] ([ActivityID]),
	CONSTRAINT [FK_WorkAllocation_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID]),
	CONSTRAINT [FK_WorkAllocations_Invoices] FOREIGN KEY ([InvoiceID]) REFERENCES [dbo].[Invoices] ([InvoiceID]),
	CONSTRAINT [FK_WorkAllocations_TrainingCategory] FOREIGN KEY ([TrainingCategoryID]) REFERENCES [dbo].[TrainingCategories] ([TrainingCategoryID])
);




GO
CREATE NONCLUSTERED INDEX [ix_WorkAllocations_UserID_ActivityID_DayWorked_includes]
	ON [dbo].[WorkAllocations]([UserID] ASC, [ActivityID] ASC, [DayWorked] ASC)
	INCLUDE([WorkAllocationID], [WorkAllocationType], [Period], [HoursWorked], [HoursBilled], [Remark], [UserCreated], [DateCreated], [UserModified], [DateModified], [ParentWorkAllocationID], [LastModifiedBy], [InvoiceID], [HourlyRate], [TrainingCategoryID], [TrainerName], [TrainingInstitute], [TicketReference]) WITH (FILLFACTOR = 80);


GO
CREATE NONCLUSTERED INDEX [IX_WorkAllocations_InvoiceID_ActivityID_UserID_Includes]
	ON [dbo].[WorkAllocations]([InvoiceID] ASC, [ActivityID] ASC, [UserID] ASC)
	INCLUDE([WorkAllocationType], [DayWorked], [Period], [HoursWorked], [Remark]) WITH (FILLFACTOR = 90);


GO
-- =============================================
-- Author:		Gavin van Gent
-- Create date: 2011-08-18
-- Description:	When a modification or delete is made on this table,
--				we want to archive the original record to an audit log,
--				and then save the changes
-- =============================================
Create TRIGGER [dbo].[ArchiveWorkAllocationTrigger]
   ON  [dbo].[WorkAllocations]
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
					@ContextID = WorkAllocationID,
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
							@ContextID = WorkAllocationID,
							@OldData = NULL,
							@NewData = (SELECT * FROM INSERTED FOR XML AUTO)
					FROM INSERTED
					
				END
			ELSE
				BEGIN
					-- This is an Update Record Action
					SELECT
							@ArchivedBy = LastModifiedBy,
							@ContextID = WorkAllocationID,
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
					'WorkAllocations',
					@ContextID,
					CASE WHEN @Action = 'I' THEN 'INSERT' ELSE CASE WHEN @Action = 'U' THEN 'UPDATE' ELSE 'DELETE' END END,
					@OldData,
					@NewData
END