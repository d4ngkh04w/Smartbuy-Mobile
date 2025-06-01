import instance from "./axiosConfig";

class AdminAuthService {
    // Đăng nhập admin
    async login(phoneNumber, password) {
        return await instance.post(
            "/admin/auth/login",
            { phoneNumber, password },
            {
                isLoginRequest: true,
            }
        );
    }

    // Đăng xuất
    async logout() {
        return await instance.post("/auth/logout");
    }

    // Refresh token
    async refreshToken() {
        return await instance.post(
            "/auth/refresh-token",
            {},
            {
                isRefreshRequest: true,
            }
        );
    }

    // Quên mật khẩu
    async forgotPassword(data) {
        return await instance.post("/auth/forgot-password", data);
    }

    // Xác minh Admin
    async verifyAdmin() {
        return await instance.get("/admin/auth/verify");
    }

    // Reset mật khẩu
    async resetPassword(resetData) {
        return await instance.post("/auth/reset-password", resetData);
    }
}

export default new AdminAuthService();
