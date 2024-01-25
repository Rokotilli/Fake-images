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
    cfg.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<JwtUtils>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<FakeImageService>();

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

app.Services.GetRequiredService<FakeImagesDbContext>().Database.Migrate();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
