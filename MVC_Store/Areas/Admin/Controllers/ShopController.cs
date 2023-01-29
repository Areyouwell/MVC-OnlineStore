using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Shop;
using X.PagedList;
using System.Drawing;
using MVC_Store.Areas.Admin.Models.ViewModels.Shop;
using Microsoft.AspNetCore.Authorization;

namespace MVC_Store.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        private readonly Db _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ShopController(Db context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Categories()
        {
            List<CategoryVM> categotyVMList;

            categotyVMList = _context.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();

            return View(categotyVMList);
        }

        [HttpPost]
        public string AddNewCategory(string catName)
        {
            string id;

            if (_context.Categories.Any(x => x.Name == catName))
                return "titletaken";

            CategoryDTO dto = new CategoryDTO();

            dto.Name = catName;
            dto.Slug = catName.Replace(" ", "-").ToLower();
            dto.Sorting = 100;

            _context.Categories.Add(dto);
            _context.SaveChanges();

            id = dto.Id.ToString();

            return id;
        }

        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            int count = 1;

            CategoryDTO dto;

            foreach (var catId in id)
            {
                dto = _context.Categories.Find(catId);
                dto.Sorting = count;

                _context.SaveChanges();

                count++;
            }
        }

        [HttpGet]
        public IActionResult DeleteCategory(int id)
        {
            CategoryDTO dto = _context.Categories.Find(id);

            _context.Categories.Remove(dto);

            _context.SaveChanges();

            TempData["SM"] = "You have deleted a category";

            return RedirectToAction("Categories");
        }

        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            if (_context.Categories.Any(x => x.Name == newCatName))
                return "titletaken";

            CategoryDTO dto = _context.Categories.Find(id);

            dto.Name = newCatName;
            dto.Slug = newCatName.Replace(" ", "-").ToLower();

            _context.SaveChanges();

            return "ok";
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            ProductVM model = new ProductVM();

            model.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");

            return View(model);
        }

        [HttpPost]
        public IActionResult AddProduct(ProductVM model, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
                return View(model);
            }

            if (_context.Products.Any(x => x.Name == model.Name))
            {
                model.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
                ModelState.AddModelError("", "That product name is taken!");
                return View(model);
            }

            int id;

            ProductDTO product = new ProductDTO();

            product.Name = model.Name;
            product.Slug = model.Name.Replace(" ", "-").ToLower();
            product.Description = model.Description;
            product.Price = model.Price;
            product.CategoryId = model.CategoryId;

            CategoryDTO catDTO = _context.Categories.FirstOrDefault(x => x.Id == model.CategoryId);

            product.CategoryName = catDTO.Name;

            _context.Products.Add(product);
            _context.SaveChanges();

            id = product.Id;

            TempData["SM"] = "You have added a product";

            #region Upload Image

            string originalDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Uploads");

            string[] paths = new string[] {
                 Path.Combine(originalDirectory, "Products"),
                 Path.Combine(originalDirectory, "Products", id.ToString()),
                 Path.Combine(originalDirectory, "Products", id.ToString(), "Thumbs"),
                 Path.Combine(originalDirectory, "Products", id.ToString(), "Gallery"),
                 Path.Combine( new string[] { originalDirectory, "Products", id.ToString(), "Gallery", "Thumbs" })
            };

            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            if (file != null && file.Length > 0)
            {
                string ext = file.ContentType.ToLower();
                List<string> extensions = new List<string>() { "image/jpg", "image/jpeg", "image/pjpeg", "image/gif", "image/x-png", "image/png" };

                if (!extensions.Contains(ext))
                {
                    model.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "The image was not uploaded - wrong image extension");
                    return View(model);
                }


                string imageName = file.FileName;

                ProductDTO dto = _context.Products.Find(id);
                dto.ImageName = imageName;

                _context.SaveChanges();

                var path = string.Format($"{paths[1]}\\{imageName}");
                var path2 = string.Format($"{paths[2]}\\{imageName}");

                file.CopyTo(new FileStream(path, FileMode.Create));

                Image img = Image.FromStream(file.OpenReadStream(), true);
                Bitmap resized = new Bitmap(img, 200, 200);
                resized.Save(path2);
                img.Dispose();
            }
            #endregion

            return RedirectToAction("AddProduct");
        }

        [HttpGet]
        public IActionResult Products(int? page, int? catId)
        {
            List<ProductVM> listOfProductVM;

            var pageNumber = page ?? 1;

            listOfProductVM = _context.Products.ToArray().Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                      .Select(x => new ProductVM(x)).ToList();

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.SelectedCat = catId.ToString();

            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);

            ViewBag.OnePageOfProducts = onePageOfProducts;

            return View(listOfProductVM);
        }

        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            ProductVM model;

            ProductDTO dto = _context.Products.Find(id);

            if (dto == null)
                return Content("That product does not exist.");

            model = new ProductVM(dto);

            model.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");

            model.GalleryImages = Directory
                                  .EnumerateFiles(Path.Combine(new string[] { _webHostEnvironment.WebRootPath, "Images", "Uploads"
                                                                            , "Products", id.ToString(), "Gallery", "Thumbs" }))
                                  .Select(fileName => Path.GetFileName(fileName));

            return View(model);
        }

        [HttpPost]
        public IActionResult EditProduct(ProductVM model, IFormFile? file)
        {
            int id = model.Id;

            model.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");

            model.GalleryImages = Directory
                                 .EnumerateFiles(Path.Combine(new string[] { _webHostEnvironment.WebRootPath, "Images", "Uploads"
                                                                            , "Products", id.ToString(), "Gallery", "Thumbs" }))
                                 .Select(fileName => Path.GetFileName(fileName));

            if (!ModelState.IsValid)
                return View(model);

            if (_context.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
            {
                ModelState.AddModelError("", "That product name is taken!");
                return View(model);
            }

            ProductDTO dto = _context.Products.Find(id);

            dto.Name = model.Name;
            dto.Slug = model.Name.Replace(" ", "-").ToLower();
            dto.Description = model.Description;
            dto.Price = model.Price;
            dto.CategoryId = model.CategoryId;
            dto.ImageName = model.ImageName;

            CategoryDTO catDTO = _context.Categories.FirstOrDefault(x => x.Id == model.CategoryId);

            dto.CategoryName = catDTO.Name;

            _context.SaveChanges();

            TempData["SM"] = "You have edited the product!";

            #region Image Upload

            if (file != null && file.Length > 0)
            {
                string ext = file.ContentType.ToLower();

                List<string> extensions = new List<string>() { "image/jpg", "image/jpeg", "image/pjpeg", "image/gif", "image/x-png", "image/png" };

                if (!extensions.Contains(ext))
                {
                    ModelState.AddModelError("", "The image was not uploaded - wrong image extension");
                    return View(model);
                }

                string originalDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Uploads");

                string[] paths = new string[] {
                    Path.Combine(originalDirectory, "Products", id.ToString()),
                    Path.Combine(originalDirectory, "Products", id.ToString(), "Thumbs")
                };

                DirectoryInfo di1 = new DirectoryInfo(paths[0]);
                DirectoryInfo di2 = new DirectoryInfo(paths[1]);

                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                string imageName = file.FileName;

                ProductDTO dto1 = _context.Products.Find(id);
                dto1.ImageName = imageName;
                _context.SaveChanges();

                var path = string.Format($"{paths[0]}\\{imageName}");
                var path2 = string.Format($"{paths[1]}\\{imageName}");

                file.CopyTo(new FileStream(path, FileMode.Create));

                Image img = Image.FromStream(file.OpenReadStream());
                Bitmap resized = new Bitmap(img, 200, 200);
                resized.Save(path2);
                img.Dispose();
            }
            #endregion

            return RedirectToAction("EditProduct");
        }

        [HttpGet]
        public IActionResult DeleteProduct(int id)
        {
            ProductDTO dto = _context.Products.Find(id);

            _context.Products.Remove(dto);

            _context.SaveChanges();

            string originalDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Uploads");
            string path1 = Path.Combine(originalDirectory, "Products", id.ToString());

            if (Directory.Exists(path1))
                Directory.Delete(path1, true);

            return RedirectToAction("Products");
        }

        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            foreach (var file in Request.Form.Files)
            {
                if (file != null && file.Length > 0)
                {
                    string originalDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Uploads");
                    string pathString1 = Path.Combine(new string[] { originalDirectory, "Products", id.ToString(), "Gallery" });
                    string pathString2 = Path.Combine(new string[] { pathString1, "Thumbs" });

                    string path = Path.Combine(pathString1, file.FileName);
                    string path2 = Path.Combine(pathString2, file.FileName);

                    file.CopyTo(new FileStream(path, FileMode.Create));

                    Image img = Image.FromStream(file.OpenReadStream(), true);
                    Bitmap resized = new Bitmap(img, 200, 200);
                    resized.Save(path2);
                    img.Dispose();
                }
            }
        }

        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Path.Combine(new string[] { _webHostEnvironment.WebRootPath, "Images", "Uploads", 
                                            "Products", id.ToString(), "Gallery", imageName });
            string fullPath2 = Path.Combine(new string[] { _webHostEnvironment.WebRootPath, "Images", "Uploads",
                                            "Products", id.ToString(), "Gallery", "Thumbs", imageName });

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        [HttpGet]
        public IActionResult Orders()
        {
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            List<OrderVM> orders = _context.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

            foreach (var order in orders)
            {
                Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                decimal total = 0m;

                List<OrderDetailsDTO> orderDetailsList = _context.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                UserDTO user = _context.Users.FirstOrDefault(x => x.Id == order.UserId);

                string username = user.Username;

                foreach (var orderDetails in orderDetailsList)
                {
                    ProductDTO product = _context.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                    decimal price = product.Price;

                    string productName = product.Name;

                    productAndQty.Add(productName, orderDetails.Quantity);

                    total += orderDetails.Quantity * price;
                }

                ordersForAdmin.Add(new OrdersForAdminVM()
                {
                    OrderNumber = order.OrderId,
                    UserName = username,
                    Total = total,
                    ProductsAndQty = productAndQty,
                    CreatedAt = order.CreatedAt
                });
            }
            return View(ordersForAdmin);
        }
    }
}
