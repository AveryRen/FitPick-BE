using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("locationingredients")]
public partial class Locationingredient
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("locationid")]
    public int? Locationid { get; set; }

    [Column("ingredientid")]
    public int? Ingredientid { get; set; }

    [Column("price")]
    [Precision(10, 2)]
    public decimal? Price { get; set; }

    [Column("availability")]
    public bool? Availability { get; set; }

    [ForeignKey("Ingredientid")]
    [InverseProperty("Locationingredients")]
    public virtual Ingredient? Ingredient { get; set; }

    [ForeignKey("Locationid")]
    [InverseProperty("Locationingredients")]
    public virtual Location? Location { get; set; }
}
