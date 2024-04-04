using HashtagHelp.DAL;
using HashtagHelp.Services.Implementations;
using HashtagHelp.Services.Implementations.Loggers;
using HashtagHelp.Services.Implementations.RocketAPI;
using HashtagHelp.Services.Implementations.InstaParser;
using HashtagHelp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using HashtagHelp.Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWTSettings"));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration.GetSection("JWTSettings:Issuer").Value,
            ValidAudience = builder.Configuration.GetSection("JWTSettings:Audience").Value,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWTSettings:SecretKey").Value))
        };
    });

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        {
            // Указываем, что используется StringEnumConverter для сериализации перечислений
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
        });
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
builder.Services.AddScoped<IDataRepository, DataRepository>();
builder.Services.AddScoped<IParserDataService, ParserDataServiceNew>();
builder.Services.AddScoped<IHashtagApiRequestService, RocketAPIRequestService>();
builder.Services.AddTransient<IProcessLogger, DesktopFileLogger>();
builder.Services.AddScoped<IGoogleApiRequestService, GoogleRequestApiService>();

builder.Services.AddHostedService<TaskManagerService>();
builder.Services.AddHttpClient();



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

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

app.UseAuthentication();

app.MapControllers();

app.Run();



