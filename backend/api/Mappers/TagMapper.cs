// using api.DTOs.Tag;
// using api.Models;

// namespace api.Mappers
// {
//     public static class TagMapper
//     {
//         public static Tag ToModel(this CreateTagDTO createTagDTO)
//         {
//             return new Tag
//             {
//                 Name = createTagDTO.Name,
//                 ProductTags = new HashSet<ProductTag>()
//             };
//         }

//         public static TagDTO ToDTO(this Tag tag)
//         {
//             return new TagDTO
//             {
//                 Id = tag.Id,
//                 Name = tag.Name,
//             };
//         }
//     }
// }