using System.Net;
using DatabaseSchema;
using DataServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

var endpoints = builder.Configuration.GetSection("Endpoints")
    .Get<Dictionary<string, KestrelEndpoint>>();

builder.WebHost.ConfigureKestrel(options => {
    if (endpoints == null) return;
    foreach (var ep in endpoints.Values) {
        var uri = new Uri(ep.Url);
        var ip = uri.Host switch {
            "localhost" => IPAddress.Loopback,
            "*" or "0.0.0.0" => IPAddress.Any,
            "::" => IPAddress.IPv6Any,
            _ => IPAddress.Parse(uri.Host)
        };
        options.Listen(ip, uri.Port, listen => {
            if (Enum.TryParse<HttpProtocols>(ep.Protocols, out var protocols))
                listen.Protocols = protocols;
            if (uri.Scheme == "https" && !ep.UseH2C)
            {
                if (ep.Certificate != null && File.Exists(ep.Certificate.Path))
                    listen.UseHttps(ep.Certificate.Path, ep.Certificate.Password);
                else
                    listen.UseHttps();
            }
        });
    }
});

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddOpenApi();

var appConfig = builder.Configuration
    .GetSection("Application")
    .Get<ApplicationConfiguration>();
builder.Services.AddSingleton(appConfig ?? throw new InvalidOperationException("No application configurations set!"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

if (app.Environment.IsDevelopment()) {
    app.MapGrpcReflectionService();
}

app.Run();

