CREATE PROCEDURE dbo.DeactivateLeaveCycleBalanceRecords 
	 @UserID VARCHAR(10),
	 @Count INT OUTPUT,
	 @UserName VARCHAR(50) OUTPUT
AS

/*
	 =============================================
	 Author:			  <Nico van der Walt>
	 Create date:	  <10/04/2017>
	 Description:	  <Deactivate obsolete leave cycle balance records>
	 =============================================

	IF OBJECT_ID('dbo.DeactivateLeaveCycleBalanceRecords') IS NOT NULL
		EXEC ('DROP PROCEDURE dbo.DeactivateLeaveCycleBalanceRecords;')
	GO
	
	-- Deactivate Leave Cycle Balance records where user/s are inactive
	DECLARE @Count INT, @UserName VARCHAR(50)
	
	EXEC dbo.DeactivateLeaveCycleBalanceRecords
		2,
		@Count OUTPUT,
		@UserName OUTPUT
	
	SELECT  @Count, @UserName

*/
	
	-- Select Name and Number of Records
	SELECT
		@Count = COUNT(u.UserID),
		@UserName = u.UserName
	FROM
		dbo.LeaveCycleBalances lcb
	INNER JOIN
		dbo.Users u
	ON
		lcb.UserID = u.UserID
	WHERE
		u.IsActive = 0
	AND
		lcb.Active = 1
	AND
		u.UserID = @UserID
	GROUP BY
		u.UserName, u.UserID

	-- Update records
	SET NOCOUNT ON

	UPDATE
		lcb
	SET
		lcb.Active = 0
	FROM
		dbo.LeaveCycleBalances lcb
	INNER JOIN
		dbo.Users u
	ON
		lcb.UserID = u.UserID
	WHERE
		u.IsActive = 0
	AND
		lcb.Active = 1
	AND
		u.UserID LIKE
		CASE WHEN IsNumeric(@UserID) = 1 THEN  
			@UserID
		ELSE
			'%' + @UserID
		END
	
	-- Set output parameters	
	SET @Count = ISNULL(@Count, 0)
	SET @UserName =  ISNULL(@UserName, 'None')