using System.ComponentModel;

namespace MVC_Store.Areas.Admin.Models.ViewModels.Shop
{
    public class OrdersForAdminVM
    {
        [DisplayName("Order Number")]
        public int OrderNumber { get; set; }
        [DisplayName("Username")]
        public string UserName { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductsAndQty { get; set; }
        [DisplayName("Created At")]
        public DateTime CreatedAt { get; set; }
    }
}
