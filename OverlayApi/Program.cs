using Domain;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OverlayApi.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FakeImagesDbContext>(cfg =>
{
    cfg.UseSqlServer(builder.Configuration.GetConnectionString(builder.Environment.IsDevelopment() ? "SqlServerDeveloping" : "SqlServerRelease"));
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ImagesOverlayConsumer>();
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
