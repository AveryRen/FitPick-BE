using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("meal_history")]
public partial class MealHistory
{
    [Key]
    [Column("historyid")]
    public int Historyid { get; set; }

    [Column("userid")]
    public int Userid { get; set; }

    [Column("mealid")]
    public int? Mealid { get; set; }

    [Column("mealtime_id")]
    public int? MealtimeId { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("quantity")]
    [Precision(10, 2)]
    public decimal? Quantity { get; set; }

    [Column("unit")]
    [StringLength(50)]
    public string? Unit { get; set; }

    [Column("calories")]
    public int? Calories { get; set; }

    [Column("createdat", TypeName = "timestamp without time zone")]
    public DateTime? Createdat { get; set; }

    [Column("consumed_at", TypeName = "timestamp without time zone")]
    public DateTime? ConsumedAt { get; set; }

    [ForeignKey("Mealid")]
    [InverseProperty("MealHistories")]
    public virtual Meal? Meal { get; set; }

    [ForeignKey("MealtimeId")]
    [InverseProperty("MealHistories")]
    public virtual MealTime? Mealtime { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("MealHistories")]
    public virtual User User { get; set; } = null!;
}
