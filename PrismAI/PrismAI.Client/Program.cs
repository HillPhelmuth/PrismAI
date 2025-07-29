using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PrismAI.Client.Services;
using PrismAI.Core.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
IConfiguration config = builder.Configuration;
builder.Services.AddSingleton(config);
//builder.Services.AddHttpClient<IQlooService, QlooServiceClient>(nameof(QlooServiceClient), client =>
//{
//    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
//});
builder.Services.AddHttpClient<ILlmRequestService, LlmRequestServiceClient>(nameof(LlmRequestServiceClient), client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});
builder.Services.AddScoped<IAiAgentService, LlmAgentClientService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<LocalBrowserStorageService>();
await builder.Build().RunAsync();
