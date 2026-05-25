using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyHealthcare.Web;
using MyHealthcare.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>()
                  ?? new ApiSettings();
builder.Services.AddSingleton(apiSettings);

builder.Services.AddSingleton<LocalStorageService>();

builder.Services.AddSingleton<AuthService>(sp => new AuthService(
    new HttpClient { BaseAddress = new Uri(apiSettings.BaseUrl) },
    sp.GetRequiredService<LocalStorageService>()
));

builder.Services.AddTransient<AuthMessageHandler>();

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiSettings.BaseUrl);
}).AddHttpMessageHandler<AuthMessageHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));

builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<MedicineService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<AdminService>();

await builder.Build().RunAsync();
