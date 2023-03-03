CREATE VIEW vwClientProjectFeedback 
AS
SELECT DISTINCT T.FeedbackThreadID, T.ProjectID, T.ClientID, U.UserID, T.DateClosed, T.FeedbackSubject
FROM FeedbackThreads T
JOIN Clients C ON T.ClientID = C.ClientID
LEFT JOIN Projects P ON T.ProjectID = P.ProjectID OR C.ClientID = P.ClientID
LEFT JOIN Activities A ON P.ProjectID = A.ProjectID
LEFT JOIN UserAllocations U ON A.ActivityID = U.ActivityID
WHERE U.UserID IS NOT NULL
GO
GRANT SELECT
    ON OBJECT::[dbo].[vwClientProjectFeedback] TO PUBLIC
    AS [dbo];

