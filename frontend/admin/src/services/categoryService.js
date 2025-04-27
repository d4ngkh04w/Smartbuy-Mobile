import instance from "./axiosConfig";

export const getBrands = async (params) => {
    return await instance.get("/brand", { params });
};

export const getBrandById = async (id, params) => {
    return await instance.get(`/brand/${id}`, { params });
};

export const createBrand = async (brandData) => {
    return await instance.post("/brand", brandData);
};

export const updateBrand = async (id, brandData) => {
    return await instance.put(`/brand/${id}`, brandData);
};

export const deleteBrand = async (id) => {
    return await instance.delete(`/brand/${id}`);
};

const categoryService = {
    getBrands,
    getBrandById,
    createBrand,
    updateBrand,
    deleteBrand,
};

export default categoryService;
