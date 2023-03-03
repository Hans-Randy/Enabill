CREATE TABLE [dbo].[Contacts] (
    [ContactID]      INT           IDENTITY (1, 1) NOT NULL,
    [ClientID]       INT           NOT NULL,
    [JobTitle]       VARCHAR (50)  NULL,
    [IsActive]       BIT           NOT NULL,
    [LastModifiedBy] VARCHAR (128) CONSTRAINT [DF_Contacts_LastModifiedBy] DEFAULT ('SetupDefault') NOT NULL,
    [ContactName]    VARCHAR (128) NULL,
    [Email]          VARCHAR (128) NULL,
    [TelephoneNo]    VARCHAR (128) NULL,
    [CellphoneNo]    VARCHAR (128) NULL,
    CONSTRAINT [PK_Contacts] PRIMARY KEY CLUSTERED ([ContactID] ASC),
    CONSTRAINT [FK_Contacts_Clients] FOREIGN KEY ([ClientID]) REFERENCES [dbo].[Clients] ([ClientID])
);


GO

CREATE TRIGGER [dbo].[ArchiveContactTrigger]
   ON  [dbo].[Contacts]
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
END