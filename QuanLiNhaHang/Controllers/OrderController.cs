using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuanLiNhaHang.Models;
using QuanLiNhaHang.Models.DTO;
using System;
using System.Linq;

namespace QuanLiNhaHang.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext; // Inject Hub
        public OrderController(AppDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // 1️⃣ Tạo đơn hàng mới
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var user = _context.Users.Find(request.UserId);
                if (user == null)
                {
                    return BadRequest(new { message = "Người dùng không tồn tại!" });
                }

                var table = _context.Tables.Find(request.TableId);
                if (table == null)
                {
                    return BadRequest(new { message = "Bàn không tồn tại!" });
                }

                var order = new Order
                {
                    UserID = request.UserId,
                    TimeCreate = DateTime.Now,
                    TimePay = DateTime.Now, // Chưa thanh toán
                    Status = 0, // Chưa thanh toán
                    idTable = table.Id,
                    NameTable = table.Name
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                table.Status = 1; // Đánh dấu bàn đang sử dụng
                table.OrderIDCurent = order.Id;
                _context.SaveChanges();

                // Chuẩn bị thông tin chi tiết đơn hàng để gửi lên bếp
                var orderDetails = new
                {
                    OrderId = order.Id,
                    TableName = table.Name,
                    TimeCreate = order.TimeCreate,
                    Items = new List<object>() // Danh sách món ăn (ban đầu rỗng, sẽ cập nhật sau khi thêm món)
                };

                // Gửi thông báo chi tiết đơn hàng đến bếp qua SignalR
                await _hubContext.Clients.All.SendAsync("ReceiveOrderNotification", orderDetails);

                return Ok(new { message = "Đơn hàng được tạo thành công!", orderId = order.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi tạo đơn hàng", error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // Cập nhật khi thêm món vào đơn hàng
        [HttpPost("add-item")]
        public async Task<IActionResult> AddOrderDetail([FromBody] AddOrderDetailRequest request)
        {
            try
            {
                var order = _context.Orders.Find(request.OrderId);
                if (order == null)
                {
                    return BadRequest(new { message = "Đơn hàng không tồn tại!" });
                }

                var product = _context.Products.Find(request.ProductId);
                if (product == null)
                {
                    return BadRequest(new { message = "Sản phẩm không tồn tại!" });
                }

                var orderDetail = new OrderDetail
                {
                    OrderId = request.OrderId,
                    idProduct = request.ProductId,
                    Quality = request.Quantity,
                    price = request.Price,
                    Status = 0 // Mặc định là chưa chế biến
                };

                _context.OrderDetails.Add(orderDetail);
                _context.SaveChanges();

                var items = _context.OrderDetails
                    .Where(od => od.OrderId == order.Id)
                    .GroupJoin(_context.Products,
                        od => od.idProduct,
                        p => p.Id,
                        (od, products) => new
                        {
                            Id = od.Id, // Thêm Id của OrderDetail
                            ProductId = od.idProduct,
                            ProductName = products.FirstOrDefault() != null ? products.FirstOrDefault().Name : "Không xác định",
                            Quantity = od.Quality,
                            Price = od.price,
                            Status = od.Status // Thêm status của món
                        }).ToList();

                var orderDetails = new
                {
                    OrderId = order.Id,
                    TableName = order.NameTable,
                    TimeCreate = order.TimeCreate,
                    Status = order.Status,
                    Items = items
                };

                await _context.SaveChangesAsync();
                await _hubContext.Clients.All.SendAsync("ReceiveOrderNotification", orderDetails);

                return Ok(new { message = "Đã thêm món vào đơn hàng!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi thêm món", error = ex.Message });
            }
        }
        // Trong OrderController.cs
        // In OrderController.cs
        [HttpGet("active-orders")]
        public IActionResult GetActiveOrders()
        {
            try
            {
                var orders = _context.Orders
                    .Where(o => o.Status == 0)
                    .Join(_context.Tables,
                        o => o.idTable,
                        t => t.Id,
                        (o, t) => new { Order = o, Table = t })
                    .Join(_context.KhuVucs,
                        ot => ot.Table.idKhuVuc,
                        k => k.Id,
                        (ot, k) => new
                        {
                            OrderId = ot.Order.Id,
                            TableName = ot.Order.NameTable,
                            TimeCreate = ot.Order.TimeCreate,
                            Status = ot.Order.Status,
                            AreaId = k.Id,
                            AreaName = k.Name,
                            Items = ot.Order.OrderDetails
                                .Join(_context.Products,
                                    od => od.idProduct,
                                    p => p.Id,
                                    (od, p) => new
                                    {
                                        Id = od.Id,
                                        ProductId = od.idProduct,
                                        ProductName = p.Name ?? "Không xác định",
                                        Quantity = od.Quality,
                                        Price = od.price,
                                        ItemStatus = od.Status
                                    }).ToList()
                        }).ToList();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi lấy danh sách đơn hàng", error = ex.Message });
            }
        }
        [HttpPut("update-item-status")]
        public async Task<IActionResult> UpdateItemStatus([FromBody] UpdateItemStatusRequest request)
        {
            try
            {
                var orderDetail = _context.OrderDetails
                    .FirstOrDefault(od => od.Id == request.OrderDetailId);

                if (orderDetail == null)
                {
                    return NotFound(new { message = "Không tìm thấy chi tiết đơn hàng!" });
                }

                orderDetail.Status = request.Status;
                _context.SaveChanges();

                // Cập nhật thông báo qua SignalR
                var order = _context.Orders.FirstOrDefault(o => o.Id == orderDetail.OrderId);
                var items = _context.OrderDetails
                    .Where(od => od.OrderId == order.Id)
                    .Join(_context.Products,
                        od => od.idProduct,
                        p => p.Id,
                        (od, p) => new
                        {
                            ProductId = od.idProduct,
                            ProductName = p.Name != null ? p.Name : "Không xác định", // Tránh dùng ??
                            Quantity = od.Quality,
                            Price = od.price,
                            Status = od.Status
                        }).ToList();

                var orderDetails = new
                {
                    OrderId = order.Id,
                    TableName = order.NameTable,
                    TimeCreate = order.TimeCreate,
                    Status = order.Status,
                    Items = items
                };

                await _hubContext.Clients.All.SendAsync("ReceiveOrderNotification", orderDetails);

                return Ok(new { message = "Đã cập nhật trạng thái món ăn!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi cập nhật trạng thái", error = ex.Message });
            }
        }
        public class UpdateItemStatusRequest
        {
            public int OrderDetailId { get; set; }
            public int Status { get; set; }
        }
        // 3️⃣ Đổi trạng thái bàn
        [HttpPut("update-table-status")]
        public IActionResult UpdateTableStatus([FromBody] UpdateTableStatusRequest request)
        {
            try
            {
                var table = _context.Tables.Find(request.TableId);
                if (table == null)
                {
                    return NotFound(new { message = "Không tìm thấy bàn!" });
                }

                table.Status = request.Status;
                _context.SaveChanges();

                return Ok(new { message = "Cập nhật trạng thái bàn thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi cập nhật trạng thái bàn", error = ex.Message });
            }
        }

        // 4️⃣ Thanh toán đơn hàng và giải phóng bàn
        // 4️⃣ Thanh toán đơn hàng và giải phóng bàn
        [HttpPut("pay")]
        public IActionResult PayOrder([FromBody] PayOrderRequest request)
        {
            try
            {
                var order = _context.Orders
                    .Include(o => o.User) // Lấy thông tin User
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product) // Lấy thông tin sản phẩm
                    .FirstOrDefault(o => o.Id == request.OrderId);

                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng!" });
                }

                // Kiểm tra danh sách chi tiết món ăn có rỗng không
                var orderDetails = _context.OrderDetails
                    .Where(od => od.OrderId == order.Id)
                    .Include(od => od.Product)
                    .ToList();

                if (orderDetails == null || !orderDetails.Any())
                {
                    return BadRequest(new { message = "Không có món ăn nào trong đơn hàng!" });
                }

                // Cập nhật trạng thái thanh toán
                order.TimePay = DateTime.Now;
                order.Status = 1; // Đã thanh toán
                _context.SaveChanges();

                // Giải phóng bàn (cập nhật trạng thái bàn)
                var table = _context.Tables.FirstOrDefault(t => t.OrderIDCurent == order.Id);
                if (table != null)
                {
                    table.Status = 0; // 0: Bàn trống
                    table.OrderIDCurent = 0;
                    _context.SaveChanges();
                }

                // Tính tổng tiền đơn hàng
                decimal totalAmount = orderDetails.Sum(od => (od.price * od.Quality));

                // Tạo hóa đơn trả về
                var bill = new BillResponse
                {
                    OrderId = order.Id,
                    UserName = order.User?.Name ?? "Không xác định", // Kiểm tra null
                    TimeCreate = order.TimeCreate,
                    TimePay = order.TimePay,
                    idTable = order.idTable,
                    NameTable = order.NameTable,
                    Items = orderDetails.Select(od => new BillItem
                    {
                        ProductId = od.idProduct,
                        ProductName = od.Product?.Name ?? "Không xác định", // Kiểm tra null
                        Quantity = od.Quality,
                        Price = od.price,
                        TotalPrice = od.price * od.Quality
                    }).ToList(),
                    TotalAmount = totalAmount
                };

                return Ok(bill);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi thanh toán", error = ex.Message });
            }
        }
        [HttpGet("all")]
        public IActionResult GetAllOrders()
        {
           
                var orders = _context.Orders
                    .Include(o => o.User) // Include User information
                    .Include(o => o.OrderDetails) // Include Order Details
                        .ThenInclude(od => od.Product) // Include Product information for each detail
                    .Join(_context.Tables, // Join with Tables
                        o => o.idTable,
                        t => t.Id,
                        (o, t) => new { Order = o, Table = t })
                    .Join(_context.KhuVucs, // Join with KhuVuc (Areas)
                        ot => ot.Table.idKhuVuc,
                        k => k.Id,
                        (ot, k) => new
                        {
                            OrderId = ot.Order.Id,
                            UserId = ot.Order.UserID,
                            UserName = ot.Order.User != null ? ot.Order.User.Name : "Không xác định",
                            TimeCreate = ot.Order.TimeCreate,
                            TimePay = ot.Order.TimePay,
                            Status = ot.Order.Status,
                            TableId = ot.Order.idTable,
                            TableName = ot.Order.NameTable,
                            AreaId = k.Id,
                            AreaName = k.Name,
                            Items = ot.Order.OrderDetails.Select(od => new
                            {
                                OrderDetailId = od.Id,
                                ProductId = od.idProduct,
                                ProductName = od.Product != null ? od.Product.Name : "Không xác định",
                                Quantity = od.Quality,
                                Price = od.price,
                                TotalItemPrice = od.price * od.Quality,
                                ItemStatus = od.Status
                            }).ToList(),
                            TotalAmount = ot.Order.OrderDetails.Sum(od => od.price * od.Quality)
                        })
                    .OrderBy(o => o.TimeCreate) // Sort by creation time
                    .ToList();

                return Ok(new
                {
                    message = "Lấy danh sách tất cả đơn hàng thành công",
                    data = orders,
                    totalCount = orders.Count
                });
            }
           
        }

        // DTOs cho request
        public class CreateOrderRequest
        {
            public int UserId { get; set; }
            public int TableId { get; set; }
        }

        public class AddOrderDetailRequest
        {
            public int OrderId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }
        public class BillResponse
        {
            public int OrderId { get; set; }
            public string UserName { get; set; }
            public DateTime TimeCreate { get; set; }
            public DateTime? TimePay { get; set; }
            public List<BillItem> Items { get; set; }
            public decimal TotalAmount { get; set; }
            public int idTable { get; set; }  // Thêm ID bàn
            public string NameTable { get; set; } // Thêm tên bàn
        }
       
        public class BillItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } // Thêm tên sản phẩm
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal TotalPrice { get; set; }
        }



        public class UpdateTableStatusRequest
        {
            public int TableId { get; set; }
            public int Status { get; set; }
        }

        public class PayOrderRequest
        {
            public int OrderId { get; set; }
        }
    }

