-- =============================================
-- Author:		Gavin van Gent
-- Create date: 2011-12-05
-- Description:	Extract of leave for users between two dates
-- =============================================
CREATE PROCEDURE dbo.UserLeave_LSP
	@StartDate DATETIME = '2011-12-01',
	@EndDate DATETIME = '2012-02-01'
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @Pending INT, @Approved INT
	SELECT
				@Pending = 1,
				@Approved = 4
	
	SELECT 
				'First Name' = U.FirstName,
				'Last Name' = U.LastName,
				'Date From' = REPLACE(CONVERT(VARCHAR(10), L.DateFrom, 111), '/', '-'),
				'Date To' = REPLACE(CONVERT(VARCHAR(10), L.DateTo, 111), '/', '-'),
				'No of Days' = NumberOfDays,
				Approved = CASE WHEN (L.ApprovalStatus = @Pending) THEN 'No' ELSE '' END
	FROM 
				Leaves L
	JOIN
				Users U
					ON L.UserID = U.UserID
	WHERE
				(L.DateFrom >= @StartDate AND L.DateFrom < @EndDate)
				OR (L.DateTo >= @StartDate AND L.DateTo < @EndDate)
	ORDER BY
				L.DateFrom,
				L.DateTo
	
END