using PustokImagesUploadTask.Base;

namespace PustokImagesUploadTask.Models
{
    public class OrderItem : BaseEntity
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public int OrderId { get; set; }
        public double SalePrice { get; set; }
        public double DiscountPrice { get; set; }
        public double CostPrice { get; set; }
        public int Count { get; set; }
        public Book? Book { get; set; }
        public Order? Order { get; set; }
        
    }
}
