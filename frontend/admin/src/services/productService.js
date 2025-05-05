import instance from "./axiosConfig";

// Get all products
export const getProducts = async () => {
    return await instance.get("/product");
};

// Get a product by ID
export const getProductById = async (id) => {
    return await instance.get(`/product/${id}`);
};

// Get products with pagination
export const getPagedProducts = async (page = 1, pageSize = 10) => {
    return await instance.get(
        `/product/page?page=${page}&pageSize=${pageSize}`
    );
};

// Create a new product (accepts FormData for image upload)
export const createProduct = async (productData) => {
    const isFormData = productData instanceof FormData;
    return await instance.post("/product", productData, {
        headers: isFormData
            ? {
                  "Content-Type": "multipart/form-data",
              }
            : {},
    });
};

// Update a product (accepts FormData for image upload)
export const updateProduct = async (id, productData) => {
    const isFormData = productData instanceof FormData;
    return await instance.put(`/product/${id}`, productData, {
        headers: isFormData
            ? {
                  "Content-Type": "multipart/form-data",
              }
            : {},
    });
};

// Activate a product
export const activateProduct = async (id) => {
    return await instance.put(`/product/${id}/activate`);
};

// Deactivate a product
export const deactivateProduct = async (id) => {
    return await instance.put(`/product/${id}/deactivate`);
};

// Create a product color with images
export const createProductColor = async (productId, colorData) => {
    const isFormData = colorData instanceof FormData;
    return await instance.post(`/product/${productId}/color`, colorData, {
        headers: isFormData
            ? {
                  "Content-Type": "multipart/form-data",
              }
            : {},
    });
};

const productService = {
    getProducts,
    getPagedProducts,
    getProductById,
    createProduct,
    updateProduct,
    activateProduct,
    deactivateProduct,
    createProductColor,
};

export default productService;
