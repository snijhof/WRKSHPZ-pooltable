using Example.Components;
using Example.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<IImageAnalyzer, ImageAnalyzer>(cfg =>
{
    var key = builder.Configuration["AzureComputerVision:Key"];
    var endpoint = builder.Configuration["AzureComputerVision:Endpoint"];
    cfg.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
    cfg.BaseAddress = new Uri(endpoint);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();