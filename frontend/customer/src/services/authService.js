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

export const googleLogin = async (tokenData) => {
    console.log("Sending token data to backend:", tokenData);
    // Make sure we're explicitly creating the object structure expected by the backend
    return await instance.post(
        "/user/auth/google-login",
        {
            Token: tokenData.Token,
        },
        {
            isLoginRequest: true, // Flag to handle login-specific errors
        }
    );
};

export const register = async (userInfo) => {
    return await instance.post("/user/auth/register", userInfo, {
        isLoginRequest: true, // Handle registration like login for error handling
    });
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

export const forgotPassword = async (data) => {
    return await instance.post("/auth/forgot-password", data);
};

export const resetPassword = async (resetData) => {
    return await instance.post("/auth/reset-password", resetData);
};

export const logout = async () => {
    return await instance.post("/auth/logout");
};

export const authService = {
    verifyUser,
    login,
    googleLogin,
    register,
    refreshToken,
    forgotPassword,
    resetPassword,
    logout,
};

export default authService;
