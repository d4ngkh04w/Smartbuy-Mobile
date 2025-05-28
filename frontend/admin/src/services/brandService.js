import instance from "./axiosConfig";

class BrandService {
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

	validateLogoFile(file) {
		if (!file) return { valid: true };

		if (!this.validateFileType(file)) {
			return {
				valid: false,
				message: "Định dạng tệp không hợp lệ. Chỉ chấp nhận PNG, JPG.",
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
	prepareBrandFormData(formData) {
		const data = new FormData();
		data.append("Name", formData.name);
		data.append("Logo", formData.logoFile || "");
		data.append("Description", formData.description || "");
		return data;
	}

	getEmptyBrandForm() {
		return {
			name: "",
			logoFile: null,
			description: "",
		};
	}

	getEditBrandForm(brand) {
		return {
			id: brand.id,
			name: brand.name,
			logoFile: null,
			description: brand.description || "",
			existingLogo: brand.logo || "",
		};
	}

	// ===== Brand API =====
	async getBrands(filters = {}) {
		try {
			const { includeProductLines = true, isActive } = filters;

			const params = new URLSearchParams();
			params.append("includeProductLines", includeProductLines);
			if (isActive !== undefined) {
				params.append("isActive", isActive);
			}

			const response = await instance.get(`/brand?${params.toString()}`);
			return response.data;
		} catch (error) {
			console.error("Error fetching brands:", error);
			throw error;
		}
	}

	async getBrandById(id, params) {
		return await instance.get(`/brand/${id}`, { params });
	}

	async createBrand(brandData) {
		return await instance.post(
			"/brand",
			brandData,
			this.getHeaders(brandData)
		);
	}

	async updateBrand(id, brandData) {
		return await instance.put(
			`/brand/${id}`,
			brandData,
			this.getHeaders(brandData)
		);
	}

	async activateBrand(id) {
		return await instance.put(`/brand/${id}/activate`);
	}

	async deactivateBrand(id) {
		return await instance.put(`/brand/${id}/deactivate`);
	}

	async toggleBrandStatus(brandId, currentStatus) {
		return currentStatus
			? await this.deactivateBrand(brandId)
			: await this.activateBrand(brandId);
	}

	// ===== Misc =====
	getBrandProductLinesCount(brands, brandId) {
		const brand = brands?.find((b) => b.id === brandId);
		return brand?.productLines?.length || 0;
	}
}

export default new BrandService();
