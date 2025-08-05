using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Healthprofile
{
    public int Profileid { get; set; }

    public int? Userid { get; set; }

    public string? Allergies { get; set; }

    public string? Chronicdiseases { get; set; }

    public string? Religiondiet { get; set; }

    public string? Dietarypreferences { get; set; }

    public string? Healthgoal { get; set; }

    public string? Lifestyle { get; set; }

    public int? Dailymeals { get; set; }

    public int? Targetcalories { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual User? User { get; set; }
}
