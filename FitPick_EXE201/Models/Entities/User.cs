using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
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

    [Column("role_id")]
    public int? RoleId { get; set; }

    [Column("status")]
    public bool? Status { get; set; }

    [Column("createdat", TypeName = "timestamp without time zone")]
    public DateTime? Createdat { get; set; }

    [Column("updatedat", TypeName = "timestamp without time zone")]
    public DateTime? Updatedat { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("is_email_verified")]
    public bool? IsEmailVerified { get; set; }

    [InverseProperty("Author")]
    public virtual ICollection<Blogpost> Blogposts { get; set; } = new List<Blogpost>();

    [ForeignKey("GenderId")]
    [InverseProperty("Users")]
    public virtual Gender? Gender { get; set; }

    [InverseProperty("User")]
    [JsonIgnore]
    public virtual ICollection<Healthprofile> Healthprofiles { get; set; } = new List<Healthprofile>();

    [InverseProperty("User")]
    [JsonIgnore]
    public virtual ICollection<MealHistory> MealHistories { get; set; } = new List<MealHistory>();

    [InverseProperty("User")]
    public virtual ICollection<MealReview> MealReviews { get; set; } = new List<MealReview>();

    [InverseProperty("User")]
    [JsonIgnore]
    public virtual ICollection<Mealplan> Mealplans { get; set; } = new List<Mealplan>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("User")]
    public virtual ICollection<PayosPayment> PayosPayments { get; set; } = new List<PayosPayment>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    [JsonIgnore]
    public virtual UserRole? Role { get; set; }
}
