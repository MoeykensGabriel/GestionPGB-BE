using GestionPGB_BE.API.Domain.Entities;
using GestionPGB_BE.API.Domain.Interfaces;
using GestionPGB_BE.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionPGB_BE.API.Infrastructure.Repositories;

public class WorkshopOrderRepository : IWorkshopOrderRepository
{
    private readonly AppDbContext _context;

    public WorkshopOrderRepository(AppDbContext context) => _context = context;

    public async Task<WorkshopOrder> CreateAsync(WorkshopOrder order)
    {
        order.Id = Guid.NewGuid();
        foreach (var item in order.Items)
            item.Id = Guid.NewGuid();

        _context.WorkshopOrders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<WorkshopOrder?> GetByIdAsync(Guid id) =>
        await _context.WorkshopOrders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IEnumerable<WorkshopOrder>> GetAllAsync() =>
        await _context.WorkshopOrders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<WorkshopOrder> UpdateAsync(WorkshopOrder order)
    {
        _context.WorkshopOrders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<WorkshopOrderItem?> GetItemByIdAsync(Guid itemId) =>
        await _context.WorkshopOrderItems
            .Include(i => i.WorkshopOrder)
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == itemId);

    public async Task<WorkshopOrderItem> UpdateItemAsync(WorkshopOrderItem item)
    {
        _context.WorkshopOrderItems.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }
}
