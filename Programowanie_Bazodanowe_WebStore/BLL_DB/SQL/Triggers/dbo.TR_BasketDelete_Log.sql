CREATE TRIGGER TR_BasketDelete_Log
ON BasketPositions
AFTER DELETE
AS
BEGIN
    INSERT INTO BasketLog (UserID, ProductID, ActionTime, Action)
    SELECT d.UserID, d.ProductID, GETDATE(), 'Removed from basket'
    FROM DELETED d;
END