using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurtis.Common.Models;

/// <summary>
/// Represents an item within an order
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Primary key for the order item
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the Order table
    /// </summary>
    [Required]
    public int OrderId { get; set; }

    /// <summary>
    /// Navigation property to the parent Order
    /// </summary>
    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

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
    /// Size of the product ordered (e.g., S, M, L, XL)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Size { get; set; } = null!;

    /// <summary>
    /// Quantity of this product ordered
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    /// <summary>
    /// Price of the product at the time of order (preserves historical pricing)
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
    public decimal Price { get; set; }
}