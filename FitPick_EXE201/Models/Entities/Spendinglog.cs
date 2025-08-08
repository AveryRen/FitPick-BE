using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("spendinglogs")]
public partial class Spendinglog
{
    [Key]
    [Column("spendingid")]
    public int Spendingid { get; set; }

    [Column("userid")]
    public int? Userid { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("amount")]
    [Precision(10, 2)]
    public decimal? Amount { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("Spendinglogs")]
    public virtual User? User { get; set; }
}
