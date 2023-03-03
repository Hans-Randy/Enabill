

/*
Create a report that reflects the following for all users: 
	 Rate per user 
	 Rate per customer
	 Expiry date of Project/Contract end date
	 Rate expiry date
*/

CREATE PROCEDURE [dbo].[UserClientRates] 
    @IsActive VARCHAR(10) = '',
    @UserName VARCHAR(50) = '',
    @EmploymentType VARCHAR(10) = ''
AS

SELECT DISTINCT
	 u.UserName,
	 u.FullName,
	 wa.HourlyRate,
	 c.ClientName,
	 ua.ChargeRate,
	 p.ProjectName,
	 a.ActivityName,
	 ua.ScheduledEndDate,
	 ua.ConfirmedEndDate
FROM dbo.Users u 
INNER JOIN dbo.WorkAllocations wa on u.UserID = wa.UserID
INNER JOIN dbo.UserAllocations ua ON u.UserId = ua.UserID
INNER JOIN dbo.Activities a ON ua.ActivityID = a.ActivityID
INNER JOIN dbo.Projects p ON a.ProjectID = p.ProjectID
INNER JOIN dbo.Clients c ON p.ClientID = c.ClientID
WHERE
		u.IsActive LIKE
		CASE WHEN IsNumeric(@IsActive) = 1 THEN  
		@IsActive
		ELSE
		'%' + @IsActive
		END
    AND
	   u.UserName LIKE '%' + @UserName
    AND
	   u.EmploymentTypeId LIKE
	   CASE WHEN IsNumeric(@EmploymentType) = 1 THEN  
	   @EmploymentType
	   ELSE
	   '%' + @EmploymentType
	   END
ORDER BY  u.FullName, u.UserName, c.ClientName, p.ProjectName, a.ActivityName

/*
Active:
	 Blank = All
	 0 = Inactive
	 1 = Active

User:
	 Blank = All
	 firstname.lastname

Employment Type:
	 Blank = All
	 1 = Permanent
	 2 = Monthly Contractor
	 4 = Hourly Contractor
	 8 = Intern

EXEC dbo.UserClientRates @IsActive = '' @UserName = '' @EmploymentType = ''
*/