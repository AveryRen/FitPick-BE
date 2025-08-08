using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("subscriptions")]
public partial class Subscription
{
    [Key]
    [Column("subscriptionid")]
    public int Subscriptionid { get; set; }

    [Column("userid")]
    public int? Userid { get; set; }

    [Column("plan_id")]
    public int? PlanId { get; set; }

    [Column("startdate", TypeName = "timestamp without time zone")]
    public DateTime Startdate { get; set; }

    [Column("enddate", TypeName = "timestamp without time zone")]
    public DateTime Enddate { get; set; }

    [Column("amount")]
    [Precision(10, 2)]
    public decimal? Amount { get; set; }

    [Column("status_id")]
    public int? StatusId { get; set; }

    [ForeignKey("PlanId")]
    [InverseProperty("Subscriptions")]
    public virtual SubscriptionPlan? Plan { get; set; }

    [ForeignKey("StatusId")]
    [InverseProperty("Subscriptions")]
    public virtual SubscriptionStatus? Status { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("Subscriptions")]
    public virtual User? User { get; set; }
}
