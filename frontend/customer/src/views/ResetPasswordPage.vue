<script setup>
import { ref, onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import authService from "@/services/authService";
import emitter from "@/utils/evenBus";

const router = useRouter();
const route = useRoute();

// Lấy token và email từ tham số URL
const token = ref("");
const email = ref("");

// Form data
const password = ref("");
const confirmPassword = ref("");
const showPassword = ref(false);
const showConfirmPassword = ref(false);
const isLoading = ref(false);
const errorMessage = ref("");
const successMessage = ref("");

// Validation errors
const passwordError = ref("");
const confirmPasswordError = ref("");
const tokenValidationStatus = ref("validating"); // validating, valid, invalid

onMounted(() => {
    // Lấy token và email từ URL parameters
    token.value = route.query.token?.toString() || "";
    email.value = route.query.email?.toString() || "";

    if (!token.value || !email.value) {
        tokenValidationStatus.value = "invalid";
        errorMessage.value =
            "Liên kết không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu đặt lại mật khẩu mới.";
        return;
    }

    // Token và email hợp lệ
    tokenValidationStatus.value = "valid";
});

/**
 * Xử lý đặt lại mật khẩu
 */
const handleResetPassword = async () => {
    // Reset errors
    errorMessage.value = "";
    passwordError.value = "";
    confirmPasswordError.value = "";

    // Validate inputs
    let isValid = true;

    if (!password.value || password.value.length < 6) {
        passwordError.value = "Mật khẩu phải có ít nhất 6 ký tự";
        isValid = false;
    }

    if (password.value !== confirmPassword.value) {
        confirmPasswordError.value = "Mật khẩu xác nhận không khớp";
        isValid = false;
    }

    if (!isValid) return;

    try {
        isLoading.value = true;

        // Call reset password API
        await authService.resetPassword({
            token: token.value,
            email: email.value,
            password: password.value,
            confirmPassword: confirmPassword.value,
        });

        // Show success message
        successMessage.value =
            "Đặt lại mật khẩu thành công. Bạn có thể đăng nhập với mật khẩu mới.";

        // Clear form
        password.value = "";
        confirmPassword.value = "";
    } catch (error) {
        console.error("Error during password reset:", error);

        if (error.response) {
            const { data } = error.response;
            errorMessage.value = data.message || "Không thể đặt lại mật khẩu";
        } else {
            emitter.emit("show-notification", {
                status: "error",
                message: "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.",
            });
        }
    } finally {
        isLoading.value = false;
    }
};

/**
 * Chuyển đến trang đăng nhập
 */
const goToLogin = () => {
    router.push("/login");
};

/**
 * Chuyển đến trang quên mật khẩu
 */
const goToForgotPassword = () => {
    router.push("/forgot-password");
};
</script>

<template>
    <div class="reset-password-page">
        <div class="reset-container">
            <div class="logo-section">
                <div class="logo-text">SmartBuy Mobile</div>
                <h2 class="tagline">
                    Tạo mật khẩu mới để bảo vệ tài khoản của bạn
                </h2>
            </div>

            <div class="form-section">
                <div class="reset-card">
                    <!-- Hiển thị khi token invalid -->
                    <div
                        v-if="tokenValidationStatus === 'invalid'"
                        class="invalid-token"
                    >
                        <div class="error-icon">
                            <i class="fas fa-exclamation-triangle"></i>
                        </div>
                        <h2 class="error-title">Liên kết không hợp lệ</h2>
                        <p class="error-message">{{ errorMessage }}</p>
                        <div class="action-buttons">
                            <button
                                class="btn-back"
                                @click="goToForgotPassword"
                            >
                                Yêu cầu liên kết mới
                            </button>
                            <button class="btn-login" @click="goToLogin">
                                Đăng nhập
                            </button>
                        </div>
                    </div>

                    <!-- Hiển thị khi đang kiểm tra token -->
                    <div
                        v-else-if="tokenValidationStatus === 'validating'"
                        class="validating"
                    >
                        <div class="loading-spinner"></div>
                        <p>Đang xác thực...</p>
                    </div>

                    <!-- Hiển thị form khi token valid -->
                    <div v-else>
                        <!-- Hiển thị thành công -->
                        <div v-if="successMessage" class="success-container">
                            <div class="success-icon">
                                <i class="fas fa-check-circle"></i>
                            </div>
                            <h2 class="success-title">Thành công!</h2>
                            <p class="success-text">
                                {{ successMessage }}
                            </p>
                            <button class="btn-login-full" @click="goToLogin">
                                Đăng nhập ngay
                            </button>
                        </div>

                        <!-- Hiển thị form nhập mật khẩu mới -->
                        <form
                            v-else
                            @submit.prevent="handleResetPassword"
                            class="reset-form"
                        >
                            <h2 class="form-title">Đặt lại mật khẩu</h2>
                            <p class="instruction-text">
                                Vui lòng tạo mật khẩu mới cho tài khoản của bạn.
                            </p>

                            <div v-if="errorMessage" class="error-message">
                                {{ errorMessage }}
                            </div>

                            <div class="form-group">
                                <label for="password">Mật khẩu mới</label>
                                <div class="password-input">
                                    <i class="fas fa-lock input-icon"></i>
                                    <input
                                        :type="
                                            showPassword ? 'text' : 'password'
                                        "
                                        id="password"
                                        v-model="password"
                                        placeholder="Nhập mật khẩu mới (ít nhất 6 ký tự)"
                                        :class="{
                                            'input-error': passwordError,
                                        }"
                                        required
                                    />
                                    <button
                                        type="button"
                                        class="toggle-password"
                                        @click="showPassword = !showPassword"
                                    >
                                        <i
                                            :class="
                                                showPassword
                                                    ? 'fas fa-eye-slash'
                                                    : 'fas fa-eye'
                                            "
                                        ></i>
                                    </button>
                                </div>
                                <p class="error-text" v-if="passwordError">
                                    {{ passwordError }}
                                </p>
                            </div>

                            <div class="form-group">
                                <label for="confirmPassword"
                                    >Xác nhận mật khẩu</label
                                >
                                <div class="password-input">
                                    <i class="fas fa-lock input-icon"></i>
                                    <input
                                        :type="
                                            showConfirmPassword
                                                ? 'text'
                                                : 'password'
                                        "
                                        id="confirmPassword"
                                        v-model="confirmPassword"
                                        placeholder="Nhập lại mật khẩu mới"
                                        :class="{
                                            'input-error': confirmPasswordError,
                                        }"
                                        required
                                    />
                                    <button
                                        type="button"
                                        class="toggle-password"
                                        @click="
                                            showConfirmPassword =
                                                !showConfirmPassword
                                        "
                                    >
                                        <i
                                            :class="
                                                showConfirmPassword
                                                    ? 'fas fa-eye-slash'
                                                    : 'fas fa-eye'
                                            "
                                        ></i>
                                    </button>
                                </div>
                                <p
                                    class="error-text"
                                    v-if="confirmPasswordError"
                                >
                                    {{ confirmPasswordError }}
                                </p>
                            </div>

                            <button
                                type="submit"
                                class="btn-reset"
                                :disabled="isLoading"
                            >
                                {{
                                    isLoading
                                        ? "Đang xử lý..."
                                        : "Đặt lại mật khẩu"
                                }}
                            </button>

                            <div class="action-links">
                                <a
                                    href="#"
                                    @click.prevent="goToLogin"
                                    class="auth-link"
                                >
                                    Quay lại đăng nhập
                                </a>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.reset-password-page {
    min-height: 100vh;
    background-color: #f0f2f5;
    background-image: linear-gradient(
        to right bottom,
        #ffffff,
        #fdf3fa,
        #fce7f6,
        #fbdaf2,
        #f9ceee
    );
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0 1rem;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
        Helvetica, Arial, sans-serif;
}

.reset-container {
    display: flex;
    align-items: center;
    max-width: 980px;
    width: 100%;
    margin: 0 auto;
    padding: 20px 0;
}

/* Logo & Tagline Section */
.logo-section {
    flex: 1;
    margin-right: 32px;
    padding-right: 32px;
    display: flex;
    flex-direction: column;
    justify-content: center;
}

.logo-text {
    font-size: 42px;
    font-weight: bold;
    color: #f86ed3;
    margin-bottom: 20px;
    line-height: 1;
    letter-spacing: -0.5px;
}

.tagline {
    font-size: 28px;
    font-weight: normal;
    line-height: 32px;
    color: #1c1e21;
    padding-right: 20px;
}

/* Form Section */
.form-section {
    width: 396px;
}

.reset-card {
    background-color: #fff;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1), 0 8px 16px rgba(0, 0, 0, 0.1);
    padding: 1.5rem;
    margin-bottom: 28px;
}

.form-title {
    text-align: center;
    margin-bottom: 1rem;
    color: #f86ed3;
    font-size: 1.5rem;
}

.instruction-text {
    text-align: center;
    margin-bottom: 1.5rem;
    color: #606770;
    font-size: 15px;
}

label {
    display: block;
    color: #606770;
    font-size: 15px;
    margin-bottom: 8px;
}

.reset-form {
    display: flex;
    flex-direction: column;
}

.form-group {
    margin-bottom: 16px;
}

/* Input styles */
.input-container,
.password-input {
    position: relative;
    width: 100%;
}

.input-icon {
    position: absolute;
    top: 50%;
    left: 12px;
    transform: translateY(-50%);
    color: #888;
    font-size: 1rem;
}

input {
    width: 100%;
    padding: 14px 16px 14px 40px;
    font-size: 17px;
    border: 1px solid #dddfe2;
    border-radius: 6px;
    color: #1c1e21;
    height: 50px;
    line-height: 50px;
}

input:focus {
    border-color: #f86ed3;
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.2);
    outline: none;
}

.toggle-password {
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    background: none;
    border: none;
    color: #606770;
    cursor: pointer;
    font-size: 16px;
}

.btn-reset {
    background-color: #f86ed3;
    border: none;
    border-radius: 6px;
    font-size: 20px;
    line-height: 48px;
    padding: 0 16px;
    width: 100%;
    color: #fff;
    font-weight: bold;
    margin-top: 8px;
    cursor: pointer;
    transition: background-color 0.3s;
}

.btn-reset:hover {
    background-color: #e05cb6;
}

.btn-reset:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

.action-links {
    display: flex;
    justify-content: center;
    margin: 16px 0;
}

.auth-link {
    color: #f86ed3;
    font-size: 17px;
    text-decoration: none;
}

.auth-link:hover {
    text-decoration: underline;
}

.error-message {
    background-color: #ffebee;
    color: #d32f2f;
    padding: 12px;
    border-radius: 6px;
    margin-bottom: 16px;
    font-size: 14px;
    text-align: center;
}

.error-text {
    color: #d32f2f;
    font-size: 12px;
    margin: 4px 0 0 4px;
}

.input-error {
    border-color: #d32f2f;
    border-width: 2px;
}

/* Success style */
.success-container {
    text-align: center;
    padding: 10px;
}

.success-icon {
    color: white;
    width: 70px;
    height: 70px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 35px;
    margin: 0 auto 25px;
    background: linear-gradient(135deg, #ff82c4, #ff5db3);
}

.success-title {
    color: #333;
    margin-bottom: 15px;
}

.success-text {
    color: #555;
    margin-bottom: 20px;
    line-height: 1.5;
}

.btn-login-full {
    width: 100%;
    padding: 14px 16px;
    font-size: 17px;
    background-color: #f86ed3;
    color: white;
    border: none;
    border-radius: 6px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.3s;
}

.btn-login-full:hover {
    background-color: #e05cb6;
}

/* Invalid token style */
.invalid-token {
    text-align: center;
    padding: 10px;
}

.error-icon {
    color: white;
    width: 70px;
    height: 70px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 35px;
    margin: 0 auto 25px;
    background: linear-gradient(135deg, #ff6b6b, #ff5252);
}

.error-title {
    color: #333;
    margin-bottom: 15px;
}

.action-buttons {
    display: flex;
    gap: 12px;
    margin-top: 20px;
}

.btn-back,
.btn-login {
    padding: 14px 16px;
    font-size: 16px;
    border-radius: 6px;
    border: none;
    flex: 1;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.3s;
}

.btn-back {
    background-color: #f0f0f0;
    color: #555;
}

.btn-back:hover {
    background-color: #e0e0e0;
}

.btn-login {
    background-color: #f86ed3;
    color: white;
}

.btn-login:hover {
    background-color: #e05cb6;
}

/* Loading spinner */
.validating {
    text-align: center;
    padding: 30px 10px;
}

.loading-spinner {
    display: inline-block;
    width: 50px;
    height: 50px;
    border: 4px solid rgba(248, 110, 211, 0.3);
    border-radius: 50%;
    border-top-color: #f86ed3;
    animation: spin 1s ease-in-out infinite;
    margin-bottom: 15px;
}

@keyframes spin {
    to {
        transform: rotate(360deg);
    }
}

/* Responsive */
@media (max-width: 900px) {
    .reset-container {
        flex-direction: column;
        text-align: center;
    }

    .logo-section {
        margin-right: 0;
        margin-bottom: 32px;
        padding-right: 0;
    }

    .logo-text {
        margin-left: auto;
        margin-right: auto;
    }

    .form-section {
        width: 100%;
        max-width: 396px;
    }
}

@media (max-width: 480px) {
    .reset-card {
        box-shadow: none;
        background-color: transparent;
        padding: 0;
    }

    .logo-text {
        font-size: 32px;
    }

    .tagline {
        font-size: 20px;
        line-height: 26px;
    }
}
</style>
