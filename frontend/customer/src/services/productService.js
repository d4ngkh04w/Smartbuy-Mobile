import axiosInstance from "./axiosConfig";

class ProductService {
    async getProducts(page, limit, filters = {}) {
        try {
            const {
                search = "",
                sortBy = "newest",
                brandName = null,
                minPrice = null,
                maxPrice = null,
            } = filters;

            const params = new URLSearchParams({
                page,
                pageSize: limit,
            });

            if (search) params.append("search", search);
            if (sortBy) params.append("sortBy", sortBy);
            if (brandName) params.append("brandName", brandName);
            if (minPrice !== null) params.append("minPrice", minPrice);
            if (maxPrice !== null) params.append("maxPrice", maxPrice);
            const response = await axiosInstance.get(`/product/page?${params.toString()}`);
            if (response) {
                return response.data;
                
            }
        } catch (error) {
            console.error("Error fetching products:", error);
            throw error;
        }
    }
    getProductById(id) {
        return axiosInstance.get(`/product/${id}`)
            .then(response => {
                if (response) {
                    console.log("Product data:", response.data.data);
                    return response.data.data;
                }
            })
            .catch(error => {
                console.error("Error fetching product by ID:", error);
                throw error;
            });
    }
    async getQuantityProduct(productId, colorId) {
        const product = await this.getProductById(productId);
        if (product) {
            const productColorData = product.colors.find(color => color.id === colorId);
            if (productColorData) {
                return productColorData.quantity;
            }
        }
    }

    getBrands() {
        return axiosInstance.get("/brand")
            .then(response => {
                if (response) {
                    return response.data;
                }
            })
            .catch(error => {
                console.error("Error fetching brands:", error);
                throw error;
            });
    }   
    getUrlImage(url) {
        let newUrlImage = url.startsWith('http') ? url : "http://localhost:5000" + `/${url}`;
        return newUrlImage;
    }   
    getCarts() {    
        return axiosInstance.get("/cart")
            .then(response => {
                if (response) {
                    return response.data;
                }
            })
            .catch(error => {
                console.error("Error fetching carts:", error);
                throw error;
            });
    }
    removeCartItem(cartId) {
        return axiosInstance.delete(`/cart/items/${cartId}`)
            .then(response => {
                if (response) {
                    return response.data;
                }
            })
            .catch(error => {
                console.error("Error removing cart item:", error);
                throw error;
            });
    }
    updateCartItem(cartId, quantity) {
    return axiosInstance.put(`/cart/items/${cartId}`, { quantity })
        .then(response => {
            if (response) {
                return response.data;
            }
        })
        .catch(error => {
            console.error("Error updating cart item:", error);
            throw error;
        });
    }
    addToCart(productId, quantity, colorId) {
        return axiosInstance.post("/cart/add", {
            productId,
            quantity,
            colorId
        })
        .then(response => {
            if (response && response.data) {
                return response.data;
            }
        })
        .catch(error => {
            console.error("Error adding product to cart:", error);
            throw error;
        });
    }


}
export default new ProductService();
