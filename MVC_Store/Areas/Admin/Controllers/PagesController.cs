using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Pages;

namespace MVC_Store.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        private readonly Db _context;
        public PagesController(Db context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<PageVM> pagelist;

            pagelist = _context.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();

            return View(pagelist);
        }

        [HttpGet]
        public IActionResult AddPage()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddPage(PageVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string slug;

            PagesDTO dto = new PagesDTO();

            dto.Title = model.Title.ToUpper();

            if (string.IsNullOrWhiteSpace(model.Slug))
            {
                slug = model.Title.Replace(" ", "-").ToLower();
            }
            else
                slug = model.Slug.Replace(" ", "-").ToLower();

            if (_context.Pages.Any(x => x.Title == model.Title))
            {
                ModelState.AddModelError("", "That title already exist.");
                return View(model);
            }
            else if (_context.Pages.Any(x => x.Slug == model.Slug))
            {
                ModelState.AddModelError("", "That slug already exist.");
                return View(model);
            }

            dto.Slug = slug;
            dto.Body = model.Body;
            dto.HasSideBar = model.HasSideBar;
            dto.Sorting = 100;

            _context.Pages.Add(dto);
            _context.SaveChanges();

            TempData["SM"] = "You have added a new page!";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditPage(int id)
        {
            PageVM model;

            PagesDTO dto = _context.Pages.Find(id);

            if (dto == null)
                return Content("The page does not exist.");

            model = new PageVM(dto);

            return View(model);
        }

        [HttpPost]
        public IActionResult EditPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int id = model.Id;

            string? slug = "home";

            PagesDTO dto = _context.Pages.Find(id);

            dto.Title = model.Title;

            if (model.Slug != "home")
            {
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }
            }
            
            if (_context.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
            {
                ModelState.AddModelError("", "That title already exist.");
                return View(model);
            }
            else if (_context.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
            {
                ModelState.AddModelError("", "That slug already exist.");
                return View(model);
            }

            dto.Slug = slug;
            dto.Body = model.Body;
            dto.HasSideBar = model.HasSideBar;

            _context.SaveChanges();

            TempData["SM"] = "You have edited the page";
           
            return RedirectToAction("EditPage");
        }

        [HttpGet]
        public IActionResult PageDetails(int id)
        {
            PageVM model;

            PagesDTO dto = _context.Pages.Find(id);

            if (dto == null)
                return Content("The page does not exist.");

            model = new PageVM(dto);

            return View(model);
        }

        [HttpGet]
        public IActionResult DeletePage(int id)
        {
            PagesDTO dto = _context.Pages.Find(id);

            _context.Pages.Remove(dto);

            _context.SaveChanges();

            TempData["SM"] = "You have deleted a page";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public void ReorderPages(int[] id)
        {
            int count = 1;

            PagesDTO dto;

            foreach (var pageId in id)
            {
                dto = _context.Pages.Find(pageId);
                dto.Sorting = count;

                _context.SaveChanges();

                count++;
            }
        }

        [HttpGet]
        public IActionResult EditSidebar()
        {
            SidebarVM model;

            SidebarDTO dto = _context.Sidebars.Find(1);

            model = new SidebarVM(dto);

            return View(model);
        }

        [HttpPost]
        public IActionResult EditSidebar(SidebarVM model)
        {
            SidebarDTO dto = _context.Sidebars.Find(1);

            dto.Body = model.Body;

            _context.SaveChanges();

            TempData["SM"] = "You have edited the sidebar!";

            return RedirectToAction("EditSidebar");
        }
    }
}
