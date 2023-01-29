using Microsoft.AspNetCore.Mvc;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Pages;

namespace MVC_Store.Controllers
{
    public class PagesController : Controller
    {
        private readonly Db _context;

        public PagesController(Db context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string page = "")
        {
            if (page == "")
                page = "home";

            PageVM model;
            PagesDTO dto;

            if (!_context.Pages.Any(x => x.Slug.Equals(page)))
                return RedirectToAction("Index", new { page = "" });

            dto = _context.Pages.Where(x => x.Slug == page).FirstOrDefault();

            ViewBag.PageTitle = dto.Title;

            if (dto.HasSideBar == true)
                ViewBag.Sidebar = "Yes";
            else
                ViewBag.Sidebar = "No";

            model = new PageVM(dto);

            return View("Index", model);
        }
    }
}
