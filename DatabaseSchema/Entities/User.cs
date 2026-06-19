using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseSchema.Entities;

public class User {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long UserId { get; set; }

    [Required]
    [Column]
    public string UserName { get; set; } = null!;

    [Required]
    [Column]
    public string Password { get; set; } = null!;

    [Required]
    [Column]
    public string NickName { get; set; } = null!;

    [Required]
    [Column(TypeName = "timestamptz(6)")]
    public DateTime CreatedAt { get; set; }
}