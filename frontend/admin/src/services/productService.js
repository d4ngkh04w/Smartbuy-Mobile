import instance from "./axiosConfig";

class ProductService {
	// ===== Helper Methods =====
	getHeaders = (data) =>
		data instanceof FormData
			? { headers: { "Content-Type": "multipart/form-data" } }
			: {};
	// ===== Image Utilities =====
	getUrlImage(url) {
		const baseUrl = import.meta.env.VITE_API_URL.replace("/api/v1", "") || "";
		return url?.startsWith("http") ? url : `${baseUrl}${url}`;
	}

	// ===== File Validation =====
	validateFileType(file) {
		const allowedTypes = [
			"image/jpeg",
			"image/jpg",
			"image/pjpeg",
			"image/png",
			"image/svg+xml",
			"image/webp",
		];
		return allowedTypes.includes(file.type);
	}

	validateFileSize(file, maxSizeMB = 2) {
		return file.size <= maxSizeMB * 1024 * 1024;
	}

	validateImageFile(file) {
		if (!file) return { valid: true };

		if (!this.validateFileType(file)) {
			return {
				valid: false,
				message:
					"Định dạng tệp không hợp lệ. Chỉ chấp nhận PNG, JPG, SVG, WEBP.",
			};
		}

		if (!this.validateFileSize(file)) {
			return {
				valid: false,
				message: "Tệp quá lớn. Tối đa 2MB.",
			};
		}

		return { valid: true };
	}
	// ===== Product API =====
	async getProducts(filters = {}) {
		try {
			const { isActive } = filters;
			const params = new URLSearchParams();

			if (isActive !== undefined) {
				params.append("isActive", isActive);
			}

			const queryString = params.toString();
			const url = queryString ? `/product?${queryString}` : "/product";

			return await instance.get(url);
		} catch (error) {
			console.error("Error fetching products:", error);
			throw error;
		}
	}

	async getProductById(id) {
		try {
			return await instance.get(`/product/${id}`);
		} catch (error) {
			console.error(`Error fetching product with ID ${id}:`, error);
			throw error;
		}
	}

	async activateProduct(id) {
		try {
			return await instance.put(`/product/${id}/activate`);
		} catch (error) {
			console.error(`Error activating product with ID ${id}:`, error);
			throw error;
		}
	}

	async deactivateProduct(id) {
		try {
			return await instance.put(`/product/${id}/deactivate`);
		} catch (error) {
			console.error(`Error deactivating product with ID ${id}:`, error);
			throw error;
		}
	}
	async toggleProductStatus(productId, currentStatus) {
		return currentStatus
			? await this.deactivateProduct(productId)
			: await this.activateProduct(productId);
	}

	async updateProduct(id, productData) {
		try {
			return await instance.put(
				`/product/${id}`,
				productData,
				this.getHeaders(productData)
			);
		} catch (error) {
			console.error(`Error updating product with ID ${id}:`, error);
			throw error;
		}
	}

	async createProduct(productData) {
		try {
			return await instance.post(
				"/product",
				productData,
				this.getHeaders(productData)
			);
		} catch (error) {
			console.error("Error creating product:", error);
			throw error;
		}
	}

	// ===== Color API =====
	async createProductColor(productId, colorData) {
		try {
			const formData = new FormData();

			formData.append("Name", colorData.name.trim());
			formData.append("Quantity", colorData.quantity.toString());
			formData.append(
				"MainImageIndex",
				colorData.mainImageIndex.toString()
			);

			if (colorData.images && colorData.images.length > 0) {
				for (let i = 0; i < colorData.images.length; i++) {
					if (this.validateImageFile(colorData.images[i]).valid) {
						formData.append("Images", colorData.images[i]);
					}
				}
			}

			return await instance.post(
				`/product/${productId}/color`,
				formData,
				this.getHeaders(formData)
			);
		} catch (error) {
			console.error(
				`Error creating color for product ID ${productId}:`,
				error
			);
			throw error;
		}
	}

	async updateProductColor(productId, colorId, colorFormData) {
		try {
			return await instance.put(
				`/product/${productId}/color/${colorId}`,
				colorFormData,
				this.getHeaders(colorFormData)
			);
		} catch (error) {
			console.error(
				`Error updating color with ID ${colorId} for product ID ${productId}:`,
				error
			);
			throw error;
		}
	}

	async deleteProductColor(productId, colorId) {
		try {
			return await instance.delete(
				`/product/${productId}/color/${colorId}`
			);
		} catch (error) {
			console.error(
				`Error deleting color with ID ${colorId} for product ID ${productId}:`,
				error
			);
			throw error;
		}	}

	// ===== Misc =====
	getProductMainImage(product) {
		if (!product || !product.colors || !product.colors.length) {
			return null;
		}

		for (const color of product.colors) {
			if (color.images && color.images.length) {
				const mainImage = color.images.find((img) => img.isMain);
				if (mainImage) {
					return this.getUrlImage(mainImage.imagePath);
				}
			}
		}

		return null;
	}
}

export default new ProductService();
