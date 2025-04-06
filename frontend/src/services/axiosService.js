import axios from "axios";

const instance = axios.create({
    baseURL: "http://localhost:3000/api", // URL của backend
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
