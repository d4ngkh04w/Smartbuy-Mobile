import axios from "./axiosService";

// Gửi username + password để đăng nhập
export const login = (credentials) => {
    return axios.post("/user/auth/login", credentials);
};

// Gửi thông tin người dùng để đăng ký
export const register = (userInfo) => {
    return axios.post("/user/auth/register", userInfo);
};
