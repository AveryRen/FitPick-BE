using System;
using System.Collections.Generic;

namespace FitPick.Repository.Models;

public partial class User
{
    public int Userid { get; set; }

    public string? Name { get; set; }

    public string Email { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public int? Age { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? Gender { get; set; } 

    public string? Role { get; set; }   

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }

    // Navigation properties
    public virtual ICollection<Blogpost> Blogposts { get; set; } = new List<Blogpost>();

    public virtual ICollection<Chatbotlog> Chatbotlogs { get; set; } = new List<Chatbotlog>();

    public virtual ICollection<Favoritemeal> Favoritemeals { get; set; } = new List<Favoritemeal>();

    public virtual ICollection<Healthprofile> Healthprofiles { get; set; } = new List<Healthprofile>();

    public virtual ICollection<Mealplan> Mealplans { get; set; } = new List<Mealplan>();

    public virtual ICollection<Meal> Meals { get; set; } = new List<Meal>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Shoppinglist> Shoppinglists { get; set; } = new List<Shoppinglist>();

    public virtual ICollection<Spendinglog> Spendinglogs { get; set; } = new List<Spendinglog>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
