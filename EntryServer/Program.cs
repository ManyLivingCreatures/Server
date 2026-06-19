using EntryServer.Services.Connection;
using EntryServer.Services.Lifecycle;
using ServerCommon.Services.Test;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITestService, TestService>();

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IClusterClientLifecycleTaskCompletionSources>();

builder.Services.AddSingleton<ILifecycleParticipant<IClusterClientLifecycle>, IClusterClientLifecycleParticipant>();

builder.UseOrleansClient(client =>
{
    client.UseLocalhostClustering();
});

builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ConnectionManager>());

var app = builder.Build();

app.UseWebSockets();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();