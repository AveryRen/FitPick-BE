using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("shoppinglists")]
public partial class Shoppinglist
{
    [Key]
    [Column("listid")]
    public int Listid { get; set; }

    [Column("userid")]
    public int? Userid { get; set; }

    [Column("date")]
    public DateOnly Date { get; set; }

    [Column("itemname")]
    [StringLength(100)]
    public string? Itemname { get; set; }

    [Column("quantity")]
    [Precision(10, 2)]
    public decimal? Quantity { get; set; }

    [Column("unit")]
    [StringLength(20)]
    public string? Unit { get; set; }

    [Column("status_id")]
    public int? StatusId { get; set; }

    [ForeignKey("StatusId")]
    [InverseProperty("Shoppinglists")]
    public virtual ShoppingStatus? Status { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("Shoppinglists")]
    public virtual User? User { get; set; }
}
