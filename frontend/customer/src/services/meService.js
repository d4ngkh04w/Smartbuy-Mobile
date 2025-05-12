import instance from "./axiosConfig";

class MeService {
    async getMe() {
        return (await instance.get("/user/me")).data.data; // Trả về thông tin người dùng
    }

    async changePassword(passwordData) {
        return await instance.post("/auth/change-password", passwordData);
    }

    // Cập nhật thông tin profile admin
    async updateUserProfile(userInfo) {
        return (
            await instance.put("/user/me", userInfo, {
                headers: {
                    "Content-Type": "multipart/form-data",
                },
            })
        ).data.user; // Trả về thông tin người dùng đã cập nhật
    }

    async sendVerificationEmail() {
        return await instance.post("/user/auth/send-verification-email");
    }

    async deleteMyAccount() {
        return await instance.delete("/user/me");
    }
}
export default new MeService();

// export const getMe = async () => {
//     return (await instance.get("/user/me")).data.data; // Trả về thông tin người dùng
// };
// export const changePassword = async (passwordData) => {
//     return await instance.post("/auth/change-password", passwordData);
// };

// // Cập nhật thông tin profile admin
// export const updateUserProfile = async (userInfo) => {
//     return (
//         await instance.put("/user/me", userInfo, {
//             headers: {
//                 "Content-Type": "multipart/form-data",
//             },
//         })
//     ).data.user; // Trả về thông tin người dùng đã cập nhật
// };

// // Gửi email xác thực
// export const sendVerificationEmail = async () => {
//     return await instance.post("/user/auth/send-verification-email");
// };

// export const deleteMyAccount = async () => {
//     return await instance.delete("/user/me");
// };

// export const meService = {
//     getMe,
//     updateUserProfile,
//     changePassword,
//     sendVerificationEmail,    
//     deleteMyAccount,
// };

// export default meService;
