CREATE TRIGGER TR_PreventDeleteProductLinkedToOrders
ON Products
INSTEAD OF DELETE
AS
BEGIN
    IF EXISTS (
        SELECT 1 FROM DELETED d
        JOIN OrderPositions op ON op.ProductId = d.ID
    )
    BEGIN
        RAISERROR('Cannot delete product that is linked to orders.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    DELETE FROM Products WHERE ID IN (SELECT ID FROM DELETED);
END