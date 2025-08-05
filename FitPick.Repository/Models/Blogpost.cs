using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class Blogpost
{
    public int Postid { get; set; }

    public string? Title { get; set; }

    public string? Shortdescription { get; set; }

    public string? Content { get; set; }

    public int? Authorid { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual User? Author { get; set; }
}
