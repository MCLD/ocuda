SET NOCOUNT ON;

DECLARE @lang_id INT = (SELECT [Id] FROM [Languages] WHERE [IsDefault] = 1);

DECLARE @feature_id INT, @feature_name NVARCHAR(255), @feature_bodytext NVARCHAR(MAX);

DECLARE feature_cursor CURSOR FOR
	SELECT [Id], [Name], [BodyText]
	FROM [Features]
	ORDER BY [Id];

OPEN feature_cursor

FETCH NEXT FROM feature_cursor
INTO @feature_id, @feature_name, @feature_bodytext;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- name
	INSERT INTO [Segments] ([IsActive], [Name]) VALUES (1, 'Feature name for ' + @feature_name);
	DECLARE @segment_id INT = SCOPE_IDENTITY();
	
	PRINT 'Updating feature ' + CAST(@feature_id as NVARCHAR(8)) + ' into with SegmentId: ' + CAST(@segment_id as NVARCHAR(8));
	UPDATE [Features] SET [NameSegmentId] = @segment_id WHERE [Id] = @feature_id;

	PRINT 'Inserting feature ' + CAST(@feature_id as NVARCHAR(8)) + ' segment name';
	INSERT INTO [SegmentTexts] ([SegmentId], [LanguageId], [Text])
		VALUES (@segment_id, @lang_id, @feature_name);

	IF @feature_bodytext IS NOT NULL
	BEGIN
		-- text
		PRINT 'Inserting feature ' + CAST(@feature_id as NVARCHAR(8)) + ' into Segments: ' + @feature_name;
		INSERT INTO [Segments] ([IsActive], [Name]) VALUES (1, 'Feature text for ' + @feature_name);
		SET @segment_id = SCOPE_IDENTITY();
	
		PRINT 'Updating feature ' + CAST(@feature_id as NVARCHAR(8)) + ' into with SegmentId: ' + CAST(@segment_id as NVARCHAR(8));
		UPDATE [Features] SET [TextSegmentId] = @segment_id WHERE [Id] = @feature_id;

		PRINT 'Inserting feature ' + CAST(@feature_id as NVARCHAR(8)) + ' segment text';
		INSERT INTO [SegmentTexts] ([SegmentId], [LanguageId], [Text])
			VALUES (@segment_id, @lang_id, @feature_bodytext);
	END

	FETCH NEXT FROM feature_cursor
		INTO @feature_id, @feature_name, @feature_bodytext;
END
CLOSE feature_cursor;
DEALLOCATE feature_cursor;

DECLARE @location_id INT, @location_text NVARCHAR(MAX), @location_name NVARCHAR(255);

DECLARE location_feature_cursor CURSOR FOR
SELECT lf.[LocationId], lf.[FeatureId], lf.[Text], f.[Name], l.[Name]
	FROM [LocationFeatures] lf
	INNER JOIN [Features] f on lf.[FeatureId] = f.[Id]
	INNER JOIN [Locations] l on lf.[LocationId] = l.[Id]
	WHERE lf.[Text] IS NOT NULL
	ORDER BY lf.[LocationId], lf.[FeatureId];

OPEN location_feature_cursor

FETCH NEXT FROM location_feature_cursor
	INTO @location_id, @feature_id, @location_text, @feature_name, @location_name;

WHILE @@FETCH_STATUS = 0
BEGIN
	PRINT 'Inserting feature ' + @feature_name + ' for location ' + @location_name + ' into Segments';
	INSERT INTO [Segments] ([IsActive], [Name]) VALUES (1, 'Location ' + @location_name + ' Feature ' + @feature_name);
	SET @segment_id = SCOPE_IDENTITY();

	PRINT 'Updating feature ' + @feature_name + ' for location ' + @location_name + ' with SegmentId ' + CAST(@segment_id AS NVARCHAR(8));
	UPDATE [LocationFeatures] SET [SegmentId] = @segment_id WHERE [LocationId] = @location_id AND [FeatureId] = @feature_id;

	PRINT 'Inserting feature ' + @feature_name + ' for location ' + @location_name + ' into SegmentTexts';
	INSERT INTO [SegmentTexts] ([SegmentId], [LanguageId], [Text])
		VALUES (@segment_id, @lang_id, @location_text);

	FETCH NEXT FROM location_feature_cursor
		INTO @location_id, @feature_id, @location_text, @feature_name, @location_name;
END
CLOSE location_feature_cursor;
DEALLOCATE location_feature_cursor;

-- ensure features have slugs/stubs
UPDATE [Features]
SET [Stub] = LTRIM(RTRIM(REPLACE([Name], ' ', '-')))
WHERE [Stub] IS NULL;

-- ensure slugs/stubs are all lowercase
UPDATE [Features]
SET [Stub] = LOWER([Stub]);
