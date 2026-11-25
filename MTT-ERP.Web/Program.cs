using MTT_ERP.Web.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5050/";

builder.Services.AddHttpClient("MTT-ERPApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAntiforgery();
app.UseStaticFiles();
app.MapStaticAssets();
app.MapReverseProxy();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
