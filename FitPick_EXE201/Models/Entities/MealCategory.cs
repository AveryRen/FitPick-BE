using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("meal_categories")]
[Index("Name", Name = "meal_categories_name_key", IsUnique = true)]
public partial class MealCategory
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; set; } = null!;

    [InverseProperty("Category")]
    public virtual ICollection<Meal> Meals { get; set; } = new List<Meal>();
}
