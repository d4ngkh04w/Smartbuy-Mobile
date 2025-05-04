using api.DTOs.Comment;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;

namespace api.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IProductRepository _productRepository;

        public CommentService(ICommentRepository commentRepository, IProductRepository productRepository)
        {
            _commentRepository = commentRepository;
            _productRepository = productRepository;
        }

        public async Task<(bool Success, string? ErrorMessage, CommentDTO? Comment)> GetCommentByIdAsync(int id)
        {
            try
            {
                var comment = await _commentRepository.GetCommentByIdAsync(id);
                if (comment == null)
                {
                    return (false, "Comment not found", null);
                }

                // Lấy những phản hồi của bình luận
                var replies = await _commentRepository.GetRepliesByParentIdAsync(id);
                var commentDto = comment.ToCommentDTO();
                commentDto.Replies = replies.Select(r => r.ToCommentDTO()).ToList();

                return (true, null, commentDto);
            }
            catch (Exception)
            {
                return (false, $"Error retrieving comment", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, CommentResponseDTO? Comments)> GetCommentsByProductIdAsync(int productId, int page, int pageSize)
        {
            try
            {
                if (!await _productRepository.ExistsByNameAsync(productId.ToString()))
                {
                    return (false, "Product not found", null);
                }

                var comments = await _commentRepository.GetCommentsByProductIdAsync(productId, page, pageSize);
                // Tổng số bình luận cho sản phẩm
                var totalComments = await _commentRepository.GetCommentsCountByProductIdAsync(productId);
                // Tổng số trang
                var totalPages = (int)Math.Ceiling(totalComments / (double)pageSize);

                // Lấy các reply của từng comment
                var commentDtos = new List<CommentDTO>();
                foreach (var comment in comments)
                {
                    var dto = comment.ToCommentDTO();
                    var replies = await _commentRepository.GetRepliesByParentIdAsync(comment.Id);
                    dto.Replies = replies.Select(r => r.ToCommentDTO()).ToList();
                    commentDtos.Add(dto);
                }

                var result = new CommentResponseDTO
                {
                    Comments = commentDtos,
                    TotalItems = totalComments,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };

                return (true, null, result);
            }
            catch (Exception)
            {
                return (false, $"Error retrieving comments", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, CommentDTO? Comment)> CreateCommentAsync(CreateCommentDTO commentDTO, Guid userId)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(commentDTO.ProductId);
                if (product == null)
                {
                    return (false, "Product not found", null);
                }

                if (commentDTO.ParentId.HasValue)
                {
                    var parentComment = await _commentRepository.GetCommentByIdAsync(commentDTO.ParentId.Value);
                    if (parentComment == null)
                    {
                        return (false, "Parent comment not found", null);
                    }

                    // Kiểm tra xem bình luận cha có thuộc về sản phẩm không
                    if (parentComment.ProductId != commentDTO.ProductId)
                    {
                        return (false, "Parent comment is not associated with the specified product", null);
                    }

                    // Rating chỉ cho bình luận gốc
                    if (commentDTO.Rating.HasValue)
                    {
                        return (false, "Rating can only be provided in top-level comments", null);
                    }
                }

                if (commentDTO.Rating.HasValue && (commentDTO.Rating < 1 || commentDTO.Rating > 5))
                {
                    return (false, "Rating must be between 1 and 5", null);
                }

                var comment = commentDTO.ToCommentModel(userId);
                comment.Content = comment.Content.Trim();
                var createdComment = await _commentRepository.CreateCommentAsync(comment);

                Console.WriteLine($"{createdComment.User.Avatar}, {createdComment.User.Name}");

                var commentDto = createdComment.ToCommentDTO();

                return (true, null, commentDto);
            }
            catch (Exception)
            {
                return (false, $"Error creating comment", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, CommentDTO? Comment)> UpdateCommentAsync(int id, UpdateCommentDTO commentDTO, Guid userId)
        {
            try
            {
                var comment = await _commentRepository.GetCommentByIdAsync(id);
                if (comment == null)
                {
                    return (false, "Comment not found", null);
                }

                if (comment.UserId != userId)
                {
                    return (false, "You are not authorized to update this comment", null);
                }

                if (comment.IsDeleted)
                {
                    return (false, "Cannot update a deleted comment", null);
                }

                if (commentDTO.Rating.HasValue && (commentDTO.Rating < 1 || commentDTO.Rating > 5))
                {
                    return (false, "Rating must be between 1 and 5", null);
                }

                if (comment.ParentId.HasValue && commentDTO.Rating.HasValue)
                {
                    return (false, "Rating can only be provided in top-level comments", null);
                }

                comment.Content = commentDTO.Content.Trim();
                comment.Rating = commentDTO.Rating ?? comment.Rating;
                comment.UpdatedAt = DateTime.Now;

                bool updated = await _commentRepository.UpdateCommentAsync(comment);
                if (!updated)
                {
                    return (false, "Failed to update comment", null);
                }

                comment = await _commentRepository.GetCommentByIdAsync(id);
                var commentDto = comment!.ToCommentDTO();

                return (true, null, commentDto);
            }
            catch (Exception)
            {
                return (false, $"Error updating comment", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteCommentAsync(int id, Guid userId)
        {
            try
            {
                var comment = await _commentRepository.GetCommentByIdAsync(id);
                if (comment == null)
                {
                    return (false, "Comment not found");
                }

                if (comment.UserId != userId)
                {
                    return (false, "You are not authorized to delete this comment");
                }

                bool deleted = await _commentRepository.DeleteCommentAsync(id);
                if (!deleted)
                {
                    return (false, "Failed to delete comment");
                }

                return (true, null);
            }
            catch (Exception)
            {
                return (false, $"Error deleting comment");
            }
        }

        public async Task<(bool Success, string? ErrorMessage, CommentDTO? Comment)> AddReactionAsync(int commentId, CommentReactionDTO reactionDTO, Guid userId)
        {
            try
            {
                var comment = await _commentRepository.GetCommentByIdAsync(commentId);
                if (comment == null)
                {
                    return (false, "Comment not found", null);
                }

                bool added = await _commentRepository.AddReactionAsync(commentId, userId, reactionDTO.IsLike);
                if (!added)
                {
                    return (false, "Failed to add reaction", null);
                }

                comment = await _commentRepository.GetCommentByIdAsync(commentId);
                var commentDto = comment!.ToCommentDTO();

                return (true, null, commentDto);
            }
            catch (Exception)
            {
                return (false, $"Error adding reaction", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, double? Rating)> GetProductAverageRatingAsync(int productId)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    return (false, "Product not found", null);
                }

                double? averageRating = await _commentRepository.GetProductAverageRatingAsync(productId);

                return (true, null, averageRating);
            }
            catch (Exception)
            {
                return (false, $"Error retrieving product rating", null);
            }
        }
    }
}