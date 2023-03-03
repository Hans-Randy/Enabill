CREATE TABLE [dbo].[Menus] (
    [MenuID]             INT           NOT NULL,
    [MenuName]           VARCHAR (50)  NOT NULL,
    [Controller]         VARCHAR (50)  NULL,
    [Action]             VARCHAR (50)  NULL,
    [CustomSort]         INT           NULL,
    [RoleBW]             INT           NOT NULL,
    [MenuImagePath]      VARCHAR (512) CONSTRAINT [DF_Menus_MenuImagePath] DEFAULT ('') NOT NULL,
    [MenuHoverImagePath] VARCHAR (512) CONSTRAINT [DF_Menus_MenuHoverImagePath] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_Menus] PRIMARY KEY CLUSTERED ([MenuID] ASC)
);

