using api.DTOs.Comment;
using api.Exceptions;
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

        public async Task<CommentDTO> GetCommentByIdAsync(int id)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id) ?? throw new NotFoundException("Comment not found");

            // Lấy những phản hồi của bình luận
            var replies = await _commentRepository.GetRepliesByParentIdAsync(id);
            var commentDto = comment.ToCommentDTO();
            commentDto.Replies = replies.Select(r => r.ToCommentDTO()).ToList();

            return commentDto;
        }

        public async Task<CommentResponseDTO> GetCommentsByProductIdAsync(int productId, int page, int pageSize)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundException("Product not found");
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

            return result;
        }

        public async Task<CommentDTO> CreateCommentAsync(CreateCommentDTO commentDTO, Guid userId)
        {
            _ = await _productRepository.GetByIdAsync(commentDTO.ProductId) ?? throw new NotFoundException("Product not found");
            if (commentDTO.ParentId.HasValue)
            {
                var parentComment = await _commentRepository.GetCommentByIdAsync(commentDTO.ParentId.Value) ?? throw new NotFoundException("Parent comment not found");

                // Kiểm tra xem bình luận cha có thuộc về sản phẩm không
                if (parentComment.ProductId != commentDTO.ProductId)
                {
                    throw new BadRequestException("Parent comment is not associated with the specified product");
                }

                // Rating chỉ cho bình luận gốc
                if (commentDTO.Rating.HasValue)
                {
                    throw new BadRequestException("Rating can only be provided in top-level comments");
                }
            }

            if (commentDTO.Rating.HasValue && (commentDTO.Rating < 1 || commentDTO.Rating > 5))
            {
                throw new BadRequestException("Rating must be between 1 and 5");
            }

            var comment = commentDTO.ToCommentModel(userId);
            comment.Content = comment.Content.Trim();
            var createdComment = await _commentRepository.CreateCommentAsync(comment);

            var commentDto = createdComment.ToCommentDTO();

            return commentDto;
        }

        public async Task<CommentDTO> UpdateCommentAsync(int id, UpdateCommentDTO commentDTO, Guid userId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id) ?? throw new NotFoundException("Comment not found");
            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this comment");
            }

            if (comment.IsDeleted)
            {
                throw new BadRequestException("Cannot update a deleted comment");
            }

            if (commentDTO.Rating.HasValue && (commentDTO.Rating < 1 || commentDTO.Rating > 5))
            {
                throw new BadRequestException("Rating must be between 1 and 5");
            }

            if (comment.ParentId.HasValue && commentDTO.Rating.HasValue)
            {
                throw new BadRequestException("Rating can only be provided in top-level comments");
            }

            comment.Content = commentDTO.Content.Trim();
            comment.Rating = commentDTO.Rating ?? comment.Rating;
            comment.UpdatedAt = DateTime.Now;

            var commentRes = await _commentRepository.UpdateCommentAsync(comment);

            return commentRes.ToCommentDTO();
        }

        public async Task DeleteCommentAsync(int id, Guid userId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id) ?? throw new NotFoundException("Comment not found");
            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this comment");
            }

            bool deleted = await _commentRepository.DeleteCommentAsync(id);
            if (!deleted)
            {
                throw new ServerException("Failed to delete comment");
            }
        }

        public async Task<CommentDTO> AddReactionAsync(int commentId, CommentReactionDTO reactionDTO, Guid userId)
        {
            _ = await _commentRepository.GetCommentByIdAsync(commentId) ?? throw new NotFoundException("Comment not found");

            bool added = await _commentRepository.AddReactionAsync(commentId, userId, reactionDTO.IsLike);
            if (!added)
            {
                throw new ServerException("Failed to add reaction");
            }

            var comment = await _commentRepository.GetCommentByIdAsync(commentId);

            return comment!.ToCommentDTO();
        }

        public async Task<double> GetProductAverageRatingAsync(int productId)
        {
            _ = await _productRepository.GetByIdAsync(productId) ?? throw new NotFoundException("Product not found");
            double? averageRating = await _commentRepository.GetProductAverageRatingAsync(productId);
            return averageRating ?? 0.0;
        }
    }
}