
CREATE PROCEDURE [dbo].[PercentageAllocationReport_LSP]
	@Year INT, 
    @Month INT, 
	@FinPeriod INT,
	@ManagerID varchar(5)
	
AS
BEGIN
	SET NOCOUNT ON;
	

------------------------variables----------------------------------------------
DECLARE @TempTable table (WorkMonth varchar(50),
						  OrderByMonth int,
						  ReportType varchar(50),
						  Person varchar(300),
						  PayrollRefNo varchar(20),
						  DivisionCode varchar(10),
						  UserBillableIndicator varchar(50),
						  ClientName varchar(100),
						  ProjectName varchar(100),
						  ActivityName varchar(100),
						  ProjectBillingMethod varchar(50),
						  SumOfHoursWorked decimal(10,2),
						  PercentageTotalHoursWorkedForMonth decimal(10,2),
						  TotalHoursWorkedForMonth decimal(10,2),
						  Manager varchar(300))
						  						  


--select * from @TempTable

--SET @Year = 2014
--SET @Month = 5
--SET @FinPeriod = 201405

DECLARE @StartDate DATETIME, @EndDate DATETIME	
SELECT @StartDate = CONVERT(DATETIME,CONVERT(VARCHAR, @Year) + '-' + CONVERT(VARCHAR, @Month) + '-01')
SELECT @EndDate = DATEADD(MM, 1, @StartDate)

--DECLARE @ManagerID varchar(5)
--SET @ManagerID = '%'

--select * from users where firstname = 'shabier'

	--select @Startdate, @Enddate
	------------------------variables----------------------------------------------

	-------------------------leaves----------------------------------------------
	
	DECLARE @LeaveTable TABLE (UserID INT, LeaveType VARCHAR(64), LeaveHours FLOAT)

	INSERT INTO
				@LeaveTable(UserID, LeaveType, LeaveHours)
	SELECT
				U.UserID,
				LT.LeaveTypeName,
				SUM(ISNULL(L.NumberOfHours, isnull(UH.WorkHoursPerDay,0)))
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
	LEFT OUTER JOIN		UserHistories UH 
						ON U.USerID = UH.UserID
						AND UH.Period = @FinPeriod
	WHERE
				WD.WorkDate >= @StartDate
				AND WD.WorkDate < @EndDate
				AND WD.IsWorkable = 1
				--AND U.EmploymentTypeID <> 4 -- 4 = hourly contractor, to be excluded from the list
				AND U.EmploymentTypeID in (1,2)
				AND U.ManagerID like @ManagerID
	GROUP BY
				U.UserID,
				LT.LeaveTypeName
	HAVING SUM(ISNULL(L.NumberOfHours, isnull(UH.WorkHoursPerDay,0))) > 0
				
	--SELECT * FROM @LeaveTable		
	
	-- add leave on activity level
	INSERT INTO @TempTable (WorkMonth, OrderByMonth, ReportType, Person, PayrollRefNo, DivisionCode, UserBillableIndicator, ClientName,
						  ProjectName, ActivityName, ProjectBillingMethod, SumOfHoursWorked, PercentageTotalHoursWorkedForMonth, TotalHoursWorkedForMonth, Manager)
	SELECT 
		WorkMonth = CAST(@Year as varchar) + '/' + CAST(@Month as varchar), 
		@Month, 
		'PerActivity', 	
		Person = isnull(U.FirstName, U.username) + ' ' + isnull(U.LastName,'') + ' (' + isnull(U.PayrollRefNo,'') + ')',
		U.PayrollRefNo, 
		DivisionCode,
		BillableIndicatorName,
		'Alacrity',
		'Leave',
		LeaveType,
		'',
		LeaveHours,
		0,
		0,
		Manager = isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
	FROM
		Users u
	LEFT OUTER JOIN @LeaveTable l on l.userid = u.userid
	JOIN dbo.BillableIndicators BI on BI.BillableIndicatorID = U.BillableIndicatorID 
	JOIN Divisions d on D.divisionid = u.divisionid
	LEFT OUTER JOIN Users M on M.UserID = U.ManagerID
	WHERE
		U.ManagerID like @ManagerID
	ORDER BY Person
	
	
	-- add leave on project level
	INSERT INTO @TempTable (WorkMonth, OrderByMonth, ReportType, Person, PayrollRefNo, DivisionCode, UserBillableIndicator, ClientName,
						  ProjectName, ActivityName, ProjectBillingMethod, SumOfHoursWorked, PercentageTotalHoursWorkedForMonth, TotalHoursWorkedForMonth, Manager)
	SELECT 
		WorkMonth = CAST(@Year as varchar) + '/' + CAST(@Month as varchar), 
		@Month, 
		'PerProject', 	
		Person = isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
		U.PayrollRefNo, 
		DivisionCode,
		BillableIndicatorName,
		'Alacrity',
		'Leave',
		'Leave Total',
		'',
		sum(LeaveHours),
		0,
		0,
		Manager = isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
	FROM
		Users u
	LEFT OUTER JOIN @LeaveTable l on l.userid = u.userid
	JOIN dbo.BillableIndicators BI on BI.BillableIndicatorID = U.BillableIndicatorID 
	JOIN Divisions d on D.divisionid = u.divisionid	
	LEFT OUTER JOIN Users M on M.UserID = U.ManagerID
	WHERE
		U.ManagerID like @ManagerID
	GROUP BY
		isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
		u.PayrollRefNo, 
		DivisionCode,
		BillableIndicatorName,
		isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
	ORDER BY Person
	
	
	-- add leave on client level
	INSERT INTO @TempTable (WorkMonth, OrderByMonth, ReportType, Person, PayrollRefNo, DivisionCode, UserBillableIndicator, ClientName,
						  ProjectName, ActivityName, ProjectBillingMethod, SumOfHoursWorked, PercentageTotalHoursWorkedForMonth, TotalHoursWorkedForMonth, Manager)
	SELECT 
		WorkMonth = CAST(@Year as varchar) + '/' + CAST(@Month as varchar), 
		@Month, 
		'PerClient', 	
		Person = isnull(U.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
		u.PayrollRefNo, 
		DivisionCode,		
		BillableIndicatorName,
		'Alacrity',
		'Leave Total',
		'',
		'',
		sum(LeaveHours),
		0,
		0,
		Manager = isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
	FROM
		Users u
	LEFT OUTER JOIN @LeaveTable l on l.userid = u.userid
	JOIN dbo.BillableIndicators BI on BI.BillableIndicatorID = U.BillableIndicatorID 
	JOIN Divisions d on D.divisionid = u.divisionid	
	LEFT OUTER JOIN Users M on M.UserID = U.ManagerID	
	WHERE
		U.ManagerID like @ManagerID
	GROUP BY
		isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
		u.PayrollRefNo, 
		DivisionCode,		
		BillableIndicatorName,
		isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
	ORDER BY Person
	
	-- add leave on user level
	INSERT INTO @TempTable (WorkMonth, OrderByMonth, ReportType, Person, PayrollRefNo, DivisionCode, UserBillableIndicator, ClientName,
						  ProjectName, ActivityName, ProjectBillingMethod, SumOfHoursWorked, PercentageTotalHoursWorkedForMonth, TotalHoursWorkedForMonth, Manager)
	SELECT 
		WorkMonth = CAST(@Year as varchar) + '/' + CAST(@Month as varchar), 
		@Month, 
		'PerUser', 	
		Person = isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
		u.PayrollRefNo, 
		DivisionCode,		
		BillableIndicatorName,
		'Leave Total',
		'',
		'',
		'',
		sum(LeaveHours),
		0,
		0,
		Manager = isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
	FROM
		Users u
	JOIN @LeaveTable l on l.userid = u.userid
	JOIN dbo.BillableIndicators BI on BI.BillableIndicatorID = U.BillableIndicatorID 
	JOIN Divisions d on D.divisionid = u.divisionid	
	LEFT OUTER JOIN Users M on M.UserID = U.ManagerID	
	WHERE
		U.ManagerID like @ManagerID
	GROUP BY
		isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
		u.PayrollRefNo, 
		DivisionCode,		
		BillableIndicatorName,
		isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
	ORDER BY Person
	-------------------------leaves----------------------------------------------

-------------------------insert per activity-----------------------------------
INSERT INTO @TempTable (WorkMonth, OrderByMonth, ReportType, Person, PayrollRefNo, DivisionCode, UserBillableIndicator, ClientName,
						  ProjectName, ActivityName, ProjectBillingMethod, SumOfHoursWorked, PercentageTotalHoursWorkedForMonth, TotalHoursWorkedForMonth, Manager)
select 
	WorkMonth = cast(year(dayworked) as varchar) + '/' + cast(month(dayworked) as varchar),
	OrderByMonth = month(dayworked),
	ReportType = 'PerActivity',
	Person = isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
	u.PayrollRefNo, 
	DivisionCode,	
	UserBillableIndicator = BillableIndicatorName,
	ClientName, ProjectName,ActivityName, 
	ProjectBillingMethod = BillingMethodName,
	SumOfHoursWorked = sum(wa.hoursworked),
	'PercentageTotalHoursWorkedForMonth' = 0,
	TotalHoursWorkedForMonth = 0,
		Manager = isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
from users u
JOIN workallocations wa on wa.userid = u.userid
JOIN activities a on a.activityid = wa.activityid
JOIN projects p on p.projectid = a.projectid
JOIN clients c on c.clientid = p.clientid
JOIN dbo.BillableIndicators BI on BI.BillableIndicatorID = U.BillableIndicatorID 
JOIN dbo.BillingMethods BM ON BM.BillingMethodID = p.BillingMethodID
JOIN Divisions d on D.divisionid = u.divisionid
LEFT OUTER JOIN Users M on M.UserID = U.ManagerID
where year(dayworked) = @Year
and month(dayworked) = @Month
--and u.isactive = 1
and U.ManagerID like @ManagerID
group by 
	cast(year(dayworked) as varchar) + '/' + cast(month(dayworked) as varchar),
	month(dayworked),
	isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
	u.PayrollRefNo, 
	DivisionCode,	
	BillableIndicatorName,
	ClientName, ProjectName, ActivityName,
	BillingMethodName,
	isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
ORDER BY
	month(dayworked),
	isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
	ClientName, ProjectName, ActivityName
	
-------------------------------------------------insert per project------------------------------------------------

INSERT INTO @TempTable (WorkMonth, OrderByMonth, ReportType, Person, PayrollRefNo, DivisionCode, UserBillableIndicator, ClientName,
						  ProjectName, ActivityName, ProjectBillingMethod, SumOfHoursWorked, PercentageTotalHoursWorkedForMonth, TotalHoursWorkedForMonth, Manager)
select 
	WorkMonth = cast(year(dayworked) as varchar) + '/' + cast(month(dayworked) as varchar),
	OrderByMonth = month(dayworked),
	ReportType = 'PerProject',
	Person = isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
	u.PayrollRefNo, 
	DivisionCode,	
	UserBillableIndicator = BillableIndicatorName,
	ClientName, ProjectName, ActivityName = 'Total',
	ProjectBillingMethod = BillingMethodName,
	SumOfHoursWorked = sum(wa.hoursworked),
	'PercentageTotalHoursWorkedForMonth' = 0,
	TotalHoursWorkedForMonth = 0,
		Manager = isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
from users u
JOIN workallocations wa on wa.userid = u.userid
JOIN activities a on a.activityid = wa.activityid
JOIN projects p on p.projectid = a.projectid
JOIN clients c on c.clientid = p.clientid
JOIN dbo.BillableIndicators BI on BI.BillableIndicatorID = U.BillableIndicatorID 
JOIN dbo.BillingMethods BM ON BM.BillingMethodID = p.BillingMethodID
JOIN Divisions d on D.divisionid = u.divisionid
LEFT OUTER JOIN Users M on M.UserID = U.ManagerID
where year(dayworked) = @Year
and month(dayworked) = @Month
--and u.isactive = 1
and U.ManagerID like @ManagerID
group by 
	cast(year(dayworked) as varchar) + '/' + cast(month(dayworked) as varchar),
	month(dayworked),
	isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
	u.PayrollRefNo, 
	DivisionCode,	
	BillableIndicatorName,
	ClientName, ProjectName,
	BillingMethodName,
	isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
ORDER BY
	month(dayworked),
	isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
	ClientName, ProjectName, ActivityName
	
--------------------------------------------Insert per client ------------------------------------------------------	

INSERT INTO @TempTable (WorkMonth, OrderByMonth, ReportType, Person, PayrollRefNo, DivisionCode, UserBillableIndicator, ClientName,
						  ProjectName, ActivityName, ProjectBillingMethod, SumOfHoursWorked, PercentageTotalHoursWorkedForMonth, TotalHoursWorkedForMonth, Manager)
select 
	WorkMonth = cast(year(dayworked) as varchar) + '/' + cast(month(dayworked) as varchar),
	OrderByMonth = month(dayworked),
	ReportType = 'PerClient',
	Person = isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
	u.PayrollRefNo, 
	DivisionCode,	
	UserBillableIndicator = BillableIndicatorName,
	ClientName, ProjectName = 'Total', ActivityName = '',
	ProjectBillingMethod = '',
	SumOfHoursWorked = sum(wa.hoursworked),
	'PercentageTotalHoursWorkedForMonth' = 0,
	TotalHoursWorkedForMonth = 0,
		Manager = isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
from users u
JOIN workallocations wa on wa.userid = u.userid
JOIN activities a on a.activityid = wa.activityid
JOIN projects p on p.projectid = a.projectid
JOIN clients c on c.clientid = p.clientid
JOIN dbo.BillableIndicators BI on BI.BillableIndicatorID = U.BillableIndicatorID 
JOIN dbo.BillingMethods BM ON BM.BillingMethodID = p.BillingMethodID
JOIN Divisions d on D.divisionid = u.divisionid
LEFT OUTER JOIN Users M on M.UserID = U.ManagerID
where year(dayworked) = @Year
and month(dayworked) = @Month
--and u.isactive = 1
and U.ManagerID like @ManagerID
group by 
	cast(year(dayworked) as varchar) + '/' + cast(month(dayworked) as varchar),
	month(dayworked),
	isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
	u.PayrollRefNo, 
	DivisionCode,	
	BillableIndicatorName,
	ClientName,
	isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
ORDER BY
	month(dayworked),
	isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
	ClientName, ProjectName, ActivityName
	
	----now add the leave----	
	
	

-------------------------------------------Insert per user-------------------------------------------------------	
INSERT INTO @TempTable (WorkMonth, OrderByMonth, ReportType, Person, PayrollRefNo, DivisionCode, UserBillableIndicator, ClientName,
						  ProjectName, ActivityName, ProjectBillingMethod, SumOfHoursWorked, PercentageTotalHoursWorkedForMonth, TotalHoursWorkedForMonth, Manager)

select 
	WorkMonth = cast(year(dayworked) as varchar) + '/' + cast(month(dayworked) as varchar),
	OrderByMonth = month(dayworked),
	ReportType = 'PerUser',
	Person = isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
	u.PayrollRefNo, 
	DivisionCode,	
	UserBillableIndicator = BillableIndicatorName,
	ClientName = 'Total', ProjectName = '', ActivityName = '',
	ProjectBillingMethod = '',
	SumOfHoursWorked = sum(wa.hoursworked),
	'PercentageTotalHoursWorkedForMonth' = 0,
	TotalHoursWorkedForMonth = 0,
		Manager = isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
from users u
JOIN workallocations wa on wa.userid = u.userid
JOIN activities a on a.activityid = wa.activityid
JOIN projects p on p.projectid = a.projectid
JOIN clients c on c.clientid = p.clientid
JOIN dbo.BillableIndicators BI on BI.BillableIndicatorID = U.BillableIndicatorID 
JOIN dbo.BillingMethods BM ON BM.BillingMethodID = p.BillingMethodID
JOIN Divisions d on D.divisionid = u.divisionid
LEFT OUTER JOIN Users M on M.UserID = U.ManagerID
where year(dayworked) = @Year
and month(dayworked) = @Month
--and u.isactive = 1
and U.ManagerID like @ManagerID
group by 
	cast(year(dayworked) as varchar) + '/' + cast(month(dayworked) as varchar),
	month(dayworked),
	isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
	u.PayrollRefNo, 
	DivisionCode,	
	BillableIndicatorName,
	isnull(M.FirstName, M.username) + ' ' + isnull(M.LastName,'')
ORDER BY
	month(dayworked),
	isnull(u.FirstName, u.username) + ' ' + isnull(u.LastName,'') + ' (' + isnull(u.PayrollRefNo,'') + ')',
	ClientName, ProjectName, ActivityName

--------------------------------update total hours worked for month--------------------------
UPDATE t
SET TotalHoursWorkedForMonth =t2.SumOfHoursWorked
FROM @TempTable t 
JOIN (	SELECT WorkMonth, Person, ReportType, sum(SumOfHoursWorked) as SumOfHoursWorked
		FROM @TempTable 
		WHERE ReportType = 'PerUser'
		GROUP BY WorkMonth, Person, ReportType)t2	ON	t2.workmonth = t.workmonth
													AND t2.Person = t.Person 		

--------------------------------update total hours worked for month--------------------------

--------------------------------update PercentageTotalHoursWorkedForMonth -----------------------------

UPDATE @TempTable
SET [PercentageTotalHoursWorkedForMonth] = CASE WHEN TotalHoursWorkedForMonth = 0 THEN 0 ELSE SumOfHoursWorked / TotalHoursWorkedForMonth * 100 END
FROM @TempTable 
				
-------------------------------update PercentageTotalHoursWorkedForMonth -----------------------------
										
select 
	Manager, 
	WorkMonth,
	OrderByMonth,
	ReportType,
	Person, PayrollRefNo, DivisionCode,
	UserBillableIndicator,
	ClientName,
	ProjectName,
	ActivityName,
	ProjectBillingMethod,
	SumOfHoursWorked,
	PercentageTotalHoursWorkedForMonth,
	TotalHoursWorkedForMonth			    
from @TempTable
where person not like 'process%'
and sumofhoursworked is not null
order by manager, orderbymonth, person, clientname, projectname, activityname											

--select * from @TempTable where reporttype = 'PerUser'
--order by orderbymonth, person, clientname, projectname, activityname

--select * from @TempTable where reporttype = 'PerClient'
--order by orderbymonth, person, clientname, projectname, activityname

--select * from @TempTable where reporttype = 'PerProject'
--order by orderbymonth, person, clientname, projectname, activityname

--select * from @TempTable where reporttype = 'PerActivity'
--order by orderbymonth, person, clientname, projectname, activityname

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PercentageAllocationReport_LSP] TO PUBLIC
    AS [dbo];

