<script setup>
import { ref, onMounted, computed } from "vue";
import { useRouter } from "vue-router";
import meService from "../services/meService";
import emitter from "../utils/evenBus.js";

const router = useRouter();
const activeTab = ref("profile");
const loading = ref(false);
const isEditing = ref(false);

// Single source of truth for user data
const userData = ref({
    name: "",
    email: "",
    phoneNumber: "",
    createdAt: "",
    avatar: "",
    gender: "",
    address: "",
    avatarFile: null,
});

// File handling refs
const fileInput = ref(null);
const avatarPreview = ref(null);

// URL hiển thị avatar
const userAvatar = computed(() => {
    if (avatarPreview.value) return avatarPreview.value;
    if (userData.value && userData.value.avatar) {
        return meService.getUrlImage(userData.value.avatar);
    }
    return "https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg";
});

// Password change form
const passwordForm = ref({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
});

onMounted(async () => {
    await fetchUserData();
});

// Fetch user data
const fetchUserData = async () => {
    loading.value = true;
    try {
        const data = await meService.getMe();

        // Format date if needed
        if (data.createdAt) {
            const date = new Date(data.createdAt);
            data.createdAt = date.toLocaleDateString("vi-VN");
        }

        // Update userData with response data
        userData.value = {
            ...data,
            avatarFile: null,
        };
    } catch (error) {
        console.error("Error fetching user profile:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể tải thông tin tài khoản",
        });
        router.push("/login");
    } finally {
        loading.value = false;
    }
};

// Handle avatar upload
const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (!file) return;

    const validation = meService.validateAvatarFile(file);
    if (!validation.valid) {
        emitter.emit("show-notification", {
            status: "error",
            message: validation.message,
        });
        return;
    }

    // Lưu file để gửi lên server
    userData.value.avatarFile = file;

    // Tạo URL để hiển thị preview
    const reader = new FileReader();
    reader.onload = (e) => {
        avatarPreview.value = e.target.result;
    };
    reader.readAsDataURL(file);
};

// Toggle edit mode
const enableEditing = () => {
    // Lưu trạng thái ban đầu trong trường hợp hủy
    userData.value = { ...userData.value, avatarFile: null };
    isEditing.value = true;
};

// Cancel editing
const cancelEditing = () => {
    isEditing.value = false;
    avatarPreview.value = "";

    // Fetch lại dữ liệu để đảm bảo dữ liệu hiển thị là mới nhất
    fetchUserData();

    if (fileInput.value) {
        fileInput.value.value = "";
    }
};

// Save profile
const saveProfile = async () => {
    loading.value = true;
    try {
        // Sử dụng utility method từ meService để chuẩn bị FormData
        const formData = meService.prepareProfileFormData(userData.value);

        // Gọi API cập nhật thông tin với FormData
        const data = await meService.updateUserProfile(formData);

        // Format date if needed
        if (data.createdAt) {
            const date = new Date(data.createdAt);
            data.createdAt = date.toLocaleDateString("vi-VN");
        }

        // Cập nhật userData với dữ liệu mới từ server
        userData.value = {
            ...data,
            avatarFile: null,
        };

        // Hiển thị thông báo thành công
        emitter.emit("show-notification", {
            status: "success",
            message: "Cập nhật thông tin thành công",
        });

        // Reset state
        isEditing.value = false;
        avatarPreview.value = "";
        if (fileInput.value) {
            fileInput.value.value = "";
        }
    } catch (error) {
        console.error("Error updating profile:", error);
        emitter.emit("show-notification", {
            status: "error",
            message:
                error.response?.data?.message || "Cập nhật thông tin thất bại",
        });
    } finally {
        loading.value = false;
    }
};

// Change password
const changePassword = async () => {
    // Validate password using meService utility
    const validation = meService.validatePasswordForm(passwordForm.value);
    if (!validation.valid) {
        emitter.emit("show-notification", {
            status: "error",
            message: validation.message,
        });
        return;
    }

    loading.value = true;
    try {
        await meService.changePassword({
            oldPassword: passwordForm.value.currentPassword,
            newPassword: passwordForm.value.newPassword,
            confirmNewPassword: passwordForm.value.confirmPassword,
        });

        emitter.emit("show-notification", {
            status: "success",
            message: "Đổi mật khẩu thành công",
        });

        // Reset password form
        passwordForm.value.currentPassword = "";
        passwordForm.value.newPassword = "";
        passwordForm.value.confirmPassword = "";
    } catch (error) {
        console.error("Error changing password:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: error.response?.data?.message || "Đổi mật khẩu thất bại",
        });
    } finally {
        loading.value = false;
    }
};

const goBack = () => {
    router.push("/dashboard");
};
</script>

<template>
    <div class="accounts-page">
        <div class="page-header">
            <div class="header-top">
                <button class="back-button" @click="goBack">
                    <i class="fas fa-arrow-left"></i> Quay lại Dashboard
                </button>
                <h1>Quản lý Tài khoản</h1>
                <!-- Thêm nút giả mà không có chức năng để giữ layout -->
                <button class="invisible-button" style="visibility: hidden">
                    <i class="fas fa-sync-alt"></i> Nút ẩn
                </button>
            </div>
        </div>

        <div class="account-container">
            <div class="account-tabs">
                <button
                    :class="['tab-button', { active: activeTab === 'profile' }]"
                    @click="activeTab = 'profile'"
                >
                    <i class="fas fa-user"></i> Thông tin cá nhân
                </button>
                <button
                    :class="[
                        'tab-button',
                        { active: activeTab === 'password' },
                    ]"
                    @click="activeTab = 'password'"
                >
                    <i class="fas fa-key"></i> Đổi mật khẩu
                </button>
            </div>

            <div class="account-content">
                <!-- Profile Information Tab -->
                <div v-if="activeTab === 'profile'" class="profile-tab">
                    <div class="profile-header">
                        <h2>Thông tin cá nhân</h2>
                        <template v-if="!isEditing">
                            <button class="edit-button" @click="enableEditing">
                                <i class="fas fa-edit"></i> Chỉnh sửa
                            </button>
                        </template>
                        <template v-else>
                            <div class="edit-actions">
                                <button
                                    class="cancel-button"
                                    @click="cancelEditing"
                                >
                                    <i class="fas fa-times"></i> Hủy
                                </button>
                                <button
                                    class="save-button"
                                    @click="saveProfile"
                                >
                                    <i class="fas fa-check"></i> Lưu
                                </button>
                            </div>
                        </template>
                    </div>

                    <div class="profile-content">
                        <div class="avatar-section">
                            <div class="avatar-container">
                                <img
                                    :src="userAvatar"
                                    alt="Avatar"
                                    class="avatar-image"
                                />
                                <div v-if="isEditing" class="avatar-overlay">
                                    <label
                                        for="avatar-upload"
                                        class="avatar-upload-label"
                                    >
                                        <i class="fas fa-camera"></i>
                                    </label>
                                    <input
                                        id="avatar-upload"
                                        type="file"
                                        accept="image/jpeg,image/png,image/jpg"
                                        @change="handleFileChange"
                                        class="avatar-upload"
                                        ref="fileInput"
                                    />
                                </div>
                            </div>
                            <span v-if="isEditing" class="avatar-helper"
                                >Thêm ảnh đại diện</span
                            >
                        </div>

                        <div class="info-section">
                            <div class="info-grid">
                                <div class="info-group">
                                    <label>Họ và tên:</label>
                                    <input
                                        v-if="isEditing"
                                        v-model="userData.name"
                                        type="text"
                                        class="info-input"
                                        placeholder="Nhập họ và tên"
                                    />
                                    <div v-else class="info-text">
                                        {{ userData.name || "Chưa cập nhật" }}
                                    </div>
                                </div>

                                <div class="info-group">
                                    <label>Email:</label>
                                    <input
                                        v-if="isEditing"
                                        v-model="userData.email"
                                        type="email"
                                        class="info-input"
                                        placeholder="Nhập email"
                                    />
                                    <div v-else class="info-text">
                                        {{ userData.email || "Chưa cập nhật" }}
                                    </div>
                                </div>
                                <div class="info-group">
                                    <label>Giới tính:</label>
                                    <select
                                        v-if="isEditing"
                                        v-model="userData.gender"
                                        class="info-input"
                                    >
                                        <option value="">
                                            -- Chọn giới tính --
                                        </option>
                                        <option value="Nam">Nam</option>
                                        <option value="Nữ">Nữ</option>
                                        <option value="Khác">Khác</option>
                                    </select>
                                    <div v-else class="info-text">
                                        {{ userData.gender || "Chưa cập nhật" }}
                                    </div>
                                </div>
                                <div class="info-group">
                                    <label>Địa chỉ:</label>
                                    <textarea
                                        v-if="isEditing"
                                        v-model="userData.address"
                                        class="info-input address-input"
                                        placeholder="Nhập địa chỉ"
                                    ></textarea>
                                    <div v-else class="info-text">
                                        {{
                                            userData.address || "Chưa cập nhật"
                                        }}
                                    </div>
                                </div>
                                <div class="info-group">
                                    <label>Số điện thoại:</label>
                                    <div class="info-text">
                                        {{
                                            userData.phoneNumber ||
                                            "Chưa cập nhật"
                                        }}
                                    </div>
                                </div>

                                <div class="info-group">
                                    <label>Ngày tạo tài khoản:</label>
                                    <div class="info-text">
                                        {{
                                            userData.createdAt ||
                                            "Chưa cập nhật"
                                        }}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Password Change Tab -->
                <div v-if="activeTab === 'password'" class="password-tab">
                    <h2>Đổi mật khẩu</h2>

                    <form
                        @submit.prevent="changePassword"
                        class="password-form"
                    >
                        <div class="form-group">
                            <label for="current-password"
                                >Mật khẩu hiện tại:</label
                            >
                            <div class="password-input-group">
                                <input
                                    id="current-password"
                                    v-model="passwordForm.currentPassword"
                                    type="password"
                                    placeholder="Nhập mật khẩu hiện tại"
                                />
                            </div>
                        </div>

                        <div class="form-group">
                            <label for="new-password">Mật khẩu mới:</label>
                            <div class="password-input-group">
                                <input
                                    id="new-password"
                                    v-model="passwordForm.newPassword"
                                    type="password"
                                    placeholder="Nhập mật khẩu mới"
                                />
                            </div>
                            <p class="password-hint">
                                Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ
                                hoa, chữ thường và số
                            </p>
                        </div>

                        <div class="form-group">
                            <label for="confirm-password"
                                >Xác nhận mật khẩu mới:</label
                            >
                            <div class="password-input-group">
                                <input
                                    id="confirm-password"
                                    v-model="passwordForm.confirmPassword"
                                    type="password"
                                    placeholder="Nhập lại mật khẩu mới"
                                />
                            </div>
                        </div>

                        <button type="submit" class="change-password-button">
                            <i class="fas fa-key"></i> Đổi mật khẩu
                        </button>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.accounts-page {
    padding: 2rem;
    background-color: #f9f9f9;
    min-height: 100vh;
}

.page-header {
    display: flex;
    flex-direction: column;
    margin-bottom: 2rem;
    background-color: white;
    padding: 2rem;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    border-left: 4px solid var(--primary-color);
}

.header-top {
    display: flex;
    justify-content: space-between;
    align-items: center;
    width: 100%;
    margin-bottom: 0.5rem;
}

.back-button {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    background-color: #fff;
    color: #666;
    border: 1px solid #e9ecef;
    border-radius: 6px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
}

.back-button i {
    color: var(--primary-color);
}

.back-button:hover {
    background-color: #f8f0f4;
    color: var(--primary-color);
}

.page-header h1 {
    margin: 0;
    font-size: 1.8rem;
    font-weight: 600;
    color: #333;
}

.account-container {
    background-color: white;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    overflow: hidden;
}

.account-tabs {
    display: flex;
    border-bottom: 1px solid #e2e8f0;
    margin-bottom: 1.5rem;
    background-color: white;
    border-radius: 12px 12px 0 0;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
    overflow: hidden;
}

.tab-button {
    padding: 1rem 1.5rem;
    background-color: white;
    border: none;
    cursor: pointer;
    color: #666;
    font-weight: 500;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.3s;
    position: relative;
}

.tab-button i {
    font-size: 1.2rem;
}

.tab-button:hover {
    color: var(--primary-color);
    background-color: #fff5fc;
}

.tab-button.active {
    color: var(--primary-color);
    border-bottom: 3px solid var(--primary-color);
    font-weight: 600;
}

.account-content {
    background-color: white;
    border-radius: 0 0 12px 12px;
    padding: 1.5rem;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

/* Profile Tab Styles */
.profile-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
}

.profile-header h2 {
    margin: 0;
    font-size: 1.5rem;
    color: #333;
}

.edit-button,
.save-button,
.cancel-button {
    padding: 0.5rem 1rem;
    border-radius: 6px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.3s;
}

.edit-button {
    background-color: #f8f9fa;
    border: 1px solid #e9ecef;
    color: #495057;
}

.edit-button:hover {
    background-color: #e9ecef;
    color: #212529;
}

.edit-actions {
    display: flex;
    gap: 0.5rem;
}

.save-button {
    background-color: var(--primary-color);
    border: 1px solid var(--primary-color);
    color: white;
}

.save-button:hover {
    background-color: #e056b2;
}

.cancel-button {
    background-color: #f8f9fa;
    border: 1px solid #e9ecef;
    color: #6c757d;
}

.cancel-button:hover {
    background-color: #e9ecef;
    color: #495057;
}

.profile-content {
    display: flex;
    flex-wrap: wrap;
    gap: 2rem;
}

.avatar-section {
    display: flex;
    flex-direction: column;
    align-items: center;
}

.avatar-container {
    position: relative;
    width: 160px;
    height: 160px;
    border-radius: 50%;
    overflow: hidden;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    margin-bottom: 0.5rem;
    transition: all 0.3s ease;
}

.avatar-container:hover {
    transform: scale(1.02);
}

.avatar-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
    transition: transform 0.3s ease;
}

.avatar-container:hover .avatar-image {
    transform: scale(1.05);
}

.avatar-overlay {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    padding: 0.5rem;
    transition: opacity 0.3s ease;
}

.avatar-upload-label {
    color: white;
    cursor: pointer;
    font-size: 1.2rem;
    transition: transform 0.3s ease;
}

.avatar-upload-label:hover {
    transform: scale(1.1);
}

.avatar-upload {
    display: none;
}

.avatar-helper {
    font-size: 0.85rem;
    color: #6c757d;
    margin-top: 0.5rem;
}

.info-section {
    flex: 1;
    min-width: 300px;
}

.info-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 1.5rem;
}

.info-group {
    margin-bottom: 1rem;
}

.info-group label {
    display: block;
    font-weight: 500;
    color: #6c757d;
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
}

.info-text {
    font-size: 1rem;
    color: #212529;
    padding: 0.5rem 0;
}

.role-badge {
    display: inline-block;
    background-color: rgba(224, 92, 182, 0.15);
    color: var(--primary-color);
    font-weight: 500;
    padding: 0.35rem 0.8rem;
    border-radius: 50px;
    font-size: 0.9rem;
}

.info-input {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ced4da;
    border-radius: 6px;
    font-size: 1rem;
    transition: all 0.3s;
}

.info-input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.25);
    outline: none;
}

.address-input {
    min-height: 100px;
    resize: vertical;
}

/* Password Tab Styles */
.password-tab h2 {
    margin-top: 0;
    margin-bottom: 2rem;
    font-size: 1.5rem;
    color: #333;
}

.password-form {
    max-width: 500px;
}

.form-group {
    margin-bottom: 1.5rem;
}

.form-group label {
    display: block;
    font-weight: 500;
    color: #6c757d;
    margin-bottom: 0.5rem;
}

.password-input-group input {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ced4da;
    border-radius: 6px;
    font-size: 1rem;
    transition: all 0.3s;
}

.password-input-group input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.25);
    outline: none;
}

.password-hint {
    font-size: 0.85rem;
    color: #6c757d;
    margin-top: 0.5rem;
}

.change-password-button {
    background-color: var(--primary-color);
    color: white;
    border: none;
    padding: 0.75rem 1.5rem;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 500;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.3s;
}

.change-password-button:hover {
    background-color: #e056b2;
}

@media (max-width: 768px) {
    .profile-content {
        flex-direction: column;
        align-items: center;
    }

    .info-section {
        width: 100%;
    }

    .info-grid {
        grid-template-columns: 1fr;
    }
}
</style>
```````````````````````````````
