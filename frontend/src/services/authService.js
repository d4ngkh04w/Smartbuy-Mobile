import axios from "./axiosService";

// Gửi username + password để đăng nhập
export const login = (credentials) => {
    return axios.post("/user/auth/login", credentials);
};

// Gửi thông tin người dùng để đăng ký
export const register = (userInfo) => {
    return axios.post("/user/auth/register", userInfo);
};

// Gửi token Google để xác minh, backend trả về JWT
export const loginWithGoogle = (token) => {
    return axios.post("/user/auth/google-login", {
        token: token,
    });
};

// export const loginWithFacebook = (token) => {
//     return axios.post("/api/auth/facebook-login", { token: token });
// };
