CREATE TABLE [dbo].[Projects] (
	[ProjectID]           INT           IDENTITY (1, 1) NOT NULL,
	[ClientID]            INT           NOT NULL,
	[ProjectName]         VARCHAR (50)  NOT NULL,
	[ProjectCode]         VARCHAR (15)  NULL,
	[ProjectDesc]         VARCHAR (MAX) NULL,
	[GroupCode]           VARCHAR (50)  NULL,
	[ProjectOwnerID]      INT           NOT NULL,
	[InvoiceAdminID]      INT           NOT NULL,
	[RegionID]            INT           CONSTRAINT [DF_Projects_RegionID] DEFAULT ((1)) NOT NULL,
	[DepartmentID]        INT           CONSTRAINT [DF_Projects_DivisonID] DEFAULT ((1)) NOT NULL,
	[BillingMethodID]     INT           CONSTRAINT [DF_Projects_BillingMethodID] DEFAULT ((1)) NOT NULL,
	[BookInAdvance]       BIT           CONSTRAINT [DF_Projects_BookInAdvance] DEFAULT ((0)) NOT NULL,
	[CanHaveNotes]        BIT           CONSTRAINT [DF_Projects_CanHaveNotes] DEFAULT ((1)) NOT NULL,
	[MustHaveRemarks]     BIT           CONSTRAINT [DF_Projects_MustHaveRemarks] DEFAULT ((0)) NOT NULL,
	[ProjectValue]        FLOAT (53)    NULL,
	[StartDate]           DATETIME      NOT NULL,
	[ScheduledEndDate]    DATETIME      NULL,
	[ConfirmedEndDate]    DATETIME      NULL,
	[DeactivatedDate]     DATETIME      NULL,
	[DeactivatedBy]       VARCHAR (100) NULL,
	[LastModifiedBy]      VARCHAR (128) NOT NULL,
	[SupportEmailAddress] VARCHAR (128) NULL,
	CONSTRAINT [PK_Project] PRIMARY KEY CLUSTERED ([ProjectID] ASC),
	CONSTRAINT [FK_Project_Client] FOREIGN KEY ([ClientID]) REFERENCES [dbo].[Clients] ([ClientID]) NOT FOR REPLICATION,
	CONSTRAINT [FK_Projects_BillingMethods] FOREIGN KEY ([BillingMethodID]) REFERENCES [dbo].[BillingMethods] ([BillingMethodID]),
	CONSTRAINT [FK_Projects_Departments] FOREIGN KEY ([DepartmentID]) REFERENCES [dbo].[Departments] ([DepartmentID])
);


GO
ALTER TABLE [dbo].[Projects] NOCHECK CONSTRAINT [FK_Project_Client];




GO
ALTER TABLE [dbo].[Projects] NOCHECK CONSTRAINT [FK_Project_Client];


GO
CREATE NONCLUSTERED INDEX [IX_Projects_ClientID_Include]
	ON [dbo].[Projects]([ClientID] ASC)
	INCLUDE([ScheduledEndDate], [ProjectName]);


GO
-- =============================================
-- Author:		Gavin van Gent
-- Create date: 2011-08-18
-- Description:	When a modification or delete is made on this table,
--				we want to archive the original record to an audit log,
--				and then save the changes
-- =============================================
CREATE TRIGGER [dbo].[ArchiveProjectTrigger]
   ON  [dbo].[Projects]
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
					@ContextID = ProjectID,
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
							@ContextID = ProjectID,
							@OldData = NULL,
							@NewData = (SELECT * FROM INSERTED FOR XML AUTO)
					FROM INSERTED
					
				END
			ELSE
				BEGIN
					-- This is an Update Record Action
					SELECT
							@ArchivedBy = LastModifiedBy,
							@ContextID = ProjectID,
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
					'Projects',
					@ContextID,
					CASE WHEN @Action = 'I' THEN 'INSERT' ELSE CASE WHEN @Action = 'U' THEN 'UPDATE' ELSE 'DELETE' END END,
					@OldData,
					@NewData
END