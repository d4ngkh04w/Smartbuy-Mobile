<script setup>
import { ref } from "vue";
import UserAvatar from "./UserAvatar.vue";
import AddressForm from "./AddressForm.vue";
import meService from "@/services/meService";
import emitter from "@/utils/evenBus";

const props = defineProps({
    userData: {
        type: Object,
        required: true,
    },
});

const emit = defineEmits(["update:userData", "change-password"]);

const isEditing = ref(false);
const errorMessage = ref("");
const loading = ref(false);
const avatarPreview = ref(null);
const avatarComponent = ref(null);
const addressFormRef = ref(null);
const verificationEmailSending = ref(false);
// Lưu trữ dữ liệu gốc để khôi phục khi hủy chỉnh sửa
const originalUserData = ref(null);

// Giới hạn ngày tối đa là ngày hôm nay
const maxDate = new Date().toISOString().split("T")[0];

// Format date for display (dd/mm/yyyy)
const formatDate = (dateString) => {
    if (!dateString) return "";
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return "";

    const day = String(date.getDate()).padStart(2, "0");
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
};

// Format date for input type="date" (yyyy-mm-dd)
const formatDateForInput = (dateString) => {
    if (!dateString) return "";
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return "";

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
};

// Toggle edit mode
const enableEditing = () => {
    // Lưu trữ dữ liệu gốc trước khi chỉnh sửa
    originalUserData.value = JSON.parse(JSON.stringify(props.userData));

    // Chuyển đổi dateOfBirth sang định dạng yyyy-mm-dd cho input type="date"
    if (props.userData.dateOfBirth) {
        const formattedDate = formatDateForInput(props.userData.dateOfBirth);
        if (formattedDate) {
            const editData = { ...props.userData };
            editData.dateOfBirth = formattedDate;
            emit("update:userData", editData);
        }
    }

    isEditing.value = true;
};

// Update address from AddressForm component
const updateAddress = (address) => {
    emit("update:userData", {
        ...props.userData,
        address: address,
    });
};

// Cancel editing
const cancelEditing = () => {
    isEditing.value = false;
    avatarPreview.value = null;
    errorMessage.value = "";

    // Reset avatar preview
    if (avatarComponent.value) {
        avatarComponent.value.resetFileInput();
    }

    // Khôi phục dữ liệu gốc từ khi bắt đầu chỉnh sửa
    if (originalUserData.value) {
        emit("update:userData", {
            ...originalUserData.value,
            avatarFile: null,
        });
    }
};

// Handle avatar upload
const handleFileChange = (file) => {
    // Create preview URL
    const reader = new FileReader();
    reader.onload = (e) => {
        avatarPreview.value = e.target.result;
    };
    reader.readAsDataURL(file);

    // Update avatar file in user data
    emit("update:userData", {
        ...props.userData,
        avatarFile: file,
    });
};

// Save profile changes
const saveProfile = async () => {
    loading.value = true;
    errorMessage.value = "";

    try {
        console.log("Saving profile...");
        // Create FormData object
        const formData = new FormData();

        // Add user info to FormData
        if (props.userData.name) {
            formData.append("name", props.userData.name);
        }

        if (props.userData.email) {
            formData.append("email", props.userData.email);
        }

        if (props.userData.phoneNumber) {
            formData.append("phoneNumber", props.userData.phoneNumber);
        }

        if (props.userData.gender) {
            formData.append("gender", props.userData.gender);
        }
        if (props.userData.address) {
            formData.append("address", props.userData.address);
        }

        // Add date of birth if provided
        if (props.userData.dateOfBirth) {
            formData.append("dateOfBirth", props.userData.dateOfBirth);
        }

        // Add avatar if changed
        if (props.userData.avatarFile) {
            formData.append("avatar", props.userData.avatarFile);
        }

        // Call API to update user profile
        const updatedUser = await meService.updateUserProfile(formData);
        console.log("Profile updated:", updatedUser);
        // Update local user data with response from server
        if (updatedUser) {
            emit("update:userData", {
                ...updatedUser,
                avatarFile: null,
                // Khôi phục lại định dạng ngày từ server (không phải định dạng input)
                dateOfBirth: updatedUser.dateOfBirth,
            }); // Show success notification
            emitter.emit("show-notification", {
                status: "success",
                message: "Cập nhật thông tin cá nhân thành công",
            });

            // Emit user-updated event to update header and other components
            emitter.emit("user-updated", updatedUser);

            // Exit edit mode
            isEditing.value = false;
            avatarPreview.value = null;

            // Reset avatar preview
            if (avatarComponent.value) {
                avatarComponent.value.resetFileInput();
            }
        }
    } catch (error) {
        console.error("Error updating profile:", error);
        errorMessage.value =
            error.response?.data?.message ||
            "Đã xảy ra lỗi khi cập nhật thông tin";

        emitter.emit("show-notification", {
            status: "error",
            message: errorMessage.value,
        });
    } finally {
        loading.value = false;
    }
};

// Send verification email
const sendVerificationEmail = async () => {
    verificationEmailSending.value = true;
    try {
        await meService.sendVerificationEmail();
        emitter.emit("show-notification", {
            status: "success",
            message: "Email xác thực đã được gửi. Vui lòng kiểm tra hộp thư.",
        });
    } catch (error) {
        console.error("Error sending verification email:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Đã xảy ra lỗi khi gửi email xác thực.",
        });
    } finally {
        verificationEmailSending.value = false;
    }
};
</script>
<template>
    <div class="profile-content">
        <div class="profile-header">
            <h2 class="profile-title">Thông tin cá nhân</h2>
            <div class="profile-actions">
                <button
                    v-if="!isEditing"
                    @click="enableEditing"
                    class="btn-edit"
                >
                    <i class="fas fa-pen"></i> Chỉnh sửa
                </button>
                <template v-else>
                    <button @click="cancelEditing" class="btn-cancel">
                        <i class="fas fa-times"></i> Hủy
                    </button>
                    <button @click="saveProfile" class="btn-save">
                        <i class="fas fa-check"></i> Lưu thay đổi
                    </button>
                </template>
            </div>
        </div>

        <div class="profile-inner">
            <!-- Avatar section -->
            <user-avatar
                :avatar="userData.avatar"
                :preview="avatarPreview"
                :editable="isEditing"
                @file-change="handleFileChange"
                ref="avatarComponent"
            >
                <h3 class="user-name">
                    {{ userData.name || "Chưa cập nhật tên" }}
                </h3>
                <p class="account-type">
                    Thành viên từ {{ formatDate(userData.createdAt) }}
                </p>
            </user-avatar>

            <!-- User info section -->
            <div class="profile-info-section">
                <div class="info-group">
                    <label>Họ và tên:</label>
                    <input
                        v-if="isEditing"
                        v-model="userData.name"
                        type="text"
                        class="info-input"
                        placeholder="Nhập họ và tên"
                    />
                    <div v-else class="info-text">
                        {{ userData.name || "Chưa cập nhật" }}
                    </div>
                </div>

                <div class="info-group">
                    <label>Email:</label>
                    <input
                        v-if="isEditing"
                        v-model="userData.email"
                        type="email"
                        class="info-input"
                        placeholder="Nhập email"
                    />
                    <div v-else class="info-text">
                        {{ userData.email || "Chưa cập nhật" }}
                        <span
                            v-if="userData.email && userData.emailConfirmed"
                            class="verified-badge"
                        >
                            <i class="fas fa-check-circle"></i> Đã xác thực
                        </span>
                        <span
                            v-else-if="
                                userData.email && !userData.emailConfirmed
                            "
                            class="unverified-badge"
                        >
                            Chưa xác thực
                            <button
                                class="btn-verify-email"
                                @click="sendVerificationEmail"
                                :disabled="verificationEmailSending"
                            >
                                <i
                                    v-if="verificationEmailSending"
                                    class="fas fa-spinner fa-spin"
                                ></i>
                                <i v-else class="fas fa-paper-plane"></i>
                                {{
                                    verificationEmailSending
                                        ? "Đang gửi..."
                                        : "Gửi email xác thực"
                                }}
                            </button>
                        </span>
                    </div>
                </div>

                <div class="info-group">
                    <label>Số điện thoại:</label>
                    <input
                        v-if="isEditing"
                        v-model="userData.phoneNumber"
                        type="tel"
                        class="info-input"
                        placeholder="Nhập số điện thoại"
                        maxlength="10"
                        pattern="[0-9]{10}"
                    />
                    <div v-else class="info-text">
                        {{ userData.phoneNumber || "Chưa cập nhật" }}
                        <span
                            v-if="
                                userData.phoneNumber &&
                                userData.phoneNumberConfirmed
                            "
                            class="verified-badge"
                        >
                            <i class="fas fa-check-circle"></i> Đã xác thực
                        </span>
                    </div>
                </div>

                <div class="info-group">
                    <label>Giới tính:</label>
                    <select
                        v-if="isEditing"
                        v-model="userData.gender"
                        class="info-input"
                    >
                        <option value="">-- Chọn giới tính --</option>
                        <option value="Nam">Nam</option>
                        <option value="Nữ">Nữ</option>
                        <option value="Khác">Khác</option>
                    </select>
                    <div v-else class="info-text">
                        {{ userData.gender || "Chưa cập nhật" }}
                    </div>
                </div>

                <div class="info-group">
                    <label>Ngày sinh:</label>
                    <input
                        v-if="isEditing"
                        v-model="userData.dateOfBirth"
                        type="date"
                        class="info-input"
                        :max="maxDate"
                    />
                    <div v-else class="info-text">
                        {{
                            formatDate(userData.dateOfBirth) || "Chưa cập nhật"
                        }}
                    </div>
                </div>

                <!-- Address section with conditional rendering -->
                <div class="info-group address-group" v-if="!isEditing">
                    <label>Địa chỉ:</label>
                    <div class="info-text address-text">
                        {{ userData.address || "Chưa cập nhật" }}
                    </div>
                </div>

                <!-- Use AddressForm component when in edit mode -->
                <div class="address-container" v-if="isEditing">
                    <address-form
                        ref="addressFormRef"
                        :initial-address="userData.address"
                        @update:address="updateAddress"
                    />
                </div>

                <div v-if="errorMessage" class="error-message">
                    {{ errorMessage }}
                </div>

                <div v-if="isEditing" class="edit-notice">
                    <i class="fas fa-info-circle"></i>
                    <span
                        >Vui lòng cung cấp thông tin chính xác để được hỗ trợ
                        tốt nhất.</span
                    >
                </div>
            </div>

            <div class="security-section" v-if="!isEditing">
                <div class="section-title">
                    <i class="fas fa-lock"></i>
                    Bảo mật
                </div>
                <button
                    class="btn-change-password"
                    @click="$emit('change-password')"
                >
                    <i class="fas fa-key"></i>
                    Đổi mật khẩu
                </button>
            </div>
        </div>
    </div>
</template>

<style scoped>
.profile-content {
    background-color: white;
    border-radius: 10px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    padding: 1.5rem;
}

.profile-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-weight: 600;
    text-shadow: 1px 1px 2px rgba(214, 51, 132, 0.1);
    position: relative;
    margin-bottom: 1.5rem;
    padding-bottom: 1rem;
    border-bottom: 1px solid #eee;
}
.profile-header::after {
    content: "";
    position: absolute;
    bottom: 0;
    left: 0;
    width: 13rem;
    height: 2px;
    background-color: #050505;
    border-radius: 10px;
}

.profile-title {
    font-size: 1.5rem;
    margin: 0;
    color: #333;
}

.profile-actions {
    display: flex;
    gap: 0.75rem;
}

.btn-edit,
.btn-save,
.btn-cancel {
    padding: 0.6rem 1rem;
    border-radius: 6px;
    border: none;
    cursor: pointer;
    font-weight: 500;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.3s ease;
}

.btn-edit {
    background-color: #f0f0f0;
    color: #333;
}

.btn-edit:hover {
    background-color: #e0e0e0;
}

.btn-save {
    background-color: var(--primary-color);
    color: white;
}

.btn-save:hover {
    background-color: #e056b2;
}

.btn-cancel {
    background-color: #f0f0f0;
    color: #555;
}

.btn-cancel:hover {
    background-color: #e0e0e0;
}

.profile-inner {
    display: flex;
    flex-direction: column;
    gap: 2rem;
}

.user-name {
    margin: 0 0 0.5rem 0;
    font-size: 1.5rem;
    color: #333;
}

.account-type {
    margin: 0;
    color: #666;
    font-size: 0.9rem;
}

/* Info section */
.profile-info-section {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1.5rem;
}

.address-group {
    grid-column: span 2;
}

.info-group {
    margin-bottom: 0.5rem;
}

.info-group label {
    display: block;
    font-weight: 500;
    color: #666;
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
}

.info-input {
    width: 100%;
    padding: 0.75rem 1rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 1rem;
}

.info-input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.15);
    outline: none;
}

.address-input {
    min-height: 100px;
    resize: vertical;
}

.info-text {
    padding: 0.75rem 0;
    color: #333;
    display: flex;
    align-items: center;
    gap: 0.75rem;
}

.address-text {
    white-space: pre-wrap;
    line-height: 1.5;
}

.verified-badge {
    background-color: #e8f5e9;
    color: #2e7d32;
    font-size: 0.8rem;
    padding: 0.25rem 0.5rem;
    border-radius: 4px;
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
}

.unverified-badge {
    background-color: #fff3e0;
    color: #e65100;
    font-size: 0.8rem;
    padding: 0.25rem 0.5rem;
    border-radius: 4px;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.btn-verify-email {
    background-color: var(--primary-color);
    color: white;
    border: none;
    padding: 0.25rem 0.5rem;
    border-radius: 4px;
    font-size: 0.8rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.25rem;
    transition: all 0.3s ease;
}

.btn-verify-email:disabled {
    background-color: #ddd;
    cursor: not-allowed;
}

.btn-verify-email:hover:not(:disabled) {
    background-color: #e056b2;
}

.error-message {
    background-color: #ffebee;
    color: #c62828;
    padding: 1rem;
    border-radius: 6px;
    margin: 1rem 0;
    font-size: 0.9rem;
}

.edit-notice {
    grid-column: span 2;
    display: flex;
    align-items: flex-start;
    gap: 0.5rem;
    background-color: #e8f4fd;
    padding: 1rem;
    border-radius: 6px;
    color: #0277bd;
    margin-top: 0.5rem;
}

.security-section {
    border-top: 1px solid #eee;
    padding-top: 1.5rem;
    margin-top: 1rem;
}

.section-title {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 1.2rem;
    font-weight: 500;
    color: #333;
    margin-bottom: 1rem;
}

.btn-change-password {
    background-color: #f0f0f0;
    border: none;
    padding: 0.75rem 1rem;
    border-radius: 6px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-weight: 500;
    color: #333;
    transition: all 0.3s ease;
}

.btn-change-password:hover {
    background-color: #e0e0e0;
}

/* Address container styling */
.address-container {
    grid-column: span 2;
}

@media (max-width: 992px) {
    .profile-info-section {
        grid-template-columns: 1fr;
    }

    .address-group {
        grid-column: span 1;
    }

    .edit-notice {
        grid-column: span 1;
    }

    .address-container {
        grid-column: span 1;
    }
}

@media (max-width: 576px) {
    .profile-header {
        flex-direction: column;
        gap: 1rem;
        align-items: flex-start;
    }

    .profile-actions {
        width: 100%;
        justify-content: space-between;
    }
}
</style>
```
