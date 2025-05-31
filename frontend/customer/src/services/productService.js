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
			const response = await axiosInstance.get(
				`/product/page?${params.toString()}`
			);
			if (response) {
				return response.data;
			}
		} catch (error) {
			console.error("Error fetching products:", error);
			throw error;
		}
	}
	async getProductById(id) {
		return await axiosInstance
			.get(`/product/${id}`)
			.then((response) => {
				if (response) {
					return response.data.data;
				}
			})
			.catch((error) => {
				console.error("Error fetching product by ID:", error);
				throw error;
			});
	}
	async getQuantityProduct(productId, colorId) {
		const product = await this.getProductById(productId);
		if (product) {
			const productColorData = product.colors.find(
				(color) => color.id === colorId
			);
			if (productColorData) {
				return productColorData.quantity;
			}
		}
	}
	async createOrder(orderData) {
		return await axiosInstance
			.post("/order", orderData)
			.then((response) => {
				return response.data.data;
			})
			.catch((error) => {
				console.error(
					"Error creating order:",
					error.response?.data || error
				);
				throw error;
			});
	}

    async getBrands() {
        return await axiosInstance.get("/brand")
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
		const baseUrl =	import.meta.env.VITE_API_URL.replace("/api/v1", "") || "";
		return url?.startsWith("http") ? url : `${baseUrl}${url}`;
	} 
    async getCarts() {    
        return await axiosInstance.get("/cart")
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
    async removeCartItem(cartId) {
        return await axiosInstance.delete(`/cart/items/${cartId}`)
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
    async updateCartItem(cartId, quantity) {
        return await axiosInstance.put(`/cart/items/${cartId}`, { quantity })
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
    async addToCart(productId, quantity, colorId) {
        return await axiosInstance.post("/cart/add", {
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
    async checkQuantityToToggleStatus(productId) {
        const product = await this.getProductById(productId);
        if (product) {
            if(product.stock === 0) {
                await this.deactivateProduct(productId)
            }
            else {
                await this.activateProduct(productId)
            }
        }
        return;
    }
}
export default new ProductService();
