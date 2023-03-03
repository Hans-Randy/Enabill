

CREATE PROCEDURE [dbo].[GetLeaveGeneral]
    @IsActive VARCHAR(10) = '',
    @FirstName VARCHAR(50) = '',
    @LastName VARCHAR(50) = '',
    @EmploymentType VARCHAR(10) = '',
    @ManagerID VARCHAR(10) = '',
	 @ManagerName VARCHAR(50) = '', -- 'firstname.lastname'
	 @DateFrom DATE = NULL,
	 @DateTo DATETIME = NULL,
	 @LeaveType VARCHAR(10) = '',
	 @ApprovalStatus VARCHAR(30) = ''
AS

SET @DateFrom = ISNULL(@DateFrom, CAST(GETDATE() AS DATE))

SELECT 
    --u.FirstName + ' ' + u.LastName [Employee],
	u.FullName [Employee],
	e.EmploymentTypeName [Employment Type],
	--m.Username [Manager],
	m.FullName [Manager],
    l.DateFrom [Date From],
    l.DateTo [Date To],
	 l.NumberOfDays [Number of Days],
    lt.LeaveTypeName [Leave Type],
	 CASE WHEN l.NumberOfHours IS NULL THEN  
		  'No'
	 ELSE
		  'Yes'
	 END [Partial],
	 CASE l.ApprovalStatus
		  WHEN 1 THEN 'Pending'
		  WHEN 2 THEN 'Declined'
		  WHEN 4 THEN 'Approved'
		  WHEN 8 THEN 'Withdrawn'
	 END [Approval Status]
FROM dbo.users u
	 INNER JOIN dbo.Users m ON u.ManagerID = m.UserID
	 INNER JOIN dbo.EmploymentTypes e ON u.EmploymentTypeID = e.EmploymentTypeID
    INNER JOIN dbo.Leaves l ON l.UserID = u.UserID
    INNER JOIN dbo.LeaveTypes lt ON lt.LeaveTypeID = l.LeaveType
WHERE 
	 u.IsActive LIKE
	 CASE WHEN IsNumeric(@IsActive) = 1 THEN  
		  @IsActive
	 ELSE
		  '%' + @IsActive
	 END
    AND
	   u.LastName LIKE '%' + @LastName
    AND
	   u.FirstName LIKE '%' +  @FirstName
    AND
	   u.EmploymentTypeId LIKE
	   CASE WHEN IsNumeric(@EmploymentType) = 1 THEN  
		  @EmploymentType
	   ELSE
		  '%' + @EmploymentType
	   END
    AND
	   m.UserId LIKE
	   CASE WHEN IsNumeric(@ManagerID) = 1 THEN  
		  @ManagerID
	   ELSE
		  '%' + @ManagerID
	   END
	 AND
		  CAST(l.DateFrom AS DATE) >= @DateFrom
	 AND
		  (l.DateTo <= @DateTo OR @DateTo IS NULL)
    AND
	   l.LeaveType LIKE
	   CASE WHEN IsNumeric(@LeaveType) = 1 THEN  
		  @LeaveType
	   ELSE
		  '%' + @LeaveType
	   END
    AND
	   l.ApprovalStatus LIKE
	   CASE WHEN IsNumeric(@ApprovalStatus) = 1 THEN  
		  @ApprovalStatus
	   ELSE
		  '%' + @ApprovalStatus
	   END
ORDER BY u.LastName, u.FirstName, l.datefrom;