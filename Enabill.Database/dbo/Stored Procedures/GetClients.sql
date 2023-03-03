

-- =============================================
-- Author:		<Fredrik Erasmus>
-- Create date: <13/11/2015>
-- Description:	<Get Clients>
-- =============================================
CREATE PROCEDURE [dbo].[GetClients]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT
		C.AccountCode,
		C.ClientID,
		C.ClientName,
		C.IsActive,
		C.LastModifiedBy,
		C.PostalAddress1,
		C.PostalAddress2,
		C.PostalAddress3,
		C.PostalCode,
		C.RegisteredName,
		C.SupportEmailAddress,
		C.VATNo,
		C.VATRate,
		C.CurrencyTypeID
		
	FROM
		[dbo].[Clients] C
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetClients] TO [Enabill_User]
    AS [dbo];

