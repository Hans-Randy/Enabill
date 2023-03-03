CREATE TABLE [dbo].[Clients] (
	[ClientID]            INT           IDENTITY (1, 1) NOT NULL,
	[ClientName]          VARCHAR (100) NOT NULL,
	[IsActive]            BIT           CONSTRAINT [DF_Clients_IsActive] DEFAULT ((0)) NOT NULL,
	[RegisteredName]      VARCHAR (100) NULL,
	[VATNo]               VARCHAR (50)  NULL,
	[VATRate]             FLOAT (53)    NULL,
	[PostalAddress1]      VARCHAR (250) NULL,
	[PostalAddress2]      VARCHAR (250) NULL,
	[PostalAddress3]      VARCHAR (250) NULL,
	[PostalCode]          VARCHAR (4)   NULL,
	[LastModifiedBy]      VARCHAR (128) CONSTRAINT [DF_Clients_LastModifiedBy] DEFAULT ('Gavin van Gent') NOT NULL,
	[AccountCode]         VARCHAR (50)  NULL,
	[SupportEmailAddress] VARCHAR (128) NULL,
	[CurrencyTypeID]	  INT NOT NULL,
	CONSTRAINT [PK_Client] PRIMARY KEY CLUSTERED ([ClientID] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_Clients_ClientName_Include]
	ON [dbo].[Clients]([ClientName] ASC)
	INCLUDE([IsActive], [AccountCode]);


GO
-- =============================================
-- Author:		Gavin van Gent
-- Create date: 2011-08-16
-- Description:	When a modification or delete is made on this table,
--				we want to archive the original record to an audit log,
--				and then save the changes
-- =============================================
CREATE TRIGGER [dbo].[ArchiveClientTrigger]
   ON  [dbo].[Clients]
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
					@ArchivedBy = LastModifiedBy,
					@ContextID = ClientID,
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
							@ContextID = ClientID,
							@OldData = NULL,
							@NewData = (SELECT * FROM INSERTED FOR XML AUTO)
					FROM INSERTED
					
				END
			ELSE
				BEGIN
					-- This is an Update Record Action
					SELECT
							@ArchivedBy = LastModifiedBy,
							@ContextID = ClientID,
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
					'Clients',
					@ContextID,
					CASE WHEN @Action = 'I' THEN 'INSERT' ELSE CASE WHEN @Action = 'U' THEN 'UPDATE' ELSE 'DELETE' END END,
					@OldData,
					@NewData
END