using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("healthgoals")]
public partial class Healthgoal
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("calorie_adjustment")]
    public int CalorieAdjustment { get; set; }

    [InverseProperty("Healthgoal")]
    public virtual ICollection<Healthprofile> Healthprofiles { get; set; } = new List<Healthprofile>();
}
