using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Infrastructure.Persistence.PersistenceModel.Entities;

[Table("item")]
internal class ItemPersistenceModel
{
    [Key]
    [Column("itemId")]
    public Guid Id { get; set; }

    [Column("itemName")]
    [StringLength(250)]
    [Required]
    public string ItemName { get; set; }

    [Column("stock")]
    [Required]
    public int Stock { get; set; }

    [Column("reserved")]
    [Required]
    public int Reserverd { get; set; }

    [Column("available")]
    [Required]
    public int Available { get; set; }

    [Column("unitaryCost", TypeName = "decimal(18,2)")]
    [Required]
    public decimal UnitaryCost { get; set; }

    public DateTime CreationDate { get; set; } = DateTime.Now;
}
