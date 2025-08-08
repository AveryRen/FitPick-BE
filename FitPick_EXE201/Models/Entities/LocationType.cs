using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("location_types")]
[Index("Name", Name = "location_types_name_key", IsUnique = true)]
public partial class LocationType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; set; } = null!;

    [InverseProperty("Type")]
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}
