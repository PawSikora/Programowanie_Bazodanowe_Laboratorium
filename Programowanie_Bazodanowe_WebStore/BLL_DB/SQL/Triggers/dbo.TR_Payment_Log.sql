
CREATE TRIGGER TR_Payment_Log
ON Orders
AFTER UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT * FROM INSERTED i
        JOIN DELETED d ON i.ID = d.ID
        WHERE i.IsPaid = 1 AND d.IsPaid = 0
    )
    BEGIN
        INSERT INTO PaymentLog (OrderID, PaidAmount, PaidDate)
        SELECT i.ID,
               (SELECT SUM(op.Price * op.Amount) FROM OrderPositions op WHERE op.OrderID = i.ID),
               GETDATE()
        FROM INSERTED i;
    END
END