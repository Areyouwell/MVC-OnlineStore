using Microsoft.AspNetCore.Mvc;
using MVC_Store.ExtensionMethod;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Cart;
using System.Net;
using System.Net.Mail;

namespace MVC_Store.Controllers
{
    public class CartController : Controller
    {
        private readonly Db _context;

        public CartController(Db context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<CartVM> cart;

            if (HttpContext.Session.Keys.Contains("cart"))
            {
                AllCartVM allCart = HttpContext.Session.Get<AllCartVM>("cart");

                cart = allCart.data;

                if (cart.Count == 0)
                {
                    ViewBag.Message = "Your cart is empty.";
                    return View();
                }

                decimal total = 0m;

                foreach (var item in cart)
                {
                    total += item.Total;
                }

                ViewBag.GrandTotal = total;

                return View(cart);
            }
            else
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }
        }

        public IActionResult AddToCartPartial(int id)
        {
            List<CartVM> cart = HttpContext.Session.Keys.Contains("cart") ? HttpContext.Session.Get<AllCartVM>("cart").data : new List<CartVM>();

            CartVM model = new CartVM();

            ProductDTO product = _context.Products.Find(id);

            var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

            if (productInCart == null)
            {
                cart.Add(new CartVM()
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = 1,
                    Price = product.Price,
                    Image = product.ImageName
                });
            }
            else
            {
                productInCart.Quantity++;
            }

            int quantity = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                quantity += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = quantity;
            model.Price = price;

            HttpContext.Session.Set<AllCartVM>("cart", new AllCartVM() { data = cart });

            return PartialView("_AddToCartPartial", model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            List<CartVM> cart = HttpContext.Session.Get<AllCartVM>("cart").data;

            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

            model.Quantity++;

            HttpContext.Session.Set<AllCartVM>("cart", new AllCartVM() { data = cart });

            var result = new { qty = model.Quantity, price = model.Price };

            return Json(result);
        }

        public IActionResult DecrementProduct(int productId)
        {
            List<CartVM> cart = HttpContext.Session.Get<AllCartVM>("cart").data;

            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

            if (model.Quantity > 1)
                model.Quantity--;
            else
            {
                model.Quantity = 0;
                cart.Remove(model);
            }

            HttpContext.Session.Set<AllCartVM>("cart", new AllCartVM() { data = cart });

            var result = new { qty = model.Quantity, price = model.Price };

            return Json(result);
        }

        public void RemoveProduct(int productId)
        {
            List<CartVM> cart = HttpContext.Session.Get<AllCartVM>("cart").data;

            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

            cart.Remove(model);

            HttpContext.Session.Set<AllCartVM>("cart", new AllCartVM() { data = cart });
        }

        public IActionResult PaymentPartial()
        {
            List<CartVM> cart = HttpContext.Session.Get<AllCartVM>("cart").data;

            return PartialView("_PaymentPartial", cart);
        }

        [HttpPost]
        public void PlaceOrder()
        {
            List<CartVM> cart = HttpContext.Session.Get<AllCartVM>("cart").data;

            string userName = User.Identity.Name;

            int orderId = 0;

            OrderDTO orderDto = new OrderDTO();

            var q = _context.Users.FirstOrDefault(x => x.Username == userName);

            int userId = q.Id;

            orderDto.UserId = userId;
            orderDto.CreatedAt = DateTime.Now;

            _context.Orders.Add(orderDto);

            _context.SaveChanges();

            orderId = orderDto.OrderId;

            foreach (var item in cart)
            {
                OrderDetailsDTO orderDetailsDto = new OrderDetailsDTO();
                orderDetailsDto.OrderId = orderId;
                orderDetailsDto.UserId = userId;
                orderDetailsDto.ProductId = item.ProductId;
                orderDetailsDto.Quantity = item.Quantity;

                _context.OrderDetails.Add(orderDetailsDto);
                _context.SaveChanges();
                orderDetailsDto = new OrderDetailsDTO();
            }

            //var client = new SmtpClient("smtp.mailspons.com", 2525)
            //{
            //    Credentials = new NetworkCredential("ce28d079f1c7488b9cfc", "c3ba3b0a55bb48e6b8e60571da5f0feb"),
            //    EnableSsl = true
            //};
            //client.Send("shop@example.com", "admin@example.com", "New Order", $"You have a new order. Order number: {orderId} ");

            HttpContext.Session.Get<AllCartVM>("cart").data = null;
        }
    }
}
