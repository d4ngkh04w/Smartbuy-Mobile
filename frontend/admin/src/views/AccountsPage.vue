<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "../stores/authStore";
import emitter from "../utils/evenBus.js";

const router = useRouter();
const authStore = useAuthStore();
const activeTab = ref("profile");

// Mock admin data (would normally come from API)
const admin = ref({
    name: "Admin User",
    email: "admin@smartbuy.com",
    phone: "0123456789",
    role: "Super Admin",
    createdAt: "15/03/2023",
    avatar: "https://randomuser.me/api/portraits/men/1.jpg",
});

// Form states
const isEditing = ref(false);
const formData = ref({
    name: "",
    phone: "",
    email: "",
    avatar: "",
});

// Password change form
const passwordForm = ref({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
});

// Activity logs mock data
const activityLogs = ref([
    {
        id: 1,
        action: "Đăng nhập",
        timestamp: "19/04/2025 08:30:15",
        ipAddress: "192.168.1.1",
        details: "Đăng nhập thành công",
    },
    {
        id: 2,
        action: "Chỉnh sửa sản phẩm",
        timestamp: "18/04/2025 15:22:10",
        ipAddress: "192.168.1.1",
        details: "Cập nhật giá sản phẩm iPhone 15 Pro",
    },
    {
        id: 3,
        action: "Thêm danh mục",
        timestamp: "17/04/2025 10:45:03",
        ipAddress: "192.168.1.1",
        details: 'Thêm danh mục "Phụ kiện gaming"',
    },
    {
        id: 4,
        action: "Xử lý đơn hàng",
        timestamp: "16/04/2025 14:18:32",
        ipAddress: "192.168.1.1",
        details: "Cập nhật trạng thái đơn hàng #123456",
    },
    {
        id: 5,
        action: "Đăng xuất",
        timestamp: "15/04/2025 17:30:00",
        ipAddress: "192.168.1.1",
        details: "Đăng xuất hệ thống",
    },
]);

onMounted(() => {
    if (!authStore.isLoggedIn()) {
        router.push("/login");
    } else {
        // In a real app, would fetch admin details from API here
        // For now just copying mock data to form
        formData.value = {
            name: admin.value.name,
            phone: admin.value.phone,
            email: admin.value.email,
            avatar: admin.value.avatar,
        };
    }
});

const enableEditing = () => {
    isEditing.value = true;
};

const cancelEditing = () => {
    isEditing.value = false;
    // Reset form data to original values
    formData.value = {
        name: admin.value.name,
        phone: admin.value.phone,
        email: admin.value.email,
        avatar: admin.value.avatar,
    };
};

const saveProfile = () => {
    // Mock update profile - in real app would call API
    admin.value = {
        ...admin.value,
        name: formData.value.name,
        phone: formData.value.phone,
        email: formData.value.email,
        avatar: formData.value.avatar,
    };

    isEditing.value = false;

    emitter.emit("show-notification", {
        status: "success",
        message: "Cập nhật thông tin thành công",
    });
};

const changePassword = () => {
    // Validate password
    if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Mật khẩu xác nhận không khớp",
        });
        return;
    }

    if (
        !passwordForm.value.currentPassword ||
        !passwordForm.value.newPassword
    ) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Vui lòng điền đầy đủ thông tin",
        });
        return;
    }

    // Mock password change - in real app would call API
    passwordForm.value = {
        currentPassword: "",
        newPassword: "",
        confirmPassword: "",
    };

    emitter.emit("show-notification", {
        status: "success",
        message: "Đổi mật khẩu thành công",
    });
};

const goBack = () => {
    router.push("/");
};

const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (file) {
        // In a real app, would upload the file to server
        // For now, just create a local URL
        const reader = new FileReader();
        reader.onload = (e) => {
            formData.value.avatar = e.target.result;
        };
        reader.readAsDataURL(file);
    }
};
</script>

<template>
    <div class="accounts-page">
        <div class="page-header">
            <button class="back-button" @click="goBack">
                <i class="fas fa-arrow-left"></i> Quay lại Dashboard
            </button>
            <h1>Quản lý Tài khoản</h1>
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
                <button
                    :class="[
                        'tab-button',
                        { active: activeTab === 'activity' },
                    ]"
                    @click="activeTab = 'activity'"
                >
                    <i class="fas fa-history"></i> Lịch sử hoạt động
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
                                    :src="
                                        isEditing
                                            ? formData.avatar
                                            : admin.avatar
                                    "
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
                                        accept="image/*"
                                        @change="handleFileChange"
                                        class="avatar-upload"
                                    />
                                </div>
                            </div>
                        </div>

                        <div class="info-section">
                            <div class="info-group">
                                <label>Họ và tên:</label>
                                <input
                                    v-if="isEditing"
                                    v-model="formData.name"
                                    type="text"
                                    class="info-input"
                                />
                                <div v-else class="info-text">
                                    {{ admin.name }}
                                </div>
                            </div>

                            <div class="info-group">
                                <label>Email:</label>
                                <input
                                    v-if="isEditing"
                                    v-model="formData.email"
                                    type="email"
                                    class="info-input"
                                />
                                <div v-else class="info-text">
                                    {{ admin.email }}
                                </div>
                            </div>

                            <div class="info-group">
                                <label>Số điện thoại:</label>
                                <input
                                    v-if="isEditing"
                                    v-model="formData.phone"
                                    type="tel"
                                    class="info-input"
                                />
                                <div v-else class="info-text">
                                    {{ admin.phone }}
                                </div>
                            </div>

                            <div class="info-group">
                                <label>Vai trò:</label>
                                <div class="info-text">{{ admin.role }}</div>
                            </div>

                            <div class="info-group">
                                <label>Ngày tạo tài khoản:</label>
                                <div class="info-text">
                                    {{ admin.createdAt }}
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

                <!-- Activity History Tab -->
                <div v-if="activeTab === 'activity'" class="activity-tab">
                    <h2>Lịch sử hoạt động</h2>

                    <div class="activity-list">
                        <div
                            v-for="log in activityLogs"
                            :key="log.id"
                            class="activity-item"
                        >
                            <div class="activity-icon">
                                <i class="fas fa-history"></i>
                            </div>
                            <div class="activity-details">
                                <div class="activity-header">
                                    <span class="activity-action">{{
                                        log.action
                                    }}</span>
                                    <span class="activity-time">{{
                                        log.timestamp
                                    }}</span>
                                </div>
                                <div class="activity-info">
                                    <span class="activity-ip"
                                        ><i class="fas fa-network-wired"></i>
                                        {{ log.ipAddress }}</span
                                    >
                                    <p class="activity-description">
                                        {{ log.details }}
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="activity-pagination">
                        <button class="pagination-button">
                            <i class="fas fa-chevron-left"></i>
                        </button>
                        <span class="pagination-info">Trang 1 / 1</span>
                        <button class="pagination-button">
                            <i class="fas fa-chevron-right"></i>
                        </button>
                    </div>
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
    align-items: center;
    margin-bottom: 2rem;
}

.back-button {
    background-color: #f5f5f5;
    border: 1px solid #ddd;
    border-radius: 6px;
    padding: 0.5rem 1rem;
    margin-right: 1rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.3s;
}

.back-button:hover {
    background-color: #f8d7e3;
    color: var(--primary-color);
    border-color: var(--primary-color);
}

.page-header h1 {
    font-size: 1.8rem;
    font-weight: 600;
    color: #333;
    margin: 0;
}

.account-container {
    background-color: white;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    overflow: hidden;
}

.account-tabs {
    display: flex;
    background-color: #f8f9fa;
    border-bottom: 1px solid #e9ecef;
}

.tab-button {
    padding: 1rem 1.5rem;
    background: none;
    border: none;
    border-bottom: 3px solid transparent;
    cursor: pointer;
    font-weight: 500;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    color: #6c757d;
    transition: all 0.3s;
}

.tab-button:hover {
    color: var(--primary-color);
}

.tab-button.active {
    color: var(--primary-color);
    border-bottom: 3px solid var(--primary-color);
    background-color: rgba(248, 110, 211, 0.05);
}

.account-content {
    padding: 2rem;
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
    flex: 0 0 200px;
}

.avatar-container {
    position: relative;
    width: 160px;
    height: 160px;
    border-radius: 50%;
    overflow: hidden;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.avatar-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
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
}

.avatar-upload-label {
    color: white;
    cursor: pointer;
    font-size: 1.2rem;
}

.avatar-upload {
    display: none;
}

.info-section {
    flex: 1;
    min-width: 300px;
}

.info-group {
    margin-bottom: 1.5rem;
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

/* Password Tab Styles */
.password-tab h2 {
    margin-top: 0;
    margin-bottom: 2rem;
    font-size: 1.5rem;
    color: #333;
}

.password-form {
    max-width: 600px;
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

.password-input-group {
    position: relative;
}

.password-input-group input {
    width: 100%;
    padding: 0.75rem;
    padding-right: 2.5rem;
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

/* Activity Tab Styles */
.activity-tab h2 {
    margin-top: 0;
    margin-bottom: 2rem;
    font-size: 1.5rem;
    color: #333;
}

.activity-list {
    border: 1px solid #e9ecef;
    border-radius: 8px;
    overflow: hidden;
}

.activity-item {
    display: flex;
    padding: 1rem;
    border-bottom: 1px solid #e9ecef;
    transition: background-color 0.3s;
}

.activity-item:hover {
    background-color: #f8f9fa;
}

.activity-item:last-child {
    border-bottom: none;
}

.activity-icon {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 40px;
    background-color: #f8f9fa;
    border-radius: 50%;
    margin-right: 1rem;
    color: var(--primary-color);
}

.activity-details {
    flex: 1;
}

.activity-header {
    display: flex;
    justify-content: space-between;
    margin-bottom: 0.5rem;
}

.activity-action {
    font-weight: 500;
    color: #212529;
}

.activity-time {
    color: #6c757d;
    font-size: 0.9rem;
}

.activity-info {
    color: #6c757d;
}

.activity-ip {
    font-size: 0.9rem;
    display: inline-flex;
    align-items: center;
    gap: 0.3rem;
    margin-bottom: 0.25rem;
}

.activity-description {
    margin: 0.25rem 0 0;
    color: #495057;
}

.activity-pagination {
    display: flex;
    justify-content: center;
    align-items: center;
    margin-top: 1.5rem;
    gap: 1rem;
}

.pagination-button {
    width: 36px;
    height: 36px;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #f8f9fa;
    border: 1px solid #e9ecef;
    border-radius: 50%;
    cursor: pointer;
    transition: all 0.3s;
}

.pagination-button:hover {
    background-color: var(--primary-color);
    color: white;
    border-color: var(--primary-color);
}

.pagination-info {
    font-size: 0.9rem;
    color: #6c757d;
}
</style>
