﻿CREATE TABLE [dbo].[FlexiDays] (
    [FlexiDayID]       INT           IDENTITY (1, 1) NOT NULL,
    [UserID]           INT           NOT NULL,
    [FlexiDate]        DATETIME      NOT NULL,
    [DateSubmitted]    DATETIME      NOT NULL,
    [LastModifiedBy]   VARCHAR (128) NOT NULL,
    [Remark]           VARCHAR (200) NULL,
    [ApprovalStatusID] INT           DEFAULT ((4)) NOT NULL,
    CONSTRAINT [PK_FlexiDays] PRIMARY KEY CLUSTERED ([FlexiDayID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_UserID_Include]
    ON [dbo].[FlexiDays]([UserID] ASC)
    INCLUDE([FlexiDate], [Remark], [ApprovalStatusID]);


GO
-- =============================================
-- Author:		Gavin van Gent
-- Create date: 2011-08-18
-- Description:	When a modification or delete is made on this table,
--				we want to archive the original record to an audit log,
--				and then save the changes
-- =============================================
CREATE TRIGGER [dbo].[ArchiveFlexiDayTrigger]
   ON  dbo.FlexiDays
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
					@ContextID = FlexiDayID,
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
							@ContextID = FlexiDayID,
							@OldData = NULL,
							@NewData = (SELECT * FROM INSERTED FOR XML AUTO)
					FROM INSERTED
					
				END
			ELSE
				BEGIN
					-- This is an Update Record Action
					SELECT
							@ArchivedBy = LastModifiedBy,
							@ContextID = FlexiDayID,
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
					'FlexiDays',
					@ContextID,
					CASE WHEN @Action = 'I' THEN 'INSERT' ELSE CASE WHEN @Action = 'U' THEN 'UPDATE' ELSE 'DELETE' END END,
					@OldData,
					@NewData
END