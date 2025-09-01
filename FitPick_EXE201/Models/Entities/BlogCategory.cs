using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("blog_category")]
public partial class BlogCategory
{
    [Key]
    [Column("categoryid")]
    public int Categoryid { get; set; }

    [Column("category_name")]
    [StringLength(100)]
    public string CategoryName { get; set; } = null!;

    [InverseProperty("Category")]
    [JsonIgnore]
    public virtual ICollection<Blogpost> Blogposts { get; set; } = new List<Blogpost>();
}
