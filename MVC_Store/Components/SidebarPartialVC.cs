using Microsoft.AspNetCore.Mvc;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Pages;

namespace MVC_Store.Components
{
    public class SidebarPartialVC : ViewComponent
    {
        private readonly Db _context;

        public SidebarPartialVC(Db context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            SidebarVM model;

            SidebarDTO dto = _context.Sidebars.Find(1);

            model = new SidebarVM(dto);

            return View("_SidebarPartial", model);
        }
    }
}
