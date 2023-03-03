
CREATE PROCEDURE [dbo].[rSP_ResourceUtilisation]
(
	@Month	int = NULL,
	@Year	int = NULL
)
AS
BEGIN

	SET NOCOUNT ON

	-- Total number of leave hours taken in month
	-- ------------------------------------------
	; WITH LeaveHoursInMonth AS
	(
		SELECT
			SUM(P0.HoursOfLeaveForMonth) AS HoursOfLeaveForMonth,
			P0.DateFromMonth,
			P0.DateFromYear,
			P0.UserID
		FROM
		(
			SELECT 
				(CASE
					WHEN DATEPART(mm, DateFrom) = DATEPART(mm, WorkDate) THEN 1
					ELSE 0
				END * 8) AS HoursOfLeaveForMonth,
				DATEPART(mm, DateFrom) AS DateFromMonth,
				DATEPART(yy, DateFrom) AS DateFromYear,
				L.UserID
			FROM 
				dbo.Leaves L
				Inner Join [dbo].[WorkDays] W ON (W.WorkDate >= L.DateFrom and W.WorkDate <= L.DateTo)  
			WHERE 
				--userid IN (207,212) 
				DATEPART(mm, DateFrom) = @Month
				AND DATEPART(yy, DateFrom) = @Year
				AND numberofhours IS NULL
				AND IsWorkable = 1
			UNION ALL
			SELECT 
				CASE
					WHEN DATEPART(mm, DateFrom) = DATEPART(mm, WorkDate) THEN NumberOfHours
					ELSE 0
				END AS HoursOfLeaveForMonth,
				DATEPART(mm, DateFrom) AS DateFromMonth,
				DATEPART(yy, DateFrom) AS DateFromYear,
				L.UserID
			FROM 
				dbo.Leaves L
				Inner Join [dbo].[WorkDays] W ON (W.WorkDate >= L.DateFrom and W.WorkDate <= L.DateTo)  
			WHERE 
				--userid IN (207,212)
				DATEPART(mm, DateFrom) = @Month
				AND DATEPART(yy, DateFrom) = @Year
				AND IsWorkable = 1
				AND numberofhours IS NOT NULL

		) P0
		GROUP BY
			P0.DateFromMonth,
			P0.DateFromYear,
			P0.UserID
	)
		

	SELECT
		U.UserID,
		--U.FirstName + ' ' + U.LastName AS 'UserName',
		U.FullName,
		U.WorkHours,
		D.DivisionName, 
		D.DivisionCode,
		A.ActivityName,
		P.ProjectName,	
		C.ClientName,
		W.DayWorked,
		W.HoursWorked,
		P0.TotalMonthWorkHours,
		P1.HoursOfLeaveForMonth
	FROM
		dbo.Users U
		INNER JOIN dbo.Divisions D ON U.DivisionID = D.DivisionID
		INNER JOIN dbo.WorkAllocations W ON U.UserID = W.UserID
		INNER JOIN dbo.Activities A ON W.ActivityID = A.ActivityID
		INNER JOIN dbo.Projects P ON A.ProjectID = P.ProjectID
		INNER JOIN dbo.Clients C ON P.ClientID = C.ClientID
		INNER JOIN dbo.Regions R ON A.RegionID = R.RegionID
		CROSS JOIN
			(
			-- Total amount of hours to be worked in the month
			-- -----------------------------------------------
			SELECT
				(COUNT(WorkDate) * 8) AS TotalMonthWorkHours
			FROM
				dbo.WorkDays W
			WHERE
				DATEPART(mm, WorkDate) = @Month
				AND DATEPART(yy, WorkDate) = @Year
				AND IsWorkable = 1
			) P0
		INNER JOIN LeaveHoursInMonth P1 ON U.UserID = P1.UserID
	WHERE
		U.IsActive = 1
		--AND U.USerID IN (207,212)
		AND DATEPART(mm, DayWorked) = @Month
		AND DATEPART(yy, DayWorked) = @Year

	SET NOCOUNT OFF

END