using Microsoft.Extensions.DependencyInjection;
using Rfsmart.Phoenix.Api.Middleware;
using Rfsmart.Phoenix.Database;
using Rfsmart.Phoenix.Licensing.Interfaces;
using Rfsmart.Phoenix.Licensing.Persistence;
using Rfsmart.Phoenix.Licensing.Services;
using Serilog.Formatting.Json;
using Serilog;
using System.Text.Json.Serialization;
using Rfsmart.Phoenix.Licensing.Web.Middleware;
using Rfsmart.Phoenix.Licensing.Swagger.Filters;
using Rfsmart.Phoenix.Logging;
using Rfsmart.Phoenix.Api.Auth;
using Rfsmart.Phoenix.Api.Config;
using Rfsmart.Phoenix.Api.Healthchecks;
using Rfsmart.Phoenix.Common.Config;
using Rfsmart.Phoenix.Configuration;
using Rfsmart.Phoenix.Licensing.Web.Auth;
using Rfsmart.Phoenix.Licensing.Models;
using Rfsmart.Phoenix.Licensing.Extensions;


Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(formatter: new JsonFormatter(renderMessage: true))
            .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppLogging();

// Add services to the container.

builder
    .Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull;
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    Swagger.ConfigureOptions(builder.Configuration)(c);

    List<string> xmlFiles = Directory
        .GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly)
        .ToList();

    foreach (string fileName in xmlFiles)
    {
        var xmlFilePath = Path.Combine(AppContext.BaseDirectory, fileName);

        if (File.Exists(xmlFilePath))
            c.IncludeXmlComments(xmlFilePath);
    }

    c.OperationFilter<TenantHeaderFilter>();
    c.OperationFilter<OrganizationHeaderFilter>();

    c.UseInlineDefinitionsForEnums();
});

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

builder.Services.AddForwardedHeaders(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddRfsmartJwtAuthentication(
    new()
    {
        UseCloudConfig = () => builder.Environment.IsProduction(),
        ConfigureAuthorizationOptions = (authorization, sp) =>
        {
            var auth0 = sp.GetRequiredService<IOptionsService<Auth0Options>>().Value;
            authorization.AddLicensingPolicies(auth0.Issuer);
        },
    }
);

builder
    .Services.AddOptions<DeployOptions>()
    .AddService()
    .BindConfiguration(DeployOptions.Position);

builder.Services.AddTenantContext();
builder.Services.AddUserContext();

builder.Services.AddAwsServices(builder);
builder.Services.AddData(builder.Configuration);

builder.Services.AddScoped<IFeatureDefinitionRepository, FeatureDefinitionRepository>();
builder.Services.AddScoped<IFeatureIssueRepository, FeatureIssueRepository>();
builder.Services.AddScoped<IFeatureIssueService, FeatureIssueService>();
builder.Services.AddScoped<IFeatureTrackingRepository, TimestreamFeatureTrackingRepository>();
builder.Services.AddScoped<IFeatureTrackingService, FeatureTrackingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Configure the HTTP request pipeline.
app.UseHealthChecks(
                "/healthcheck",
                new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    ResponseWriter = ApiHealthCheckWriters.BuildDefault(),
                }
            ).UseSwagger()
    .UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RF-SMART Licensing API");
        c.RoutePrefix = string.Empty;
    })
    .UseHttpsRedirection()
    .UseTenantContext().UseUserContext()
    .UseMiddleware<TenantContextValidationMiddleware>()
    .UseRequestLogging()
    .UseAuthentication()
    .UseTenantContext()
    .UseUserContext()
    .UseAuthorization();

app.MapControllers();

await app.RunAsync();
