CREATE TABLE [dbo].[Users] (
    [UserID]                 INT              IDENTITY (1, 1) NOT NULL,
    [UserName]               VARCHAR (50)     NOT NULL,
    [Password]               VARCHAR (200)    NULL,
    [IsActive]               BIT              CONSTRAINT [DF_User_IsActive] DEFAULT ((0)) NOT NULL,
    [FirstName]              VARCHAR (50)     NOT NULL,
    [LastName]               VARCHAR (50)     NOT NULL,
    [Email]                  VARCHAR (128)    NOT NULL,
    [WorkHours]              FLOAT (53)       NOT NULL,
    [ManagerID]              INT              NOT NULL,
    [BillableIndicatorID]    INT              NOT NULL,
    [DivisionID]             INT              CONSTRAINT [DF_User_DivisionID] DEFAULT ((1)) NOT NULL,
    [EmploymentTypeID]       INT              CONSTRAINT [DF_User_EmploymentTypeID] DEFAULT ((1)) NOT NULL,
    [EmployStartDate]        DATETIME         NOT NULL,
    [RegionID]               INT              CONSTRAINT [DF_User_RegionID] DEFAULT ((1)) NOT NULL,
    [Phone]                  VARCHAR (15)     NULL,
    [CanLogin]               BIT              CONSTRAINT [DF_User_CanLogin] DEFAULT ((0)) NOT NULL,
    [MustResetPwd]           BIT              CONSTRAINT [DF_User_MustResetPwd] DEFAULT ((0)) NOT NULL,
    [ExternalRef]            VARCHAR (255)    NULL,
    [ForgottenPasswordToken] UNIQUEIDENTIFIER NULL,
    [LastModifiedBy]         VARCHAR (128)    NOT NULL,
    [FlexiBalanceTakeOn]     FLOAT (53)       CONSTRAINT [DF_Users_FlexiBalanceTakeOn] DEFAULT ((0)) NOT NULL,
    [AnnualLeaveTakeOn]      FLOAT (53)       CONSTRAINT [DF_Users_LeaveTakeOn] DEFAULT ((0)) NOT NULL,
    [PayrollRefNo]           VARCHAR (10)     NULL,
    [IsSystemUser]           BIT              DEFAULT ((0)) NOT NULL,
    [EmployEndDate]          DATETIME         NULL,
    [BirthDate]              DATE             NULL,
    [FullName]               AS               (([FirstName]+' ')+[LastName]),
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([UserID] ASC),
    CONSTRAINT [FK_User_Division] FOREIGN KEY ([DivisionID]) REFERENCES [dbo].[Divisions] ([DivisionID]),
    CONSTRAINT [FK_User_EmploymentType] FOREIGN KEY ([EmploymentTypeID]) REFERENCES [dbo].[EmploymentTypes] ([EmploymentTypeID]),
    CONSTRAINT [FK_User_Region] FOREIGN KEY ([RegionID]) REFERENCES [dbo].[Regions] ([RegionID]),
    CONSTRAINT [FK_Users_BillableIndicators] FOREIGN KEY ([BillableIndicatorID]) REFERENCES [dbo].[BillableIndicators] ([BillableIndicatorID])
);


GO
CREATE NONCLUSTERED INDEX [IX_RegionID]
    ON [dbo].[Users]([RegionID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_DivisionID]
    ON [dbo].[Users]([DivisionID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Users_UserName_Include]
    ON [dbo].[Users]([UserName] ASC)
    INCLUDE([WorkHours], [IsActive], [DivisionID]);


GO
-- =============================================
-- Author:		Gavin van Gent
-- Create date: 2011-08-18
-- Description:	When a modification or delete is made on this table,
--				we want to archive the original record to an audit log,
--				and then save the changes
-- =============================================
CREATE TRIGGER [dbo].[ArchiveUserTrigger]
   ON  [dbo].[Users]
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
					@ContextID = UserID,
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
							@ContextID = UserID,
							@OldData = NULL,
							@NewData = (SELECT * FROM INSERTED FOR XML AUTO)
					FROM INSERTED
					
				END
			ELSE
				BEGIN
					-- This is an Update Record Action
					SELECT
							@ArchivedBy = LastModifiedBy,
							@ContextID = UserID,
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
					'Users',
					@ContextID,
					CASE WHEN @Action = 'I' THEN 'INSERT' ELSE CASE WHEN @Action = 'U' THEN 'UPDATE' ELSE 'DELETE' END END,
					@OldData,
					@NewData
END
GO
GRANT SELECT
    ON OBJECT::[dbo].[Users] TO [EnabillReportRole]
    AS [dbo];

