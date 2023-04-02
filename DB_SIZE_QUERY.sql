SELECT 
    DB_NAME(database_id) AS DatabaseName,
    CONVERT(DECIMAL(10,2), (SUM(size * 8.0) / 1024.0 / 1024.0)) AS DatabaseSizeGB
FROM 
    sys.master_files
GROUP BY 
    database_id
ORDER BY 
    DatabaseSizeGB DESC;
