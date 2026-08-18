using Microsoft.EntityFrameworkCore;
using Shipping.Application.Interfaces;
using Shipping.Domain.Entities;
using Shipping.Infrastructure.Persistence;

namespace Shipping.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ShippingDbContext _context;

    public CustomerRepository(ShippingDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
    {
        await _context.Customers.AddAsync(customer, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Customers.AnyAsync(c => c.Id == id, ct);
    }
}

public class ShipmentRepository : IShipmentRepository
{
    private readonly ShippingDbContext _context;

    public ShipmentRepository(ShippingDbContext context)
    {
        _context = context;
    }

    public async Task<Shipment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Shipments
            .Include(s => s.StatusHistory)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IReadOnlyList<Shipment>> GetAllAsync(int skip = 0, int take = 50, CancellationToken ct = default)
    {
        return await _context.Shipments
            .Include(s => s.StatusHistory)
            .OrderByDescending(s => s.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Shipment shipment, CancellationToken ct = default)
    {
        await _context.Shipments.AddAsync(shipment, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Shipment shipment, CancellationToken ct = default)
    {
        _context.Shipments.Update(shipment);
        await _context.SaveChangesAsync(ct);
    }
}
