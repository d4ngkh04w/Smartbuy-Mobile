import instance from "./axiosConfig";

class ProductLineService {
	getHeaders = (data) =>
		data instanceof FormData
			? { headers: { "Content-Type": "multipart/form-data" } }
			: {};

	// ===== Image Utilities =====
	getUrlImage(url) {
		const baseUrl =
			import.meta.env.VITE_API_URL.replace("/api/v1", "") || "";
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

	// ===== Form Utilities =====
	prepareProductLineFormData(formData) {
		const data = new FormData();
		data.append("Name", formData.name);
		data.append("BrandId", formData.brandId);
		data.append("Image", formData.imageFile || "");
		data.append("Description", formData.description || "");
		return data;
	}

	getEmptyProductLineForm() {
		return {
			name: "",
			brandId: null,
			imageFile: null,
			description: "",
		};
	}

	getEditProductLineForm(productLine) {
		return {
			id: productLine.id,
			name: productLine.name,
			brandId: null,
			imageFile: null,
			description: productLine.description || "",
			existingImage: productLine.image || "",
		};
	}

	// ===== ProductLine API =====
	async getProductLines(filters = {}) {
		try {
			const { includeProducts = true, isActive } = filters;

			const params = new URLSearchParams();
			params.append("includeProducts", includeProducts);
			if (isActive !== undefined) {
				params.append("isActive", isActive);
			}

			const response = await instance.get(
				`/product-line?${params.toString()}`
			);
			return response.data;
		} catch (error) {
			console.error("Error fetching product lines:", error);
			throw error;
		}
	}

	async getProductLinesByBrand(brandId, params = {}) {
		const response = await instance.get(
			`/product-line/by-brand/${brandId}`,
			{ params }
		);
		return response.data;
	}

	async getProductLineById(id, params) {
		const response = await instance.get(`/product-line/${id}`, { params });
		return response.data;
	}

	async createProductLine(productLineData) {
		return await instance.post(
			"/product-line",
			productLineData,
			this.getHeaders(productLineData)
		);
	}

	async updateProductLine(id, productLineData) {
		return await instance.put(
			`/product-line/${id}`,
			productLineData,
			this.getHeaders(productLineData)
		);
	}

	async activateProductLine(id) {
		return await instance.put(`/product-line/${id}/activate`);
	}

	async deactivateProductLine(id) {
		return await instance.put(`/product-line/${id}/deactivate`);
	}

	async toggleProductLineStatus(productLineId, currentStatus) {
		return currentStatus
			? await this.deactivateProductLine(productLineId)
			: await this.activateProductLine(productLineId);
	}

	// ===== Misc =====
	getProductLineProductsCount(productLines, productLineId) {
		const productLine = productLines?.find((pl) => pl.id === productLineId);
		return productLine?.products?.length || 0;
	}
}

export default new ProductLineService();
