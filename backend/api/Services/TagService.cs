using api.DTOs.Tag;
using api.Exceptions;
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

        public async Task<TagDTO> CreateTagAsync(CreateTagDTO tagDTO)
        {
            bool exists = await _tagRepository.TagExistsAsync(tagDTO.Name);
            if (exists)
            {
                throw new AlreadyExistsException("Tag already exists");
            }

            Tag tag = tagDTO.ToModel();
            var createdTag = await _tagRepository.CreateTagAsync(tag);

            return createdTag.ToDTO();
        }

        public async Task DeleteTagAsync(int id)
        {
            var tag = await _tagRepository.GetTagByIdAsync(id) ?? throw new NotFoundException("Tag not found");
            await _tagRepository.DeleteTagAsync(tag);
        }

        public async Task<IEnumerable<TagDTO>> GetAllTagsAsync()
        {
            var tags = await _tagRepository.GetAllTagsAsync();
            if (tags == null || !tags.Any())
            {
                throw new NotFoundException("Not found any tags");
            }

            return tags.Select(t => t.ToDTO());
        }

        public async Task<TagDTO> GetTagByIdAsync(int id)
        {
            var tag = await _tagRepository.GetTagByIdAsync(id) ?? throw new NotFoundException("Tag not found");
            return tag.ToDTO();
        }

        public async Task<TagDTO> UpdateTagAsync(int id, UpdateTagDTO tagDTO)
        {
            var tag = await _tagRepository.GetTagByIdAsync(id) ?? throw new NotFoundException("Tag not found");

            // Only update fields that are provided in the DTO
            if (!string.IsNullOrEmpty(tagDTO.Name))
            {
                tag.Name = tagDTO.Name;
            }

            var result = await _tagRepository.UpdateTagAsync(tag);

            return result.ToDTO();
        }
    }
}