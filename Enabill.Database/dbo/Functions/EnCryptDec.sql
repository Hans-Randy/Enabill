
CREATE FUNCTION [dbo].[EnCryptDec]
(
	@Phrase nvarchar(128),
	@Value DECIMAL(18,2)
)
RETURNS VARBINARY(128)
AS
/*
Encrypt a Decimal value with a phrase
*/
BEGIN
	RETURN EncryptByPassphrase(@Phrase, CONVERT(VARBINARY(128), @Value))
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[EnCryptDec] TO PUBLIC
    AS [dbo];

