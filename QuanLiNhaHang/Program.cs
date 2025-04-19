using Microsoft.EntityFrameworkCore;
using QuanLiNhaHang.Models;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Đọc chuỗi kết nối từ biến môi trường hoặc appsettings.json
var connectionString = builder.Configuration.GetConnectionString("con")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__con");

// Đăng ký DbContext với SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddSignalR();

// Đọc URL frontend từ biến môi trường hoặc mặc định là localhost
var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:4200";

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true) // Cho phép tất cả domain
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Cần thiết nếu dùng SignalR với WebSocket
    });
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Sử dụng CORS trước các middleware khác
app.UseCors("AllowSpecificOrigin");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Tạm thời bật Swagger trong Production để kiểm tra (tùy chọn, xóa nếu không cần)
else
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

// Cấu hình Kestrel để sử dụng cổng từ biến môi trường PORT
app.Run();