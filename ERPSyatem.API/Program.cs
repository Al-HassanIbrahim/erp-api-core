using System.Text;
using ERPSystem.Application.Interfaces;
using ERPSystem.Application.Interfaces.ERPSystem.Application.Interfaces;
using ERPSystem.Application.Services.Core;
using ERPSystem.Application.Services.Inventory;
using ERPSystem.Application.Services.Products;
using ERPSystem.Application.Services.Sales;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Identity;
using ERPSystem.Infrastructure.Repositories;
using ERPSystem.Infrastructure.Repositories.Core;
using ERPSystem.Infrastructure.Repositories.ERPSystem.Infrastructure.Repositories;
using ERPSystem.Infrastructure.Repositories.Inventory;
using ERPSystem.Infrastructure.Repositories.Sales;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace ERPSyatem.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Core
            builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
            builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
            builder.Services.AddScoped<ICompanyModuleRepository, CompanyModuleRepository>();
            builder.Services.AddScoped<IModuleAccessService, ModuleAccessService>();


            builder.Services.AddScoped<ICompanyProfileService, CompanyProfileService>();
            builder.Services.AddScoped<IModuleService, ModuleService>();
            builder.Services.AddScoped<ICompanyModuleService, CompanyModuleService>();
            builder.Services.AddScoped<ICompanyUserService, CompanyUserService>();

            //Product
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IUnitOfMeasureService, UnitOfMeasureService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();

            //Inventory
            builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

            builder.Services.AddScoped<IWarehouseService, WarehouseService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();

            // Inventory Reports
            builder.Services.AddScoped<IInventoryReportsRepository, InventoryReportsRepository>();
            builder.Services.AddScoped<IInventoryReportsService, InventoryReportsService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            // Sales
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<ISalesInvoiceRepository, SalesInvoiceRepository>();
            builder.Services.AddScoped<ISalesDeliveryRepository, SalesDeliveryRepository>();
            builder.Services.AddScoped<ISalesReceiptRepository, SalesReceiptRepository>();
            builder.Services.AddScoped<ISalesReturnRepository, SalesReturnRepository>();

            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
            builder.Services.AddScoped<ISalesDeliveryService, SalesDeliveryService>();
            builder.Services.AddScoped<ISalesReceiptService, SalesReceiptService>();
            builder.Services.AddScoped<ISalesReturnService, SalesReturnService>();


            #region Swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter: Bearer {your JWT token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            #endregion

            //Security 
            builder.Services.AddIdentity<ApplicationUser,IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            builder.Services.AddAuthentication(options => //How to validate
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; 
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options=> //check if verified token
            {
                options.SaveToken = true;
                //options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                   ValidateIssuer=true,
                   ValidIssuer = builder.Configuration["JWT:IssuerIP"],

                   ValidateAudience = true,
                   ValidAudience = builder.Configuration["JWT:AudienceIP"],

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
                };
            });

            //CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); 
                });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await DbSeeder.SeedModulesAsync(context);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("MyPolicy");
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
