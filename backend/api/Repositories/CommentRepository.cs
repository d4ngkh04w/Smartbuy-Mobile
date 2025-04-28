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
            // Get top-level comments with pagination (comments that don't have a parent)
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

        public async Task<bool> UpdateCommentAsync(Comment comment)
        {
            _db.Entry(comment).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
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

        public async Task<double?> GetProductAverageRatingAsync(int productId)
        {
            var ratings = await _db.Comments
                .Where(c => c.ProductId == productId && c.Rating.HasValue)
                .Select(c => c.Rating!.Value)
                .ToListAsync();

            if (!ratings.Any())
                return null;

            return ratings.Average();
        }
    }
}