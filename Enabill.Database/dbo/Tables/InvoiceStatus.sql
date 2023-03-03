CREATE TABLE [dbo].[InvoiceStatus] (
    [InvoiceStatusID] INT          NOT NULL,
    [StatusName]      VARCHAR (15) NOT NULL,
    CONSTRAINT [PK_InvoiceStatuses] PRIMARY KEY CLUSTERED ([InvoiceStatusID] ASC)
);

