namespace Model
{
    public class Product : BaseEntity
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public Product() : base() { }

        public Product(int id, string productName, decimal price, int stockQuantity)
            : base(id)
        {
            ProductName = productName;
            Price = price;
            StockQuantity = stockQuantity;
        }
    }
}
