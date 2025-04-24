using api.DTOs.Tag;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;

namespace api.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<(bool Success, string? ErrorMessage, TagDTO? Tag)> CreateTagAsync(CreateTagDTO tagDTO)
        {
            try
            {
                bool exists = await _tagRepository.TagExistsAsync(tagDTO.Name);
                if (exists)
                {
                    return (false, "Tag already exists", null);
                }

                Tag tag = tagDTO.ToModel();
                var createdTag = await _tagRepository.CreateTagAsync(tag);

                return (true, null, createdTag.ToDTO());
            }
            catch (Exception)
            {
                return (false, $"Error creating tag", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteTagAsync(int id)
        {
            try
            {
                var tag = await _tagRepository.GetTagByIdAsync(id);
                if (tag == null)
                {
                    return (false, "Tag not found");
                }

                await _tagRepository.DeleteTagAsync(tag);
                return (true, null);
            }
            catch (Exception)
            {
                return (false, $"Error deleting tag");
            }
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<TagDTO>? Tags)> GetAllTagsAsync()
        {
            try
            {
                var tags = await _tagRepository.GetAllTagsAsync();
                if (tags == null || !tags.Any())
                {
                    return (false, "No tags found", null);
                }

                return (true, null, tags.Select(t => t.ToDTO()));
            }
            catch (Exception)
            {
                return (false, $"Error retrieving tags", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, TagDTO? Tag)> GetTagByIdAsync(int id)
        {
            try
            {
                var tag = await _tagRepository.GetTagByIdAsync(id);
                if (tag == null)
                {
                    return (false, "Tag not found", null);
                }

                return (true, null, tag.ToDTO());
            }
            catch (Exception)
            {
                return (false, $"Error retrieving tag", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, TagDTO? Tag)> UpdateTagAsync(int id, UpdateTagDTO tagDTO)
        {
            try
            {
                var tag = await _tagRepository.GetTagByIdAsync(id);
                if (tag == null)
                {
                    return (false, "Tag not found", null);
                }

                // Only update fields that are provided in the DTO
                if (!string.IsNullOrEmpty(tagDTO.Name))
                {
                    tag.Name = tagDTO.Name;
                }

                bool result = await _tagRepository.UpdateTagAsync(tag);
                if (!result)
                {
                    return (false, "Failed to update tag", null);
                }

                return (true, null, tag.ToDTO());
            }
            catch (Exception)
            {
                return (false, $"Error updating tag", null);
            }
        }
    }
}