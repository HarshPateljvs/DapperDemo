use master;
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'DapperDemoDb')
BEGIN
    CREATE DATABASE DapperDemoDb;
END;
