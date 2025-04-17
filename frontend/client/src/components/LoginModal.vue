<script setup>
import { ref } from "vue";
import { login, register, loginWithGoogle, getUserInfo } from "../services/authService.js";
import emitter from "../utils/evenBus.js";
import { useAuthStore } from "../stores/authStore.js";
import RegisterInfoForm from "./RegisterInfoForm.vue";

const authStore = useAuthStore();

// State
const loginPhoneNumber = ref("");
const loginPassword = ref("");

const registerPhoneNumber = ref("");
const registerPassword = ref("");
const registerPasswordConfirm = ref("");
const registerEmail = ref("");
const argeed = ref(false);

const isRightPanelActive = ref(false);
const showRegisterInfoForm = ref(false);
const registeredUser = ref(null);

// Emit
const emit = defineEmits(["close", "login-success"]);

// ===== REFRESH FORM ===== //
const resetFormRegister = () => {
    registerPhoneNumber.value = "";
    registerPassword.value = "";
    registerPasswordConfirm.value = "";
    registerEmail.value = "";
    argeed.value = false;
};

const resetFormLogin = () => {
    loginPhoneNumber.value = "";
    loginPassword.value = "";
};

// ===== XỬ LÝ LOGIN ===== //
const handleLogin = async () => {
    try {
        // Gửi yêu cầu đăng nhập
        const message = await login({
            phoneNumber: loginPhoneNumber.value,
            password: loginPassword.value,
        });
        

        emitter.emit("show-notification", {
            status: "success",
            message: "Đăng nhập thành công!",
        });

        emit("login-success");
        emit("close");
    } catch (err) {
        console.error(err);

        emitter.emit("show-notification", {
            status: "error",
            message: "Đăng nhập thất bại.",
        });
    }
};

// ===== XỬ LÝ ĐĂNG KÝ ===== //
const handleRegister = async () => {
    if (registerPassword.value !== registerPasswordConfirm.value) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Mật khẩu không khớp.",
        });
        return;
    }

    if (!argeed.value) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Bạn cần đồng ý với điều khoản sử dụng.",
        });
        return;
    }

    try {
        const userInfo = {
            phoneNumber: registerPhoneNumber.value,
            password: registerPassword.value,
            email: registerEmail.value,
            confirmPassword: registerPasswordConfirm.value,
        };

        const message = await register(userInfo);

        emitter.emit("show-notification", {
            status: "success",
            message: "Đăng ký thành công!",
        });
        
        const user = await getUserInfo();
        authStore.setUser(user.data); // Lưu thông tin người dùng vào store
        console.log("User info:", user.data);

        // Hiển thị form cập nhật thông tin
        showRegisterInfoForm.value = true;
    } catch (err) {
        emitter.emit("show-notification", {
            status: "error",
            message: err.response?.data?.message || "Đăng ký thất bại.",
        });
        console.error(err);
    }
};

// ===== XỬ LÝ SAU KHI CẬP NHẬT THÔNG TIN HOẶC BỎ QUA ===== //
const handleInfoFormClose = () => {
    showRegisterInfoForm.value = false;
    emit("login-success"); // Đánh dấu là người dùng đã đăng nhập
    emit("close"); // Đóng modal
};

// ===== LOGIN GOOGLE ===== //
const handleGoogleLogin = async (response) => {
    const credential = response.credential;

    try {
        const res = await loginWithGoogle(credential);

        emitter.emit("show-notification", {
            status: "success",
            message: "Đăng nhập bằng Google thành công!",
        });

        emit("login-success");
        emit("close");
    } catch (err) {
        console.error("Lỗi đăng nhập bằng Google:", err);

        emitter.emit("show-notification", {
            status: "error",
            message: "Đăng nhập bằng Google thất bại.",
        });
    }
};

// ===== FACEBOOK LOGIN (Tạm thời chưa dùng) ===== //
const handleFacebookLogin = () => {
    // TODO: Thêm xử lý đăng nhập Facebook nếu cần
};

// ===== HIỂN THỊ FORM ===== //
const showRegister = () => {
    isRightPanelActive.value = true;
    resetFormRegister();
};

const showLogin = () => {
    isRightPanelActive.value = false;
    resetFormLogin();
};
</script>

<template>
    <div class="modal-overlay" @click.self="$emit('close')">
        <div
            v-if="!showRegisterInfoForm"
            :class="['container', { 'right-panel-active': isRightPanelActive }]"
        >
            <!-- Đăng kí -->
            <div class="form-container register-container">
                <form action="#">
                    <h1>Đăng ký</h1>
                    <input
                        type="text"
                        placeholder="Số điện thoại"
                        v-model="registerPhoneNumber"
                    />
                    <input
                        type="email"
                        placeholder="Email"
                        v-model="registerEmail"
                    />
                    <input
                        type="password"
                        placeholder="Mật khẩu"
                        v-model="registerPassword"
                    />
                    <input
                        type="password"
                        placeholder="Xác nhận mật khẩu"
                        v-model="registerPasswordConfirm"
                    />

                    <div class="content">
                        <div class="checkbox">
                            <input
                                type="checkbox"
                                id="checkbox"
                                name="checkbox"
                                v-model="argeed"
                            />
                            <label for="checkbox" class="terms-label">
                                Đồng ý với <a href="#">Điều khoản Sử dụng</a> và
                                <a href="#">Chính Sách Bảo mật</a>
                            </label>
                        </div>
                    </div>

                    <button @click.prevent="handleRegister">Đăng ký</button>

                    <div class="separator"><span>Hoặc</span></div>

                    <div class="social-container">
                        <GoogleLogin
                            :callback="handleGoogleLogin"
                            :buttonConfig="{
                                type: 'icon',
                                size: 'large',
                                theme: 'outline',
                                text: 'continue_with',
                                shape: 'pill',
                                logo_alignment: 'center',
                            }"
                        />
                        <button
                            type="button"
                            class="facebook-login-btn"
                            @click="handleFacebookLogin"
                        >
                            <img
                                src="../assets/image/facebook.png"
                                alt="Facebook"
                            />
                        </button>
                    </div>
                </form>
            </div>

            <!-- Đăng nhập -->
            <div class="form-container login-container">
                <form acction="#">
                    <h1>Đăng nhập</h1>
                    <input
                        type="text"
                        placeholder="Số điện thoại"
                        v-model="loginPhoneNumber"
                    />
                    <input
                        type="password"
                        placeholder="Mật khẩu"
                        v-model="loginPassword"
                    />

                    <div class="content">
                        <div class="pass-link">
                            <a href="#">Quên mật khẩu?</a>
                        </div>
                    </div>

                    <button @click.prevent="handleLogin">Đăng nhập</button>

                    <div class="separator"><span>Hoặc</span></div>

                    <div class="social-container">
                        <GoogleLogin
                            :callback="handleGoogleLogin"
                            :buttonConfig="{
                                type: 'icon',
                                size: 'large',
                                theme: 'outline',
                                text: 'continue_with',
                                shape: 'pill',
                                logo_alignment: 'center',
                            }"
                        />
                        <button
                            type="button"
                            class="facebook-login-btn"
                            @click="handleFacebookLogin"
                        >
                            <img
                                src="../assets/image/facebook.png"
                                alt="Facebook"
                            />
                        </button>
                    </div>
                </form>
            </div>

            <div class="overlay-container">
                <div class="overlay">
                    <div class="overlay-panel overlay-left">
                        <h1 class="title">Xin chào</h1>
                        <p>Bạn đã có tài khoản? Đăng nhập ngay.</p>
                        <button class="ghost" @click="showLogin">
                            Đăng nhập
                        </button>
                    </div>
                    <div class="overlay-panel overlay-right">
                        <h1 class="title">Đến với chúng tôi</h1>
                        <p>Bạn đã sẵn sàng khám phá? Hãy tạo tài khoản ngay!</p>
                        <button class="ghost" @click="showRegister">
                            Đăng ký
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Form cập nhật thông tin sau khi đăng ký -->
        <RegisterInfoForm
            v-if="showRegisterInfoForm"
            :userInfo="authStore.user"
            @close="handleInfoFormClose"
            @update-success="handleInfoFormClose"
        />
    </div>
</template>

<style scoped>
/* Reset and global styles */
* {
    box-sizing: border-box;
}
body,
h1,
p,
span,
a,
button,
input,
label {
    font-family: "Poppins", sans-serif;
}

/* Overlay */
.modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    flex-direction: column;
    overflow: hidden;
}

/* Typography */
h1 {
    color: #333;
    font-weight: 600;
    letter-spacing: -1.5px;
    margin: 0 0 15px;
}
h1.title {
    color: #fff;
    font-size: 47px;
    line-height: 47px;
    text-shadow: 0 0 10px rgba(16, 64, 74, 0.5);
}

p {
    font-size: 14px;
    font-weight: 100;
    line-height: 20px;
    letter-spacing: 0.5px;
    margin: 20px 0 30px;
    text-shadow: 0 0 10px rgba(16, 64, 74, 0.5);
}

span {
    font-size: 14px;
    margin-top: 25px;
}

/* Links */
a {
    color: #333;
    font-size: 14px;
    text-decoration: none;
    margin: 15px 0;
    transition: 0.3s ease-in-out;
}
a:hover {
    color: #f86ed3;
}

.terms-label {
    font-size: 11px !important;
    color: #333;
}
.terms-label a {
    font-size: 11px !important;
    color: blue;
}
.terms-label a:hover,
.pass-link {
    text-align: right;
    width: 100%;
}

.pass-link a:hover {
    text-decoration: underline;
}
.pass-link a {
    color: blue;
    text-decoration: none;
}

/* Content section */
.content {
    display: flex;
    width: 100%;
    height: 50px;
    align-items: center;
    justify-content: space-between;
}
.content .checkbox {
    display: flex;
    align-items: center;
    justify-content: center;
    white-space: nowrap;
}
.content input {
    accent-color: #333;
    width: 12px;
    height: 12px;
}
.content label {
    font-size: 14px;
    user-select: none;
    padding-left: 5px;
}

/* Buttons */
button {
    position: relative;
    border-radius: 20px;
    border: 2px solid rgba(255, 148, 226, 1);
    background: rgba(255, 148, 226, 0.8);
    color: #fff;
    font-size: 15px;
    font-weight: bold;
    margin: 5px;
    padding: 12px 80px;
    text-transform: capitalize;
    transition: 0.3s ease-in-out;
}
button:hover {
    letter-spacing: 0.5px;
}
button:active {
    transform: scale(0.95);
}
button:focus {
    outline: none;
}

button.ghost {
    background-color: rgba(225, 225, 225, 0.2);
    border: 2px solid #fff;
    color: #fff;
}
button.ghost i {
    position: absolute;
    opacity: 1;
    transition: 0.3s ease-in-out;
}
button.ghost i.register {
    right: 70px;
}
button.ghost i.login {
    left: 70px;
}
button.ghost:hover i.register {
    right: 40px;
}
button.ghost:hover i.login {
    left: 40px;
}

/* Separator */
.separator {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 100%;
}
.separator span {
    background: white;
    padding: 0 5px;
    font-size: 16px;
    color: #dad7d7;
    margin: 10px 0;
}
.separator::before,
.separator::after {
    content: "";
    flex: 1;
    height: 1px;
    background: #dad7d7;
}

/* Social login */
.social-container {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 10px;
    transition: 0.3s ease-in-out;
}

.facebook-login-btn {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    border: 1px solid rgb(218, 220, 224);
    background-color: #fff;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: box-shadow 0.2s ease, transform 0.2s ease;
    padding: 0;
    text-indent: -9999px;
}
.facebook-login-btn:hover {
    border-color: #d2e3fc;
    background-color: #f8faff;
}
.facebook-login-btn img {
    width: 22px;
    height: 22px;
}

/* Form and container */
form {
    background-color: #fff;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-direction: column;
    padding: 0 50px;
    height: 100%;
    text-align: center;
}

input {
    background-color: rgba(255, 148, 226, 0.5);
    border-radius: 10px;
    border: none;
    padding: 12px 15px;
    margin: 8px 0;
    width: 100%;
}

/* Autofill fix */
input:-webkit-autofill {
    box-shadow: rgba(255, 148, 226, 0.5) !important;
    -webkit-text-fill-color: #000 !important;
    transition: background-color 5000s ease-in-out 0s;
}

/* Panel containers */
.container {
    margin-top: -100px;
    background-color: #fff;
    border-radius: 25px;
    box-shadow: 0 14px 28px rgba(0, 0, 0, 0.25), 0 10px 10px rgba(0, 0, 0, 0.22);
    position: relative;
    overflow: hidden;
    width: 768px;
    max-width: 100%;
    min-height: 500px;
}

.form-container {
    position: absolute;
    top: 0;
    height: 100%;
    transition: all 0.6s ease-in-out;
}
.login-container {
    left: 0;
    width: 50%;
    z-index: 2;
}
.register-container {
    left: 0;
    width: 50%;
    opacity: 0;
    z-index: 1;
}

.container.right-panel-active .login-container {
    transform: translateX(100%);
}
.container.right-panel-active .register-container {
    transform: translateX(100%);
    opacity: 1;
    z-index: 5;
    animation: show 0.6s;
}

@keyframes show {
    0%,
    49.99% {
        opacity: 0;
        z-index: 1;
    }
    50%,
    100% {
        opacity: 1;
        z-index: 5;
    }
}

/* Overlay effect */
.overlay-container {
    position: absolute;
    top: 0;
    left: 50%;
    width: 50%;
    height: 100%;
    overflow: hidden;
    transition: transform 0.6s ease-in-out;
    z-index: 100;
}
.container.right-panel-active .overlay-container {
    transform: translateX(-100%);
}

.overlay {
    background-image: url("../assets/gif/login1.gif");
    background-repeat: no-repeat;
    background-size: cover;
    background-position: 0 0;
    color: #fff;
    position: relative;
    left: -100%;
    width: 200%;
    height: 100%;
    transform: translateX(0);
    transition: transform 0.6s ease-in-out;
}
.overlay::before {
    content: "";
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: linear-gradient(
        to top,
        rgba(46, 94, 109, 0.4) 40%,
        rgba(46, 94, 109, 0)
    );
}
.container.container.right-panel-active .overlay {
    transform: translateX(50%);
}

.overlay-panel {
    position: absolute;
    top: 0;
    width: 50%;
    height: 100%;
    padding: 0 40px;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    text-align: center;
    transform: translateX(0);
    transition: transform 0.6s ease-in-out;
}
.overlay-left {
    transform: translateX(-20%);
}
.container.right-panel-active .overlay-left {
    transform: translateX(0);
}
.overlay-right {
    right: 0;
    transform: translateX(0);
}
.container.right-panel-active .overlay-right {
    transform: translateX(20%);
}
</style>
