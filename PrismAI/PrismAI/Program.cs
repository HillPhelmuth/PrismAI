using Blazored.LocalStorage;
using PrismAI.Client.Pages;
using PrismAI.Components;
using PrismAI.Core.Services;
using PrismAI.Hubs;
using PrismAI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
var config = builder.Configuration;
builder.Services.AddHttpClient<IQlooService, QlooService>(nameof(QlooService),
    client =>
    {
        client.BaseAddress = new Uri("https://hackathon.api.qloo.com");
        client.DefaultRequestHeaders.Add("X-Api-Key", config["Qloo:ApiKey"]);
    });
builder.Services.AddSingleton<ILlmRequestService, LlmRequestService>();
builder.Services.AddScoped<IAiAgentService, LlmRequestService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<LocalBrowserStorageService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PrismAI.Client._Imports).Assembly);
app.MapHub<EventHub>("/eventHub");
app.Run();
