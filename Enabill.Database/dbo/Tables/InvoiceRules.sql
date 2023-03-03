CREATE TABLE [dbo].[InvoiceRules] (
	[InvoiceRuleID]             INT           IDENTITY (1, 1) NOT NULL,
	[InvoiceSubCategoryID]      INT           NOT NULL,
	[BillingMethodID]           INT           NOT NULL,
	[ClientID]                  INT           NOT NULL,
	[ProjectID]                 INT           NULL,
	[DefaultContactID]          INT           NOT NULL,
	[OrderNo]                   VARCHAR (50)  NULL,
	[DateFrom]                  DATETIME      NOT NULL,
	[DateTo]                    DATETIME      NULL,
	[LastInvoiceDate]           DATETIME      NULL,
	[LastInvoiceID]             INT           NULL,
	[LastWorkingDay]            INT           NULL,
	[InvoiceDay]                INT           NULL,
	[ShowHoursOnInvoice]        BIT           NOT NULL,
	[HoursPaidFor]              INT           NULL,
	[AccrualPeriods]            INT           NULL,
	[InvoiceAdditionalHours]    BIT           CONSTRAINT [DF_InvoiceRules_InvoiceAdditionalHours] DEFAULT ((0)) NOT NULL,
	[InvoiceAmountExclVAT]      FLOAT (53)    NULL,
	[UserCreated]               VARCHAR (50)  NOT NULL,
	[DateCreated]               DATETIME      CONSTRAINT [DF_InvoiceRule_DateCreated] DEFAULT (getdate()) NOT NULL,
	[ConfirmedEndDate]          DATETIME      NULL,
	[LastModifiedBy]            VARCHAR (128) NOT NULL,
	[PrintOptionTypeID]         INT           NULL,
	[PrintCredits]              BIT           DEFAULT ((0)) NOT NULL,
	[Description]               VARCHAR (500) NULL,
	[PrintLayoutTypeID]         INT           NULL,
	[PrintTimeSheet]            BIT           DEFAULT ((1)) NOT NULL,
	[PrintTicketRemarkOptionID] INT           DEFAULT ((1)) NOT NULL,
	CONSTRAINT [PK_InvoiceRules] PRIMARY KEY CLUSTERED ([InvoiceRuleID] ASC),
	CONSTRAINT [FK_InvoiceRule_BillingMethods] FOREIGN KEY ([BillingMethodID]) REFERENCES [dbo].[BillingMethods] ([BillingMethodID]),
	CONSTRAINT [FK_InvoiceRule_Clients] FOREIGN KEY ([ClientID]) REFERENCES [dbo].[Clients] ([ClientID]),
	CONSTRAINT [FK_InvoiceRule_InvoiceSubCategories] FOREIGN KEY ([InvoiceSubCategoryID]) REFERENCES [dbo].[InvoiceSubCategories] ([InvoiceSubCategoryID]),
	CONSTRAINT [FK_InvoiceRules_Contacts] FOREIGN KEY ([DefaultContactID]) REFERENCES [dbo].[Contacts] ([ContactID]),
	CONSTRAINT [FK_InvoiceRules_Projects] FOREIGN KEY ([ProjectID]) REFERENCES [dbo].[Projects] ([ProjectID])
);




GO
CREATE NONCLUSTERED INDEX [IX_ClientID]
	ON [dbo].[InvoiceRules]([ClientID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_InvoiceRules_ProjectID_Include]
	ON [dbo].[InvoiceRules]([ProjectID] ASC)
	INCLUDE([BillingMethodID], [ClientID]);


GO
-- =============================================
-- Author:		Gavin van Gent
-- Create date: 2011-08-18
-- Description:	When a modification or delete is made on this table,
--				we want to archive the original record to an audit log,
--				and then save the changes
-- =============================================
Create TRIGGER [dbo].[ArchiveInvoiceRuleTrigger]
   ON  [dbo].[InvoiceRules]
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
					@ContextID = InvoiceRuleID,
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
							@ContextID = InvoiceRuleID,
							@OldData = NULL,
							@NewData = (SELECT * FROM INSERTED FOR XML AUTO)
					FROM INSERTED
					
				END
			ELSE
				BEGIN
					-- This is an Update Record Action
					SELECT
							@ArchivedBy = LastModifiedBy,
							@ContextID = InvoiceRuleID,
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
					'InvoiceRules',
					@ContextID,
					CASE WHEN @Action = 'I' THEN 'INSERT' ELSE CASE WHEN @Action = 'U' THEN 'UPDATE' ELSE 'DELETE' END END,
					@OldData,
					@NewData
END