<script setup>
import { ref, onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { resetPassword } from "../services/authService";
import emitter from "../utils/evenBus.js";

const router = useRouter();
const route = useRoute();

// Tham số từ URL
const token = ref("");
const email = ref("");

// Form state
const password = ref("");
const confirmPassword = ref("");
const isSubmitting = ref(false);
const resetSuccess = ref(false);
const errorMessage = ref("");

// Lấy token và email từ query parameters
onMounted(() => {
    token.value = route.query.token || "";
    email.value = route.query.email || "";

    // Kiểm tra token và email
    if (!token.value || !email.value) {
        errorMessage.value =
            "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
    }
});

// Xử lý đặt lại mật khẩu
const handleResetPassword = async () => {
    // Validate
    if (password.value !== confirmPassword.value) {
        errorMessage.value = "Mật khẩu và xác nhận mật khẩu không khớp";
        return;
    }

    if (password.value.length < 6) {
        errorMessage.value = "Mật khẩu phải có ít nhất 6 ký tự";
        return;
    }

    isSubmitting.value = true;
    errorMessage.value = "";

    try {
        // Gọi API đặt lại mật khẩu
        await resetPassword({
            token: token.value,
            email: email.value,
            password: password.value,
            confirmPassword: confirmPassword.value,
        });

        // Đặt lại thành công
        resetSuccess.value = true;

        // Thông báo thành công
        emitter.emit("show-notification", {
            status: "success",
            message: "Mật khẩu của bạn đã được đặt lại thành công!",
        });

        // Sau 3 giây, chuyển hướng về trang chủ
        setTimeout(() => {
            router.push("/");
        }, 3000);
    } catch (err) {
        console.error("Lỗi đặt lại mật khẩu:", err);

        // Hiển thị lỗi cụ thể từ server nếu có
        if (err.response?.data?.message) {
            errorMessage.value = err.response.data.message;
        } else {
            errorMessage.value =
                "Đã có lỗi xảy ra khi đặt lại mật khẩu. Vui lòng thử lại sau.";
        }

        emitter.emit("show-notification", {
            status: "error",
            message: "Đặt lại mật khẩu thất bại.",
        });
    } finally {
        isSubmitting.value = false;
    }
};

// Chuyển về trang chủ
const goToHomePage = () => {
    router.push("/");
};
</script>

<template>
    <div class="reset-password-page">
        <div class="reset-password-container">
            <div v-if="!resetSuccess">
                <!-- Form đặt lại mật khẩu -->
                <div class="form-header">
                    <!-- <div class="logo-container">
                        <img src="../assets/image/logo.png" alt="SmartBuy Logo" class="logo" />
                    </div> -->
                    <h1>Đặt lại mật khẩu</h1>
                    <p class="subtitle">
                        Tạo mật khẩu mới cho tài khoản của bạn
                    </p>
                </div>

                <div class="error-display" v-if="errorMessage">
                    <p>{{ errorMessage }}</p>
                </div>

                <!-- Form nhập mật khẩu mới -->
                <form
                    @submit.prevent="handleResetPassword"
                    v-if="token && email && !errorMessage"
                >
                    <div class="form-group">
                        <label for="password">Mật khẩu mới</label>
                        <div class="password-input">
                            <input
                                type="password"
                                id="password"
                                v-model="password"
                                placeholder="Nhập mật khẩu mới"
                                required
                            />
                        </div>
                    </div>

                    <div class="form-group">
                        <label for="confirmPassword">Xác nhận mật khẩu</label>
                        <div class="password-input">
                            <input
                                type="password"
                                id="confirmPassword"
                                v-model="confirmPassword"
                                placeholder="Nhập lại mật khẩu mới"
                                required
                            />
                        </div>
                    </div>

                    <button
                        type="submit"
                        class="reset-button"
                        :disabled="isSubmitting"
                    >
                        {{
                            isSubmitting ? "Đang xử lý..." : "Đặt lại mật khẩu"
                        }}
                    </button>
                </form>

                <!-- Thông báo lỗi token không hợp lệ -->
                <div class="invalid-link" v-else-if="errorMessage">                
                    <button class="home-button" @click="goToHomePage">
                        Về trang chủ
                    </button>
                </div>
            </div>

            <!-- Thông báo đặt lại mật khẩu thành công -->
            <div v-else class="success-message">
                <div class="success-icon">✓</div>
                <h2>Đặt lại mật khẩu thành công!</h2>
                <p>Mật khẩu của bạn đã được thay đổi thành công.</p>
                <p class="redirect-message">
                    Bạn sẽ được chuyển hướng đến trang chủ trong vài giây...
                </p>
                <button class="home-button" @click="goToHomePage">
                    Về trang chủ ngay
                </button>
            </div>
        </div>
    </div>
</template>

<style scoped>
.reset-password-page {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 71vh;
    background-color: #f8f0f4;
    padding: 20px;
}

.reset-password-container {
    background-color: white;
    border-radius: 10px;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
    padding: 30px;
    width: 100%;
    max-width: 450px;
}

.form-header {
    text-align: center;
    margin-bottom: 25px;
}

.logo-container {
    margin-bottom: 20px;
}

.logo {
    width: 208px;
    height: 59px;
    object-fit: cover;
}

h1 {
    color: #ff5db3;
    margin: 0 0 10px;
    font-size: 28px;
    font-weight: 600;
}

.subtitle {
    color: #666;
    font-size: 14px;
    margin: 0 0 20px;
}

.form-group {
    margin-bottom: 20px;
}

label {
    display: block;
    margin-bottom: 8px;
    font-weight: 500;
    color: #333;
}

input {
    width: 100%;
    padding: 12px 15px;
    border: 1px solid #ddd;
    border-radius: 5px;
    font-size: 14px;
    transition: border-color 0.3s;
}

input:focus {
    outline: none;
    border-color: #ff82c4;
}

.password-input {
    position: relative;
}

.error-display {
    background-color: #fff8f8;
    border-left: 4px solid #ff3860;
    color: #ff3860;
    padding: 10px 15px;
    margin-bottom: 20px;
    font-size: 14px;
    border-radius: 4px;
}

.reset-button {
    width: 100%;
    padding: 14px;
    background: linear-gradient(135deg, #ff82c4 0%, #ff5db3 100%);
    color: white;
    border: none;
    border-radius: 5px;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.3s ease;
    margin-top: 10px;
}

.reset-button:hover {
    background: linear-gradient(135deg, #ff5db3 0%, #ff82c4 100%);
    transform: translateY(-2px);
    box-shadow: 0 4px 10px rgba(255, 93, 179, 0.3);
}

.reset-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
    transform: none;
    box-shadow: none;
}

.home-button {
    padding: 12px 25px;
    background: #f0f0f0;
    color: #555;
    border: none;
    border-radius: 5px;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.3s ease;
    margin-top: 20px;
    display: inline-block;
}

.home-button:hover {
    background: #e0e0e0;
}

.invalid-link {
    text-align: center;
    padding: 20px;
}

.invalid-link p {
    color: #ff3860;
    margin-bottom: 20px;
}

/* Success message styling */
.success-message {
    text-align: center;
    padding: 20px 0;
}

.success-icon {
    background: linear-gradient(135deg, #ff82c4, #ff5db3);
    color: white;
    width: 70px;
    height: 70px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 35px;
    margin: 0 auto 25px;
}

h2 {
    color: #ff5db3;
    margin-bottom: 15px;
}

.redirect-message {
    font-size: 13px;
    color: #888;
    font-style: italic;
    margin-top: 15px;
    margin-bottom: 20px;
}
</style>
