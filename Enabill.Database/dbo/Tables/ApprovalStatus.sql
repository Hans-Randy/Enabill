CREATE TABLE [dbo].[ApprovalStatus] (
    [ApprovalStatusID]   INT          NOT NULL,
    [ApprovalStatusName] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_ApprovalStatus] PRIMARY KEY CLUSTERED ([ApprovalStatusID] ASC)
);

