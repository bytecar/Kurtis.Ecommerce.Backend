using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurtis.Common.Models
{
    /// <summary>
    /// Represents the many-to-many relationship between Products and Collections
    /// </summary>
    public class ProductCollection
    {
        /// <summary>
        /// Primary key for the product-collection association
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the Product table
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Navigation property to the Product
        /// </summary>
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        /// <summary>
        /// Foreign key to the Collection table
        /// </summary>
        [Required]
        public int CollectionId { get; set; }

        /// <summary>
        /// Navigation property to the Collection
        /// </summary>
        [ForeignKey(nameof(CollectionId))]
        public Collection? Collection { get; set; }

        /// <summary>
        /// Timestamp when the product was added to the collection
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
