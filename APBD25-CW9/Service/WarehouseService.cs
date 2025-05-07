using System.Data;
using System.Data.Common;
using APBD25_CW9.Models;
using Microsoft.Data.SqlClient;

namespace APBD25_CW9.Service;

public class WarehouseService : IWarehouseService
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public WarehouseService(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("Default");
    }

    public async Task<int> sqlOperation(WarehouseDto warehouseDto, CancellationToken cancellationToken)
    {
        if (warehouseDto.Amount <= 0)
            return -400;
        string productCheckCommand = @"SELECT COUNT(*) from Product where IdProduct =@IdProduct;";
        string warehouseCheckCommand = @"Select COUNT(*) from [Warehouse] where IdWarehouse = @IdWarehouse";
        string orderCheckCommand = @"SELECT IdOrder,Amount, CreatedAt from [Order]  where IdProduct = @IdProduct";
        string Product_WarehouseCheckCommand = "SELECT COUNT(*) FROM Product_Warehouse where IdOrder =@IdOrder";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync(cancellationToken);
            using (SqlCommand cmd = new SqlCommand(productCheckCommand, conn))
            {
                cmd.Parameters.AddWithValue("@IdProduct", warehouseDto.IdProduct);

                var check = (int)(await cmd.ExecuteScalarAsync());
                if (check == 0)
                    return -404;
            }

            using (SqlCommand cmd = new SqlCommand(warehouseCheckCommand, conn))
            {
                cmd.Parameters.AddWithValue("@IdWarehouse", warehouseDto.IdWarehouse);
                var check = (int)(await cmd.ExecuteScalarAsync());
                if (check == 0)
                    return -404;
            }

            var idOrder = -1;
            using (SqlCommand cmd = new SqlCommand())
            {
                var amount = -1;
                DateTime createdAt = DateTime.MaxValue;
                cmd.Parameters.AddWithValue("@IdProduct", warehouseDto.IdProduct);
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync())
                    {
                        idOrder = reader.GetInt32(0);
                        amount = reader.GetInt32(1);
                        createdAt = reader.GetDateTime(2);
                    }

                    if (amount == -1)
                        return -404;
                    if (amount < warehouseDto.Amount && warehouseDto.CreatedAt > createdAt)
                        return -400;
                }
            }

            using (SqlCommand cmd = new SqlCommand(Product_WarehouseCheckCommand, conn))
            {
                var check = (int)(await cmd.ExecuteScalarAsync());
                if (check != 0)
                    return -404;
            }

            using (SqlCommand cmd = new SqlCommand(warehouseCheckCommand, conn))
            {
                // BEGIN TRANSACTION

                DbTransaction transaction = await conn.BeginTransactionAsync();
                cmd.Transaction = transaction as SqlTransaction;
                try
                {
                    await transaction.CommitAsync();
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
                // END TRANSACTION
            }
        }

        throw new NotImplementedException();
    }
    
    public async Task ProcedureAsync()
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        command.CommandText = "NazwaProcedury";
        command.CommandType = CommandType.StoredProcedure;
        
        command.Parameters.AddWithValue("@Id", 2);
        
        await command.ExecuteNonQueryAsync();
        
    }
}