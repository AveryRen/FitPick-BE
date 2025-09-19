using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("payos_payments")]
public partial class PayosPayment
{
    [Key]
    [Column("paymentid")]
    public int Paymentid { get; set; }

    [Column("userid")]
    public int Userid { get; set; }

    [Column("order_code")]
    public long OrderCode { get; set; }

    [Column("payment_link_id")]
    [StringLength(100)]
    public string PaymentLinkId { get; set; } = null!;

    [Column("amount")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Column("transaction_datetime", TypeName = "timestamp without time zone")]
    public DateTime? TransactionDatetime { get; set; }

    [Column("checkout_url")]
    [StringLength(255)]
    public string? CheckoutUrl { get; set; }

    [Column("createdat", TypeName = "timestamp without time zone")]
    public DateTime? Createdat { get; set; }

    [Column("updatedat", TypeName = "timestamp without time zone")]
    public DateTime? Updatedat { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("PayosPayments")]
    public virtual User User { get; set; } = null!;
}
