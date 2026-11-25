# MTT ERP

A UAE-focused B2B invoicing, receivables, and financial analytics ERP application built with **.NET 10**, **Blazor Interactive Server**, **MudBlazor**, **ASP.NET Core Minimal APIs**, **Entity Framework Core (SQLite)**, and **YARP Reverse Proxy**.

## Live Demo

A live sample is hosted on Render: **[Dashboard — MTT ERP](https://mtt-erp.onrender.com/)**

> **Note:** On Render's free tier, the container may spin down after inactivity, so the initial load can take a few seconds. The container uses an ephemeral SQLite database seeded with sample UAE B2B data on startup.

---

## Features

- **Executive Dashboard (`/`)**
  - Real-time KPI summary cards: **Total Receivables**, **Paid**, **Unpaid**, and **Overdue** (with overdue percentage of unpaid).
  - **Recent Activity** table displaying the latest invoices.
  - **UAE VAT & Collection Health** summary tracking collection rate and estimated 5% output VAT.
- **Invoice Management (`/invoices`)**
  - Filter invoices by status (**All**, **Paid**, **Unpaid**) and search by client name, email, or invoice code.
  - Paginated data grid with quick **Mark Paid** (`PATCH`) and **Delete** (`DELETE`) actions.
  - **Create Invoice** modal dialog with form validation and a live **5% UAE VAT** calculation preview (`Subtotal`, `UAE VAT (5%)`, `Total Receivable`).
- **B2B Customer Directory (`/customers`)**
  - Aggregated view of B2B client accounts derived from invoice records, displaying invoice counts, total billed (AED), pending balance (AED), and account standing (**Settled** vs. **Balance Due**).
- **Financial Analytics (`/analytics`)**
  - Executive metrics: **Avg. Invoice Value**, **5% UAE VAT Liability**, **Collection Efficiency**, and **Active B2B Clients**.
  - **Billed Revenue vs. Collected Cash Flow** 6-month rolling line chart (5 baseline months + live current ledger).
  - **Receivables Aging Split** donut chart (**Paid & Cleared**, **Current Unpaid**, **Overdue**).
  - **Top Clients by Billed Volume** bar chart and **Revenue Concentration** breakdown.
- **Modern MudBlazor UI**
  - Responsive navigation drawer, **Dark / Light Mode** toggle (dark mode enabled by default), and `Inter` typography.
- **Interactive API Documentation**
  - Built-in OpenAPI specification (`/openapi/v1.json`) and **Scalar API Reference** (`/scalar`) proxied through the web app via YARP.

---

## Architecture & Project Structure

```text
MTT-ERP/
├── MTT-ERP.slnx                  # .NET 10 solution file
├── Dockerfile                    # Multi-stage Docker build running API + Web in one container
├── MTT-ERP.Shared/               # Shared domain models, validation attributes & DTOs
│   └── InvoiceModels.cs          # Invoice, InvoiceStatus, InvoiceSummaryDto, CustomerSummaryDto
├── MTT-ERP.Api/                  # ASP.NET Core Minimal API + EF Core SQLite backend
│   ├── Data/MTT-ERPDbContext.cs  # EF Core DbContext
│   ├── Migrations/               # EF Core SQLite migrations
│   └── Program.cs                # API endpoints, DB migration, sample seeding, OpenAPI/Scalar
└── MTT-ERP.Web/                  # Blazor Web App (Interactive Server + MudBlazor + YARP)
    ├── Components/
    │   ├── Dialogs/              # CreateInvoiceDialog.razor
    │   ├── Layout/               # MainLayout.razor, ReconnectModal.razor
    │   └── Pages/                # Dashboard, Invoices, Customers, Analytics
    ├── appsettings.json          # ApiBaseUrl & YARP ReverseProxy routes (/api, /scalar, /openapi)
    └── Program.cs
```

### How Requests Flow

- `MTT-ERP.Web` communicates with `MTT-ERP.Api` using a named `HttpClient` (`MTT-ERPApi`) configured via `ApiBaseUrl` (defaulting to `http://localhost:5050/` in development or `http://127.0.0.1:5050/` in `appsettings.json` / Docker).
- **YARP Reverse Proxy** in `MTT-ERP.Web` forwards external requests for `/api/{**catch-all}`, `/scalar/{**catch-all}`, and `/openapi/{**catch-all}` to `MTT-ERP.Api`, allowing both services to be exposed through a single public port in production.

---

## Tech Stack

- **Runtime:** .NET 10 (`net10.0`)
- **Frontend:** Blazor Web App (`InteractiveServer`), [MudBlazor 9](https://mudblazor.com/)
- **Backend:** ASP.NET Core Minimal APIs
- **Data Access:** Entity Framework Core 10 + SQLite (`mtt_erp.db`)
- **Reverse Proxy:** YARP (`Yarp.ReverseProxy` 2.3.0)
- **API Docs:** `Microsoft.AspNetCore.OpenApi` + `Scalar.AspNetCore`

---

## API Endpoints

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `GET` | `/api/invoices` | List invoices (supports optional `status` and `search` query parameters) |
| `GET` | `/api/invoices/summary` | Get KPI metrics (`TotalReceivables`, `PaidAmount`, `UnpaidAmount`, `OverdueAmount`, `OverduePercentage`) |
| `GET` | `/api/customers` | Get aggregated B2B customer billing summaries |
| `POST` | `/api/invoices` | Create a new invoice (auto-generates an `#XXXX` invoice code) |
| `PATCH` | `/api/invoices/{id}/status` | Update an invoice's `InvoiceStatus` (`Completed`, `Pending`, `Canceled`) |
| `DELETE` | `/api/invoices/{id}` | Delete an invoice by ID |

Scalar API Reference is available at `/scalar` (e.g., `http://localhost:5050/scalar` or via the Web app at `http://localhost:8080/scalar`).

---

## Getting Started (Local Development)

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Run Locally

Open two terminals from the repository root (`MTT-ERP/`):

1. **Start the API** (listens on `http://localhost:5050`):
   ```bash
   dotnet run --project MTT-ERP.Api
   ```
   On first startup, the API automatically runs EF Core migrations (`mtt_erp.db`) and seeds 6 sample UAE B2B invoices.

2. **Start the Blazor Web App** (listens on `http://localhost:8080`):
   ```bash
   dotnet run --project MTT-ERP.Web
   ```

3. Open `http://localhost:8080` in your browser.

---

## Running with Docker

The included `Dockerfile` builds both projects and starts `MTT-ERP.Api` internally on port `5050` alongside `MTT-ERP.Web` on `${PORT:-8080}`:

```bash
docker build -t mtt-erp .
docker run -p 8080:8080 mtt-erp
```

Then navigate to `http://localhost:8080` (or `http://localhost:8080/scalar` for the API explorer).
