import axios from "axios";
import authService from "./authService";

const apiUrl = import.meta.env.VITE_API_URL ;

const instance = axios.create({
	baseURL: apiUrl,
	timeout: 10000,
	withCredentials: true, // Enables sending cookies with cross-origin requests
	headers: {
		"Content-Type": "application/json",
	},
});

//Add a response interceptor to handle token refresh
instance.interceptors.response.use(
	(response) => response,
	async (error) => {
		const originalRequest = error.config;
		if (originalRequest?.isLoginRequest) {
			return Promise.reject(error); // để catch trong login() xử lý
		}
		if (originalRequest?.isRefreshRequest === true) {
			console.error("Lỗi xác thực:", error);
			window.location.href = "/login";
			return Promise.reject(error);
		}

		if (error.response?.status === 401 && !originalRequest._retry) {
			originalRequest._retry = true;

			try {
				await authService.refreshToken();
				console.log("refreshToken thành công");
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
