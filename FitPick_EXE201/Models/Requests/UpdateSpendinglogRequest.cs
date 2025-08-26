using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitPick_EXE201.Models.Requests
{
    public class UpdateSpendinglogRequest
    {
        public decimal? Amount { get; set; }
        public string? Note { get; set; }
    }
}
