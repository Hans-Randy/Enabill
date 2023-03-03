
CREATE VIEW [dbo].[vwCostToCompany] AS
SELECT CAST(UC.UserID as Varchar(5)) + '-' + CAST(UC.Period as varchar(6)) as UKey, UC.UserID, UC.Period,  isnull(dbo.DeCryptDec(PR.PassPhraseName,UC.CostToCompany),0) AS CTC 
FROM UserCostToCompanies UC, PassPhrases PR
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwCostToCompany] TO [EnabillReportRole]
    AS [dbo];

