CREATE VIEW dbo.vwLatestUserAllocationEntry AS
SELECT UA.userid, UA.activityid, max(UA.StartDate) AS StartDate                   
FROM UserAllocations UA 
WHERE UA.StartDate < getdate()
GROUP BY UserID, ActivityID