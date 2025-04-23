import axios from "axios";


const apiUrl = import.meta.env.VITE_API_URL || "/api/v1";

const instance = axios.create({
    baseURL: apiUrl,
    timeout: 10000,
    withCredentials: true, // Enables sending cookies with cross-origin requests
    headers: {
        "Content-Type": "application/json",
    },
});

// Add a response interceptor to handle token refresh
instance.interceptors.response.use(
    (response) => {
        return response;
    },
    async (error) => {
        console.log("Interceptor bắt lỗi:", error);

        const originalRequest = error.config;
        console.log("Error response:", error.response);
        // If the error is 401 (Unauthorized) and we haven't retried the request yet
        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;

            try {
                console.log("Refreshing token...");
                // Call the refresh token endpoint directly without using authService
                await instance.post("/auth/refresh-token");

                // After refreshing token, retry the original request
                // The new token will be in the cookies automatically
                return instance(originalRequest);
            } catch (refreshError) {
                // Set flag to indicate token refresh has failed
                localStorage.setItem("token_refresh_failed", "true");

                // If refresh token fails, redirect to login
                try {
                    await instance.post("/auth/logout");
                } catch (logoutError) {
                    console.error("Không thể đăng xuất:", logoutError);
                }
                window.location.href = "/login";
                return Promise.reject(refreshError);
            }
        }

        return Promise.reject(error);
    }
);

export default instance;
