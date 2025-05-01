namespace api.DTOs.Comment
{
    public class CommentResponseDTO
    {
        public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}