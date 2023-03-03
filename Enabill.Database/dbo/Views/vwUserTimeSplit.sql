

CREATE view [dbo].[vwUserTimeSplit] AS
SELECT  UserID,
        UserName,
		FullName,
        DivisionName,
        RegionName,
        DepartmentName,
        ClientID,
        ClientName,
        ProjectID,
        ProjectName,
        ActivityID,
        ActivityName,
        convert(char(4),Year(DayWorked)) + case when len(convert(char(2),Month(DayWorked))) < 2 then '0' + convert(char(2),Month(DayWorked)) else convert(char(2),Month(DayWorked))end as Period,
        DayWorked,
        HoursWorked,
        Remark,
        case when TrainingCategoryName = 'Please select' then '' else TrainingCategoryName end as TrainingType,
        TrainerName,
        TrainingInstitute,
        WorkAllocationID
FROM dbo.vwUserActivityTimeSpent
UNION
SELECT U.Userid,
       U.UserName,
	   U.FullName,
       DivisionName,
       RegionName,
       'Leave and Exceptions' as DepartmentName,
       1 as ClientID,
       'Alacrity' as ClientName,
       -1 as ProjectID,
       'Leave and Exceptions' as ProjectName,
       -1 as ActivityID,
       LeaveTypeName + ' Leave' as ActivityName,
       convert(char(4),Year(WorkDate)) + case when len(convert(char(2),Month(WorkDate))) < 2 then '0' + convert(char(2),Month(WorkDate)) else convert(char(2),Month(WorkDate))end as Period,
       WorkDate as DayWorked,      
       HoursTaken As HoursWorked,
       Remark,
       '' as TrainingType,
       '' as TrainerName,
       '' as TrainingInstitute,
       -1*(U.UserID + L.LeaveTypeID + year(WorkDate) + month(WorkDate) +  day(WorkDate)) as WorkAllocationID ---Trying to force a unique key else linq query duplicate records     
FROM [dbo].[vwIndividualLeaveDays] L INNER JOIN
dbo.Users U On L.UserID = U.UserID INNER JOIN
dbo.Regions R on U.RegionID = R.RegionID
UNION
SELECT U.Userid,
       U.UserName,
	   U.FullName,
       DivisionName,
       RegionName,
       'Leave and Exceptions' as DepartmentName,
       1 as ClientID,
       'Alacrity' as ClientName,
       -1 as ProjectID,
       'Leave and Exceptions' as ProjectName,
       -1 as ActivityID,
       'FlexiDay' as ActivityName,
       convert(char(4),Year(FlexiDate)) + case when len(convert(char(2),Month(FlexiDate))) < 2 then '0' + convert(char(2),Month(FlexiDate)) else convert(char(2),Month(FlexiDate))end as Period,
       FlexiDate as DayWorked,      
       U.WorkHours As HoursWorked,
       Remark,
       '' as TrainingType,
       '' as TrainerName,
       '' as TrainingInstitute,
       -1*(U.UserID +  year(FlexiDate) + month(FlexiDate) +  day(FlexiDate)) as WorkAllocationID ---Trying to force a unique key else linq query duplicate records            
FROM  [dbo].[FlexiDays]F INNER JOIN
dbo.Users U on U.UserID = F.UserID INNER JOIN
dbo.Divisions D on U.DivisionID = D.DivisionID INNER JOIN
dbo.Regions R on U.RegionID = R.RegionID
UNION
SELECT UserID ,
       UserName,
	   FullName,
       DivisionName,
       RegionName,
       'Leave and Exceptions' as DepartmentName,
       1 as ClientID,
       ClientName,
       -1 as ProjectID,
       'Leave and Exceptions' as ProjectName,
       -1 as ActivityID,
       'Exceptions' AS ActivityName,
       Period,
       DayWorked,      
       HoursWorked,
       '' as Remark,
       '' as TrainingType,
       '' as TrainerName,
       '' as TrainingInstitute,
       -1*(UserID + year(DayWorked) + month(DayWorked) +  day(DayWorked)) as WorkAllocationID ---Trying to force a unique key else linq query duplicate records        
FROM dbo.vwUserWorkAllocationExceptions