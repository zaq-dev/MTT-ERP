using Microsoft.EntityFrameworkCore;
using MTT_ERP.Shared;

namespace MTT_ERP.Api.Data;

public class MTT_ERPDbContext(DbContextOptions<MTT_ERPDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
}
