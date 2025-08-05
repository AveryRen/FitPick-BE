using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Chatbotlog
{
    public int Logid { get; set; }

    public int? Userid { get; set; }

    public string? Question { get; set; }

    public string? Answer { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? User { get; set; }
}
