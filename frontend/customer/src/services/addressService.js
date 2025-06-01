// Dịch vụ lấy dữ liệu địa chỉ hành chính Việt Nam
import axios from "axios";

const API_BASE_URL = "https://provinces.open-api.vn/api";

const addressService = {
	/**
	 * Lấy danh sách tất cả các tỉnh thành phố
	 * @returns {Promise<Array>} Danh sách tỉnh/thành phố
	 */
	getProvinces: async () => {
		try {
			const response = await axios.get(`${API_BASE_URL}/p/`);
			const provinces = response.data;
			return provinces;
		} catch (error) {
			console.error("Error fetching provinces:", error);
			throw error;
		}
	},

	/**
	 * Lấy danh sách quận/huyện theo mã tỉnh/thành phố
	 * @param {number} provinceCode - Mã tỉnh/thành phố
	 * @returns {Promise<Array>} Danh sách quận/huyện
	 */
	getDistrictsByProvince: async (provinceCode) => {
		try {
			const response = await axios.get(
				`${API_BASE_URL}/p/${provinceCode}?depth=2`
			);
			return response.data.districts || [];
		} catch (error) {
			console.error(
				`Error fetching districts for province ${provinceCode}:`,
				error
			);
			throw error;
		}
	},

	/**
	 * Lấy danh sách phường/xã theo mã quận/huyện
	 * @param {number} districtCode - Mã quận/huyện
	 * @returns {Promise<Array>} Danh sách phường/xã
	 */
	getWardsByDistrict: async (districtCode) => {
		try {
			const response = await axios.get(
				`${API_BASE_URL}/d/${districtCode}?depth=2`
			);
			return response.data.wards || [];
		} catch (error) {
			console.error(
				`Error fetching wards for district ${districtCode}:`,
				error
			);
			throw error;
		}
	},

	/**
	 * Tạo chuỗi địa chỉ đầy đủ từ các thành phần
	 * @param {Object} addressData - Dữ liệu địa chỉ gồm các thành phần
	 * @returns {string} Địa chỉ đầy đủ dạng chuỗi
	 */
	formatFullAddress: (addressData) => {
		const { detail, ward, district, province } = addressData;
		const components = [];

		if (detail) components.push(detail);
		if (ward?.name) components.push(ward.name);
		if (district?.name) components.push(district.name);
		if (province?.name) components.push(province.name);

		return components.join(", ");
	},
};

export default addressService;
