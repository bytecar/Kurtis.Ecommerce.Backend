using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurtis.Common.Models;

/// <summary>
/// Tracks products recently viewed by users
/// </summary>
public class RecentlyViewed
{
    /// <summary>
    /// Primary key for the recently viewed record
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
    /// Timestamp when the product was viewed
    /// </summary>
    [Required]
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}
