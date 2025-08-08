using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("shopping_statuses")]
[Index("Name", Name = "shopping_statuses_name_key", IsUnique = true)]
public partial class ShoppingStatus
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; set; } = null!;

    [InverseProperty("Status")]
    public virtual ICollection<Shoppinglist> Shoppinglists { get; set; } = new List<Shoppinglist>();
}
