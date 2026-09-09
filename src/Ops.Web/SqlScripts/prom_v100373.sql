SET NOCOUNT ON;

UPDATE [Emedia] SET [SortAs] = CASE SUBSTRING([Name], 0, CHARINDEX(' ', [Name]))
    WHEN 'A' THEN SUBSTRING([Name], CHARINDEX(' ', [Name]) + 1, LEN([Name]) - CHARINDEX(' ', [Name]) + 1) + ', A'
    WHEN 'An' THEN SUBSTRING([Name], CHARINDEX(' ', [Name]) + 1, LEN([Name]) - CHARINDEX(' ', [Name]) + 1) + ', An'
    WHEN 'The' THEN SUBSTRING([Name], CHARINDEX(' ', [Name]) + 1, LEN([Name]) - CHARINDEX(' ', [Name]) + 1) + ', The'
    ELSE [Name]
END
