using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("healthprofiles")]
public partial class Healthprofile
{
    [Key]
    [Column("profileid")]
    public int Profileid { get; set; }

    [Column("userid")]
    public int? Userid { get; set; }

    [Column("allergies")]
    public string? Allergies { get; set; }

    [Column("chronicdiseases")]
    public string? Chronicdiseases { get; set; }

    [Column("religiondiet")]
    [StringLength(100)]
    public string? Religiondiet { get; set; }

    [Column("dietarypreferences")]
    public string? Dietarypreferences { get; set; }

    [Column("healthgoal")]
    [StringLength(50)]
    public string? Healthgoal { get; set; }

    [Column("lifestyle")]
    [StringLength(50)]
    public string? Lifestyle { get; set; }

    [Column("dailymeals")]
    public int? Dailymeals { get; set; }

    [Column("targetcalories")]
    public int? Targetcalories { get; set; }

    [Column("updatedat", TypeName = "timestamp without time zone")]
    public DateTime? Updatedat { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("Healthprofiles")]
    public virtual User? User { get; set; }
}
