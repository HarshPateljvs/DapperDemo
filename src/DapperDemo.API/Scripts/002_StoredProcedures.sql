USE DapperDemoDb;

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Employees')
BEGIN
    CREATE TABLE Employees (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Department NVARCHAR(100) NOT NULL,
        Salary DECIMAL(18, 2) NOT NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetAllEmployees]') AND type IN (N'P', N'PC')
)
BEGIN
    EXEC('
    CREATE PROCEDURE sp_GetAllEmployees
    AS
    BEGIN
        SELECT * FROM Employees;
    END
    ');
END;

IF NOT EXISTS (
    SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_GetEmployeeById]') AND type IN (N'P', N'PC')
)
BEGIN
    EXEC('
    CREATE PROCEDURE sp_GetEmployeeById
        @Id INT
    AS
    BEGIN
        SELECT * FROM Employees WHERE Id = @Id;
    END
    ');
END;

IF NOT EXISTS (
    SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_AddEmployee]') AND type IN (N'P', N'PC')
)
BEGIN
    EXEC('
    CREATE PROCEDURE sp_AddEmployee
        @Name NVARCHAR(100),
        @Department NVARCHAR(100),
        @Salary DECIMAL(18, 2),
        	@Id int
    AS
    BEGIN
        INSERT INTO Employees (Name, Department, Salary)
        VALUES (@Name, @Department, @Salary);
    END
    ');
END;

IF NOT EXISTS (
    SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_UpdateEmployee]') AND type IN (N'P', N'PC')
)
BEGIN
    EXEC('
    CREATE PROCEDURE sp_UpdateEmployee
        @Id INT,
        @Name NVARCHAR(100),
        @Department NVARCHAR(100),
        @Salary DECIMAL(18, 2)
    AS
    BEGIN
        UPDATE Employees
        SET Name = @Name,
            Department = @Department,
            Salary = @Salary
        WHERE Id = @Id;
    END
    ');
END;

IF NOT EXISTS (
    SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DeleteEmployee]') AND type IN (N'P', N'PC')
)
BEGIN
    EXEC('
    CREATE PROCEDURE sp_DeleteEmployee
        @Id INT
    AS
    BEGIN
        DELETE FROM Employees WHERE Id = @Id;
    END
    ');
END;
