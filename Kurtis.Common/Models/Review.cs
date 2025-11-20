using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurtis.Common.Models
{
    /// <summary>
    /// Represents a customer review for a product
    /// </summary>
    public class Review
    {
        /// <summary>
        /// Primary key for the review
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
        /// Rating from 1 to 5
        /// </summary>
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        /// <summary>
        /// Review comment text
        /// </summary>
        [MaxLength(2000)]
        public string? Comment { get; set; }

        /// <summary>
        /// Timestamp when the review was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
