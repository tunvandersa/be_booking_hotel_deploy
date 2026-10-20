using do_an_epu.Data;
using do_an_epu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using do_an_epu.ServicesExtensions;
using do_an_epu.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CloudinaryDotNet;
using do_an_epu.Middleware;


var builder = WebApplication.CreateBuilder(args);

var cloudinarySection = builder.Configuration.GetSection("Cloudinary");
var cloudName = cloudinarySection["CloudName"];
var apiKey = cloudinarySection["ApiKey"];
var apiSecret = cloudinarySection["ApiSecret"];

var account = new Account(cloudName, apiKey, apiSecret);
var cloudinary = new Cloudinary(account);

builder.Services.Configure<MultiTenancyOptions>(
    builder.Configuration.GetSection(MultiTenancyOptions.Section));
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton(cloudinary);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.ConfigureRepositories();

builder.Services.ConfigureServices();

// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = builder.Configuration["Redis:ConnectionString"];
//     options.InstanceName = "BookingApp:";
// });
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddIdentity<AppUser, IdentityRole<int>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;

})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://localhost:5173", "http://localhost:5174")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // === Cấu hình quan trọng ===
        // Xác thực rằng chìa khóa ký (Signing Key) là hợp lệ
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

        // Xác thực nhà phát hành (Issuer)
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        // Xác thực bên nhận (Audience)
        ValidateAudience = true,
        ValidAudience = jwtAudience,

        // Yêu cầu token phải còn hạn
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Không cho phép chênh lệch thời gian
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // Bỏ qua phản hồi mặc định
            context.HandleResponse();

            // Thiết lập cấu trúc trả về
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "Bạn chưa đăng nhập hoặc Token không hợp lệ.",
                statusCode = 401
            });

            return context.Response.WriteAsync(result);
        },

        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                message = "Bạn không có quyền truy cập vào chức năng này (Cần quyền Admin).",
                statusCode = 403
            });

            return context.Response.WriteAsync(result);
        }
    };
});
builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<TenantMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        var response = new { error = exception?.Message };
        context.Response.StatusCode = 500;

        if (exception is DomainException)
        {
            context.Response.StatusCode = 400;
        }
        else if (exception is NotFoundException)
        {
            context.Response.StatusCode = 404;
        }

        await context.Response.WriteAsJsonAsync(response);
    });
});
app.UseSession();
app.MapControllers();

app.Run();
