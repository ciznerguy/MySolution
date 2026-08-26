using System;

namespace Model
{
    public class Purchase : BaseEntity
    {
        public int CustomerId { get; set; }
        public Person Customer { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime PurchaseDate { get; set; }
        public int Quantity { get; set; }

        public Purchase() : base() { }

        public Purchase(int id, int customerId, int productId, DateTime purchaseDate, int quantity)
            : base(id)
        {
            CustomerId = customerId;
            ProductId = productId;
            PurchaseDate = purchaseDate;
            Quantity = quantity;
        }
    }
}
