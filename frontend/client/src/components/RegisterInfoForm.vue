<template>
    <div class="register-info-overlay" @click.self="$emit('close')">
        <div class="register-info-container">
            <div class="register-info-header">
                <h1>Hoàn thiện thông tin cá nhân</h1>
                <p>Cập nhật thông tin cá nhân của bạn để trải nghiệm tốt hơn</p>
            </div>

            <form class="register-info-form" @submit.prevent="updateProfile">
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
                    <span class="avatar-helper">Thêm ảnh đại diện</span>
                </div>

                <div class="form-row">
                    <div class="form-group">
                        <label for="name">Họ và tên</label>
                        <input
                            type="text"
                            id="name"
                            v-model="authStore.user.name"
                            placeholder="Nhập họ và tên của bạn"
                        />
                        <span class="error-message" v-if="errors.name">{{
                            errors.name
                        }}</span>
                    </div>

                    <div class="form-group">
                        <label for="gender">Giới tính</label>
                        <select id="gender" v-model="authStore.user.gender">
                            <option value="Nam">Nam</option>
                            <option value="Nữ">Nữ</option>
                            <option value="Khác">Khác</option>
                        </select>
                    </div>
                </div>

                <div class="form-group">
                    <label for="address">Địa chỉ</label>
                    <input
                        type="text"
                        id="address"
                        v-model="authStore.user.address"
                        placeholder="Nhập địa chỉ của bạn"
                    />
                </div>

                <div class="form-row">
                    <div class="form-group" style="width: 65%">
                        <label for="email">Email</label>
                        <input
                            type="email"
                            id="email"
                            v-model="authStore.user.email"
                            :readonly="!!authStore.user.email"
                            :class="{ readonly: !!authStore.user.email }"
                            placeholder="Nhập địa chỉ email của bạn"
                        />
                        <span class="error-message" v-if="errors.email">{{
                            errors.email
                        }}</span>
                    </div>

                    <div class="form-group" style="width: 35%">
                        <label for="phoneNumber">Số điện thoại</label>
                        <input
                            type="text"
                            id="phoneNumber"
                            v-model="authStore.user.phoneNumber"
                            readonly
                            class="readonly"
                        />
                    </div>
                </div>

                <div class="form-action">
                    <button type="button" class="skip-btn" @click="skipUpdate">
                        Bỏ qua
                    </button>
                    <button
                        type="submit"
                        class="update-btn"
                        :disabled="isSubmitting"
                    >
                        <span v-if="!isSubmitting">Cập nhật thông tin</span>
                        <span v-else>Đang cập nhật...</span>
                    </button>
                </div>
            </form>
        </div>
    </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from "vue";
import { useAuthStore } from "../stores/authStore";
import { updateUserProfile } from "../services/authService";
import emitter from "../utils/evenBus";

const emit = defineEmits(["close", "update-success"]);

const authStore = useAuthStore();
const fileInput = ref(null);
const isSubmitting = ref(false);

// Lưu file avatar mới để gửi lên server
const newAvatarFile = ref(null);
// Lưu URL preview của avatar mới để hiển thị
const avatarPreview = ref(null);

// Lưu trữ lỗi validation
const errors = reactive({
    name: "",
    email: "",
});

// URL hiển thị avatar
const userAvatar = computed(() => {
    // Nếu có preview mới, ưu tiên hiển thị
    if (avatarPreview.value) {
        return avatarPreview.value;
    }

    // Nếu có avatar trong store (URL đường dẫn)
    if (authStore.user && authStore.user.avatar) {
        return authStore.user.avatar;
    }

    // Fallback mặc định
    return "https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg";
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

    // Lưu file để gửi lên server
    newAvatarFile.value = file;

    // Tạo URL để hiển thị preview
    const reader = new FileReader();
    reader.onload = (e) => {
        avatarPreview.value = e.target.result;
    };
    reader.readAsDataURL(file);
};

// Cập nhật thông tin profile
const updateProfile = async () => {
    try {
        isSubmitting.value = true;

        // Tạo đối tượng chứa dữ liệu cập nhật
        const updateData = {
            name: authStore.user.name,
            email: authStore.user.email,
            phoneNumber: authStore.user.phoneNumber,
            address: authStore.user.address,
            gender: authStore.user.gender,
        };

        // Chỉ gửi file avatar nếu người dùng đã chọn ảnh mới
        if (newAvatarFile.value) {
            updateData.avatar = newAvatarFile.value;
        }

        await updateUserProfile(updateData);

        // Hiển thị thông báo thành công
        emitter.emit("show-notification", {
            status: "success",
            message: "Cập nhật thông tin thành công!",
        });

        // Báo cho component cha biết đã cập nhật thành công
        emit("update-success");

        // Đóng modal
        emit("close");
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

// Bỏ qua việc cập nhật thông tin
const skipUpdate = () => {
    emitter.emit("show-notification", {
        status: "warning",
        message: "Vui lòng cập nhật thông tin sau!",
    });
    emit("close");
};

// Khởi tạo dữ liệu khi component được tạo
onMounted(() => {
    // Không cần thực hiện gì nếu đã có dữ liệu trong authStore
});
</script>

<style scoped>
.register-info-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.6);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
    backdrop-filter: blur(3px);
}

.register-info-container {
    width: 650px;
    max-width: 90%;
    background-color: #fff;
    border-radius: 16px;
    box-shadow: 0 15px 30px rgba(0, 0, 0, 0.15);
    padding: 32px;
    animation: slideDown 0.4s ease-out;
    transition: transform 0.3s ease;
}

.register-info-container:hover {
    transform: translateY(-2px);
}

@keyframes slideDown {
    0% {
        transform: translateY(-30px);
        opacity: 0;
    }
    100% {
        transform: translateY(0);
        opacity: 1;
    }
}

.register-info-header {
    text-align: center;
    margin-bottom: 28px;
}

.register-info-header h1 {
    font-size: 26px;
    color: #333;
    margin-bottom: 12px;
    font-weight: 600;
}

.register-info-header p {
    font-size: 15px;
    color: #666;
    margin: 0;
}

.register-info-form {
    display: flex;
    flex-direction: column;
    gap: 22px;
}

.avatar-section {
    display: flex;
    flex-direction: column;
    align-items: center;
    margin-bottom: 15px;
}

.avatar-container {
    position: relative;
    width: 110px;
    height: 110px;
    overflow: hidden;
    border-radius: 50%;
    border: 3px solid var(--primary-color);
    margin-bottom: 10px;
    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
    transition: all 0.3s ease;
}

.avatar-container:hover {
    transform: scale(1.03);
    box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15);
}

.avatar-preview {
    width: 100%;
    height: 100%;
    object-fit: cover;
    transition: transform 0.5s ease;
}

.avatar-container:hover .avatar-preview {
    transform: scale(1.1);
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
    transition: opacity 0.3s, background-color 0.3s;
    cursor: pointer;
}

.avatar-overlay i {
    color: white;
    font-size: 28px;
    transition: transform 0.3s ease;
}

.avatar-container:hover .avatar-overlay {
    opacity: 1;
    background-color: rgba(0, 0, 0, 0.6);
}

.avatar-container:hover .avatar-overlay i {
    transform: scale(1.1);
}

.avatar-helper {
    font-size: 13px;
    color: #666;
    margin-top: 5px;
    transition: color 0.3s;
}

.avatar-container:hover + input + .avatar-helper {
    color: var(--primary-color);
}

.form-row {
    display: flex;
    gap: 18px;
}

.form-group {
    display: flex;
    flex-direction: column;
    flex: 1;
    margin-bottom: 5px;
}

.form-group label {
    font-size: 14px;
    font-weight: 500;
    margin-bottom: 6px;
    color: #444;
    transition: color 0.3s;
}

.form-group input,
.form-group select {
    padding: 12px 14px;
    border: 1.5px solid #ddd;
    border-radius: 8px;
    font-size: 14px;
    transition: all 0.3s ease;
    background-color: #fcfcfc;
}

.form-group input:hover:not(.readonly),
.form-group select:hover {
    border-color: #bbb;
    background-color: #f8f8f8;
}

.form-group input:focus,
.form-group select:focus {
    border-color: var(--primary-color);
    outline: none;
    box-shadow: 0 0 0 3px rgba(224, 92, 182, 0.15);
    background-color: white;
}

.form-group:focus-within label {
    color: var(--primary-color);
}

.form-group input.readonly {
    background-color: #f5f5f5;
    cursor: not-allowed;
    color: #666;
    border-color: #e0e0e0;
}

.error-message {
    color: #e74c3c;
    font-size: 12px;
    margin-top: 5px;
    animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
    from {
        opacity: 0;
        transform: translateY(-5px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.form-action {
    display: flex;
    justify-content: space-between;
    margin-top: 15px;
}

.skip-btn {
    background-color: transparent;
    border: 1.5px solid #ddd;
    color: #666;
    padding: 11px 22px;
    border-radius: 8px;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.3s ease;
}

.skip-btn:hover {
    background-color: #f5f5f5;
    border-color: #ccc;
    color: #444;
    transform: translateY(-2px);
}

.skip-btn:active {
    transform: translateY(0);
}

.update-btn {
    background-color: var(--primary-color);
    color: white;
    border: none;
    padding: 12px 26px;
    border-radius: 8px;
    font-size: 14px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.3s ease;
    box-shadow: 0 4px 10px rgba(224, 92, 182, 0.2);
}

.update-btn:hover {
    background-color: #e05cb6;
    transform: translateY(-2px);
    box-shadow: 0 6px 15px rgba(224, 92, 182, 0.3);
}

.update-btn:active {
    transform: translateY(0);
    box-shadow: 0 2px 5px rgba(224, 92, 182, 0.2);
}

.update-btn:disabled {
    background-color: #ccc;
    cursor: not-allowed;
    box-shadow: none;
    transform: none;
}

@media (max-width: 576px) {
    .register-info-container {
        padding: 20px;
    }

    .form-row {
        flex-direction: column;
        gap: 15px;
    }

    .skip-btn,
    .update-btn {
        padding: 10px 15px;
    }
}
</style>
