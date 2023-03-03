CREATE VIEW dbo.vwUserLastWorkSessions AS
SELECT [UserID],
       max(StartTime) as LastWorkSessionDate     
FROM [dbo].[WorkSessions]
WHERE StartTime <=  CURRENT_TIMESTAMP
GROUP by userid