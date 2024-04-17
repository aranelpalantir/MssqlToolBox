DECLARE @TableName NVARCHAR(128)
DECLARE @IndexName NVARCHAR(128)
DECLARE @ObjectId INT
DECLARE @IndexId INT
DECLARE @Fragmentation FLOAT
DECLARE @SQL NVARCHAR(MAX)

DECLARE cur CURSOR FOR
SELECT  t.name AS TableName, ix.name AS IndexName, ips.avg_fragmentation_in_percent AS Fragmentation,ix.object_id,ix.index_id
FROM sys.indexes ix
INNER JOIN sys.tables t ON ix.object_id = t.object_id
INNER JOIN sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'SAMPLED') ips ON ix.object_id = ips.object_id AND ix.index_id = ips.index_id
WHERE ix.type_desc IN ('NONCLUSTERED', 'CLUSTERED')
AND t.is_ms_shipped = 0
AND ix.name IS NOT NULL

OPEN cur
FETCH NEXT FROM cur INTO @TableName, @IndexName, @Fragmentation, @ObjectId, @IndexId

WHILE @@FETCH_STATUS = 0
BEGIN

    PRINT 'Fragmentation status for Index [' + @IndexName + ']: ' + CAST(@Fragmentation AS NVARCHAR) + '%'

	IF @Fragmentation>5 
		BEGIN
		SET @SQL = 'ALTER INDEX ' + QUOTENAME(@IndexName) + ' ON ' + QUOTENAME(@TableName) + ' REBUILD;'
		EXEC(@SQL)

		SET @SQL = 'ALTER INDEX ' + QUOTENAME(@IndexName) + ' ON ' + QUOTENAME(@TableName) + ' REORGANIZE;'
		EXEC(@SQL)

		SET @SQL = 'SELECT @Fragmentation = avg_fragmentation_in_percent FROM sys.dm_db_index_physical_stats (DB_ID(),' + CAST(@ObjectId AS NVARCHAR) + ',' + CAST(@IndexId AS NVARCHAR) + ', NULL, ''SAMPLED'')'
		EXEC sp_executesql @SQL, N'@Fragmentation FLOAT OUTPUT', @Fragmentation OUTPUT

		PRINT 'Fragmentation status for Index [' + @IndexName + '] after Rebuild and Reorganize: ' + CAST(@Fragmentation AS NVARCHAR) + '%'

		SET @SQL = 'UPDATE STATISTICS ' + QUOTENAME(@TableName) + ' ' + QUOTENAME(@IndexName)
		EXEC(@SQL)
	END

    FETCH NEXT FROM cur INTO @TableName, @IndexName, @Fragmentation, @ObjectId, @IndexId
END

CLOSE cur
DEALLOCATE cur

