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
    public List<int>? Allergies { get; set; }

    [Column("chronicdiseases")]
    public List<int>? Chronicdiseases { get; set; }

    [Column("religiondiet")]
    public List<int>? Religiondiet { get; set; }

    [Column("dietarypreferences")]
    public List<int>? Dietarypreferences { get; set; }

    [Column("healthgoalid")]
    public int? Healthgoalid { get; set; }

    [Column("lifestyleid")]
    public int? Lifestyleid { get; set; }

    [Column("dailymeals")]
    public int? Dailymeals { get; set; }

    [Column("targetcalories")]
    public int? Targetcalories { get; set; }

    [Column("status")]
    public bool? Status { get; set; }

    [Column("updatedat", TypeName = "timestamp without time zone")]
    public DateTime? Updatedat { get; set; }

    [ForeignKey("Healthgoalid")]
    [InverseProperty("Healthprofiles")]
    public virtual Healthgoal? Healthgoal { get; set; }

    [ForeignKey("Lifestyleid")]
    [InverseProperty("Healthprofiles")]
    public virtual Lifestyle? Lifestyle { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("Healthprofiles")]
    public virtual User? User { get; set; }
}
