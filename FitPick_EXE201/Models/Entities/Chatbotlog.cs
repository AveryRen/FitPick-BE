using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("chatbotlogs")]
public partial class Chatbotlog
{
    [Key]
    [Column("logid")]
    public int Logid { get; set; }

    [Column("userid")]
    public int? Userid { get; set; }

    [Column("question")]
    public string? Question { get; set; }

    [Column("answer")]
    public string? Answer { get; set; }

    [Column("createdat", TypeName = "timestamp without time zone")]
    public DateTime? Createdat { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("Chatbotlogs")]
    public virtual User? User { get; set; }
}
