// using api.Database;
// using api.Interfaces.Repositories;
// using api.Models;
// using Microsoft.EntityFrameworkCore;

// namespace api.Repositories
// {
//     public class TagRepository : ITagRepository
//     {
//         private readonly AppDBContext _db;

//         public TagRepository(AppDBContext db)
//         {
//             _db = db;
//         }

//         public async Task<Tag> CreateTagAsync(Tag tag)
//         {
//             _db.Tags.Add(tag);
//             await _db.SaveChangesAsync();

//             return tag;
//         }

//         public async Task DeleteTagAsync(Tag tag)
//         {
//             _db.Tags.Remove(tag);
//             await _db.SaveChangesAsync();
//         }

//         public async Task<IEnumerable<Tag>> GetAllTagsAsync()
//         {
//             return await _db.Tags.ToListAsync();
//         }

//         public async Task<Tag?> GetTagByIdAsync(int id)
//         {
//             return await _db.Tags.FindAsync(id);
//         }

//         public async Task<bool> TagExistsAsync(string name)
//         {
//             return await _db.Tags.AnyAsync(t => t.Name.ToLower() == name.ToLower());
//         }

//         public async Task<Tag> UpdateTagAsync(Tag tag)
//         {
//             _db.Tags.Update(tag);
//             await _db.SaveChangesAsync();
//             return tag;
//         }
//     }
// }