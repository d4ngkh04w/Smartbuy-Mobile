import instance from "./axiosConfig";

// Get all product lines with optional query parameters
export const getProductLines = async (params) => {
    return await instance.get("/product-line", { params });
};

// Get a product line by ID with optional query parameters
export const getProductLineById = async (id, params) => {
    return await instance.get(`/product-line/${id}`, { params });
};

// Create a new product line (accepts FormData for image upload)
export const createProductLine = async (productLineData) => {
    // Nếu productLineData là FormData, đảm bảo header được đặt đúng
    const isFormData = productLineData instanceof FormData;
    return await instance.post("/product-line", productLineData, {
        headers: isFormData
            ? {
                  "Content-Type": "multipart/form-data",
              }
            : {},
    });
};

// Update a product line (accepts FormData for image upload)
export const updateProductLine = async (id, productLineData) => {
    // Nếu productLineData là FormData, đảm bảo header được đặt đúng
    const isFormData = productLineData instanceof FormData;
    return await instance.put(`/product-line/${id}`, productLineData, {
        headers: isFormData
            ? {
                  "Content-Type": "multipart/form-data",
              }
            : {},
    });
};

// Delete a product line
export const deleteProductLine = async (id) => {
    return await instance.delete(`/product-line/${id}`);
};

// All product line services
export const productLineService = {
    getProductLines,
    getProductLineById,
    createProductLine,
    updateProductLine,
    deleteProductLine,
};

export default productLineService;
