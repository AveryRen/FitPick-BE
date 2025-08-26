using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("notifications")]
public partial class Notification
{
    [Key]
    [Column("notificationid")]
    public int Notificationid { get; set; }

    [Column("userid")]
    public int? Userid { get; set; }

    [Column("title")]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Column("message")]
    public string Message { get; set; } = null!;

    [Column("type_id")]
    public int? TypeId { get; set; }

    [Column("isread")]
    public bool? Isread { get; set; }

    [Column("createdat", TypeName = "timestamp without time zone")]
    public DateTime? Createdat { get; set; }

    [Column("scheduledat", TypeName = "timestamp without time zone")]
    public DateTime? Scheduledat { get; set; }

    [ForeignKey("TypeId")]
    [InverseProperty("Notifications")]
    public virtual NotificationType? Type { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("Notifications")]
    public virtual User? User { get; set; }
}
