CREATE TABLE [dbo].[UserCostToCompanies] (
    [UserCostToCompanyID] INT             IDENTITY (1, 1) NOT NULL,
    [UserID]              INT             NOT NULL,
    [Period]              INT             NOT NULL,
    [CostToCompany]       VARBINARY (128) NOT NULL,
    [ModifiedByID]        INT             CONSTRAINT [DF__UserCostT__Modif__1B7E091A] DEFAULT ((-1)) NOT NULL,
    [ModifiedDate]        DATETIME        CONSTRAINT [DF__UserCostT__Modif__1C722D53] DEFAULT (getdate()) NOT NULL,
    [ModifyReason]        VARCHAR (50)    CONSTRAINT [DF__UserCostT__Modif__1D66518C] DEFAULT ('') NULL,
    CONSTRAINT [PK_UserCostToCompanies] PRIMARY KEY CLUSTERED ([UserCostToCompanyID] ASC),
    CONSTRAINT [FK_UserCostToCompanies_Months] FOREIGN KEY ([Period]) REFERENCES [dbo].[Months] ([Period]),
    CONSTRAINT [FK_UserCostToCompanies_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID]),
    CONSTRAINT [IX_UserCostToCompanies] UNIQUE NONCLUSTERED ([UserID] ASC, [Period] ASC)
);


GO
GRANT SELECT
    ON OBJECT::[dbo].[UserCostToCompanies] TO [EnabillReportRole]
    AS [dbo];

