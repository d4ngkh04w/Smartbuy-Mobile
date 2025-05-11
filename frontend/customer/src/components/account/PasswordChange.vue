<script setup>
import { ref } from "vue";
import meService from "@/services/meService";
import emitter from "@/utils/evenBus";

const loading = ref(false);
const errorMessage = ref("");

// Password form data
const passwordForm = ref({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
});

// Toggle password visibility
const showPassword = ref({
    current: false,
    new: false,
    confirm: false,
});

// Change password function
const changePassword = async () => {
    loading.value = true;
    errorMessage.value = "";

    // Validate passwords
    if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
        errorMessage.value = "Mật khẩu xác nhận không khớp";
        loading.value = false;
        return;
    }

    try {
        await meService.changePassword({
            oldPassword: passwordForm.value.currentPassword,
            newPassword: passwordForm.value.newPassword,
            confirmNewPassword: passwordForm.value.confirmPassword,
        });

        // Show success notification
        emitter.emit("show-notification", {
            status: "success",
            message: "Đổi mật khẩu thành công",
        });

        // Reset form
        passwordForm.value = {
            currentPassword: "",
            newPassword: "",
            confirmPassword: "",
        };
    } catch (error) {
        console.error("Error changing password:", error);
        errorMessage.value =
            error.response?.data?.message || "Đã xảy ra lỗi khi đổi mật khẩu";

        emitter.emit("show-notification", {
            status: "error",
            message: "Đã xảy ra lỗi khi đổi mật khẩu",
        });
    } finally {
        loading.value = false;
    }
};
</script>
<template>
    <div class="password-content">
        <div class="back-header">
            <button class="back-button" @click="$emit('back')">
                <i class="fas fa-arrow-left"></i> Quay lại
            </button>
            <h2>Đổi mật khẩu</h2>
        </div>

        <form @submit.prevent="changePassword" class="password-form">
            <div class="form-group">
                <label for="current-password">Mật khẩu hiện tại:</label>
                <div class="password-input-group">
                    <input
                        id="current-password"
                        v-model="passwordForm.currentPassword"
                        :type="showPassword.current ? 'text' : 'password'"
                        placeholder="Nhập mật khẩu hiện tại"
                        required
                    />
                    <button
                        type="button"
                        class="toggle-password"
                        @click="showPassword.current = !showPassword.current"
                    >
                        <i
                            :class="
                                showPassword.current
                                    ? 'fas fa-eye-slash'
                                    : 'fas fa-eye'
                            "
                        ></i>
                    </button>
                </div>
            </div>

            <div class="form-group">
                <label for="new-password">Mật khẩu mới:</label>
                <div class="password-input-group">
                    <input
                        id="new-password"
                        v-model="passwordForm.newPassword"
                        :type="showPassword.new ? 'text' : 'password'"
                        placeholder="Nhập mật khẩu mới"
                        required
                        minlength="6"
                    />
                    <button
                        type="button"
                        class="toggle-password"
                        @click="showPassword.new = !showPassword.new"
                    >
                        <i
                            :class="
                                showPassword.new
                                    ? 'fas fa-eye-slash'
                                    : 'fas fa-eye'
                            "
                        ></i>
                    </button>
                </div>
                <p class="password-hint">Mật khẩu phải có ít nhất 6 ký tự</p>
            </div>

            <div class="form-group">
                <label for="confirm-password">Xác nhận mật khẩu mới:</label>
                <div class="password-input-group">
                    <input
                        id="confirm-password"
                        v-model="passwordForm.confirmPassword"
                        :type="showPassword.confirm ? 'text' : 'password'"
                        placeholder="Nhập lại mật khẩu mới"
                        required
                    />
                    <button
                        type="button"
                        class="toggle-password"
                        @click="showPassword.confirm = !showPassword.confirm"
                    >
                        <i
                            :class="
                                showPassword.confirm
                                    ? 'fas fa-eye-slash'
                                    : 'fas fa-eye'
                            "
                        ></i>
                    </button>
                </div>
            </div>

            <div v-if="errorMessage" class="error-message">
                {{ errorMessage }}
            </div>

            <button
                type="submit"
                class="change-password-button"
                :disabled="loading"
            >
                <i class="fas fa-key"></i>
                {{ loading ? "Đang xử lý..." : "Đổi mật khẩu" }}
            </button>
        </form>
    </div>
</template>

<style scoped>
.password-content {
    background-color: white;
    border-radius: 10px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    padding: 1.5rem;
}

.back-header {
    display: flex;
    align-items: center;
    gap: 1rem;
    margin-bottom: 2rem;
}

.back-button {
    background: none;
    border: none;
    color: #666;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 0;
    font-size: 0.9rem;
    transition: color 0.3s ease;
}

.back-button:hover {
    color: var(--primary-color);
}

.back-header h2 {
    margin: 0;
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
    color: #666;
    margin-bottom: 0.5rem;
}

.password-input-group {
    position: relative;
    width: 100%;
}

.password-input-group input {
    width: 100%;
    padding: 0.75rem 1rem;
    padding-right: 3rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 1rem;
}

.password-input-group input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.15);
    outline: none;
}

.toggle-password {
    position: absolute;
    right: 1rem;
    top: 50%;
    transform: translateY(-50%);
    background: none;
    border: none;
    color: #777;
    cursor: pointer;
}

.password-hint {
    font-size: 0.85rem;
    color: #777;
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
    transition: all 0.3s ease;
}

.change-password-button:hover {
    background-color: #e056b2;
}

.change-password-button:disabled {
    background-color: #e0e0e0;
    cursor: not-allowed;
}

.error-message {
    background-color: #ffebee;
    color: #c62828;
    padding: 1rem;
    border-radius: 6px;
    margin: 1rem 0;
    font-size: 0.9rem;
}
</style>
