import instance from "./axiosConfig";
export const getMe = async (options = {}) => {
    const config = { ...options };
    return (await instance.get("/user/me", config)).data.data; // Trả về thông tin người dùng
};
export const changePassword = async (passwordData) => {
    return await instance.post("/auth/change-password", passwordData);
};

// Cập nhật thông tin profile admin
export const updateUserProfile = async (userInfo) => {
    return (
        await instance.put("/user/me", userInfo, {
            headers: {
                "Content-Type": "multipart/form-data",
            },
        })
    ).data.data; // Trả về thông tin người dùng đã cập nhật
};

// Gửi email xác thực
export const sendVerificationEmail = async () => {
    return await instance.post("/user/auth/send-verification-email");
};

export const deleteMyAccount = async () => {
    return await instance.delete("/user/me");
};

export const meService = {
    getMe,
    updateUserProfile,
    changePassword,
    sendVerificationEmail,
    deleteMyAccount,
};

export default meService;
