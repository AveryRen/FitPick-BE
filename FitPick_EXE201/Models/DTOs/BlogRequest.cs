namespace FitPick_EXE201.Models.DTOs
{
    public class BlogRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int CategoryId { get; set; }
        public bool? Status { get; set; }
        public List<BlogMediaRequest>? Medias { get; set; }
    }

    public class BlogMediaRequest
    {
        public string MediaUrl { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;
        public int OrderIndex { get; set; } = 0;
    }

}
