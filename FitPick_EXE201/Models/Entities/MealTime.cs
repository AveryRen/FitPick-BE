using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("meal_times")]
[Index("Name", Name = "meal_times_name_key", IsUnique = true)]
public partial class MealTime
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; set; } = null!;

    [InverseProperty("Mealtime")]
    public virtual ICollection<MealHistory> MealHistories { get; set; } = new List<MealHistory>();

    [InverseProperty("Mealtime")]
    public virtual ICollection<Mealplan> Mealplans { get; set; } = new List<Mealplan>();
}
