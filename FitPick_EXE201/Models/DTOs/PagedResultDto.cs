namespace FitPick_EXE201.Models.DTOs
{
    public class PagedResult<T>
    {
        // Properties cho MealReview (sử dụng Data)
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        
        // TotalPages có thể được set hoặc computed
        private int? _totalPages;
        public int TotalPages 
        { 
            get => _totalPages ?? (int)Math.Ceiling((double)TotalCount / PageSize);
            set => _totalPages = value;
        }
        
        // Properties cho Blog (sử dụng Items) - để tương thích ngược
        public IEnumerable<T> Items 
        { 
            get => Data; 
            set => Data = value; 
        }
        public int TotalItems 
        { 
            get => TotalCount; 
            set => TotalCount = value; 
        }
        public int PageNumber 
        { 
            get => Page; 
            set => Page = value; 
        }
    }

    public class MealRatingStatsDto
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new Dictionary<int, int>();
    }
}
