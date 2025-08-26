using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("user_ingredients")]
public partial class UserIngredient
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("userid")]
    public int? Userid { get; set; }

    [Column("ingredientid")]
    public int? Ingredientid { get; set; }

    [Column("quantity")]
    [Precision(10, 2)]
    public decimal Quantity { get; set; }

    [Column("unit")]
    [StringLength(20)]
    public string? Unit { get; set; }

    [Column("updatedat", TypeName = "timestamp without time zone")]
    public DateTime? Updatedat { get; set; }

    [ForeignKey("Ingredientid")]
    [InverseProperty("UserIngredients")]
    public virtual Ingredient? Ingredient { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("UserIngredients")]
    public virtual User? User { get; set; }
}
