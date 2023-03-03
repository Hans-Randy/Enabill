

-- =============================================
-- Author:		Gavin van Gent
-- Create date: 2012-01-18
-- Description:	Encrypts a decimal value with a passphrase to return a varbinary value
-- =============================================
CREATE PROCEDURE [dbo].[EnCryptDecSP] 
	-- Add the parameters for the stored procedure here
	@Phrase NVARCHAR(128), 
	@Value DECIMAL(18, 2)
AS
BEGIN	
	SET NOCOUNT ON;

    SELECT CONVERT(VARBINARY(128), EncryptByPassphrase(@Phrase, CONVERT(VARBINARY(128), @Value)))
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[EnCryptDecSP] TO PUBLIC
    AS [dbo];

