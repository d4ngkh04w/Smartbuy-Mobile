<script setup>
import { ref } from "vue";
import { forgotPassword } from "../services/authService.js";
import emitter from "../utils/evenBus.js";

// State
const email = ref("");
const isSubmitting = ref(false);
const showSuccessMessage = ref(false);
const errorMessage = ref("");

// Emit
const emit = defineEmits(["close", "back-to-login"]);

// Xử lý form quên mật khẩu
const handleForgotPassword = async () => {
    // Validate email
    if (!email.value || !isValidEmail(email.value)) {
        errorMessage.value = "Vui lòng nhập một địa chỉ email hợp lệ";
        return;
    }

    try {
        isSubmitting.value = true;
        errorMessage.value = "";

        // Gọi API quên mật khẩu
        await forgotPassword(email.value);

        // Hiển thị thông báo thành công
        showSuccessMessage.value = true;
    } catch (err) {
        console.error("Lỗi gửi yêu cầu đặt lại mật khẩu:", err);
        errorMessage.value = "Có lỗi xảy ra. Vui lòng thử lại sau.";

        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể gửi email đặt lại mật khẩu.",
        });
    } finally {
        isSubmitting.value = false;
    }
};

// Kiểm tra email hợp lệ
const isValidEmail = (email) => {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailRegex.test(email);
};

// Quay lại màn hình đăng nhập
const backToLogin = () => {
    emit("back-to-login");
};
</script>

<template>
    <div class="forgot-password-container">
        <form @submit.prevent="handleForgotPassword" v-if="!showSuccessMessage">
            <h1>Quên mật khẩu</h1>

            <p class="description">
                Nhập địa chỉ email của bạn và chúng tôi sẽ gửi cho bạn liên kết
                để đặt lại mật khẩu.
            </p>

            <div class="input-group">
                <input
                    type="email"
                    v-model="email"
                    placeholder="Nhập email của bạn"
                    required
                />
                <p class="error-message" v-if="errorMessage">
                    {{ errorMessage }}
                </p>
            </div>

            <div class="button-group">
                <button type="button" class="back-btn" @click="backToLogin">
                    Trở lại
                </button>

                <button
                    type="submit"
                    class="submit-btn"
                    :disabled="isSubmitting"
                >
                    {{ isSubmitting ? "Đang gửi..." : "Gửi" }}
                </button>
            </div>
        </form>

        <!-- Thông báo thành công -->
        <div class="success-container" v-else>
            <div class="success-icon">✓</div>
            <h2>Đã gửi email</h2>
            <p>
                Chúng tôi đã gửi email hướng dẫn đặt lại mật khẩu đến địa chỉ
                {{ email }}. Vui lòng kiểm tra hộp thư đến của bạn và làm theo
                hướng dẫn.
            </p>
            <p class="note">
                Nếu bạn không nhận được email trong vòng vài phút, vui lòng kiểm
                tra thư mục spam hoặc thử lại.
            </p>

            <div class="button-group">
                <button class="close-btn" @click="$emit('close')">Đóng</button>
            </div>
        </div>
    </div>
</template>

<style scoped>
.forgot-password-container {
    background-color: white;
    padding: 30px;
    border-radius: 10px;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
    width: 100%;
    max-width: 400px;
}

h1 {
    color: #ff5db3;
    margin: 0 0 20px;
    text-align: center;
    font-size: 24px;
}

.description {
    font-size: 14px;
    color: #666;
    margin-bottom: 20px;
    text-align: center;
}

.input-group {
    margin-bottom: 20px;
}

input {
    width: 100%;
    padding: 12px;
    margin: 10px 0;
    border-radius: 5px;
    border: 1px solid #ddd;
    font-size: 14px;
    outline: none;
    transition: border-color 0.3s;
}

input:focus {
    border-color: #ff82c4;
}

.button-group {
    display: flex;
    gap: 10px;
    margin-top: 20px;
}

button {
    padding: 12px 20px;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    font-weight: 600;
    font-size: 14px;
    transition: all 0.3s ease;
    flex: 1;
}

button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

.back-btn {
    background-color: #f0f0f0;
    color: #555;
}

.back-btn:hover {
    background-color: #e0e0e0;
}

.submit-btn,
.close-btn {
    background: linear-gradient(135deg, #ff82c4, #ff5db3);
    color: white;
}

.submit-btn:hover,
.close-btn:hover {
    background: linear-gradient(135deg, #ff5db3, #ff82c4);
    transform: translateY(-2px);
    box-shadow: 0 4px 10px rgba(255, 93, 179, 0.3);
}

.error-message {
    color: #ff3860;
    font-size: 12px;
    margin: 5px 0 0;
}

/* Success message styling */
.success-container {
    text-align: center;
    padding: 10px;
}

.success-icon {
    background: linear-gradient(135deg, #ff82c4, #ff5db3);
    color: white;
    width: 60px;
    height: 60px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 30px;
    margin: 0 auto 20px;
}

h2 {
    color: #ff5db3;
    margin-bottom: 15px;
}

.note {
    font-size: 13px;
    color: #888;
    margin-top: 20px;
    font-style: italic;
}
</style>
