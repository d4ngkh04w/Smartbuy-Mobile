// using api.DTOs.Tag;
// using api.Exceptions;
// using api.Helpers;
// using api.Interfaces.Repositories;
// using api.Interfaces.Services;
// using api.Mappers;
// using api.Models;

// namespace api.Services
// {
//     public class TagService : ITagService
//     {
//         private readonly ITagRepository _tagRepository;
//         private readonly ICacheService _cacheService;
//         private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

//         public TagService(ITagRepository tagRepository, ICacheService cacheService)
//         {
//             _tagRepository = tagRepository;
//             _cacheService = cacheService;
//         }

//         public async Task<TagDTO> CreateTagAsync(CreateTagDTO tagDTO)
//         {
//             bool exists = await _tagRepository.TagExistsAsync(tagDTO.Name);
//             if (exists)
//             {
//                 throw new AlreadyExistsException("Tag already exists");
//             }

//             Tag tag = tagDTO.ToModel();
//             var createdTag = await _tagRepository.CreateTagAsync(tag);
//             _cacheService.RemoveAllTagsCache();

//             return createdTag.ToDTO();
//         }

//         public async Task DeleteTagAsync(int id)
//         {
//             var tag = await _tagRepository.GetTagByIdAsync(id) ?? throw new NotFoundException("Tag not found");
//             await _tagRepository.DeleteTagAsync(tag);

//             _cacheService.RemoveTagCache(id);
//             _cacheService.RemoveAllTagsCache();
//         }

//         public async Task<IEnumerable<TagDTO>> GetAllTagsAsync()
//         {
//             string cacheKey = CacheKeyManager.GetAllTagsKey();

//             if (_cacheService.TryGetValue(cacheKey, out IEnumerable<TagDTO>? cachedTags) && cachedTags != null)
//             {
//                 return cachedTags;
//             }

//             var tags = await _tagRepository.GetAllTagsAsync();
//             if (tags == null || !tags.Any())
//             {
//                 throw new NotFoundException("Not found any tags");
//             }

//             var tagsDto = tags.Select(t => t.ToDTO()).ToList();

//             _cacheService.Set(cacheKey, tagsDto, _cacheDuration);

//             return tagsDto;
//         }

//         public async Task<TagDTO> GetTagByIdAsync(int id)
//         {
//             string cacheKey = CacheKeyManager.GetTagKey(id);

//             if (_cacheService.TryGetValue(cacheKey, out TagDTO? cachedTag) && cachedTag != null)
//             {
//                 return cachedTag;
//             }

//             var tag = await _tagRepository.GetTagByIdAsync(id) ?? throw new NotFoundException("Tag not found");
//             var tagDto = tag.ToDTO();

//             // Store in cache
//             _cacheService.Set(cacheKey, tagDto, _cacheDuration);

//             return tagDto;
//         }

//         public async Task<TagDTO> UpdateTagAsync(int id, UpdateTagDTO tagDTO)
//         {
//             var tag = await _tagRepository.GetTagByIdAsync(id) ?? throw new NotFoundException("Tag not found");

//             if (!string.IsNullOrEmpty(tagDTO.Name))
//             {
//                 tag.Name = tagDTO.Name;
//             }

//             var result = await _tagRepository.UpdateTagAsync(tag);

//             _cacheService.RemoveTagCache(id);
//             _cacheService.RemoveAllTagsCache();

//             return result.ToDTO();
//         }
//     }
// }