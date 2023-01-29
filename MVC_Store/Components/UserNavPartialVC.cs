using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Account;

namespace MVC_Store.Components
{
    public class UserNavPartialVC : ViewComponent
    {
        private readonly Db _context;

        public UserNavPartialVC(Db context)
        {
            _context = context;
        }

        [Authorize]
        public IViewComponentResult Invoke()
        {
            string? userName = User.Identity.Name;

            UserNavPartialVM model;

            UserDTO dto = _context.Users.FirstOrDefault(x => x.Username == userName);

            model = new UserNavPartialVM()
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            return View("_UserNavPartial", model);
        }
    }
}
