import axios from "./axiosConfig";

// Đăng nhập cho admin
export const login = async (phoneNumber, password) => {
    return await axios.post("/admin/auth/login", { phoneNumber, password });
};

// Đăng xuất
export const logout = async () => {
    return await axios.post("/auth/logout");
};

// Refresh token
export const refreshToken = async (refreshToken) => {
    return await axios.post("admin/auth/refresh-token", { refreshToken });
};

// Quên mật khẩu
export const forgotPassword = async (data) => {
    return await axios.post("/auth/forgot-password", data);
};

// Đặt lại mật khẩu
export const resetPassword = async (data) => {
    return await axios.post("/auth/reset-password", data);
};

// Auth services
export const authService = {
    login,
    logout,
    refreshToken,
    forgotPassword,
    resetPassword,
};

export default authService;
