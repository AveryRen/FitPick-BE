using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("blogposts")]
public partial class Blogpost
{
    [Key]
    [Column("postid")]
    public int Postid { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("content")]
    public string? Content { get; set; }

    [Column("authorid")]
    public int? Authorid { get; set; }

    [Column("categoryid")]
    public int? Categoryid { get; set; }

    [Column("status")]
    public bool Status { get; set; }

    [Column("createdat", TypeName = "timestamp without time zone")]
    public DateTime? Createdat { get; set; }

    [Column("updatedat", TypeName = "timestamp without time zone")]
    public DateTime? Updatedat { get; set; }

    [ForeignKey("Authorid")]
    [InverseProperty("Blogposts")]
    public virtual User? Author { get; set; }

    [InverseProperty("Blog")]
    public virtual ICollection<BlogMedium> BlogMedia { get; set; } = new List<BlogMedium>();

    [ForeignKey("Categoryid")]
    [InverseProperty("Blogposts")]
    public virtual BlogCategory? Category { get; set; }
}
