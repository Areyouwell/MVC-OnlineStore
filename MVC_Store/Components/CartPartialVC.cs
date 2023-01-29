using Microsoft.AspNetCore.Mvc;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Cart;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;
using MVC_Store.ExtensionMethod;

namespace MVC_Store.Components
{
    public class CartPartialVC : ViewComponent
    {
        private readonly Db _context;

        public CartPartialVC(Db context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            CartVM model = new CartVM();

            int quantity = 0;

            decimal price = 0m;

            if (HttpContext.Session.Keys.Contains("cart"))
            {
                AllCartVM allCart = HttpContext.Session.Get<AllCartVM>("cart");

                List<CartVM> list = allCart.data;

                foreach (var item in list)
                {
                    quantity += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = quantity;
                model.Price = price;
            }
            else
            {
                model.Quantity = 0;
                model.Price = 0m;
            }

            return View("_CartPartial", model);
        }
    }
}
