using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("subscription_plans")]
[Index("Name", Name = "subscription_plans_name_key", IsUnique = true)]
public partial class SubscriptionPlan
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; set; } = null!;

    [InverseProperty("Plan")]
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
