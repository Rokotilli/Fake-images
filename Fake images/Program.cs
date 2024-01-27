using Fake_images.Auth;
using Fake_images.Models.Context;
using Fake_images.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FakeImagesDbContext>(cfg =>
{
    if (builder.Environment.IsDevelopment())
    {
        cfg.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerDeveloping"));
    }
    else
    {
        cfg.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerRelease"));
    }
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore;
});

builder.Services.AddSwaggerGen();
builder.Services.AddScoped<JwtUtils>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<FakeImageService>();
builder.Services.AddHttpClient("AzureComputerVision");

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                   .AddJwtBearer(options =>
                   {
                       options.TokenValidationParameters = new TokenValidationParameters
                       {
                           ValidateIssuer = true,
                           ValidateAudience = true,
                           ValidateLifetime = true,
                           ValidateIssuerSigningKey = true,
                           ValidIssuer = builder.Configuration.GetValue<string>("JwtIssuer"),
                           ValidAudience = builder.Configuration.GetValue<string>("JwtAudience"),
                           IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSecurityKey")))
                       };                       
                   });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.Services.GetRequiredService<FakeImagesDbContext>().Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
