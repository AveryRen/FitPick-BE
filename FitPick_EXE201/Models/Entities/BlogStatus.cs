using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("blog_statuses")]
[Index("Name", Name = "blog_statuses_name_key", IsUnique = true)]
public partial class BlogStatus
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; set; } = null!;

    [InverseProperty("Status")]
    public virtual ICollection<Blogpost> Blogposts { get; set; } = new List<Blogpost>();
}
