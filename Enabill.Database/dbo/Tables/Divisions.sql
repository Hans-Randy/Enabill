CREATE TABLE [dbo].[Divisions] (
    [DivisionID]   INT          NOT NULL,
    [DivisionName] VARCHAR (50) NOT NULL,
    [DivisionCode] VARCHAR (6)  NOT NULL,
    CONSTRAINT [PK_Division] PRIMARY KEY CLUSTERED ([DivisionID] ASC)
);

