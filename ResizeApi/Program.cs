using Domain;
using MassTransit;
using MessageBus.Messages;
using Microsoft.EntityFrameworkCore;
using ResizeApi.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FakeImagesDbContext>(cfg =>
{
    cfg.UseSqlServer(builder.Configuration.GetConnectionString(builder.Environment.IsDevelopment() ? "SqlServerDeveloping" : "SqlServerRelease"));
});

builder.Services.AddMassTransit(x =>
{
    x.AddRequestClient<ImagesRemoveBackEvent>(timeout: 35000);
    x.AddConsumer<ImagesResizeConsumer>();
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
