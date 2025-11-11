using System;
namespace Kurtis.Common.Models
{
    public class ProductCollection
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int CollectionId { get; set; }
        public Collection? Collection { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
