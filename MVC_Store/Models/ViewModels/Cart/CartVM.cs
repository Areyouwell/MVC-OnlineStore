namespace MVC_Store.Models.ViewModels.Cart
{
    public class AllCartVM
    {
        public List<CartVM> data { get; set; }
    }

    public class CartVM
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get { return Quantity * Price; } }
        public string? Image { get; set; }
    }
}
