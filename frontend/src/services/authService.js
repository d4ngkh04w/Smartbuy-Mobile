import axios from "./axiosConfig";

// Gửi username + password để đăng nhập
export const login = (credentials) => {
    return axios
        .post("/user/auth/login", credentials)
        .then((response) => {
            const { token } = response.data;
            return { token };
        })
        .catch((error) => {
            throw error;
        });
};

// Đăng ký tài khoản
export const register = (userInfo) => {
    return axios.post("/user/auth/register", userInfo);
};

// Login bằng Google
export const loginWithGoogle = (token) => {
    return axios.post("/user/auth/google-login", { token });
};

// Lấy info user
export const getUserInfo = () => {
    return axios.get("/user/info");
};

// Làm mới access token
export const refreshToken = async () => {
    return await axios.post("/user/auth/refresh-token");
};

// export const loginWithFacebook = (token) => {
//     return axios.post("/api/auth/facebook-login", { token: token });
// };
