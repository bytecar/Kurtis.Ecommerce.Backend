using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurtis.Common.Models
{
    /// <summary>
    /// Represents a product in the e-commerce catalog
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Primary key for the product
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        [Required]
        [MaxLength(250)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Product description
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? Description { get; set; }

        /// <summary>
        /// Regular price of the product
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        public decimal Price { get; set; }

        /// <summary>
        /// Discounted price (if on sale)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountedPrice { get; set; }

        /// <summary>
        /// Foreign key to the Brand table
        /// </summary>
        [Required]
        public int BrandId { get; set; }

        /// <summary>
        /// Navigation property to the Brand
        /// </summary>
        [ForeignKey(nameof(BrandId))]
        public Brand? Brand { get; set; }

        /// <summary>
        /// Foreign key to the Category table
        /// </summary>
        [Required]
        public int CategoryId { get; set; }

        /// <summary>
        /// Navigation property to the Category
        /// </summary>
        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        /// <summary>
        /// Gender category (men, women, unisex)
        /// </summary>
        [MaxLength(50)]
        public string? Gender { get; set; }

        /// <summary>
        /// JSON array of available sizes
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? SizesJson { get; set; }

        /// <summary>
        /// Average rating from customer reviews
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Total number of ratings received
        /// </summary>
        public int RatingCount { get; set; }

        /// <summary>
        /// Whether this product is featured
        /// </summary>
        public bool Featured { get; set; }

        /// <summary>
        /// Whether this product is marked as new
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// JSON array of product image URLs
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? ImageUrlsJson { get; set; }

        /// <summary>
        /// Timestamp when the product was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the product was last updated
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        /// <summary>
        /// Collection of inventory items for this product
        /// </summary>
        public ICollection<Inventory>? Inventories { get; set; }

        /// <summary>
        /// Collection of product-collection associations
        /// </summary>
        public ICollection<ProductCollection>? ProductCollections { get; set; }

        /// <summary>
        /// Collection of reviews for this product
        /// </summary>
        public ICollection<Review>? Reviews { get; set; }

        /// <summary>
        /// Collection of order items containing this product
        /// </summary>
        public ICollection<OrderItem>? OrderItems { get; set; }

        /// <summary>
        /// Collection of wishlist entries for this product
        /// </summary>
        public ICollection<Wishlist>? Wishlists { get; set; }

        /// <summary>
        /// Collection of recently viewed records for this product
        /// </summary>
        public ICollection<RecentlyViewed>? RecentlyViewedRecords { get; set; }
    }
}
