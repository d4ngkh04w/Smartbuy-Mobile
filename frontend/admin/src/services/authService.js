import instance from "./axiosConfig";

// Đăng nhập cho admin
export const login = async (phoneNumber, password) => {
    return await instance.post("/admin/auth/login", { phoneNumber, password });
};

// Đăng xuất
export const logout = async () => {
    return await instance.post("/auth/logout");
};

// Refresh token
export const refreshToken = async () => {
    return await instance.post("/auth/refresh-token");
};

// Quên mật khẩu
export const forgotPassword = async (data) => {
    return await instance.post("/auth/forgot-password", data);
};

// Đặt lại mật khẩu
export const resetPassword = async (data) => {
    return await instance.post("/auth/reset-password", data);
};

// Lấy thông tin người dùng hiện tại
export const getMe = async () => {
    try {
        const response = await instance.get("/user/me");
        console.log("getMe response:", response.data);
        return response;
    } catch (error) {
        // Improve error logging with more details
        console.error("getMe error:", {
            status: error.response?.status,
            message: error.message,
            code: error.code,
        });
        throw error;
    }
};

// Auth services
export const authService = {
    login,
    logout,
    refreshToken,
    forgotPassword,
    resetPassword,
    getMe,
};

export default authService;
