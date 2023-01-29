using Microsoft.AspNetCore.Mvc;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Shop;

namespace MVC_Store.Components
{
    public class CategoriesMenuPartialVC : ViewComponent
    {
        private readonly Db _context;

        public CategoriesMenuPartialVC(Db context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            List<CategoryVM> categoryVMList;

            categoryVMList = _context.Categories.ToArray().OrderBy(x => x.Sorting)
                             .Select(x => new CategoryVM(x)).ToList();

            return View("_CategoryMenuPartial", categoryVMList);
        }
    }
}
