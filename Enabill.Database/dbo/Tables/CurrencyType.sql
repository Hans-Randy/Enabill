CREATE TABLE [dbo].[CurrencyType]
(
	[CurrencyTypeID] INT NOT NULL PRIMARY KEY,
	[CurrencyName] varchar(50) NOT NULL,
	[CurrencyISO] Varchar(3) NOT NULL
	CONSTRAINT [PK_Currency] PRIMARY KEY CLUSTERED (CurrencyTypeID ASC)
);
