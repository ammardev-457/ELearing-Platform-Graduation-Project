using ELProject.DataAccess;
using ELProject.DataAccess.Repositories;
using ELProject.DataAccess.Repositories.Repos;
using ELProject.DataAccess.Seed;
using ELProject.Domain.Models;
using ELProject.Services;
using ELProject.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. إضافة الـ Controllers والـ OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 2. إعداد قاعدة البيانات
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. إعداد الـ Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 4. تسجيل الـ Services و الـ Repositories (DI)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<AuthRepository>(); // مهم جداً عشان الـ Login
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

builder.Services.AddHttpClient<PaymobService>();

// 5. إعداد الـ Authentication (JWT & Google)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
    };
});
// .AddGoogle(options =>
// {
//     options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//     options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
// });

// 6. إعداد الـ CORS (عشان الـ ngrok والفرونت اند)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// --- إعداد الـ Pipeline (الترتيب هنا حيوي) ---

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// تأكد إن الـ CORS قبل الـ Auth
app.UseCors();

// الترتيب الصحيح: التعرف على المستخدم أولاً ثم التحقق من صلاحياته
app.UseAuthentication(); 
app.UseAuthorization();

// لربط الـ Routes بالـ Controllers
app.MapControllers();

// عمل الـ Seed للداتا (Roles, Admin, etc.)
await DataSeeder.SeedDataAsync(app.Services);

app.Run();