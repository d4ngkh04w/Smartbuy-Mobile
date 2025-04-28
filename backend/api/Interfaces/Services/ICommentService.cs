using api.DTOs.Comment;

namespace api.Interfaces.Services
{
    public interface ICommentService
    {
        Task<(bool Success, string? ErrorMessage, CommentDTO? Comment)> GetCommentByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage, CommentResponseDTO? Comments)> GetCommentsByProductIdAsync(int productId, int page, int pageSize);
        Task<(bool Success, string? ErrorMessage, CommentDTO? Comment)> CreateCommentAsync(CreateCommentDTO commentDTO, Guid userId);
        Task<(bool Success, string? ErrorMessage, CommentDTO? Comment)> UpdateCommentAsync(int id, UpdateCommentDTO commentDTO, Guid userId);
        Task<(bool Success, string? ErrorMessage)> DeleteCommentAsync(int id, Guid userId);
        Task<(bool Success, string? ErrorMessage, CommentDTO? Comment)> AddReactionAsync(int commentId, CommentReactionDTO reactionDTO, Guid userId);
        Task<(bool Success, string? ErrorMessage, double? Rating)> GetProductAverageRatingAsync(int productId);
    }
}