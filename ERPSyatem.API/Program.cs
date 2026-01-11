
using ERPSystem.Application.Interfaces;
using ERPSystem.Application.Services;
using ERPSystem.Application.Services.Inventory;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories;
using ERPSystem.Infrastructure.Repositories.ERPSystem.Infrastructure.Repositories;
using ERPSystem.Infrastructure.Repositories.Inventory;
using Microsoft.EntityFrameworkCore;

namespace ERPSyatem.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //Product
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

            builder.Services.AddScoped<IProductService, ProductService>();

            //Inventory
            builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

            builder.Services.AddScoped<IWarehouseService, WarehouseService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); 
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("MyPolicy");
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
