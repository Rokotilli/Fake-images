using Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserApi.Middlewares;
using UserApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FakeImagesDbContext>(cfg =>
{
    cfg.UseSqlServer(builder.Configuration.GetConnectionString(builder.Environment.IsDevelopment() ? "SqlServerDeveloping" : "SqlServerRelease"));
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore;
});

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UserService>();

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

if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<OcelotAuthMiddleware>();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
