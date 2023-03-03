


CREATE VIEW [dbo].[vwUserActivityTimeSpent] AS
SELECT U.UserID,
       U.UserName,
	   U.FullName,
       DV.DivisionName,
       R.RegionName,
       D.DepartmentName,
       C.ClientID,
       C.ClientName,
       P.ProjectID,
       P.ProjectName,
       A.ActivityID,
       A.ActivityName,
       WA.[Period],
       WA.DayWorked,
       WA.HoursWorked,
       WA.Remark,
       TC.TrainingCategoryName,
       WA.TrainerName,
       WA.TrainingInstitute,
       WA.WorkAllocationID  
 FROM dbo.Users U INNER JOIN 
 dbo.WorkAllocations WA ON U.UserID = WA.UserID INNER JOIN 
 dbo.Activities A ON WA.ActivityID = A.ActivityID INNER JOIN 
 dbo.Projects P ON A.ProjectID = P.ProjectID INNER JOIN 
 dbo.Clients C ON P.ClientID = C.ClientID INNER JOIN
 dbo.Departments D ON P.DepartmentID = D.DepartmentID INNER JOIN 
 dbo.Regions R ON P.RegionID = R.RegionID INNER JOIN
 dbo.Divisions DV ON U.DivisionID = DV.DivisionID LEFT JOIN
 dbo.TrainingCategories TC on WA.TrainingCategoryID = TC.TrainingCategoryID