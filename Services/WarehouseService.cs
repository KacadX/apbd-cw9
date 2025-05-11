using System.Data;
using apbd_cw9.Models;
using Microsoft.Data.SqlClient;

namespace apbd_cw9.Services;

public class WarehouseService(IConfiguration configuration) : IWarehouseService
{
    public async Task<int> AddProductToWarehouse(AddProductDto product)
    {
        await using SqlConnection connection = new SqlConnection(configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand("", connection);

        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.CommandText = @"SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            int count = (int)await command.ExecuteScalarAsync();

            if (count <= 0)
            {
                throw new Exception($"Product {product.IdProduct} does not exist");
            }
            
            command.CommandText = @"SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
            count = (int)await command.ExecuteScalarAsync();

            if (count <= 0)
            {
                throw new Exception($"Warehouse {product.IdWarehouse} does not exist");
            }
            
            command.CommandText = @"
            SELECT IdOrder
            FROM [Order]
            WHERE [Order].IdProduct = @IdProduct
            AND [Order].Amount = @Amount
            AND [Order].CreatedAt < @CreatedAt
            ";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            command.Parameters.AddWithValue("@Amount", product.Amount);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);
            var idOrder = await command.ExecuteScalarAsync();

            if (idOrder == null)
            {
                throw new Exception($"Order for product id: {product.IdProduct} does not exist");
            }
            
            command.CommandText = @"
            SELECT COUNT(*)
            FROM Product_Warehouse
            WHERE IdOrder = @IdOrder
            ";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            count = (int)await command.ExecuteScalarAsync();

            if (count > 0)
            {
                throw new Exception($"Order for product {product.IdProduct} has been already fulfilled");
            }
            
            command.CommandText = @"
            UPDATE [Order]
            SET [Order].FulfilledAt = @FulfilledAt
            WHERE [Order].IdOrder = @IdOrder
            ";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
            await command.ExecuteNonQueryAsync();
            
            command.CommandText = @"
            SELECT Price
            FROM Product
            WHERE IdProduct = @IdProduct
            ";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            decimal price = (decimal)await command.ExecuteScalarAsync();
            
            command.CommandText = @"
            INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
            VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)
            SELECT CAST(SCOPE_IDENTITY() as int);
            ";
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", product.Amount);
            command.Parameters.AddWithValue("@Price", price * product.Amount);
            command.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);

            int result = (int)await command.ExecuteScalarAsync();

            await transaction.CommitAsync();
            return result;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> AddProductToWarehouseProcedure(AddProductDto product)
    {
        string command = "AddProductToWarehouse";

        await using SqlConnection conn = new SqlConnection(configuration.GetConnectionString("Default"));
        await using SqlCommand cmd = new SqlCommand(command, conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@IdProduct", product.IdProduct);
        cmd.Parameters.AddWithValue("@IdWarehouse", product.IdWarehouse);
        cmd.Parameters.AddWithValue("@Amount", product.Amount);
        cmd.Parameters.AddWithValue("@CreatedAt", product.CreatedAt);

        await conn.OpenAsync();
        int result = (int)await cmd.ExecuteScalarAsync();

        return result;
    }
}