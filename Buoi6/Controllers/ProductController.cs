using Buoi6.Models;
using Buoi6.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace Buoi6.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;

        public ProductController(IProductRepository productRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }

        // Hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // Hiển thị form thêm sản phẩm mới
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
            return View();
        }

        // Xử lý thêm sản phẩm mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    // Lưu hình ảnh vào thư mục
                    product.ImageUrl = await SaveImage(imageUrl);
                }

                await _productRepository.AddAsync(product);
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ, hiển thị form với dữ liệu đã nhập
            var categories = await _context.Categories.ToListAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        // Lưu hình ảnh
        private async Task<string> SaveImage(IFormFile image)
        {
            // Tạo thư mục nếu chưa có
            var uploadsFolder = Path.Combine("wwwroot", "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file unique
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var savePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/" + fileName; // Trả về đường dẫn tương đối
        }

        // Hiển thị form cập nhật sản phẩm
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories.ToListAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // Xử lý cập nhật sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    // Lưu hình ảnh mới nếu được tải lên
                    product.ImageUrl = await SaveImage(imageUrl);
                }

                await _productRepository.UpdateAsync(product);
                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories.ToListAsync();
            ViewBag.CategoryId = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // Hiển thị thông tin chi tiết sản phẩm
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Hiển thị form xác nhận xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
            {
                // Xóa file ảnh
                var imagePath = Path.Combine("wwwroot", product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _productRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}