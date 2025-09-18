using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace FitPick_EXE201.Models.Entities;

[Table("meal_instructions")]
public partial class MealInstruction
{
    [Key]
    [Column("instruction_id")]
    public int InstructionId { get; set; }

    [Column("meal_id")]
    public int? MealId { get; set; }

    [Column("step_number")]
    public int StepNumber { get; set; }

    [Column("instruction")]
    public string Instruction { get; set; } = null!;

    [ForeignKey("MealId")]
    [InverseProperty("MealInstructions")]
    public virtual Meal? Meal { get; set; }
}
