using Microsoft.AspNetCore.Mvc;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Pages;

namespace MVC_Store.Components
{
    public class PagesMenuPartialVC : ViewComponent
    {
        private readonly Db _context;

        public PagesMenuPartialVC(Db context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            List<PageVM> pageVMList;

            pageVMList = _context.Pages.ToArray().OrderBy(x => x.Sorting)
                         .Where(x => x.Slug != "home").Select(x => new PageVM(x)).ToList();

            return View("_PagesMenuPartial", pageVMList);
        }
    }
}
