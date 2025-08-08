using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("notification_types")]
[Index("Name", Name = "notification_types_name_key", IsUnique = true)]
public partial class NotificationType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; set; } = null!;

    [InverseProperty("Type")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
