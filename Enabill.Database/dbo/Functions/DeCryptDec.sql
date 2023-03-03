CREATE FUNCTION [dbo].[DeCryptDec]
(
	@Phrase nvarchar(128),
	@Value VARBINARY(128)
)
RETURNS dec(18,2)
AS
/*
Decrypt a Decimal value with a phrase
*/
BEGIN
	RETURN CONVERT(dec(18,2), DecryptByPassphrase(@Phrase, @Value))
END