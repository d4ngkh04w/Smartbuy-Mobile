import instance from "./axiosConfig";

// Đăng nhập cho admin
export const login = async (phoneNumber, password) => {
    return await instance.post(
        "/admin/auth/login",
        { phoneNumber, password },
        {
            isLoginRequest: true,
        }
    );
};

// Đăng xuất
export const logout = async () => {
    return await instance.post("/auth/logout");
};

// Refresh token
export const refreshToken = async () => {
    return await instance.post(
        "/auth/refresh-token",
        {},
        {
            isRefreshRequest: true,
        }
    );
};

// Quên mật khẩu
export const forgotPassword = async (data) => {
    return await instance.post("/auth/forgot-password", data);
};

export const verifyAdmin = async () => {
    return await instance.get("/admin/auth/verify");
};

export const resetPassword = async (resetData) => {
    return await instance.post("/auth/reset-password", resetData);
};
// Auth services
export const authService = {
    login,
    logout,
    refreshToken,
    forgotPassword,  
    verifyAdmin,  
};

export default authService;
