using ServerCommon.Services.Test;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITestService, TestService>();

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.UseOrleansClient(client =>
{
    client.UseLocalhostClustering();
});

var app = builder.Build();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();