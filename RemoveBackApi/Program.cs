using Domain;
using MassTransit;
using MessageBus.Messages;
using Microsoft.EntityFrameworkCore;
using RemoveBackApi.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("AzureComputerVision");

builder.Services.AddDbContext<FakeImagesDbContext>(cfg =>
{
    cfg.UseSqlServer(builder.Configuration.GetConnectionString(builder.Environment.IsDevelopment() ? "SqlServerDeveloping" : "SqlServerRelease"));
});

builder.Services.AddMassTransit(x =>
{
    x.AddRequestClient<ImagesOverlayEvent>(timeout: 30000);
    x.AddConsumer<ImagesRemoveBackConsumer>();
    x.UsingRabbitMq((cxt, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(cxt);
    });
});

var app = builder.Build();

app.Run();
