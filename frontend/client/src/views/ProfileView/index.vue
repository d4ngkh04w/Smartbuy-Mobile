<template>
    <div class="profile-container">
        <div class="profile-header">
            <h1>Thông tin cá nhân</h1>
            <p>Cập nhật thông tin cá nhân của bạn</p>
        </div>

        <div class="profile-content">
            <div class="avatar-section">
                <div class="avatar-container">
                    <img
                        :src="userAvatar"
                        alt="Avatar"
                        class="avatar-preview"
                    />
                    <div class="avatar-overlay" @click="triggerFileInput">
                        <i class="fa-solid fa-camera"></i>
                    </div>
                </div>
                <input
                    type="file"
                    ref="fileInput"
                    accept="image/jpeg,image/png,image/jpg"
                    style="display: none"
                    @change="handleAvatarChange"
                />
                <button class="avatar-update-btn" @click="triggerFileInput">
                    Thay đổi ảnh đại diện
                </button>
            </div>

            <form class="profile-form" @submit.prevent="updateProfile">
                <div class="form-group">
                    <label for="name">Họ và tên</label>
                    <input
                        type="text"
                        id="name"
                        v-model="userInfo.name"
                        placeholder="Nhập họ và tên của bạn"
                    />
                    <span class="error-message" v-if="errors.name">{{
                        errors.name
                    }}</span>
                </div>

                <div class="form-group">
                    <label for="email">Email</label>
                    <input
                        type="email"
                        id="email"
                        v-model="userInfo.email"
                        placeholder="Nhập địa chỉ email của bạn"
                    />
                    <span class="error-message" v-if="errors.email">{{
                        errors.email
                    }}</span>
                </div>

                <div class="form-group">
                    <label for="phoneNumber">Số điện thoại</label>
                    <input
                        type="text"
                        id="phoneNumber"
                        v-model="userInfo.phoneNumber"
                        placeholder="Nhập số điện thoại của bạn"
                        readonly
                    />
                    <span class="info-message"
                        >Số điện thoại không thể thay đổi</span
                    >
                </div>

                <div class="form-group">
                    <label for="address">Địa chỉ</label>
                    <input
                        type="text"
                        id="address"
                        v-model="userInfo.address"
                        placeholder="Nhập địa chỉ của bạn"
                    />
                </div>

                <div class="form-group">
                    <label for="gender">Giới tính</label>
                    <select id="gender" v-model="userInfo.gender">
                        <option value="Nam">Nam</option>
                        <option value="Nữ">Nữ</option>
                        <option value="Khác">Khác</option>
                    </select>
                </div>

                <button
                    type="submit"
                    class="update-btn"
                    :disabled="isSubmitting"
                >
                    <span v-if="!isSubmitting">Cập nhật thông tin</span>
                    <span v-else>Đang cập nhật...</span>
                </button>
            </form>
        </div>
    </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from "vue";
import { useAuthStore } from "../../stores/authStore";
import { getUserInfo, updateUserProfile } from "../../services/authService";
import emitter from "../../utils/evenBus";

const authStore = useAuthStore();
const fileInput = ref(null);
const isSubmitting = ref(false);

// Thông tin người dùng
const userInfo = reactive({
    name: "",
    email: "",
    phoneNumber: "",
    address: "",
    gender: "Nam",
    avatar: null,
});

// Lưu trữ lỗi validation
const errors = reactive({
    name: "",
    email: "",
    phoneNumber: "",
});

// URL hiển thị avatar
const userAvatar = computed(() => {
    if (userInfo.avatarPreview) {
        return userInfo.avatarPreview;
    }

    if (authStore.user && authStore.user.avatar) {
        return authStore.user.avatar;
    }

    return "https://via.placeholder.com/150";
});

// Mở dialog chọn file
const triggerFileInput = () => {
    fileInput.value.click();
};

// Xử lý khi người dùng chọn avatar mới
const handleAvatarChange = (event) => {
    const file = event.target.files[0];
    if (!file) return;

    // Kiểm tra định dạng file
    const allowedTypes = ["image/jpeg", "image/png", "image/jpg"];
    if (!allowedTypes.includes(file.type)) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Chỉ chấp nhận file hình ảnh (jpg, jpeg, png)",
        });
        return;
    }

    // Giới hạn kích thước file (15MB)
    if (file.size > 15 * 1024 * 1024) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Kích thước file quá lớn (tối đa 15MB)",
        });
        return;
    }

    // Cập nhật file avatar và hiển thị preview
    userInfo.avatar = file;

    // Tạo URL để hiển thị preview
    const reader = new FileReader();
    reader.onload = (e) => {
        userInfo.avatarPreview = e.target.result;
    };
    reader.readAsDataURL(file);
};

// Validate thông tin người dùng
const validateUserInfo = () => {
    let isValid = true;

    // Reset lỗi
    errors.name = "";
    errors.email = "";

    // Validate tên
    if (userInfo.name && !/^[a-zA-Z\s]+$/.test(userInfo.name)) {
        errors.name = "Tên chỉ được chứa chữ cái và khoảng trắng";
        isValid = false;
    }

    // Validate email
    if (
        userInfo.email &&
        !/^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/.test(userInfo.email)
    ) {
        errors.email = "Email không hợp lệ";
        isValid = false;
    }

    return isValid;
};

// Cập nhật thông tin profile
const updateProfile = async () => {
    if (!validateUserInfo()) {
        return;
    }

    try {
        isSubmitting.value = true;

        const response = await updateUserProfile(userInfo);

        // Cập nhật lại thông tin user trong store
        const userResponse = await getUserInfo();
        authStore.setUser(userResponse.data);

        // Hiển thị thông báo thành công
        emitter.emit("show-notification", {
            status: "success",
            message: "Cập nhật thông tin thành công!",
        });

        // Reset trường avatar để tránh gửi lại ảnh khi cập nhật lần tiếp theo
        userInfo.avatar = null;
        userInfo.avatarPreview = null;
    } catch (error) {
        console.error("Lỗi cập nhật thông tin:", error);

        // Hiển thị thông báo lỗi
        emitter.emit("show-notification", {
            status: "error",
            message:
                error.response?.data?.message || "Cập nhật thông tin thất bại",
        });
    } finally {
        isSubmitting.value = false;
    }
};

// Lấy thông tin người dùng hiện tại khi component được tạo
onMounted(async () => {
    try {
        if (!authStore.user) {
            const response = await getUserInfo();
            authStore.setUser(response.data);
        }

        // Điền thông tin người dùng vào form
        if (authStore.user) {
            userInfo.name = authStore.user.name || "";
            userInfo.email = authStore.user.email || "";
            userInfo.phoneNumber = authStore.user.phoneNumber || "";
            userInfo.address = authStore.user.address || "";
            userInfo.gender = authStore.user.gender || "Nam";
        }
    } catch (error) {
        console.error("Lỗi lấy thông tin người dùng:", error);
    }
});
</script>

<style scoped>
.profile-container {
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
    background-color: #fff;
    border-radius: 8px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.profile-header {
    text-align: center;
    margin-bottom: 30px;
}

.profile-header h1 {
    font-size: 28px;
    color: #333;
    margin-bottom: 10px;
}

.profile-header p {
    font-size: 16px;
    color: #666;
}

.profile-content {
    display: flex;
    flex-direction: column;
    gap: 30px;
}

.avatar-section {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 15px;
}

.avatar-container {
    position: relative;
    width: 150px;
    height: 150px;
    overflow: hidden;
    border-radius: 50%;
    border: 3px solid var(--primary-color);
}

.avatar-preview {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.avatar-overlay {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    opacity: 0;
    transition: opacity 0.3s;
    cursor: pointer;
}

.avatar-overlay i {
    color: white;
    font-size: 24px;
}

.avatar-container:hover .avatar-overlay {
    opacity: 1;
}

.avatar-update-btn {
    background-color: transparent;
    border: 1px solid var(--primary-color);
    color: var(--primary-color);
    padding: 8px 20px;
    border-radius: 20px;
    cursor: pointer;
    transition: all 0.3s;
}

.avatar-update-btn:hover {
    background-color: var(--primary-color);
    color: white;
}

.profile-form {
    display: flex;
    flex-direction: column;
    gap: 20px;
}

.form-group {
    display: flex;
    flex-direction: column;
}

.form-group label {
    font-size: 14px;
    font-weight: 500;
    margin-bottom: 5px;
    color: #333;
}

.form-group input,
.form-group select {
    padding: 12px 15px;
    border: 1px solid #ddd;
    border-radius: 4px;
    font-size: 14px;
}

.form-group input:focus,
.form-group select:focus {
    border-color: var(--primary-color);
    outline: none;
}

.form-group input[readonly] {
    background-color: #f5f5f5;
    cursor: not-allowed;
}

.error-message {
    color: #e74c3c;
    font-size: 12px;
    margin-top: 5px;
}

.info-message {
    color: #3498db;
    font-size: 12px;
    margin-top: 5px;
}

.update-btn {
    background-color: var(--primary-color);
    color: white;
    border: none;
    padding: 12px;
    border-radius: 4px;
    font-size: 16px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.3s;
    margin-top: 10px;
}

.update-btn:hover {
    background-color: #e05cb6;
}

.update-btn:disabled {
    background-color: #ccc;
    cursor: not-allowed;
}

@media (min-width: 768px) {
    .profile-content {
        flex-direction: row;
        align-items: flex-start;
    }

    .avatar-section {
        width: 30%;
    }

    .profile-form {
        width: 70%;
    }
}
</style>
