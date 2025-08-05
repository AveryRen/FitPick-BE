using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Notification
{
    public int Notificationid { get; set; }

    public int? Userid { get; set; }

    public string? Title { get; set; }

    public string? Message { get; set; }

    public bool? Isread { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? User { get; set; }
}
