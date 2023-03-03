CREATE TABLE [dbo].[ForecastInvoiceLinks] (
    [ForecastInvoiceLinkID] INT IDENTITY (1, 1) NOT NULL,
    [ForecastHeaderID]      INT NOT NULL,
    [ForecastDetailID]      INT NOT NULL,
    [InvoiceID]             INT NOT NULL,
    CONSTRAINT [PK_ForecastInvoiceLinks] PRIMARY KEY CLUSTERED ([ForecastInvoiceLinkID] ASC),
    CONSTRAINT [FK_FCInvoiceLink_FCDetail] FOREIGN KEY ([ForecastDetailID]) REFERENCES [dbo].[ForecastDetails] ([ForecastDetailID]),
    CONSTRAINT [FK_FCInvoiceLink_FCHeader] FOREIGN KEY ([ForecastHeaderID]) REFERENCES [dbo].[ForecastHeaders] ([ForecastHeaderID]),
    CONSTRAINT [FK_FCInvoiceLink_Invoice] FOREIGN KEY ([InvoiceID]) REFERENCES [dbo].[Invoices] ([InvoiceID])
);


GO
CREATE NONCLUSTERED INDEX [IX_InvoiceID]
    ON [dbo].[ForecastInvoiceLinks]([InvoiceID] ASC);


GO
GRANT SELECT
    ON OBJECT::[dbo].[ForecastInvoiceLinks] TO [EnabillReportRole]
    AS [dbo];

