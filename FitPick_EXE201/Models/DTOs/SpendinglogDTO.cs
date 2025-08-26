using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitPick_EXE201.Models.DTOs
{
    public class SpendinglogDTO
    {
        public int Spendingid { get; set; }
        public int? Userid { get; set; }
        public DateOnly Date { get; set; }
        public decimal? Amount { get; set; }
        public string? Note { get; set; }
    }
}
