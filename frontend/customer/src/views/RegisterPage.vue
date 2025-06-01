<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import authService from "@/services/authService";
import emitter from "@/utils/evenBus";

const router = useRouter();
const phoneNumber = ref("");
const email = ref("");
const password = ref("");
const confirmPassword = ref("");
const agreeTerms = ref(false);
const showPassword = ref(false);
const showConfirmPassword = ref(false);
const isLoading = ref(false);

// Validation errors
const errorMessage = ref("");
const phoneNumberError = ref("");
const emailError = ref("");
const passwordError = ref("");
const confirmPasswordError = ref("");

/**
 * Xử lý đăng ký tài khoản
 */
const handleRegister = async () => {
	// Reset errors
	errorMessage.value = "";
	phoneNumberError.value = "";
	emailError.value = "";
	passwordError.value = "";
	confirmPasswordError.value = "";

	// Validate inputs
	let isValid = true;

	// Validate phone number
	if (!isValidPhoneNumber(phoneNumber.value)) {
		phoneNumberError.value = "Số điện thoại không hợp lệ";
		isValid = false;
	}

	// Validate email
	if (!isValidEmail(email.value)) {
		emailError.value = "Email không hợp lệ";
		isValid = false;
	}

	// Validate password
	if (!password.value || password.value.length < 6) {
		passwordError.value = "Mật khẩu phải có ít nhất 6 ký tự";
		isValid = false;
	}

	// Validate confirm password
	if (password.value !== confirmPassword.value) {
		confirmPasswordError.value = "Mật khẩu xác nhận không khớp";
		isValid = false;
	}

	// Validate terms agreement
	if (!agreeTerms.value) {
		errorMessage.value = "Vui lòng đồng ý với điều khoản dịch vụ";
		isValid = false;
	}

	if (!isValid) return;

	try {
		isLoading.value = true;

		// Call register API
		await authService.register({
			phoneNumber: phoneNumber.value,
			email: email.value,
			password: password.value,
			confirmPassword: confirmPassword.value,
		});

		emitter.emit("show-notification", {
			status: "success",
			message: "Đăng ký thành công",
		});

		// Redirect to profile update page
		router.push("/profile-setup");
	} catch (error) {
		console.error("Error during registration:", error);
		if (error.response) {
			const { status, data } = error.response;

			if (status === 409) {
				// Conflict - Email/phone already exists
				if (data.message === "Phone number already exists") {
					errorMessage.value = "Số điện thoại đã tồn tại";
				} else if (data.message === "Email already exists") {
					errorMessage.value = "Email đã tồn tại";
				} else {
					errorMessage.value = "Số điện thoại hoặc email đã tồn tại";
				}
			} else {
				emitter.emit("show-notification", {
					status: "error",
					message: data.message || "Đăng ký thất bại",
				});
			}
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
 * Validate phone number input (only allow digits and limit to 10 characters)
 */
const validatePhoneInput = (event) => {
	const input = event.target.value.replace(/\D/g, "").slice(0, 10);
	phoneNumber.value = input;
	phoneNumberError.value =
		input.length != 10 ? "Số điện thoại phải có 10 số" : "";
};

/**
 * Check if phone number is valid (10 digits)
 */
const isValidPhoneNumber = (phone) => {
	return /^(0[\d]{9})$/.test(phone);
};

/**
 * Check if email is valid
 */
const isValidEmail = (email) => {
	const emailRegex = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/;
	return emailRegex.test(email);
};

/**
 * Validate email input
 */
const validateEmailInput = () => {
	emailError.value = isValidEmail(email.value) ? "" : "Email không hợp lệ";
};

/**
 * Validate password input
 */
const validatePasswordInput = () => {
	passwordError.value =
		password.value.length < 6 ? "Mật khẩu phải có ít nhất 6 ký tự" : "";

	// If confirm password is already filled, check match
	if (confirmPassword.value) {
		validateConfirmPasswordInput();
	}
};

/**
 * Validate confirm password input
 */
const validateConfirmPasswordInput = () => {
	confirmPasswordError.value =
		password.value !== confirmPassword.value
			? "Mật khẩu xác nhận không khớp"
			: "";
};
</script>

<template>
	<div class="register-page">
		<div class="register-container">
			<div class="logo-section">
				<div class="logo-text">SmartBuy Mobile</div>
				<h2 class="tagline">
					Đăng ký để nhận ưu đãi đặc biệt và theo dõi đơn hàng dễ
					dàng!
				</h2>
			</div>

			<div class="form-section">
				<div class="register-card">
					<form
						@submit.prevent="handleRegister"
						class="register-form"
					>
						<h2 class="form-title">Đăng ký</h2>

						<div v-if="errorMessage" class="error-message">
							{{ errorMessage }}
						</div>

						<div class="form-group">
							<div class="input-container">
								<i class="fas fa-phone input-icon"></i>
								<input
									type="text"
									id="phoneNumber"
									v-model="phoneNumber"
									placeholder="Số điện thoại"
									@input="validatePhoneInput"
									:class="{ 'input-error': phoneNumberError }"
									required
								/>
							</div>
							<p class="error-text" v-if="phoneNumberError">
								{{ phoneNumberError }}
							</p>
						</div>

						<div class="form-group">
							<div class="input-container">
								<i class="fas fa-envelope input-icon"></i>
								<input
									type="email"
									id="email"
									v-model="email"
									placeholder="Email"
									@blur="validateEmailInput"
									:class="{ 'input-error': emailError }"
									required
								/>
							</div>
							<p class="error-text" v-if="emailError">
								{{ emailError }}
							</p>
						</div>

						<div class="form-group">
							<div class="password-input">
								<i class="fas fa-lock input-icon"></i>
								<input
									:type="showPassword ? 'text' : 'password'"
									id="password"
									v-model="password"
									placeholder="Mật khẩu (ít nhất 6 ký tự)"
									@blur="validatePasswordInput"
									:class="{ 'input-error': passwordError }"
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
									placeholder="Xác nhận mật khẩu"
									@blur="validateConfirmPasswordInput"
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
							<p class="error-text" v-if="confirmPasswordError">
								{{ confirmPasswordError }}
							</p>
						</div>

						<div class="form-group terms-group">
							<label class="checkbox-container">
								<input
									type="checkbox"
									v-model="agreeTerms"
									id="terms"
								/>
								<span class="checkmark"></span>
								<div class="terms-text">
									Tôi đồng ý với
									<router-link to="/terms" class="terms-link"
										>Điều khoản dịch vụ</router-link
									>
									và
									<router-link
										to="/privacy"
										class="terms-link"
										>Chính sách bảo mật</router-link
									>
								</div>
							</label>
						</div>

						<button
							type="submit"
							class="btn-register"
							:disabled="isLoading"
						>
							{{ isLoading ? "Đang xử lý..." : "Đăng ký" }}
						</button>

						<div class="login-prompt">
							Đã có tài khoản?
							<router-link to="/login" class="auth-link">
								Đăng nhập
							</router-link>
						</div>
					</form>
				</div>
			</div>
		</div>
	</div>
</template>

<style scoped>
.register-page {
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

.register-container {
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

.register-card {
	background-color: #fff;
	border-radius: 8px;
	box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1), 0 8px 16px rgba(0, 0, 0, 0.1);
	padding: 1.5rem;
	margin-bottom: 28px;
}

.register-form {
	display: flex;
	flex-direction: column;
}

.form-title {
	text-align: center;
	margin-bottom: 1.5rem;
	color: #f86ed3;
	font-size: 1.5rem;
}

.form-group {
	margin-bottom: 12px;
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

.input-container input,
.password-input input {
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

.btn-register {
	background-color: #f86ed3;
	border: none;
	border-radius: 6px;
	font-size: 20px;
	line-height: 48px;
	padding: 0 16px;
	width: 100%;
	color: #fff;
	font-weight: bold;
	margin-top: 16px;
	cursor: pointer;
	transition: background-color 0.3s;
}

.btn-register:hover {
	background-color: #e05cb6;
}

.btn-register:disabled {
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

.error-text {
	color: #d32f2f;
	font-size: 12px;
	margin: 4px 0 0 4px;
}

.input-error {
	border-color: #d32f2f;
	border-width: 2px;
}

/* Terms checkbox */
.terms-group {
	margin: 16px 0;
}

.checkbox-container {
	display: flex;
	position: relative;
	padding-left: 35px;
	cursor: pointer;
	font-size: 14px;
	user-select: none;
	align-items: center;
}

.checkbox-container input {
	position: absolute;
	opacity: 0;
	cursor: pointer;
	height: 0;
	width: 0;
}

.checkmark {
	position: absolute;
	left: 0;
	height: 20px;
	width: 20px;
	background-color: #eee;
	border-radius: 4px;
}

.checkbox-container:hover input ~ .checkmark {
	background-color: #ccc;
}

.checkbox-container input:checked ~ .checkmark {
	background-color: #f86ed3;
}

.checkmark:after {
	content: "";
	position: absolute;
	display: none;
}

.checkbox-container input:checked ~ .checkmark:after {
	display: block;
}

.checkbox-container .checkmark:after {
	left: 7px;
	top: 3px;
	width: 5px;
	height: 10px;
	border: solid white;
	border-width: 0 2px 2px 0;
	transform: rotate(45deg);
}

.terms-text {
	margin-left: 5px;
	line-height: 1.4;
}

.terms-link {
	color: #f86ed3;
	text-decoration: none;
}

.terms-link:hover {
	text-decoration: underline;
}

.login-prompt {
	margin-top: 16px;
	text-align: center;
	font-size: 14px;
	color: #606770;
}

.auth-link {
	color: #f86ed3;
	text-decoration: none;
	font-weight: 600;
}

.auth-link:hover {
	text-decoration: underline;
}

/* Responsive */
@media (max-width: 900px) {
	.register-container {
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
	.register-card {
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
