<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import emitter from "../utils/evenBus.js";
import authService  from "../services/authService.js";

const router = useRouter();

// Login state
const phoneNumber = ref("");
const password = ref("");
const showPassword = ref(false);
const isSubmitting = ref(false);
const phoneNumberError = ref("");

// Forgot password state
const showForgotPasswordForm = ref(false);
const email = ref("");
const forgotPasswordError = ref("");
const showForgotPasswordSuccess = ref(false);
const isSubmittingForgotPassword = ref(false);

// Thêm kiểm tra khi trang login được tải

/**
 * Xử lý đăng nhập
 */
const login = async () => {
	// Reset error
	phoneNumberError.value = "";

	// Validate phone number
	if (!isValidPhoneNumber(phoneNumber.value)) {
		emitter.emit("show-notification", {
			status: "error",
			message: "Số điện thoại không hợp lệ",
		});
		return;
	}

	if (!password.value) {
		emitter.emit("show-notification", {
			status: "error",
			message: "Vui lòng nhập mật khẩu",
		});
		return;
	}

	try {
		isSubmitting.value = true;

		// Gọi API đăng nhập
		const test = await authService.login(phoneNumber.value, password.value);
		emitter.emit("show-notification", {
			status: "success",
			message: "Đăng nhập thành công",
		});

		// Reset form
		phoneNumber.value = "";
		password.value = "";
		showPassword.value = false;

		// Chuyển đến dashboard
		router.replace("/dashboard");
	} catch (error) {
		emitter.emit("show-notification", {
			status: "error",
			message: "Đăng nhập thất bại",
		});
	} finally {
		isSubmitting.value = false;
	}
};

/**
 * Validate số điện thoại (chỉ cho phép số và giới hạn 10 chữ số)
 */
const validatePhoneInput = (event) => {
	const input = event.target.value.replace(/\D/g, "").slice(0, 10);
	phoneNumber.value = input;
	phoneNumberError.value =
		input.length != 10 ? "Số điện thoại không hợp lệ" : "";
};

/**
 * Kiểm tra số điện thoại có hợp lệ không (10 chữ số)
 */
const isValidPhoneNumber = (phone) => {
	return /^[0-9]{10}$/.test(phone);
};

/**
 * Xử lý quên mật khẩu
 */
const handleForgotPassword = async () => {
	// Validate email
	if (!email.value || !isValidEmail(email.value)) {
		forgotPasswordError.value = "Vui lòng nhập một địa chỉ email hợp lệ";
		return;
	}

	try {
		isSubmittingForgotPassword.value = true;
		forgotPasswordError.value = "";

		// Gọi API quên mật khẩu
		await authService.forgotPassword({ email: email.value });

		// Hiển thị thông báo thành công
		showForgotPasswordSuccess.value = true;
	} catch (err) {
		console.error("Lỗi gửi yêu cầu đặt lại mật khẩu:", err);
		forgotPasswordError.value =
			err.response?.data?.message ||
			"Có lỗi xảy ra. Vui lòng thử lại sau.";

		emitter.emit("show-notification", {
			status: "error",
			message: "Không thể gửi email đặt lại mật khẩu.",
		});
	} finally {
		isSubmittingForgotPassword.value = false;
	}
};

/**
 * Hiển thị form quên mật khẩu
 */
const showForgotPassword = () => {
	showForgotPasswordForm.value = true;
};

/**
 * Quay lại form đăng nhập
 */
const backToLogin = () => {
	showForgotPasswordForm.value = false;
	showForgotPasswordSuccess.value = false;
	email.value = "";
	forgotPasswordError.value = "";
};

/**
 * Kiểm tra email có hợp lệ không
 */
const isValidEmail = (email) => {
	const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
	return emailRegex.test(email);
};
</script>

<template>
	<div class="login-container">
		<!-- Login Form -->
		<div v-if="!showForgotPasswordForm" class="login-box">
			<div class="logo-section">
				<img
					src="../assets/images/logo.png"
					alt="Smartbuy Logo"
					class="logo"
				/>
				<h1>Đăng nhập</h1>
			</div>

			<div class="form-group">
				<label for="phoneNumber">Số điện thoại</label>
				<div class="input-container">
					<i class="fas fa-phone input-icon"></i>
					<input
						id="phoneNumber"
						type="tel"
						v-model="phoneNumber"
						placeholder="Nhập số điện thoại (10 số)"
						maxlength="10"
						@input="validatePhoneInput"
						:class="{ 'input-error': phoneNumberError }"
					/>
				</div>
				<p class="error-message" v-if="phoneNumberError">
					{{ phoneNumberError }}
				</p>
			</div>

			<div class="form-group">
				<label for="password">Mật khẩu</label>
				<div class="password-input-container">
					<i class="fas fa-lock input-icon"></i>
					<input
						id="password"
						:type="showPassword ? 'text' : 'password'"
						v-model="password"
						placeholder="Nhập mật khẩu"
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

			<div class="forgot-password-link">
				<a href="#" @click.prevent="showForgotPassword"
					>Quên mật khẩu?</a
				>
			</div>

			<button
				class="login-button"
				@click="login"
				:disabled="isSubmitting"
			>
				{{ isSubmitting ? "Đang xử lý..." : "Đăng nhập" }}
			</button>
		</div>

		<!-- Forgot Password Form -->
		<div v-else class="login-box forgot-password-box">
			<div v-if="!showForgotPasswordSuccess">
				<div class="logo-section">
					<img
						src="../assets/images/logo.png"
						alt="Smartbuy Logo"
						class="logo"
					/>
					<h1>Quên mật khẩu</h1>
				</div>

				<p class="forgot-password-description">
					Nhập địa chỉ email của bạn và chúng tôi sẽ gửi cho bạn liên
					kết để đặt lại mật khẩu.
				</p>

				<div class="form-group">
					<label for="email">Email</label>
					<input
						id="email"
						type="email"
						v-model="email"
						placeholder="Nhập email của bạn"
					/>
					<p class="error-message" v-if="forgotPasswordError">
						{{ forgotPasswordError }}
					</p>
				</div>

				<div class="button-group">
					<button class="back-button" @click="backToLogin">
						Trở lại
					</button>
					<button
						class="send-button"
						@click="handleForgotPassword"
						:disabled="isSubmittingForgotPassword"
					>
						{{ isSubmittingForgotPassword ? "Đang gửi..." : "Gửi" }}
					</button>
				</div>
			</div>

			<!-- Success Message -->
			<div v-else class="success-container">
				<div class="success-icon">✓</div>
				<h2>Đã gửi email</h2>
				<p>
					Chúng tôi đã gửi email hướng dẫn đặt lại mật khẩu đến địa
					chỉ
					{{ email }}. Vui lòng kiểm tra hộp thư đến của bạn và làm
					theo hướng dẫn.
				</p>
				<p class="note">
					Nếu bạn không nhận được email trong vòng vài phút, vui lòng
					kiểm tra thư mục spam hoặc thử lại.
				</p>

				<button class="back-button full-width" @click="backToLogin">
					Quay lại đăng nhập
				</button>
			</div>
		</div>
	</div>
</template>

<style scoped>
.login-container {
	display: flex;
	justify-content: center;
	align-items: center;
	min-height: 100vh;
	background: #f5f5f5;
	background-image: linear-gradient(
		to right bottom,
		#ffffff,
		#fdf3fa,
		#fce7f6,
		#fbdaf2,
		#f9ceee
	);
}

.login-box {
	width: 100%;
	max-width: 400px;
	padding: 2rem;
	background: #fff;
	border-radius: 12px;
	box-shadow: 0 8px 20px rgba(248, 110, 211, 0.15);
	transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.login-box:hover {
	transform: translateY(-5px);
	box-shadow: 0 12px 25px rgba(248, 110, 211, 0.25);
}

.logo-section {
	text-align: center;
	margin-bottom: 2rem;
}

.logo {
	width: 168px;
	height: 48px;
	object-fit: cover;
	margin-bottom: 2rem;
}

h1 {
	font-size: 1.8rem;
	color: var(--primary-color);
	margin: 0;
	font-weight: 600;
}

h2 {
	color: var(--primary-color);
	margin-bottom: 15px;
	font-size: 1.5rem;
}

.form-group {
	margin-bottom: 1.5rem;
}

label {
	display: block;
	margin-bottom: 0.5rem;
	font-weight: 500;
	color: #555;
}

input {
	width: 100%;
	padding: 0.75rem;
	border: 1.5px solid #ddd;
	border-radius: 8px;
	font-size: 1rem;
	transition: all 0.3s;
	background-color: #fcfcfc;
}

input:focus {
	outline: none;
	border-color: var(--primary-color);
	box-shadow: 0 0 0 3px rgba(248, 110, 211, 0.15);
	background-color: white;
}

.login-button {
	width: 100%;
	padding: 0.85rem;
	background-color: var(--primary-color);
	color: white;
	border: none;
	border-radius: 8px;
	font-size: 1rem;
	font-weight: 500;
	cursor: pointer;
	transition: all 0.3s;
	margin-top: 0.5rem;
	box-shadow: 0 4px 10px rgba(248, 110, 211, 0.2);
}

.login-button:hover {
	background-color: var(--primary-hover);
	transform: translateY(-2px);
	box-shadow: 0 6px 15px rgba(248, 110, 211, 0.3);
}

.login-button:active {
	transform: translateY(0);
}

.login-button:disabled {
	background-color: #ccc;
	cursor: not-allowed;
	box-shadow: none;
}

.password-input-container {
	position: relative;
	width: 100%;
}

.password-input-container .input-icon {
	position: absolute;
	top: 50%;
	left: 12px;
	transform: translateY(-50%);
	color: #888;
	font-size: 1rem;
}

.password-input-container input {
	width: 100%;
	padding-right: 40px;
	padding-left: 40px;
}

.toggle-password {
	position: absolute;
	top: 50%;
	right: 10px;
	transform: translateY(-50%);
	background: none;
	border: none;
	cursor: pointer;
	padding: 0;
	margin: 0;
	width: auto;
	height: auto;
	color: #666;
	transition: color 0.3s;
}

.toggle-password:hover {
	color: var(--primary-color);
}

.toggle-password i {
	font-size: 1.2rem;
}

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
	padding-left: 40px;
}

.forgot-password-link {
	text-align: right;
	margin-bottom: 1rem;
}

.forgot-password-link a {
	color: var(--primary-color);
	font-size: 0.9rem;
	text-decoration: none;
	transition: all 0.3s;
}

.forgot-password-link a:hover {
	text-decoration: underline;
	color: var(--primary-hover);
}

/* Forgot Password Styles */
.forgot-password-box {
	max-width: 450px;
}

.forgot-password-description {
	font-size: 14px;
	color: #666;
	margin-bottom: 20px;
	text-align: center;
}

.button-group {
	display: flex;
	gap: 10px;
	margin-top: 20px;
}

.back-button,
.send-button {
	padding: 0.85rem;
	border: none;
	border-radius: 8px;
	font-size: 1rem;
	font-weight: 500;
	cursor: pointer;
	transition: all 0.3s;
	flex: 1;
}

.back-button {
	background-color: #f0f0f0;
	color: #555;
}

.back-button:hover {
	background-color: #e0e0e0;
	transform: translateY(-2px);
}

.back-button.full-width {
	width: 100%;
	margin-top: 20px;
}

.send-button {
	background-color: var(--primary-color);
	color: white;
	box-shadow: 0 4px 10px rgba(248, 110, 211, 0.2);
}

.send-button:hover {
	background-color: var(--primary-hover);
	transform: translateY(-2px);
	box-shadow: 0 6px 15px rgba(248, 110, 211, 0.3);
}

.send-button:disabled,
.back-button:disabled {
	opacity: 0.7;
	cursor: not-allowed;
	transform: none;
	box-shadow: none;
}

.input-error {
	border-color: #ff3860;
	box-shadow: 0 0 0 1px #ff3860;
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
	width: 70px;
	height: 70px;
	border-radius: 50%;
	display: flex;
	align-items: center;
	justify-content: center;
	font-size: 35px;
	margin: 0 auto 25px;
}

.note {
	font-size: 13px;
	color: #888;
	margin-top: 20px;
	font-style: italic;
}
</style>
