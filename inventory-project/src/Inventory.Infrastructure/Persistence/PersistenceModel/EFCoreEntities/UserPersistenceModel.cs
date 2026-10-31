using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Infrastructure.Persistence.PersistenceModel.Entities;

[Table("user")]
internal class UserPersistenceModel
{
    [Key]
    [Column("userId")]
    public Guid Id { get; set; }

    [Column("fullName")]
    [StringLength(250)]
    [Required]
    public string FullName { get; set; }
}
