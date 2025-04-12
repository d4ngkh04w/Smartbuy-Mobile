import axios from "axios";

// Use environment variable or default to current setup
const apiUrl = import.meta.env.VITE_API_URL || "/api";

const instance = axios.create({
    baseURL: apiUrl, // URL của backend
    timeout: 10000,
    headers: {
        "Content-Type": "application/json",
    },
});

// Trước mỗi request, kiểm tra xem token có trong localStorage không
instance.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem("jwt");
        if (token) {
            config.headers["Authorization"] = `Bearer ${token}`; // Gửi JWT trong header
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

export default instance;
