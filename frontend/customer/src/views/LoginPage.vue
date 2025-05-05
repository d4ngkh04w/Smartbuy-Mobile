<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import authService from "@/services/authService";
import emitter from "@/utils/evenBus";

const phoneNumber = ref("");
const password = ref("");
const showPassword = ref(false);
const isLoading = ref(false);
const errorMessage = ref("");
const router = useRouter();

const handleLogin = async () => {
    if (!validatePhoneNumber(phoneNumber.value)) {
        errorMessage.value =
            "Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại 10 số bắt đầu bằng 0.";
        return;
    }

    try {
        isLoading.value = true;
        errorMessage.value = "";

        await authService.login(phoneNumber.value, password.value);

        // If login is successful, navigate to home page
        router.push("/");
    } catch (error) {
        console.error("Error logging in:", error);

        // Handle different error responses
        if (error.response) {
            const { status, data } = error.response;

            if (status === 401) {
                errorMessage.value =
                    data.message ||
                    "Số điện thoại hoặc mật khẩu không chính xác";
            } else if (status === 403) {
                errorMessage.value =
                    "Tài khoản của bạn không có quyền truy cập";
            } else if (data && data.message) {
                errorMessage.value = data.message;
            } else {
                errorMessage.value =
                    "Đăng nhập không thành công. Vui lòng thử lại.";
            }
        } else {
            errorMessage.value =
                "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.";
        }
    } finally {
        isLoading.value = false;
    }
};

const loginWithGoogle = () => {
    console.log("Login with Google");
    // Thực hiện đăng nhập với Google
};

const validatePhoneNumber = (phone) => {
    // Kiểm tra số điện thoại hợp lệ (10 số và bắt đầu bằng 0)
    const phoneRegex = /^0\d{9}$/;
    return phoneRegex.test(phone);
};
</script>

<template>
    <div class="login-page">
        <div class="login-container">
            <div class="logo-section">
                <div class="logo-text">SmartBuy Mobile</div>
                <h2 class="tagline">
                    SmartBuy Mobile – Mua smartphone chính hãng, giá tốt, giao
                    nhanh!
                </h2>
            </div>

            <div class="form-section">
                <div class="login-card">
                    <form @submit.prevent="handleLogin" class="login-form">
                        <div v-if="errorMessage" class="error-message">
                            {{ errorMessage }}
                        </div>

                        <div class="form-group">
                            <input
                                type="text"
                                id="phoneNumber"
                                v-model="phoneNumber"
                                placeholder="Số điện thoại"
                                required
                            />
                        </div>

                        <div class="form-group">
                            <div class="password-input">
                                <input
                                    :type="showPassword ? 'text' : 'password'"
                                    id="password"
                                    v-model="password"
                                    placeholder="Mật khẩu"
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
                        </div>

                        <button
                            type="submit"
                            class="btn-login"
                            :disabled="isLoading"
                        >
                            {{ isLoading ? "Đang đăng nhập..." : "Đăng nhập" }}
                        </button>

                        <div class="login-links">
                            <router-link
                                to="/forgot-password"
                                class="auth-link"
                            >
                                Quên mật khẩu?
                            </router-link>
                            <router-link to="/register" class="auth-link">
                                Đăng ký
                            </router-link>
                        </div>

                        <div class="divider">
                            <span>hoặc</span>
                        </div>

                        <button
                            type="button"
                            class="btn-google"
                            @click="loginWithGoogle"
                        >
                            <div class="google-icon-wrapper">
                                <img
                                    class="google-icon"
                                    src="https://upload.wikimedia.org/wikipedia/commons/5/53/Google_%22G%22_Logo.svg"
                                    alt="Google logo"
                                />
                            </div>
                            <span class="btn-google-text"
                                >Đăng nhập với Google</span
                            >
                        </button>
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.login-page {
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

.login-container {
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

.login-card {
    background-color: #fff;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1), 0 8px 16px rgba(0, 0, 0, 0.1);
    padding: 1.5rem;
    margin-bottom: 28px;
}

.login-form {
    display: flex;
    flex-direction: column;
}

.form-group {
    margin-bottom: 12px;
}

input {
    width: 100%;
    padding: 14px 16px;
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

.password-input {
    position: relative;
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

.btn-login {
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

.btn-login:hover {
    background-color: #e05cb6;
}

.btn-login:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

.login-links {
    display: flex;
    justify-content: space-between;
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

.divider {
    display: flex;
    align-items: center;
    margin: 20px 0;
}

.divider::before,
.divider::after {
    content: "";
    flex: 1;
    height: 1px;
    background-color: #dadde1;
}

.divider span {
    color: #606770;
    font-size: 14px;
    margin: 0 16px;
}

/* Google Button Styling */
.btn-google {
    display: flex;
    align-items: center;
    background-color: white;
    border: 1px solid #dadce0;
    border-radius: 4px;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
    padding: 0;
    height: 46px;
    width: 100%;
    cursor: pointer;
    transition: all 0.2s;
    margin-bottom: 8px;
    position: relative;
    overflow: hidden;
}

.btn-google:hover {
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.15);
    background-color: #f8f8f8;
}

.btn-google:active {
    background-color: #f1f1f1;
}

.google-icon-wrapper {
    display: flex;
    justify-content: center;
    align-items: center;
    width: 46px;
    height: 46px;
    background-color: white;
    border-radius: 2px;
}

.google-icon {
    width: 20px;
    height: 20px;
}

.btn-google-text {
    flex: 1;
    text-align: center;
    font-family: "Roboto", sans-serif;
    font-size: 16px;
    font-weight: 500;
    color: #3c4043;
    margin-right: 46px;
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

/* Responsive */
@media (max-width: 900px) {
    .login-container {
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
    .login-card {
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
