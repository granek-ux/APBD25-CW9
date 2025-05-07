using APBD25_CW9.Models;
using Microsoft.AspNetCore.Mvc;

namespace APBD25_CW9.Service;

public interface IWarehouseService
{
    
    public Task<int> sqlOperation(WarehouseDto warehouseDto,CancellationToken cancellationToken);
    
}