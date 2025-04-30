import instance from "./axiosConfig";

// Get all products
export const getProducts = async () => {
    return await instance.get("/product");
};

// Get a product by ID
export const getProductById = async (id) => {
    return await instance.get(`/product/${id}`);
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

// Delete a product
export const deleteProduct = async (id) => {
    return await instance.delete(`/product/${id}`);
};

const productService = {
    getProducts,
    getProductById,
    createProduct,
    updateProduct,
    deleteProduct,
};

export default productService;
