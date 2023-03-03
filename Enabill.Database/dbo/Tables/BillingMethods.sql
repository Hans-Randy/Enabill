CREATE TABLE [dbo].[BillingMethods] (
    [BillingMethodID]   INT          NOT NULL,
    [BillingMethodName] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_BillingMethods] PRIMARY KEY CLUSTERED ([BillingMethodID] ASC)
);

