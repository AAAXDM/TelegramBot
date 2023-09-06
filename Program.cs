using Microsoft.EntityFrameworkCore;
using System.Net;
using Telegram.Bot;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json");


string? connection = ConnectionHelper.GetConnectionString(builder.Configuration);

Environment.SetEnvironmentVariable
("DATABASE_URL", builder.Configuration["DatabaseUrl"]);

if (connection == null)
{
    throw new Exception("No connection string to database");
}

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Loopback, 5000);
}).UseContentRoot(Environment.CurrentDirectory);

var client =  builder.Services.AddHttpClient("telegram_bot_client")
                .AddTypedClient<ITelegramBotClient>((httpClient) =>
                {                    
                    TelegramBotClientOptions options = new(builder.Configuration["Token"]);
                    return new TelegramBotClient(options, httpClient);
                });


builder.Services.AddDbContext<DataContext>(opt => opt.UseNpgsql(connection));
builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddHostedService<ConfigureWebhook>();
builder.Services.AddControllers().AddNewtonsoftJson();

var app = builder.Build();

app.MapBotWebhookRoute<TelegramBotController>(route: builder.Configuration["Route"]);

app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.Run();


