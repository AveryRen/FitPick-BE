using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("favoritemeals")]
public partial class Favoritemeal
{
    [Key]
    [Column("favoriteid")]
    public int Favoriteid { get; set; }

    [Column("userid")]
    public int? Userid { get; set; }

    [Column("mealid")]
    public int? Mealid { get; set; }

    [Column("rating")]
    public int? Rating { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    [ForeignKey("Mealid")]
    [InverseProperty("Favoritemeals")]
    public virtual Meal? Meal { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("Favoritemeals")]
    public virtual User? User { get; set; }
}
