using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDBContext _db;

        public CommentRepository(AppDBContext db)
        {
            _db = db;
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _db.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Comment>> GetCommentsByProductIdAsync(int productId, int page, int pageSize)
        {
            // Lấy các bình luận không có replies (ParentId = null) và phân trang
            return await _db.Comments
                .Include(c => c.User)
                .Where(c => c.ProductId == productId && c.ParentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetRepliesByParentIdAsync(int parentId)
        {
            return await _db.Comments
                .Include(c => c.User)
                .Where(c => c.ParentId == parentId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetCommentsCountByProductIdAsync(int productId)
        {
            return await _db.Comments
                .Where(c => c.ProductId == productId && c.ParentId == null)
                .CountAsync();
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            await _db.Comments.AddAsync(comment);
            await _db.SaveChangesAsync();

            return await _db.Comments
                .Include(c => c.User)
                .FirstAsync(c => c.Id == comment.Id);
        }

        public async Task<Comment> UpdateCommentAsync(Comment comment)
        {
            _db.Comments.Update(comment);
            await _db.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var comment = await _db.Comments.FindAsync(id);
            if (comment == null)
                return false;

            comment.IsDeleted = true;
            comment.DeletedAt = DateTime.Now;

            _db.Comments.Update(comment);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddReactionAsync(int commentId, Guid userId, bool isLike)
        {
            var comment = await _db.Comments.FindAsync(commentId);
            if (comment == null)
                return false;

            if (isLike)
                comment.Likes += 1;
            else
                comment.Dislikes += 1;

            _db.Comments.Update(comment);
            return await _db.SaveChangesAsync() > 0;
        }
        public async Task<double> GetProductAverageRatingAsync(int productId)
        {
            var ratings = await _db.Comments
                .Where(c => c.ProductId == productId && c.Rating.HasValue)
                .Select(c => c.Rating!.Value)
                .ToListAsync();

            if (!ratings.Any())
                return 0;

            return ratings.Average();
        }

        public async Task<int> GetProductRatingCountAsync(int productId)
        {
            return await _db.Comments
                .Where(c => c.ProductId == productId && c.Rating.HasValue)
                .CountAsync();
        }

        public async Task<Dictionary<int, int>> GetProductRatingDistributionAsync(int productId)
        {
            // Initialize dictionary with all possible ratings (1-5) set to 0
            var distribution = new Dictionary<int, int>
            {
                { 1, 0 },
                { 2, 0 },
                { 3, 0 },
                { 4, 0 },
                { 5, 0 }
            };

            // Get rating counts grouped by rating value
            var ratingGroups = await _db.Comments
                .Where(c => c.ProductId == productId && c.Rating.HasValue)
                .GroupBy(c => c.Rating!.Value)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            // Update the distribution dictionary with actual counts
            foreach (var group in ratingGroups)
            {
                if (group.Rating >= 1 && group.Rating <= 5)
                {
                    distribution[group.Rating] = group.Count;
                }
            }

            return distribution;
        }
    }
}