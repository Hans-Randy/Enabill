

-- =============================================
-- Author:		<Tulisa Gantile>
-- Create date: <28/05/2021>
-- Description:	<Get the currencies>
-- =============================================
CREATE PROCEDURE [dbo].[GetCurrencyType]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	SELECT
		C.CurrencyISO,
		C.CurrencyTypeID,
		C.CurrencyName
	FROM
		[dbo].[CurrencyType] C
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[GetCurrencyType] TO [Enabill_User]
    AS [dbo];


