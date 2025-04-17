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

// No need for Authorization header when using HTTP-only cookies
// The cookies will be automatically sent with each request

export default instance;
