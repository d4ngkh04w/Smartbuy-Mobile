import instance from "./axiosConfig";
export const getMe = async () => {
	return (await instance.get("/user/me")).data.data; // Trả về thông tin người dùng
};
export const changePassword = async (passwordData) => {
	return await instance.post("/auth/change-password", passwordData);
};

// Cập nhật thông tin profile admin
export const updateUserProfile = async (userInfo) => {
	return (
		await instance.put("/user/me", userInfo, {
			headers: {
				"Content-Type": "multipart/form-data",
			},
		})
	).data.user; // Trả về thông tin người dùng đã cập nhật
};

// Auth services
export const meService = {
	getMe,
	updateUserProfile,
	changePassword,
};

export default meService;
