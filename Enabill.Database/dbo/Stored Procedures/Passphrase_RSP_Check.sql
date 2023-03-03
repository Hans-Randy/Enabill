CREATE PROC [dbo].[Passphrase_RSP_Check]
	@PassPhrase NVARCHAR(128)
AS
/*
Check if PassPhrase is Valid
*/
SET NOCOUNT ON 

SELECT TOP 1
	CostToCompany = dbo.DeCryptDec(@PassPhrase, CONVERT(VARBINARY(128), CostToCompany))
FROM
	UserCostToCompanies
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Passphrase_RSP_Check] TO PUBLIC
    AS [dbo];

