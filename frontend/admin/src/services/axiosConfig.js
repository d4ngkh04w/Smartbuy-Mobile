import axios from "axios";

const apiUrl = import.meta.env.VITE_API_URL || "/api";

const instance = axios.create({
    baseURL: apiUrl,
    timeout: 10000,
    withCredentials: true, // Enables sending cookies with cross-origin requests
    headers: {
        "Content-Type": "application/json",
    },
});

// Add a request interceptor to add auth token to each request
instance.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem("auth_token");
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Add a response interceptor to handle token refresh
instance.interceptors.response.use(
    (response) => {
        return response;
    },
    async (error) => {
        const originalRequest = error.config;

        // If the error is 401 and we haven't retried the request yet
        if (error.response.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;

            try {
                const refreshToken = localStorage.getItem("refresh_token");
                if (!refreshToken) {
                    // Redirect to login if no refresh token
                    window.location.href = "/login";
                    return Promise.reject(error);
                }

                // Call the refresh token endpoint
                const response = await axios.post(
                    `${import.meta.env.VITE_API_URL}/auth/admin/refresh-token`,
                    { refreshToken }
                );

                const { token } = response.data;
                localStorage.setItem("auth_token", token);

                // Retry the original request with the new token
                originalRequest.headers.Authorization = `Bearer ${token}`;
                return instance(originalRequest);
            } catch (refreshError) {
                // If refresh token fails, logout and redirect to login
                localStorage.removeItem("auth_token");
                localStorage.removeItem("refresh_token");
                window.location.href = "/login";
                return Promise.reject(refreshError);
            }
        }

        return Promise.reject(error);
    }
);

export default instance;
