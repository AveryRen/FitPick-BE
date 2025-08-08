using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("mealplans")]
public partial class Mealplan
{
    [Key]
    [Column("planid")]
    public int Planid { get; set; }

    [Column("userid")]
    public int? Userid { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("mealid")]
    public int? Mealid { get; set; }

    [Column("mealtime_id")]
    public int? MealtimeId { get; set; }

    [Column("status_id")]
    public int? StatusId { get; set; }

    [ForeignKey("Mealid")]
    [InverseProperty("Mealplans")]
    public virtual Meal? Meal { get; set; }

    [ForeignKey("MealtimeId")]
    [InverseProperty("Mealplans")]
    public virtual MealTime? Mealtime { get; set; }

    [ForeignKey("StatusId")]
    [InverseProperty("Mealplans")]
    public virtual PlanStatus? Status { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("Mealplans")]
    public virtual User? User { get; set; }
}
