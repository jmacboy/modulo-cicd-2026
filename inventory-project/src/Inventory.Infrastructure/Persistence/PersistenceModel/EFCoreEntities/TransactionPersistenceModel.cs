using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Infrastructure.Persistence.PersistenceModel.Entities;

[Table("transaction")]
internal class TransactionPersistenceModel
{
    [Key]
    [Column("transactionId")]
    public Guid Id { get; set; }

    [Required]
    [Column("userCreatorId")]
    public Guid UserCreatorId { get; set; }

    public UserPersistenceModel UserCreator { get; set; }

    [Required]
    [Column("transactionType")]
    [MaxLength(25)]
    public string TransactionType { get; set; }

    [Required]
    [Column("creationDate")]
    public DateTime CreationDate { get; set; }

    [Column("completedDate")]
    public DateTime? CompletedDate { get; set; }

    [Column("cancelDate")]
    public DateTime? CancelDate { get; set; }

    [Required]
    [Column("totalCost", TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }

    [Required]
    [Column("status")]
    [MaxLength(25)]
    public string Status { get; set; }


    [Column("sourceType")]
    [MaxLength(25)]
    public string? SourceType { get; set; }

    [Column("sourceId")]
    public Guid? SourceId { get; set; }


    public List<TransactionItemStoredModel> Items { get; set; }

}
