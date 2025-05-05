import instance from "./axiosConfig";

export const verifyUser = async () => {
    return await instance.get("/user/auth/verify");
};

export const login = async (phoneNumber, password) => {
    return await instance.post(
        "/user/auth/login",
        { phoneNumber, password },
        {
            isLoginRequest: true, // Flag to handle login-specific errors
        }
    );
};

export const refreshToken = async () => {
    return await instance.post(
        "/auth/refresh-token",
        {},
        {
            isRefreshRequest: true,
        }
    );
};

export const logout = async () => {
    return await instance.post("/auth/logout");
};

export const authService = {
    verifyUser,
    login,
    refreshToken,
    logout,
};

export default authService;
