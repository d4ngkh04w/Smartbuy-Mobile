import axios from "./axiosConfig";

// Auth services
export const authService = {
    // Đăng nhập cho admin
    login: async (email, password) => {
        return await axios.post("/auth/admin/login", { email, password });
    },

    // Đăng xuất
    logout: async () => {
        return await axios.post("/auth/admin/logout");
    },

    // Refresh token
    refreshToken: async (refreshToken) => {
        return await axios.post("/auth/admin/refresh-token", { refreshToken });
    },
};

export default authService;
