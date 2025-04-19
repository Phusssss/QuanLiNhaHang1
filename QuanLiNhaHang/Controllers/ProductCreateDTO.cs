namespace QuanLiNhaHang.Controllers
{
    public class ProductCreateDTO
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int idTypeProduct { get; set; }
        public IFormFile? Image { get; set; } // Dùng để nhận file ảnh tải lên
    }
}
