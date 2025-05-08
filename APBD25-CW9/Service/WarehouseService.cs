using System.Data;
using System.Data.Common;
using APBD25_CW9.Models;
using Microsoft.Data.SqlClient;
using NuGet.Packaging.Signing;

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
        string productCheckCommand = @"SELECT Price from Product where IdProduct =@IdProduct;";
        string warehouseCheckCommand = @"Select COUNT(*) from [Warehouse] where IdWarehouse = @IdWarehouse";
        string orderCheckCommand =
            @"SELECT IdOrder, CreatedAt from [Order]  where IdProduct = @IdProduct and Amount = @Amount;";
        string Product_WarehouseCheckCommand = "SELECT COUNT(*) FROM Product_Warehouse where IdOrder =@IdOrder";
        string orderUpdateCommand = "UPDATE [Order] set FulfilledAt = @DateNow where IdOrder = @IdOrder;";
        string intsetProduct_WarehouseCommand = @"Insert Into Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) 
                                                    values (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);SELECT SCOPE_IDENTITY();";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            float productPrice = 0;
            await conn.OpenAsync(cancellationToken);
            using (SqlCommand cmd = new SqlCommand(productCheckCommand, conn))
            {
                var idOrder = -1;
                cmd.Parameters.AddWithValue("@IdProduct", warehouseDto.IdProduct);

                var checkP = (float)(await cmd.ExecuteScalarAsync(cancellationToken));
                if (checkP == 0)
                    return -404;
                productPrice = checkP;


                cmd.Parameters.Clear();

                cmd.CommandText = warehouseCheckCommand;

                cmd.Parameters.AddWithValue("@IdWarehouse", warehouseDto.IdWarehouse);

                DateTime createdAt = DateTime.MaxValue;


                var checkW = (int)(await cmd.ExecuteScalarAsync(cancellationToken));
                if (checkW == 0)
                    return -404;

                cmd.Parameters.Clear();

                cmd.CommandText = orderCheckCommand;


                cmd.Parameters.AddWithValue("@IdProduct", warehouseDto.IdProduct);
                cmd.Parameters.AddWithValue("@Amount", warehouseDto.Amount);


                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        idOrder = reader.GetInt32(0);
                        createdAt = reader.GetDateTime(1);
                    }

                    if (idOrder == -1)
                        return -404;

                    if (warehouseDto.CreatedAt > createdAt)
                        return -400;
                }

                cmd.Parameters.Clear();


                DbTransaction transaction = await conn.BeginTransactionAsync();
                cmd.Transaction = transaction as SqlTransaction;
                try
                {
                    DateTime timeNow = DateTime.Now;
                    cmd.CommandText = orderUpdateCommand;
                    cmd.Parameters.AddWithValue("@IdOrder", idOrder);
                    cmd.Parameters.AddWithValue("@DateNow", timeNow);
                    
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                    
                    
                    cmd.Parameters.Clear();
                    cmd.CommandText = intsetProduct_WarehouseCommand;
                    
                    productPrice *= warehouseDto.Amount;
                    cmd.Parameters.AddWithValue("@IdWarehouse", warehouseDto.IdWarehouse);
                    cmd.Parameters.AddWithValue("@IdProduct", warehouseDto.IdProduct);
                    cmd.Parameters.AddWithValue("@IdOrder", idOrder);
                    cmd.Parameters.AddWithValue("@Amount", warehouseDto.Amount);
                    cmd.Parameters.AddWithValue("@Price", productPrice);
                    cmd.Parameters.AddWithValue("@CreatedAt", timeNow);
                    
                    var insertedId = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
                    
                    await transaction.CommitAsync();
                    return insertedId;
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        return 0;
    }

    public async Task<int> ProcedureAsync(WarehouseDto warehouseDto, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        await connection.OpenAsync(cancellationToken);

        command.CommandText = "AddProductToWarehouse";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@IdProduct", warehouseDto.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", warehouseDto.IdWarehouse);
        command.Parameters.AddWithValue("@Amount", warehouseDto.Amount);
        command.Parameters.AddWithValue("@CreatedAt", warehouseDto.CreatedAt);

        int result = (int) await command.ExecuteScalarAsync(cancellationToken);

        return result;
    }
}