using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurtis.Common.Models
{
    /// <summary>
    /// Represents a customer order
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Primary key for the order
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the User table (nullable for guest checkout)
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Navigation property to the User
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        /// <summary>
        /// Customer email address
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Customer full name
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Shipping/billing address
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = null!;

        /// <summary>
        /// City
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string City { get; set; } = null!;

        /// <summary>
        /// State/Province
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string State { get; set; } = null!;

        /// <summary>
        /// Postal/ZIP code
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; } = null!;

        /// <summary>
        /// Contact phone number
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = null!;

        /// <summary>
        /// Order status (pending, processing, shipped, delivered, cancelled)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "pending";

        /// <summary>
        /// Total order amount
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Total must be non-negative")]
        public decimal Total { get; set; }

        /// <summary>
        /// Timestamp when the order was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when the order was last updated
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Collection of items in this order
        /// </summary>
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Collection of returns associated with this order
        /// </summary>
        public ICollection<Return>? Returns { get; set; }
    }
}