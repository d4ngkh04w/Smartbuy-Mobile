import axios from "axios";
import authService from "./authService";

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
	(response) => response,
	async (error) => {
		const originalRequest = error.config;

		if (error.response?.status === 401 && !originalRequest._retry) {
			originalRequest._retry = true;

			try {
				await authService.refreshToken();

				return instance(originalRequest);
			} catch (refreshError) {
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
