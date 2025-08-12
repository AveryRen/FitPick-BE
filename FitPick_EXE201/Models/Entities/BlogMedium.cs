using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("blog_media")]
public partial class BlogMedium
{
    [Key]
    [Column("media_id")]
    public int MediaId { get; set; }

    [Column("blog_id")]
    public int BlogId { get; set; }

    [Column("media_url")]
    public string MediaUrl { get; set; } = null!;

    [Column("media_type")]
    [StringLength(20)]
    public string MediaType { get; set; } = null!;

    [Column("order_index")]
    public int? OrderIndex { get; set; }

    [ForeignKey("BlogId")]
    [InverseProperty("BlogMedia")]
    [JsonIgnore]
    public virtual Blogpost Blog { get; set; } = null!;
}
