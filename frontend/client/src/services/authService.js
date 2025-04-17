import axios from "./axiosConfig";

// Gửi username + password để đăng nhập
export const login = async (credentials) => {
    const response = await axios.post("/user/auth/login", credentials);
    return response.data.message; // Success message, token is stored in HTTP-only cookie
};

// Đăng ký tài khoản
export const register = async (userInfo) => {
    const response = await axios.post("/user/auth/register", userInfo);
    return response.data.message; // Success message, token is stored in HTTP-only cookie
};

// Login bằng Google
export const loginWithGoogle = async (credential) => {
    return await axios.post("/user/auth/google-login", {
        token: credential,
        email: "", // This might need to be extracted from credential depending on your backend
    });
};

// Lấy info user
export const getUserInfo = async () => {
    return await axios.get("/user/me");
};

// Làm mới access token
export const refreshToken = async () => {
    const response = await axios.post("/auth/refresh-token");
    return response.data.message; // Success message, new token is stored in HTTP-only cookie
};

export const logout = async () => {
    return await axios.post("/auth/logout");
};

// Cập nhật thông tin profile người dùng
export const updateUserProfile = async (userInfo) => {
    // Sử dụng FormData để hỗ trợ upload file (avatar)
    const formData = new FormData();

    // Thêm các trường thông tin vào formData
    if (userInfo.name) formData.append("name", userInfo.name);
    if (userInfo.email) formData.append("email", userInfo.email);
    if (userInfo.phoneNumber)
        formData.append("phoneNumber", userInfo.phoneNumber);
    if (userInfo.address) formData.append("address", userInfo.address);
    if (userInfo.gender) formData.append("gender", userInfo.gender);
    if (userInfo.avatar && userInfo.avatar instanceof File) {
        formData.append("avatar", userInfo.avatar);
    }

    return await axios.put("/user/me", formData, {
        headers: {
            "Content-Type": "multipart/form-data",
        },
    });
};
