--get size of db:
WITH SchemaSizes AS (
    SELECT SCHEMA_NAME(schema_id) AS SchemaName,
    SUM(used_pages * 8.0 / 1024) AS UsedSpaceMB,
    SUM(reserved_pages * 8.0 / 1024) AS ReservedSpaceMB
FROM sys.tables t
JOIN sys.indexes i ON t.object_id = i.object_id
CROSS APPLY (
    SELECT SUM(used_page_count) AS used_pages,
SUM(reserved_page_count) AS reserved_pages
FROM sys.dm_db_partition_stats
WHERE object_id = t.object_id
AND index_id = i.index_id
    ) AS partition_stats
GROUP BY SCHEMA_NAME(schema_id)
    )
SELECT SchemaName,
    UsedSpaceMB,
    ReservedSpaceMB,
(UsedSpaceMB + ReservedSpaceMB) AS TotalSpaceMB
FROM SchemaSizes
ORDER BY TotalSpaceMB DESC;
