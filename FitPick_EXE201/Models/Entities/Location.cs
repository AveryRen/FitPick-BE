using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("locations")]
public partial class Location
{
    [Key]
    [Column("locationid")]
    public int Locationid { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string? Name { get; set; }

    [Column("address")]
    [StringLength(255)]
    public string? Address { get; set; }

    [Column("latitude")]
    [Precision(10, 6)]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    [Precision(10, 6)]
    public decimal? Longitude { get; set; }

    [Column("type_id")]
    public int? TypeId { get; set; }

    [InverseProperty("Location")]
    public virtual ICollection<Locationingredient> Locationingredients { get; set; } = new List<Locationingredient>();

    [ForeignKey("TypeId")]
    [InverseProperty("Locations")]
    public virtual LocationType? Type { get; set; }
}
