import instance from "./axiosConfig";

class MeService {
    // ===== Headers Utility =====
    getHeaders = (data) =>
        data instanceof FormData
            ? { headers: { "Content-Type": "multipart/form-data" } }
            : {};

    // ===== Image Utilities =====
    getUrlImage(url) {
        const baseUrl =
            import.meta.env.VITE_API_URL.replace("/api/v1", "") || "";
        return url?.startsWith("http") ? url : `${baseUrl}${url}`;
    }

    // ===== File Validation =====
    validateFileType(file) {
        const allowedTypes = ["image/jpeg", "image/png", "image/jpg"];
        return allowedTypes.includes(file.type);
    }

    validateFileSize(file, maxSizeMB = 15) {
        return file.size <= maxSizeMB * 1024 * 1024;
    }

    validateAvatarFile(file) {
        if (!file) return { valid: true };

        if (!this.validateFileType(file)) {
            return {
                valid: false,
                message: "Định dạng tệp không hợp lệ. Chỉ chấp nhận PNG, JPG.",
            };
        }

        if (!this.validateFileSize(file, 15)) {
            return {
                valid: false,
                message: "Tệp quá lớn. Tối đa 15MB.",
            };
        }

        return { valid: true };
    }

    // ===== Password Validation =====
    validatePassword(password) {
        if (!password)
            return { valid: false, message: "Mật khẩu không được để trống" };

        if (password.length < 8) {
            return {
                valid: false,
                message: "Mật khẩu phải có ít nhất 8 ký tự",
            };
        }

        const hasUpperCase = /[A-Z]/.test(password);
        const hasLowerCase = /[a-z]/.test(password);
        const hasNumbers = /\d/.test(password);

        if (!hasUpperCase || !hasLowerCase || !hasNumbers) {
            return {
                valid: false,
                message: "Mật khẩu phải bao gồm chữ hoa, chữ thường và số",
            };
        }

        return { valid: true };
    }

    validatePasswordForm(passwordForm) {
        if (!passwordForm.currentPassword) {
            return { valid: false, message: "Vui lòng nhập mật khẩu hiện tại" };
        }

        if (!passwordForm.newPassword) {
            return { valid: false, message: "Vui lòng nhập mật khẩu mới" };
        }

        if (!passwordForm.confirmPassword) {
            return { valid: false, message: "Vui lòng xác nhận mật khẩu mới" };
        }

        if (passwordForm.newPassword !== passwordForm.confirmPassword) {
            return { valid: false, message: "Mật khẩu xác nhận không khớp" };
        }

        return this.validatePassword(passwordForm.newPassword);
    }

    // ===== Form Utilities =====
    prepareProfileFormData(userData) {
        const formData = new FormData();

        if (userData.name) formData.append("name", userData.name);
        if (userData.email) formData.append("email", userData.email);
        if (userData.gender) formData.append("gender", userData.gender);
        if (userData.address) formData.append("address", userData.address);
        if (userData.avatarFile) formData.append("avatar", userData.avatarFile);

        return formData;
    }

    getEmptyUserForm() {
        return {
            name: "",
            email: "",
            phoneNumber: "",
            gender: "",
            address: "",
            avatarFile: null,
        };
    }

    // ===== User API =====
    async getMe() {
        try {
            const response = await instance.get("/user/me");
            return response.data.data;
        } catch (error) {
            console.error("Error fetching user profile:", error);
            throw error;
        }
    }

    async updateUserProfile(userInfo) {
        try {
            const response = await instance.put(
                "/user/me",
                userInfo,
                this.getHeaders(userInfo)
            );
            return response.data.data;
        } catch (error) {
            console.error("Error updating user profile:", error);
            throw error;
        }
    }

    async changePassword(passwordData) {
        try {
            const response = await instance.post(
                "/auth/change-password",
                passwordData
            );
            return response.data;
        } catch (error) {
            console.error("Error changing password:", error);
            throw error;
        }
    }
}

export default new MeService();
