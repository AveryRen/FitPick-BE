﻿namespace FitPick_EXE201.Models.Requests
{
    public class UpdateUserProfileRequest
    {
        public string? Fullname { get; set; }
        public int? GenderId { get; set; }
        public int? Age { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? Country { get; set; }
    }
}
