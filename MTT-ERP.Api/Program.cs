using Microsoft.EntityFrameworkCore;
using MTT_ERP.Api.Data;
using MTT_ERP.Shared;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<MTT_ERPDbContext>(options =>
    options.UseSqlite("Data Source=mtt_erp.db")
);

var app = builder.Build();

// Auto-create database & seed realistic Dubai B2B sample data on first run
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MTT_ERPDbContext>();
    db.Database.Migrate();

    if (!db.Invoices.Any())
    {
        db.Invoices.AddRange(
            new Invoice
            {
                InvoiceCode = "#3210",
                CustomerName = "Al Noor Logistics LLC",
                CustomerEmail = "accounts@alnoorlogistics.ae",
                CreateDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(10),
                Quantity = 12,
                Subtotal = 18500m,
                Status = InvoiceStatus.Pending,
            },
            new Invoice
            {
                InvoiceCode = "#4213",
                CustomerName = "Emirates Cloud Tech",
                CustomerEmail = "billing@emiratescloud.ae",
                CreateDate = DateTime.Today.AddDays(-8),
                DueDate = DateTime.Today.AddDays(7),
                Quantity = 32,
                Subtotal = 42000m,
                Status = InvoiceStatus.Completed,
            },
            new Invoice
            {
                InvoiceCode = "#1230",
                CustomerName = "Marina Retail Group",
                CustomerEmail = "finance@marinaretail.ae",
                CreateDate = DateTime.Today.AddDays(-18),
                DueDate = DateTime.Today.AddDays(-3),
                Quantity = 42,
                Subtotal = 28400m,
                Status = InvoiceStatus.Pending,
            },
            new Invoice
            {
                InvoiceCode = "#1845",
                CustomerName = "Palm Hospitality Co.",
                CustomerEmail = "ap@palmhospitality.ae",
                CreateDate = DateTime.Today.AddDays(-12),
                DueDate = DateTime.Today.AddDays(-1),
                Quantity = 86,
                Subtotal = 12500m,
                Status = InvoiceStatus.Canceled,
            },
            new Invoice
            {
                InvoiceCode = "#2910",
                CustomerName = "DIFC Fintech Labs",
                CustomerEmail = "pay@difcfintech.ae",
                CreateDate = DateTime.Today.AddDays(-3),
                DueDate = DateTime.Today.AddDays(14),
                Quantity = 56,
                Subtotal = 64000m,
                Status = InvoiceStatus.Completed,
            },
            new Invoice
            {
                InvoiceCode = "#3098",
                CustomerName = "Jebel Ali Trading",
                CustomerEmail = "ops@jebelalitrading.ae",
                CreateDate = DateTime.Today.AddDays(-20),
                DueDate = DateTime.Today.AddDays(-5),
                Quantity = 45,
                Subtotal = 14678m,
                Status = InvoiceStatus.Pending,
            }
        );
        db.SaveChanges();
    }
}

// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
//     app.MapScalarApiReference();
// }

app.MapOpenApi();
app.MapScalarApiReference();

// GET all invoices (with optional status or search query)
app.MapGet("/api/invoices", async (string? status, string? search, MTT_ERPDbContext db) =>
{
    var query = db.Invoices.AsQueryable();

    if (
        !string.IsNullOrWhiteSpace(status)
        && Enum.TryParse<InvoiceStatus>(status, true, out var parsedStatus)
    )
        query = query.Where(i => i.Status == parsedStatus);

    if (!string.IsNullOrWhiteSpace(search))
    {
        var term = search.ToLower();
        query = query.Where(i =>
            i.CustomerName.ToLower().Contains(term)
            || i.CustomerEmail.ToLower().Contains(term)
            || i.InvoiceCode.ToLower().Contains(term)
        );
    }

    return await query.OrderByDescending(i => i.Id).ToListAsync();
});

// GET KPI summary metrics
app.MapGet("/api/invoices/summary", async (MTT_ERPDbContext db) =>
{
    var all = await db.Invoices.ToListAsync();
    var active = all.Where(i => i.Status != InvoiceStatus.Canceled).ToList();

    var total = active.Sum(i => i.TotalAmount);
    var paid = active.Where(i => i.Status == InvoiceStatus.Completed).Sum(i => i.TotalAmount);
    var unpaid = active.Where(i => i.Status == InvoiceStatus.Pending).Sum(i => i.TotalAmount);
    var overdue = active
        .Where(i => i.Status == InvoiceStatus.Pending && i.DueDate < DateTime.Today)
        .Sum(i => i.TotalAmount);
    var overduePct = unpaid > 0 ? (int)Math.Round((overdue / unpaid) * 100) : 0;

    return new InvoiceSummaryDto(total, paid, unpaid, overdue, overduePct);
});

// GET aggregated B2B customers from invoices
app.MapGet("/api/customers", async (MTT_ERPDbContext db) =>
{
    var invoices = await db.Invoices.ToListAsync();

    return invoices
        .GroupBy(i => new { i.CustomerName, i.CustomerEmail })
        .Select(g => new CustomerSummaryDto(
            g.Key.CustomerName,
            g.Key.CustomerEmail,
            g.Count(),
            g.Where(x => x.Status != InvoiceStatus.Canceled).Sum(x => x.TotalAmount),
            g.Where(x => x.Status == InvoiceStatus.Pending).Sum(x => x.TotalAmount)
        ))
        .OrderByDescending(c => c.TotalBilled)
        .ToList();
});

// POST create invoice
app.MapPost("/api/invoices", async (Invoice invoice, MTT_ERPDbContext db) =>
{
    invoice.InvoiceCode = $"#{Random.Shared.Next(1000, 9999)}";
    db.Invoices.Add(invoice);
    await db.SaveChangesAsync();
    return Results.Created($"/api/invoices/{invoice.Id}", invoice);
});

// PATCH toggle invoice status
app.MapPatch(
    "/api/invoices/{id}/status",
    async (int id, InvoiceStatus status, MTT_ERPDbContext db) =>
    {
        var invoice = await db.Invoices.FindAsync(id);
        if (invoice is null)
            return Results.NotFound();

        invoice.Status = status;
        await db.SaveChangesAsync();
        return Results.Ok(invoice);
    }
);

// DELETE invoice
app.MapDelete("/api/invoices/{id}", async (int id, MTT_ERPDbContext db) =>
{
    var invoice = await db.Invoices.FindAsync(id);
    if (invoice is null)
        return Results.NotFound();

    db.Invoices.Remove(invoice);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
