using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Sales
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
        }

        public async Task<List<Customer>> GetAllByCompanyAsync(
            int companyId,
            bool? isActive = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Customers
                .AsNoTracking()
                .Where(c => c.CompanyId == companyId && !c.IsDeleted);

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            return await query
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<Customer?> GetByCodeAsync(int companyId, string code, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CompanyId == companyId
                                       && c.Code == code
                                       && !c.IsDeleted, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int companyId, string code, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Customers
                .Where(c => c.CompanyId == companyId && c.Code == code && !c.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            await _context.Customers.AddAsync(customer, cancellationToken);
        }

        public void Update(Customer customer)
        {
            _context.Customers.Update(customer);
        }

        public void Delete(Customer customer)
        {
            customer.IsDeleted = true;
            _context.Customers.Update(customer);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}