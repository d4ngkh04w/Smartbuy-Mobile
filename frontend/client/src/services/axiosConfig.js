import axios from "axios";

const apiUrl = import.meta.env.VITE_API_URL || "/api";
const axiosInstance = axios.create({
    baseURL: apiUrl,
    timeout: 10000,
    withCredentials: true,
    headers: {
        "Content-Type": "application/json",
    }
})

// Request Interceptor
axiosInstance.interceptors.request.use(
    (config) => {
    const token = "";
    if(token){
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
    },
    (error) => {
        console.error("Request error:", error);
        return Promise.reject(error);
    }
)
//Response Interceptor

axiosInstance.interceptors.response.use(
    (response) => {
        return response;
    },
    (error) => {
        if (error.response) {
            const { status } = error.response
            if (status === 401) {
                console.error("Unauthorized! Redirecting to login...");
                window.location.href = "/login";
            } else if (status === 500) {
                console.error("Server error! Please try again later.");
            }
            return Promise.reject(error);
        }
    }
)

export default axiosInstance;