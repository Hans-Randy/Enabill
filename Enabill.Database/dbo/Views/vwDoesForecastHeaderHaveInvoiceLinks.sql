

CREATE VIEW [dbo].[vwDoesForecastHeaderHaveInvoiceLinks]
AS

SELECT DISTINCT ForecastHeaderID, period, 'true' AS HasInvoicesLinked FROM dbo.vwForecastWithInvoicesDetails
UNION
SELECT DISTINCT ForecastHeaderID, period, 'false' AS HasInvoicesLinked FROM dbo.vwForecastWithoutInvoices
WHERE ForecastHeaderID NOT IN
(SELECT ForecastHeaderID FROM dbo.vwForecastWithInvoicesDetails)
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwDoesForecastHeaderHaveInvoiceLinks] TO [EnabillReportRole]
    AS [dbo];

