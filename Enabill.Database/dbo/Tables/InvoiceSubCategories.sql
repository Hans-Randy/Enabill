CREATE TABLE [dbo].[InvoiceSubCategories] (
    [InvoiceSubCategoryID] INT           IDENTITY (1, 1) NOT NULL,
    [InvoiceCategoryID]    INT           NOT NULL,
    [SubCategoryName]      VARCHAR (100) NOT NULL,
    [RefCode]              VARCHAR (10)  NULL,
    CONSTRAINT [PK_InvoiceSubCategories] PRIMARY KEY CLUSTERED ([InvoiceSubCategoryID] ASC),
    CONSTRAINT [FK_InvoiceSubCategories_InvoiceCategories] FOREIGN KEY ([InvoiceCategoryID]) REFERENCES [dbo].[InvoiceCategories] ([InvoiceCategoryID])
);

