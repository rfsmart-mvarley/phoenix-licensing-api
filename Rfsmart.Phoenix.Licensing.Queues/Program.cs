using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rfsmart.Phoenix.Accounts.Client;
using Rfsmart.Phoenix.Accounts.Client.Events;
using Rfsmart.Phoenix.Accounts.Queues.EventHandlers;
using Rfsmart.Phoenix.Accounts.Queues.Services;
using Rfsmart.Phoenix.Caching;
using Rfsmart.Phoenix.Common.Config;
using Rfsmart.Phoenix.Common.Context;
using Rfsmart.Phoenix.Configuration;
using Rfsmart.Phoenix.Events;
using Rfsmart.Phoenix.Events.Config;
using Rfsmart.Phoenix.Logging;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppLogging();
builder.ConfigureServices(
    (context, services) =>
    {
        services.AddScoped<IContextProvider<UserContext>>(sp => new ContextProvider<UserContext>
        {
            Context = new UserContext
            {
                UserName = "API_QUEUE_CONSUMER",
                OrganizationCode = "API_QUEUE_CONSUMER_ORG",
            },
        });

        services
            .AddDefaultCache()
            .BindConfiguration("Redis:Accounts")
            .FallbackToParamStore("accounts");

        services
            .AddOptions<JsonSerializerOptions>()
            .Configure(o =>
            {
                o.PropertyNameCaseInsensitive = true;
            })
            .AddService();

        services.AddAccountsClient();

        services.AddTransient<IOrganizationsClient, OrganizationsClient>();
        services.AddTransient<ITenantSchemasClient, TenantSchemasClient>();
        services.AddTransient<IOrgInitializer, OrgInitializer>();

        services.AddOptions<DeployOptions>().BindConfiguration(DeployOptions.Position).AddService();

        services
            .AddEvents(context.Configuration)
            .WithBackgroundEventQueue()
            .Configure(events =>
            {
                events.Exchanges.TryAdd(
                    "accounts",
                    new ExchangeDeclaration { Type = "topic", Exchange = "accounts" }
                );
                events.Exchanges.TryAdd(
                    "dlx",
                    new ExchangeDeclaration { Type = "topic", Exchange = "dlx" }
                );
            });

        services
            .AddBasicEventConsumer<DlxAllHandler, JsonObject>()
            .BindConfiguration("Events:Consumers:DlxAllHandler");
        services
            .AddBasicEventConsumer<UserCreatedHandler, UserCreatedEvent>()
            .BindConfiguration("Events:Consumers:UserCreatedHandler");
        services
            .AddBasicEventConsumer<UserUpdatedHandler, UserUpdatedEvent>()
            .BindConfiguration("Events:Consumers:UserUpdatedHandler");
        services
            .AddBasicEventConsumer<OrganizationCreatedHandler, OrganizationCreatedEvent>()
            .BindConfiguration("Events:Consumers:OrganizationCreatedHandler");
        services
            .AddBasicEventConsumer<OrganizationDeletedHandler, OrganizationDeletedEvent>()
            .BindConfiguration("Events:Consumers:OrganizationDeletedHandler");
        services
            .AddBasicEventConsumer<TenantCreatedHandler, TenantCreatedEvent>()
            .BindConfiguration("Events:Consumers:TenantCreatedHandler");
        services
            .AddBasicEventConsumer<StatusEventHandler, JsonObject>()
            .BindConfiguration("Events:Consumers:StatusEventHandler");
        services
            .AddBasicEventConsumer<TenantDeletedHandler, TenantDeletedEvent>()
            .BindConfiguration("Events:Consumers:TenantDeletedHandler");
    }
);

var app = builder.Build();
await app.RunAsync();
