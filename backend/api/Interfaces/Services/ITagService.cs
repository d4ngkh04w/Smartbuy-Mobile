using api.DTOs.Tag;

namespace api.Interfaces.Services
{
    public interface ITagService
    {
        Task<(bool Success, string? ErrorMessage, TagDTO? Tag)> GetTagByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage, IEnumerable<TagDTO>? Tags)> GetAllTagsAsync();
        Task<(bool Success, string? ErrorMessage, TagDTO? Tag)> CreateTagAsync(CreateTagDTO tagDTO);
        Task<(bool Success, string? ErrorMessage, TagDTO? Tag)> UpdateTagAsync(int id, UpdateTagDTO tagDTO);
        Task<(bool Success, string? ErrorMessage)> DeleteTagAsync(int id);
    }
}