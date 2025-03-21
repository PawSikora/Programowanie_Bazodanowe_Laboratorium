CREATE PROCEDURE LoginUser
    @Login NVARCHAR(50),
    @Password NVARCHAR(50)
AS
BEGIN
    SELECT COUNT(*) AS Success
    FROM Users
    WHERE Login = @Login AND Password = @Password AND IsActive = 1;
END