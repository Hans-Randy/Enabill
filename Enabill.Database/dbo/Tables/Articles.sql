CREATE TABLE [dbo].[Articles] (
    [ArticleID]      INT           IDENTITY (1, 1) NOT NULL,
    [ArticleDate]    DATETIME      NOT NULL,
    [ArticleSubject] VARCHAR (50)  NOT NULL,
    [ArticleText]    VARCHAR (MAX) NOT NULL,
    [ReleaseDate]    DATETIME      NULL,
    [IsPublished]    BIT           NOT NULL,
    [ArticleTypeID]  INT           NOT NULL,
    [DateModified]   DATETIME      NULL,
    [UserModified]   VARCHAR (50)  NULL,
    CONSTRAINT [PK_News] PRIMARY KEY CLUSTERED ([ArticleID] ASC)
);

