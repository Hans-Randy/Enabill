CREATE TABLE [dbo].[InvoiceCategories] (
    [InvoiceCategoryID] INT          NOT NULL,
    [CategoryName]      VARCHAR (50) NOT NULL,
    [ExternalRef]       VARCHAR (10) NULL,
    CONSTRAINT [PK_InvoiceCategories] PRIMARY KEY CLUSTERED ([InvoiceCategoryID] ASC)
);

