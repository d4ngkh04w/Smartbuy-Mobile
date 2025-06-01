using api.DTOs.Comment;

namespace api.Interfaces.Services
{
    public interface ICommentService
    {
        Task<CommentDTO> GetCommentByIdAsync(int id);
        Task<CommentResponseDTO> GetCommentsByProductIdAsync(int productId, int page, int pageSize);
        Task<CommentDTO> CreateCommentAsync(CreateCommentDTO commentDTO, Guid userId);
        Task<CommentDTO> UpdateCommentAsync(int id, UpdateCommentDTO commentDTO, Guid userId);
        Task DeleteCommentAsync(int id, Guid userId);
        Task<CommentDTO> AddReactionAsync(int commentId, CommentReactionDTO reactionDTO, Guid userId);
        Task<ProductRatingStatsDTO> GetProductRatingStatsAsync(int productId);
    }
}