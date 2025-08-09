using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("users")]
[Index("Email", Name = "users_email_key", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("userid")]
    public int Userid { get; set; }

    [Column("fullname")]
    [StringLength(100)]
    public string? Fullname { get; set; }

    [Column("email")]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Column("passwordhash")]
    [StringLength(255)]
    public string Passwordhash { get; set; } = null!;

    [Column("gender_id")]
    public int? GenderId { get; set; }

    [Column("age")]
    public int? Age { get; set; }

    [Column("height")]
    [Precision(5, 2)]
    public decimal? Height { get; set; }

    [Column("weight")]
    [Precision(5, 2)]
    public decimal? Weight { get; set; }

    [Column("country")]
    [StringLength(100)]
    public string? Country { get; set; }

    [Column("city")]
    [StringLength(100)]
    public string? City { get; set; }

    [Column("role_id")]
    public int? RoleId { get; set; }

    [Column("status")]
    public bool? Status { get; set; }

    [Column("createdat", TypeName = "timestamp without time zone")]
    public DateTime? Createdat { get; set; }

    [Column("updatedat", TypeName = "timestamp without time zone")]
    public DateTime? Updatedat { get; set; }

    [InverseProperty("Author")]
    public virtual ICollection<Blogpost> Blogposts { get; set; } = new List<Blogpost>();

    [InverseProperty("User")]
    public virtual ICollection<Chatbotlog> Chatbotlogs { get; set; } = new List<Chatbotlog>();

    [InverseProperty("User")]
    public virtual ICollection<Favoritemeal> Favoritemeals { get; set; } = new List<Favoritemeal>();

    [ForeignKey("GenderId")]
    [InverseProperty("Users")]
    public virtual Gender? Gender { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Healthprofile> Healthprofiles { get; set; } = new List<Healthprofile>();

    [InverseProperty("User")]
    public virtual ICollection<Mealplan> Mealplans { get; set; } = new List<Mealplan>();

    [InverseProperty("CreatedbyNavigation")]
    public virtual ICollection<Meal> Meals { get; set; } = new List<Meal>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual UserRole? Role { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Shoppinglist> Shoppinglists { get; set; } = new List<Shoppinglist>();

    [InverseProperty("User")]
    public virtual ICollection<Spendinglog> Spendinglogs { get; set; } = new List<Spendinglog>();

    [InverseProperty("User")]
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
