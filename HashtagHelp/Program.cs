using HashtagHelp.DAL;
using HashtagHelp.Services.Implementations;
using HashtagHelp.Services.Implementations.Loggers;
using HashtagHelp.Services.Implementations.InstagramData;
using HashtagHelp.Services.Implementations.InstData2;
using HashtagHelp.Services.Implementations.RocketAPI;
using HashtagHelp.Services.Implementations.InstaParser;
using HashtagHelp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var connectionString = builder.Configuration.GetConnectionString("MYSQL");
if (connectionString != null)
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        builder => builder.CommandTimeout(30));
    });

builder.Services.AddScoped<IFunnelService, FunnelService>();
builder.Services.AddScoped<IApiRequestService, InstaParserAPIRequestService>();
builder.Services.AddScoped<IFollowersGetterService, RocketAPIFollowersGetterService>();
builder.Services.AddScoped<IFollowingTagsGetterService, InstData2FollowingTagsGetterService>();
builder.Services.AddScoped<IIdGetterService, RocketAPIIdGetterService>();
builder.Services.AddScoped<IDataRepository, DataRepository>();
builder.Services.AddScoped<IParserDataService, ParserDataService>();
builder.Services.AddScoped<IHashtagApiRequestService, RocketAPIRequestService>();
builder.Services.AddScoped<IProcessLogger, DesktopFileLogger>();
builder.Services.AddScoped<IGoogleApiRequestService, GoogleRequestApiService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
