import instance from "./axiosConfig";

// Get all brands with optional query parameters
export const getBrands = async (params = {}) => {
    // Bảo đảm luôn bao gồm productLines trong response mặc định
    const queryParams = {
        ...params,
        includeProductLines:
            params.includeProductLines !== undefined
                ? params.includeProductLines
                : true,
    };
    return await instance.get("/brand", { params: queryParams });
};

// Get a brand by ID with optional query parameters
export const getBrandById = async (id, params) => {
    return await instance.get(`/brand/${id}`, { params });
};

// Create a new brand (accepts FormData for image upload)
export const createBrand = async (brandData) => {
    // Nếu brandData là FormData, đảm bảo header được đặt đúng
    const isFormData = brandData instanceof FormData;
    return await instance.post("/brand", brandData, {
        headers: isFormData
            ? {
                  "Content-Type": "multipart/form-data",
              }
            : {},
    });
};

// Update a brand (accepts FormData for image upload)
export const updateBrand = async (id, brandData) => {
    // Nếu brandData là FormData, đảm bảo header được đặt đúng
    const isFormData = brandData instanceof FormData;
    return await instance.put(`/brand/${id}`, brandData, {
        headers: isFormData
            ? {
                  "Content-Type": "multipart/form-data",
              }
            : {},
    });
};

// Delete a brand
export const deleteBrand = async (id) => {
    return await instance.delete(`/brand/${id}`);
};

// All brand services
export const brandService = {
    getBrands,
    getBrandById,
    createBrand,
    updateBrand,
    deleteBrand,
};

export default brandService;
