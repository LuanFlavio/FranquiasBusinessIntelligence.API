using DataAccess;
using DomainDependencyInjection;
using Lamar.Microsoft.DependencyInjection;
using Lamar;
using Microsoft.OpenApi.Models;
using System.Collections.Concurrent;
using System.Reflection;
using FranquiasBusinessIntelligence.API.Services.Session;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLamar((context, registry) =>
{
    registry.AddDbContext<FranquiasBIDbContext>();

    registry.Include(DomainServiceRegister.GetRegister());

    registry.For<ConcurrentDictionary<string, Session>>().Use<ConcurrentDictionary<string, Session>>().Singleton();

    registry.AddEndpointsApiExplorer();
    registry.AddSwaggerGen(opt =>
    {

        opt.AddSecurityDefinition("token", new OpenApiSecurityScheme()
        {
            Reference = new OpenApiReference()
            {
                Id = "token",
                Type = ReferenceType.SecurityScheme,
            },
            Description = "Token provided by API",
            Name = "api-fbi-token",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
        });

        var openApiSecurityRequirement = new OpenApiSecurityRequirement();
        openApiSecurityRequirement.Add(
            new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference()
                {
                    Id = "token",
                    Type = ReferenceType.SecurityScheme,
                },
            },
            new List<string>());

        opt.AddSecurityRequirement(openApiSecurityRequirement);

        opt.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        opt.IgnoreObsoleteActions();
        opt.IgnoreObsoleteProperties();
        opt.CustomSchemaIds(type => type.FullName);

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        opt.IncludeXmlComments(xmlPath);
    });
});

// Add services to the container.

//builder.Services.AddDbContext<FranquiasBIDbContext>();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//Cors
builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin", options => 
        options
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("currentPage", "itemsPerPage", "countItems", "totalPages", "api-fbi-token"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

//app.UseAuthorization();

app.MapControllers();

app.Run();
