using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Location
{
    public int Locationid { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public virtual ICollection<Locationingredient> Locationingredients { get; set; } = new List<Locationingredient>();
}
