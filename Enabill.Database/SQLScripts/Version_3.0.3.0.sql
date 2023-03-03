USE [Enabill]
GO

SET QUOTED_IDENTIFIER ON
GO
-------------------------------------------------
-- Currency
CREATE TABLE [dbo].[CurrencyType](
	[CurrencyTypeID] [int] NOT NULL,
	[CurrencyName] [varchar](50) NOT NULL,
	[CurrencyISO] [varchar](3) NOT NULL,
	
 CONSTRAINT [PK_CurrencyType] PRIMARY KEY CLUSTERED 
(
	[CurrencyTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO dbo.CurrencyType (CurrencyTypeID,CurrencyName,CurrencyISO)
VALUES
(1,'Rand','ZAR')
INSERT INTO dbo.CurrencyType (CurrencyTypeID,CurrencyName,CurrencyISO)
VALUES
(2,'US Dollar','USD')
INSERT INTO dbo.CurrencyType (CurrencyTypeID,CurrencyName,CurrencyISO)
VALUES
(3,'British Pound','GBP')

ALTER TABLE dbo.Clients ADD
	CurrencyTypeID int NOT NULL CONSTRAINT DF_Clients_CurrencyTypeID DEFAULT 1
GO

CREATE PROCEDURE [dbo].[GetCurrencyType]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		C.CurrencyISO,
		C.CurrencyTypeID,
		C.CurrencyName
	FROM
		[dbo].[CurrencyType] C
END
GO

GRANT EXECUTE
	ON OBJECT::[dbo].[GetCurrencyType] TO [Enabill_User]
	AS [dbo];
GO
-------------------------------------------------
-- Roles
INSERT INTO 
	[dbo].[Roles]
	(
		[RoleID]
		,[RoleName]
	)
	VALUES
	(
		8192
		, 'Payroll Manager'
	)
	,(
		16384
		, 'Contract Manager'
	)
GO
-------------------------------------------------
-- Clients
ALTER TABLE Clients ALTER COLUMN PostalCode VARCHAR (20)
