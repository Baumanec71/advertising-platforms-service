using advertising_platforms_service.DAL.Interfaces;
using advertising_platforms_service.DAL.Repositories;
using advertising_platforms_service.Service.Implementations;
using advertising_platforms_service.Service.Interfaces;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddSingleton<IAdvertisingPlatformRepository, AdvertisingPlatformRepository>();
builder.Services.AddScoped<IAdvertisingPlatformService, AdvertisingPlatformService>();
builder.Services.AddScoped<IWorkToTxtFile, WorkToFile>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
