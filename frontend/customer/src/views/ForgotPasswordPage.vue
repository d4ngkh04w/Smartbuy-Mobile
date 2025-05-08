<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import authService from "@/services/authService";
import emitter from "@/utils/evenBus";

const router = useRouter();
const email = ref("");
const isLoading = ref(false);
const errorMessage = ref("");
const showSuccessMessage = ref(false);

/**
 * Xử lý quên mật khẩu
 */
const handleForgotPassword = async () => {
    // Reset error
    errorMessage.value = "";

    // Validate email
    if (!isValidEmail(email.value)) {
        errorMessage.value = "Vui lòng nhập một địa chỉ email hợp lệ";
        return;
    }

    try {
        isLoading.value = true;

        // Call forgot password API
        await authService.forgotPassword({ email: email.value });

        // Show success message
        showSuccessMessage.value = true;
    } catch (error) {
        console.error("Error during password reset request:", error);

        if (error.response) {
            const { data } = error.response;
            errorMessage.value =
                data.message || "Không thể gửi yêu cầu đặt lại mật khẩu";
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
 * Check if email is valid
 */
const isValidEmail = (email) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
};

/**
 * Quay lại màn hình đăng nhập
 */
const backToLogin = () => {
    router.push("/login");
};
</script>

<template>
    <div class="forgot-password-page">
        <div class="forgot-container">
            <div class="logo-section">
                <div class="logo-text">SmartBuy Mobile</div>
                <h2 class="tagline">
                    Quên mật khẩu? Đừng lo lắng, chúng tôi sẽ giúp bạn lấy lại
                    quyền truy cập!
                </h2>
            </div>

            <div class="form-section">
                <div class="forgot-card">
                    <div v-if="!showSuccessMessage">
                        <h2 class="form-title">Quên mật khẩu</h2>
                        <p class="instruction-text">
                            Nhập địa chỉ email của bạn và chúng tôi sẽ gửi cho
                            bạn liên kết để đặt lại mật khẩu.
                        </p>

                        <div v-if="errorMessage" class="error-message">
                            {{ errorMessage }}
                        </div>

                        <form
                            @submit.prevent="handleForgotPassword"
                            class="forgot-form"
                        >
                            <div class="form-group">
                                <div class="input-container">
                                    <i class="fas fa-envelope input-icon"></i>
                                    <input
                                        type="email"
                                        id="email"
                                        v-model="email"
                                        placeholder="Nhập địa chỉ email"
                                        required
                                    />
                                </div>
                            </div>

                            <div class="form-actions">
                                <button
                                    type="button"
                                    class="btn-back"
                                    @click="backToLogin"
                                >
                                    Quay lại
                                </button>
                                <button
                                    type="submit"
                                    class="btn-send"
                                    :disabled="isLoading"
                                >
                                    {{
                                        isLoading
                                            ? "Đang gửi..."
                                            : "Gửi yêu cầu"
                                    }}
                                </button>
                            </div>
                        </form>
                    </div>

                    <div v-else class="success-container">
                        <div class="success-icon">
                            <i class="fas fa-check-circle"></i>
                        </div>
                        <h2 class="success-title">Đã gửi yêu cầu</h2>
                        <p class="success-text">
                            Chúng tôi đã gửi hướng dẫn đặt lại mật khẩu đến địa
                            chỉ email
                            <span class="email-highlight">{{ email }}</span
                            >. Vui lòng kiểm tra hộp thư đến của bạn và làm theo
                            hướng dẫn.
                        </p>
                        <p class="note">
                            Nếu bạn không nhận được email trong vòng vài phút,
                            vui lòng kiểm tra thư mục spam hoặc thử lại.
                        </p>
                        <button class="btn-back-full" @click="backToLogin">
                            Quay lại đăng nhập
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.forgot-password-page {
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

.forgot-container {
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

.forgot-card {
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

.forgot-form {
    display: flex;
    flex-direction: column;
}

.form-group {
    margin-bottom: 16px;
}

/* Input styles */
.input-container {
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

.input-container input {
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

.form-actions {
    display: flex;
    gap: 12px;
    margin-top: 16px;
}

.btn-back,
.btn-send {
    padding: 14px 16px;
    border-radius: 6px;
    font-size: 17px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.3s ease;
    flex: 1;
    border: none;
}

.btn-back {
    background-color: #f0f0f0;
    color: #555;
}

.btn-back:hover {
    background-color: #e0e0e0;
}

.btn-send {
    background-color: #f86ed3;
    color: white;
}

.btn-send:hover {
    background-color: #e05cb6;
}

.btn-send:disabled {
    opacity: 0.7;
    cursor: not-allowed;
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

/* Success Message Styling */
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

.email-highlight {
    font-weight: bold;
    color: #333;
}

.note {
    font-size: 14px;
    color: #888;
    margin: 20px 0;
    font-style: italic;
}

.btn-back-full {
    width: 100%;
    padding: 14px 16px;
    border: none;
    border-radius: 6px;
    background-color: #f0f0f0;
    color: #555;
    font-size: 17px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.3s ease;
    margin-top: 10px;
}

.btn-back-full:hover {
    background-color: #e0e0e0;
}

/* Responsive */
@media (max-width: 900px) {
    .forgot-container {
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
    .forgot-card {
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
