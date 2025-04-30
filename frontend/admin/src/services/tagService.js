import instance from "./axiosConfig";

// Get all tags
export const getAllTags = async () => {
    return await instance.get("/tags");
};

// Get a tag by ID
export const getTagById = async (id) => {
    return await instance.get(`/tags/${id}`);
};

// Create a new tag
export const createTag = async (tagData) => {
    return await instance.post("/tags", tagData);
};

// Update a tag
export const updateTag = async (id, tagData) => {
    return await instance.put(`/tags/${id}`, tagData);
};

// Delete a tag
export const deleteTag = async (id) => {
    return await instance.delete(`/tags/${id}`);
};

// All tag services
export const tagService = {
    getAllTags,
    getTagById,
    createTag,
    updateTag,
    deleteTag,
};

export default tagService;
