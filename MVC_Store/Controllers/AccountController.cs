using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_Store.ExtensionMethod;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Account;
using MVC_Store.Models.ViewModels.Cart;
using MVC_Store.Models.ViewModels.Shop;
using System.Security.Claims;

namespace MVC_Store.Controllers
{
    public class AccountController : Controller
    {
        private readonly Db _context;
        public AccountController(Db context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login");
        }

        [HttpGet]
        [ActionName("create-account")]
        public IActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        [HttpPost]
        [ActionName("create-account")]
        public IActionResult CreateAccount(UserVM model)
        {
            if (!ModelState.IsValid)
                return View("CreateAccount", model);

            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Password do not match!");
                return View("CreateAccount", model);
            }

            if (_context.Users.Any(x => x.Username.Equals(model.Username)))
            {
                ModelState.AddModelError("", $"Username {model.Username} is taken.");
                model.Username = "0";
                return View("CreateAccount", model);
            }

            UserDTO userDTO = new UserDTO()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailAdress = model.EmailAdress,
                Username = model.Username,
                Password = model.Password
            };

            _context.Users.Add(userDTO);

            _context.SaveChanges();

            int id = userDTO.Id;

            UserRoleDTO userRoleDTO = new UserRoleDTO()
            {
                UserId = id,
                RoleId = 2
            };

            _context.UserRoles.Add(userRoleDTO);

            _context.SaveChanges();

            TempData["SM"] = "You are now registered and can login.";

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            string userName = User.Identity.Name;

            if (!string.IsNullOrEmpty(userName))
                return RedirectToAction("user-profile");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginUserVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool isValid = false;

            if (_context.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                isValid = true;

            UserDTO dto = _context.Users.FirstOrDefault(x => x.Username.Equals(model.Username));

            string[] roles = _context.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray();

            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }
            else
            {
                var claims = new List<Claim> 
                { 
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(type: "RememberMe", value: model.RememberMe.ToString()) 
                };

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));
                }

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return Redirect("/");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Set<AllCartVM>("cart", new AllCartVM() { data = new List<CartVM>() });

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect("Login");
        }

        [HttpGet]
        [Authorize]
        [ActionName("user-profile")]
        public IActionResult UserProfile()
        {
            string userName = User.Identity.Name;

            UserProfileVM model;

            UserDTO dto = _context.Users.FirstOrDefault(x => x.Username == userName);

            model = new UserProfileVM(dto);

            return View("UserProfile", model);
        }

        [HttpPost]
        [Authorize]
        [ActionName("user-profile")]
        public IActionResult UserProfile(UserProfileVM model)
        {
            bool userNameIsChanged = false;

            if (!ModelState.IsValid)
                return View("UserProfile", model);

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not much.");
                    return View("UserProfile", model);
                }
            }

            string userName = User.Identity.Name;

            if (userName != model.Username)
            {
                userName = model.Username;
                userNameIsChanged = true;
            }

            if (_context.Users.Where(x => x.Id != model.Id).Any(x => x.Username == userName))
            {
                ModelState.AddModelError("", $"Username {model.Username} already exist.");
                model.Username = "";
                return View("UserProfile", model);
            }

            UserDTO dto = _context.Users.Find(model.Id);

            dto.FirstName = model.FirstName;
            dto.LastName = model.LastName;
            dto.EmailAdress = model.EmailAdress;
            dto.Username = model.Username;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                dto.Password = model.Password;
            }

            _context.SaveChanges();

            TempData["SM"] = "You have edited your profile";

            if (!userNameIsChanged)
                return View("UserProfile", model);
            else
                return RedirectToAction("Logout");
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public IActionResult Orders()
        {
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            UserDTO user = _context.Users.FirstOrDefault(x => x.Username == User.Identity.Name);

            int userId = user.Id;

            List<OrderVM> orders = _context.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

            foreach (var order in orders)
            {
                Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                decimal total = 0m;

                List<OrderDetailsDTO> orderDetailsDto = _context.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                foreach (var orderDetails in orderDetailsDto)
                {
                    ProductDTO product = _context.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                    decimal price = product.Price;

                    string productName = product.Name;

                    productsAndQty.Add(productName, orderDetails.Quantity);

                    total += orderDetails.Quantity * price;
                }

                ordersForUser.Add(new OrdersForUserVM()
                {
                    OrderNumber = order.OrderId,
                    Total = total,
                    ProductsAndQty = productsAndQty,
                    CreatedAt = order.CreatedAt
                });
            }
            return View(ordersForUser);
        }
    }
}
