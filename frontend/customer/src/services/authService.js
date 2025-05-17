import instance from "./axiosConfig";
import emitter from "../utils/evenBus.js";
class AuthService {
    async verifyUser() {
        return await instance.get("/user/auth/verify");
    }

    async login(phoneNumber, password) {
        return await instance.post(
            "/user/auth/login",
            { phoneNumber, password },
            {
                isLoginRequest: true, // Flag to handle login-specific errors
            }
        );
    }

    async googleLogin(tokenData) {      
        return await instance.post(
            "/user/auth/google-login",
            {
                Token: tokenData.Token,
            },
            {
                isLoginRequest: true, // Flag to handle login-specific errors
            }
        );
    }

    async register(userInfo) {
        return await instance.post("/user/auth/register", userInfo, {
            isLoginRequest: true, // Handle registration like login for error handling
        });
    }

    async refreshToken() {
        return await instance.post(
            "/auth/refresh-token",
            {},
            {
                isRefreshRequest: true,
            }
        );
    }

    async forgotPassword(data) {
        return await instance.post("/auth/forgot-password", data);
    }

    async resetPassword(resetData) {
        return await instance.post("/auth/reset-password", resetData);
    }

    async logout() {
        emitter.emit("logout");
        return await instance.post("/auth/logout");
    }

    async sendVerificationEmail() {
        return await instance.post("/user/auth/send-verification-email");
    }

    async verifyEmail(verificationData) {
        return await instance.post("/user/auth/verify-email", verificationData);
    }
}
export default new AuthService();
