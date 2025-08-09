using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("lifestyles")]
public partial class Lifestyle
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("multiplier")]
    [Precision(5, 3)]
    public decimal Multiplier { get; set; }

    [InverseProperty("Lifestyle")]
    public virtual ICollection<Healthprofile> Healthprofiles { get; set; } = new List<Healthprofile>();
}
