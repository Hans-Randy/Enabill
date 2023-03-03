CREATE TABLE [dbo].[Months] (
    [Period]             INT        NOT NULL,
    [OverheadPercentage] FLOAT (53) CONSTRAINT [DF_Months_OverheadPercentage] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Months] PRIMARY KEY CLUSTERED ([Period] ASC)
);

