using Microsoft.EntityFrameworkCore;
using QuanLiNhaHang.Models;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Đọc chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("con");

// Đăng ký DbContext với SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddSignalR();

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", // 👈 Đổi tên policy để phân biệt
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // 👈 Cụ thể frontend URL của bạn
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // 👈 Cho phép credentials như cookie hoặc token
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Sử dụng CORS trước các middleware khác
app.UseCors("AllowSpecificOrigin"); // 👈 Áp dụng policy mới

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();

// Thêm đường dẫn cho SignalR Hub
app.MapHub<OrderHub>("/orderHub");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();