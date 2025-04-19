namespace QuanLiNhaHang.Models.DTO
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int ItemStatus { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
