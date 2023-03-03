

-- =============================================
-- Author:		Gavin van Gent
-- Create date: 2012-01-18
-- Description:	Decrypts a passphrase to return the protected value encrypted with the passphrase
-- =============================================
CREATE PROCEDURE [dbo].[DeCryptDecSP] 
	@Phrase NVARCHAR(128),
	@Value VARBINARY(128)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT CONVERT(VARCHAR(128), CONVERT(dec(18,2), DecryptByPassphrase(@Phrase, @Value)))
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[DeCryptDecSP] TO PUBLIC
    AS [dbo];

