using apbd_cw9.Models;

namespace apbd_cw9.Services;

public interface IWarehouseService
{
    Task<int> AddProductToWarehouse(AddProductDto product);
    Task<int> AddProductToWarehouseProcedure(AddProductDto product);
}