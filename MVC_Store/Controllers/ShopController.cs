using Microsoft.AspNetCore.Mvc;
using MVC_Store.Models.ViewModels.Shop;
using MVC_Store.Models.Data;

namespace MVC_Store.Controllers
{
    public class ShopController : Controller
    {
        private readonly Db _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ShopController(Db context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public IActionResult Category(string name)
        {
            List<ProductVM> productVMList;

            CategoryDTO categoryDTO = _context.Categories.Where(x => x.Slug == name).FirstOrDefault();

            int catId = categoryDTO.Id;

            productVMList = _context.Products.ToArray()
                            .Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

            var productCat = _context.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

            if (productCat == null)
            {
                var catName = _context.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                ViewBag.CategoryName = catName;
            }
            else
            {
                ViewBag.CategoryName = productCat.CategoryName;
            }

            return View(productVMList);
        }

        [ActionName("product-details")]
        public IActionResult ProductDetails(string name)
        {
            ProductDTO dto;
            ProductVM model;

            int id = 0;

            if (!_context.Products.Any(x => x.Slug.Equals(name)))
                return RedirectToAction("Index", "Shop");

            dto = _context.Products.Where(x => x.Slug == name).FirstOrDefault();

            id = dto.Id;

            model = new ProductVM(dto);

            string path = Path.Combine(new string[] { _webHostEnvironment.WebRootPath, "Images", "Uploads", "Products", 
                                                      id.ToString(), "Gallery", "Thumbs" });

            model.GalleryImages = Directory.EnumerateFiles(path).Select(fileName => Path.GetFileName(fileName));

            return View("ProductDetails", model);
        }
    }
}
