using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurtis.Common.Models
{
    /// <summary>
    /// Represents a product in a user's wishlist
    /// </summary>
    public class Wishlist
    {
        /// <summary>
        /// Primary key for the wishlist item
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the User table
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Navigation property to the User
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

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
        /// Timestamp when the product was added to wishlist
        /// </summary>
        [Required]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
