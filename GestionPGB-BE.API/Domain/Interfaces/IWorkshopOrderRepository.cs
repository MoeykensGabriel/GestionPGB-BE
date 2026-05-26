using GestionPGB_BE.API.Domain.Entities;

namespace GestionPGB_BE.API.Domain.Interfaces;

public interface IWorkshopOrderRepository
{
    Task<WorkshopOrder> CreateAsync(WorkshopOrder order);
    Task<WorkshopOrder?> GetByIdAsync(Guid id);
    Task<IEnumerable<WorkshopOrder>> GetAllAsync();
    Task<WorkshopOrder> UpdateAsync(WorkshopOrder order);
    Task<WorkshopOrderItem?> GetItemByIdAsync(Guid itemId);
    Task<WorkshopOrderItem> UpdateItemAsync(WorkshopOrderItem item);
}
