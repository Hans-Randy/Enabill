
CREATE PROCEDURE [dbo].[Report_LSP_UserTimeAllocationBreakdown] 
	@FinPeriod int = 201202
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @StartDate DATETIME, @EndDate DATETIME	 
	SELECT @StartDate = CONVERT(DATETIME,substring(CONVERT(VARCHAR, @FinPeriod),1,4) + '-' + substring(CONVERT(VARCHAR, @FinPeriod),5,2) + '-01')
	SELECT @EndDate = DATEADD(MM, 1, @StartDate)
	--EndDate will have first day of following month
	--eg, @StartDate = 2011-10-01, @EndDate = 2011-11-01
	
	------------------------------------------------------------------------------
	
	DECLARE @LeaveTable TABLE (UserID INT, LeaveType VARCHAR(64), LeaveHours FLOAT)

	INSERT INTO
				@LeaveTable(UserID, LeaveType, LeaveHours)
	SELECT
				U.UserID,
				LT.LeaveTypeName,
				SUM(ISNULL(L.NumberOfHours, UH.WorkHoursPerDay))
	FROM
				WorkDays WD
	JOIN
				Leaves L
						ON WD.WorkDate >= L.DateFrom
						AND WD.WorkDate <= L.DateTo
						AND L.ApprovalStatus = 4 --Approved
	JOIN
				LeaveTypes LT
						ON L.LeaveType = LT.LeaveTypeID
	JOIN
				Users U
						ON	L.UserID = U.UserID
	JOIN		UserHistories UH 
						ON U.USerID = UH.UserID
						AND UH.Period = @FinPeriod
	WHERE
				WD.WorkDate >= @StartDate
				AND WD.WorkDate < @EndDate
				AND WD.IsWorkable = 1
				--AND U.EmploymentTypeID <> 4 -- 4 = hourly contractor, to be excluded from the list
				AND U.EmploymentTypeID in (1,2)
	GROUP BY
				U.UserID,
				LT.LeaveTypeName	
	
	------------------------------------------------------------------------------
	
	DECLARE @MonthlyHours TABLE (UserID INT, HoursWorked FLOAT, TotalHours FLOAT)

	INSERT INTO @MonthlyHours (UserID , HoursWorked, TotalHours)
	SELECT 
				U.UserID,
				SUM(ISNULL(WA.HoursWorked, 0)),
				SUM(ISNULL(WA.HoursWorked, 0)) 
				+ ISNULL((SELECT SUM(ISNULL(LeaveHours, 0)) FROM @LeaveTable WHERE UserID = U.UserID GROUP BY UserID), 0)
	FROM
				Users U
	LEFT JOIN
				WorkAllocations WA 
						ON U.UserID = WA.UserID
						AND WA.DayWorked >= @StartDate
						AND WA.DayWorked < @EndDate
	GROUP BY
				U.UserID
				
	--SELECT * FROM @MonthlyHours
				
	------------------------------------------------------------------------------
	
	DECLARE @ResultTable TABLE
									(UserID INT,
									 Client VARCHAR(100),
									 Project VARCHAR(100),
									 Activity VARCHAR(100),
									 Region VARCHAR(100),
									 Department VARCHAR(100),
									 HoursWorkedOnProject FLOAT)
									 
	INSERT INTO
				@ResultTable		(UserID,
									 Client,
									 Project,
									 Activity,
									 Region,
									 Department,
									 HoursWorkedOnProject)
	SELECT
				U.UserID,
				C.ClientName,
				P.ProjectName,
				A.ActivityName,
				R.RegionName,
				D.DepartmentName,
				SUM(WA.HoursWorked)
	FROM
				Users U
	JOIN
				WorkAllocations WA 
						ON U.UserID = WA.UserID
						AND WA.DayWorked >= @StartDate
						AND WA.DayWorked < @EndDate
	JOIN
				Activities A 
						ON WA.ActivityID = A.ActivityID
	JOIN
				Projects P
						ON A.ProjectID = P.ProjectID
	JOIN
				Clients C
						ON P.ClientID = C.ClientID
	JOIN
				Regions R
						ON A.RegionID = R.RegionID
	JOIN
				Departments D
						ON A.DepartmentID = D.DepartmentID

	JOIN
				@MonthlyHours MH
						ON U.UserID = MH.UserID				
	GROUP BY
				U.UserID,
				WA.ActivityID,
				WA.UserID,
				C.ClientName,
				P.ProjectName,
				A.ActivityName,
				R.RegionName,
				D.DepartmentName,
				MH.HoursWorked,
				MH.TotalHours
				
				
				
	INSERT INTO
				@ResultTable		(UserID,
									 Client,
									 Project,
									 Activity,
									 Region,
									 Department,
									 HoursWorkedOnProject)
	SELECT
				U.UserID,
				'Alacrity',
				'Leave',
				LT.LeaveType,
				R.RegionName,
				--D.DivisionName,
				
				CASE WHEN UPPER(D.DivisionName) = UPPER('Operations SDS') THEN 'SDS' ELSE
					CASE WHEN UPPER(D.DivisionName) = UPPER ('Operations SCS') THEN 'SCS' ELSE
						CASE WHEN UPPER(D.DivisionName) = UPPER('Operations') THEN 'OPS' ELSE
							D.DivisionName
				END END END,
				
				SUM(LT.LeaveHours)
	FROM
				Users U
	JOIN
				@LeaveTable LT
						ON U.UserID = LT.UserID
	JOIN
				Regions R
						ON U.RegionID = R.RegionID
	JOIN
				Divisions D
						ON U.DivisionID = D.DivisionID
	GROUP BY
				U.UserID,
				Lt.LeaveType,
				R.RegionName,
				D.DivisionName
		
	------------------------------------------------------------------------------

	SELECT 
				U.UserName,
				--Name = U.FirstName + ' ' + U.LastName,
				U.FullName,
				'PayRoll Ref No' = U.PayrollRefNo,
				Division = D.DivisionName,
				Client = RT.Client,
				Project = RT.Project,
				Activity = RT.Activity,
				Region = RT.Region,
				Department = RT.Department,
				'Hours Worked On Project' = RT.HoursWorkedOnProject,
				'Total Month Hours' = MH.TotalHours,
				'% of Worked Time Spent on Activity' = CONVERT(DECIMAL(18,2), RT.HoursWorkedOnProject / MH.TotalHours * 100)
	FROM
				Users U
	JOIN
				Divisions D
						ON U.DivisionID = D.DivisionID
	JOIN
				@ResultTable RT
						ON U.UserID = RT.UserID
	JOIN
				@MonthlyHours MH 
						ON U.UserID = MH.UserID
	GROUP BY
				U.UserName,
				U.FullName,
				U.FirstName,
				U.LastName,
				U.PayrollRefNo,
				D.DivisionName,
				RT.Client,
				RT.Project,
				RT.Activity,
				RT.Region,
				RT.Department,
				RT.HoursWorkedOnProject,
				MH.TotalHours
	ORDER BY
				U.FullName
	
	------------------------------------------------------------------------------
	
END