namespace FitPick_EXE201.Models.DTOs
{
    public class BlogResponse
    {
        public int Postid { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public int? Categoryid { get; set; }
        public bool Status { get; set; }
        public DateTime? Createdat { get; set; }
        public DateTime? Updatedat { get; set; }
        public List<BlogMediaResponse>? Medias { get; set; }

        // Bạn có thể thêm thông tin tác giả (không chứa Blogposts)
        public AuthorResponse? Author { get; set; }
    }

    public class BlogMediaResponse
    {
        public int MediaId { get; set; }
        public string MediaUrl { get; set; }
        public string MediaType { get; set; }
        public int? OrderIndex { get; set; }
    }

    public class AuthorResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

}
