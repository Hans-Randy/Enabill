CREATE FUNCTION [dbo].[Fn_OptimalThroughputFillFactorForRandom] 
( 
 @KeyBytes float, 
 @GrowthPercentage float 
) 
RETURNS int 
AS 
BEGIN 
	If @KeyBytes <2 
		SET @KeyBytes=2 
	If @GrowthPercentage > 0.06
		SET @GrowthPercentage = 0.06 
	If @GrowthPercentage < 0.001
		SET @GrowthPercentage = 0.001
	
	DECLARE @FillFactor float 
	DECLARE @Rate float 
	DECLARE @Offset float 
	
	Set @Rate=-5.2312 * Power(@keybytes,-0.244) -- R = 0.95 
	Set @Offset=1 -0.2193 * Power(@keybytes, -0.462) -- R = 0.99 
	Set @FillFactor=CEILING(100 * 
	(@Rate *@GrowthPercentage +@Offset)) 
	
	If @FillFactor <50 
		SET @FillFactor=50 
	If @FillFactor >99 
		SET @FillFactor=99 
	
	RETURN @FillFactor 
END