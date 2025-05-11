<template>
    <div class="delete-account-content">
        <div class="back-header">
            <button class="back-button" @click="$emit('back')">
                <i class="fas fa-arrow-left"></i> Quay lại
            </button>
            <h2>Xóa tài khoản</h2>
        </div>

        <div class="delete-warning">
            <i class="fas fa-exclamation-triangle"></i>
            <div class="warning-text">
                <h3>Cảnh báo: Hành động không thể khôi phục</h3>
                <p>
                    Khi xóa tài khoản, tất cả dữ liệu cá nhân, lịch sử đơn hàng,
                    và điểm thưởng sẽ bị xóa vĩnh viễn.
                </p>
            </div>
        </div>

        <div class="deletion-info">
            <h4>Trước khi xóa tài khoản, vui lòng lưu ý:</h4>
            <ul>
                <li>
                    <i class="fas fa-shopping-bag"></i>Tất cả đơn hàng đang xử
                    lý sẽ vẫn được hoàn thành
                </li>
                <li>
                    <i class="fas fa-coins"></i>Điểm thưởng và ưu đãi chưa sử
                    dụng sẽ bị mất
                </li>
                <li>
                    <i class="fas fa-history"></i>Lịch sử mua hàng sẽ không còn
                    truy cập được
                </li>
                <li>
                    <i class="fas fa-user-shield"></i>Thông tin cá nhân sẽ bị
                    xóa khỏi hệ thống
                </li>
            </ul>
        </div>

        <form @submit.prevent="confirmDelete" class="delete-form">
            <div class="form-group">
                <label for="password">Nhập mật khẩu để xác nhận:</label>
                <div class="password-input-group">
                    <input
                        id="password"
                        v-model="password"
                        :type="showPassword ? 'text' : 'password'"
                        placeholder="Nhập mật khẩu hiện tại của bạn"
                        required
                    />
                    <button
                        type="button"
                        class="toggle-password"
                        @click="showPassword = !showPassword"
                    >
                        <i
                            :class="
                                showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'
                            "
                        ></i>
                    </button>
                </div>
            </div>

            <div class="form-group">
                <label for="confirm"
                    >Xác nhận bằng cách gõ "XÓA TÀI KHOẢN":</label
                >
                <input
                    id="confirm"
                    v-model="confirmText"
                    type="text"
                    class="form-input"
                    placeholder='Gõ "XÓA TÀI KHOẢN"'
                    required
                />
            </div>

            <div v-if="errorMessage" class="error-message">
                {{ errorMessage }}
            </div>

            <button
                type="submit"
                class="delete-account-button"
                :disabled="
                    loading || !password || confirmText !== 'XÓA TÀI KHOẢN'
                "
            >
                <i class="fas fa-user-times"></i>
                {{ loading ? "Đang xử lý..." : "Xóa tài khoản của tôi" }}
            </button>
        </form>
    </div>
</template>

<script setup>
import { ref } from "vue";
import meService from "@/services/meService";
import emitter from "@/utils/evenBus";
import { useRouter } from "vue-router";

const emit = defineEmits(["back"]);
const router = useRouter();

const password = ref("");
const confirmText = ref("");
const showPassword = ref(false);
const errorMessage = ref("");
const loading = ref(false);

const confirmDelete = async () => {
    if (confirmText.value !== "XÓA TÀI KHOẢN") {
        errorMessage.value = "Vui lòng nhập chính xác cụm từ xác nhận";
        return;
    }

    loading.value = true;
    errorMessage.value = "";

    try {
        // Sử dụng API deleteMyAccount đã được backend thiết kế
        await meService.deleteMyAccount({ password: password.value });

        // Hiển thị thông báo thành công
        emitter.emit("show-notification", {
            status: "success",
            message: "Tài khoản đã được xóa thành công",
        });

        // Chuyển hướng về trang chủ
        router.push("/");
    } catch (error) {
        console.error("Error deleting account:", error);
        errorMessage.value =
            error.response?.data?.message || "Đã xảy ra lỗi khi xóa tài khoản";

        emitter.emit("show-notification", {
            status: "error",
            message: errorMessage.value,
        });
    } finally {
        loading.value = false;
    }
};
</script>

<style scoped>
.delete-account-content {
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

.delete-warning {
    background-color: #fff5f5;
    border-left: 4px solid #e53935;
    border-radius: 6px;
    padding: 1.5rem;
    margin-bottom: 2rem;
    display: flex;
    gap: 1rem;
    align-items: flex-start;
}

.delete-warning i {
    font-size: 2rem;
    color: #e53935;
}

.warning-text h3 {
    margin: 0 0 0.5rem 0;
    color: #e53935;
    font-size: 1.1rem;
}

.warning-text p {
    margin: 0;
    color: #666;
    line-height: 1.5;
}

.deletion-info {
    margin-bottom: 2rem;
}

.deletion-info h4 {
    font-size: 1rem;
    color: #333;
    margin-bottom: 1rem;
}

.deletion-info ul {
    list-style-type: none;
    padding: 0;
    margin: 0;
}

.deletion-info li {
    margin-bottom: 0.75rem;
    padding-left: 1.5rem;
    position: relative;
    color: #555;
}

.deletion-info li i {
    position: absolute;
    left: 0;
    top: 0.2rem;
    color: #666;
}

.delete-form {
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

.form-input,
.password-input-group input {
    width: 100%;
    padding: 0.75rem 1rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 1rem;
}

.form-input:focus,
.password-input-group input:focus {
    border-color: #e53935;
    box-shadow: 0 0 0 2px rgba(229, 57, 53, 0.15);
    outline: none;
}

.password-input-group {
    position: relative;
    width: 100%;
}

.password-input-group input {
    padding-right: 3rem;
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

.error-message {
    background-color: #ffebee;
    color: #c62828;
    padding: 1rem;
    border-radius: 6px;
    margin: 1rem 0;
    font-size: 0.9rem;
}

.delete-account-button {
    background-color: #e53935;
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

.delete-account-button:hover:not(:disabled) {
    background-color: #d32f2f;
}

.delete-account-button:disabled {
    background-color: #e0e0e0;
    cursor: not-allowed;
}
</style>
