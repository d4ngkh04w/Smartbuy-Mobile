<script setup>
import { ref, onMounted, watch } from "vue";
import { useRouter } from "vue-router";
import meService from "@/services/meService";
import addressService from "@/services/addressService";
import emitter from "@/utils/evenBus";

const router = useRouter();
const isLoading = ref(false);
const errorMessage = ref("");

// Thông tin hồ sơ người dùng
const userProfile = ref({
    name: "",
    gender: "",
    address: "",
    avatarFile: null,
    dateOfBirth: "",
});

// Thông tin chi tiết địa chỉ
const addressData = ref({
    province: null,
    district: null,
    ward: null,
    detail: "",
});

// Danh sách địa chỉ
const provinces = ref([]);
const districts = ref([]);
const wards = ref([]);

// Loading states cho địa chỉ
const loadingProvinces = ref(false);
const loadingDistricts = ref(false);
const loadingWards = ref(false);

// Avatar preview
const avatarPreview = ref(null);
const fileInput = ref(null);

// Giới hạn ngày tối đa là ngày hôm nay
const maxDate = new Date().toISOString().split("T")[0];

/**
 * Xử lý tải lên avatar
 */
const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (!file) return;

    // Kiểm tra định dạng file
    const allowedTypes = ["image/jpeg", "image/png", "image/jpg"];
    if (!allowedTypes.includes(file.type)) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Chỉ chấp nhận file hình ảnh (jpg, jpeg, png)",
        });
        return;
    }

    // Giới hạn kích thước file (15MB)
    if (file.size > 15 * 1024 * 1024) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Kích thước file quá lớn (tối đa 15MB)",
        });
        return;
    }

    // Lưu file để gửi lên server
    userProfile.value.avatarFile = file;

    // Tạo URL preview
    const reader = new FileReader();
    reader.onload = (e) => {
        avatarPreview.value = e.target.result;
    };
    reader.readAsDataURL(file);
};

/**
 * Lấy danh sách tỉnh/thành phố
 */
const fetchProvinces = async () => {
    try {
        loadingProvinces.value = true;
        provinces.value = await addressService.getProvinces();
    } catch (error) {
        console.error("Lỗi khi lấy danh sách tỉnh/thành phố:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể tải danh sách tỉnh/thành phố",
        });
    } finally {
        loadingProvinces.value = false;
    }
};

/**
 * Lấy danh sách quận/huyện theo tỉnh đã chọn
 */
const fetchDistricts = async () => {
    if (!addressData.value.province) {
        districts.value = [];
        addressData.value.district = null;
        return;
    }

    try {
        loadingDistricts.value = true;
        districts.value = await addressService.getDistrictsByProvince(
            addressData.value.province.code
        );
        // Reset quận/huyện và phường/xã đã chọn
        addressData.value.district = null;
        addressData.value.ward = null;
        wards.value = [];
    } catch (error) {
        console.error("Lỗi khi lấy danh sách quận/huyện:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể tải danh sách quận/huyện",
        });
    } finally {
        loadingDistricts.value = false;
    }
};

/**
 * Lấy danh sách phường/xã theo quận đã chọn
 */
const fetchWards = async () => {
    if (!addressData.value.district) {
        wards.value = [];
        addressData.value.ward = null;
        return;
    }

    try {
        loadingWards.value = true;
        wards.value = await addressService.getWardsByDistrict(
            addressData.value.district.code
        );
        // Reset phường/xã đã chọn
        addressData.value.ward = null;
    } catch (error) {
        console.error("Lỗi khi lấy danh sách phường/xã:", error);
        emitter.emit("show-notification", {
            status: "error",
            message: "Không thể tải danh sách phường/xã",
        });
    } finally {
        loadingWards.value = false;
    }
};

/**
 * Cập nhật địa chỉ đầy đủ khi có thay đổi
 */
watch(
    [
        () => addressData.value.province,
        () => addressData.value.district,
        () => addressData.value.ward,
        () => addressData.value.detail,
    ],
    () => {
        userProfile.value.address = addressService.formatFullAddress(
            addressData.value
        );
    },
    { deep: true }
);

/**
 * Lưu thông tin hồ sơ người dùng
 */
const saveProfile = async () => {
    try {
        isLoading.value = true;
        errorMessage.value = "";

        // Tạo FormData để gửi
        const formData = new FormData();

        // Thêm thông tin vào FormData
        if (userProfile.value.name) {
            formData.append("name", userProfile.value.name);
        }

        if (userProfile.value.gender) {
            formData.append("gender", userProfile.value.gender);
        }

        if (userProfile.value.address) {
            formData.append("address", userProfile.value.address);
        }

        // Thêm ngày sinh nếu có
        if (userProfile.value.dateOfBirth) {
            formData.append("dateOfBirth", userProfile.value.dateOfBirth);
        }

        // Thêm avatar nếu có
        if (userProfile.value.avatarFile) {
            formData.append("avatar", userProfile.value.avatarFile);
        }

        // Gọi API cập nhật thông tin
        const response = await meService.updateUserProfile(formData);

        // Hiển thị thông báo thành công
        emitter.emit("show-notification", {
            status: "success",
            message: "Cập nhật thông tin thành công",
        });

        // Chuyển đến trang chủ
        router.push("/");
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
        isLoading.value = false;
    }
};

/**
 * Bỏ qua cập nhật và chuyển đến trang chủ
 */
const skipSetup = () => {
    router.push("/");
};

// Tải thông tin người dùng nếu có
onMounted(async () => {
    try {
        // Tải danh sách tỉnh/thành phố
        await fetchProvinces();

        // Tải thông tin hồ sơ người dùng
        const userData = await meService.getMe();
        if (userData) {
            userProfile.value.name = userData.name || "";
            userProfile.value.gender = userData.gender || "";
            userProfile.value.dateOfBirth = userData.dateOfBirth
                ? new Date(userData.dateOfBirth).toISOString().split("T")[0]
                : "";

            // Nếu đã có địa chỉ, hiển thị ở ô chi tiết để người dùng có thể cập nhật theo định dạng mới
            if (userData.address) {
                addressData.value.detail = userData.address;
                userProfile.value.address = userData.address;
            }
        }
    } catch (error) {
        console.error("Error fetching data:", error);
    }
});
</script>

<template>
    <div class="profile-setup-page">
        <div class="setup-container">
            <div class="setup-card">
                <h1 class="setup-title">Hoàn tất thông tin cá nhân</h1>
                <p class="setup-description">
                    Cập nhật thông tin của bạn để có trải nghiệm mua sắm tốt
                    nhất
                </p>

                <div v-if="errorMessage" class="error-message">
                    {{ errorMessage }}
                </div>

                <form @submit.prevent="saveProfile" class="setup-form">
                    <div class="avatar-section">
                        <div class="avatar-container">
                            <img
                                :src="
                                    avatarPreview ||
                                    'https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg'
                                "
                                alt="Avatar"
                                class="avatar-image"
                            />
                            <div class="avatar-overlay">
                                <label
                                    for="avatar-upload"
                                    class="avatar-upload-label"
                                >
                                    <i class="fas fa-camera"></i> Tải ảnh lên
                                </label>
                                <input
                                    id="avatar-upload"
                                    type="file"
                                    accept="image/jpeg,image/png,image/jpg"
                                    @change="handleFileChange"
                                    class="avatar-upload"
                                    ref="fileInput"
                                />
                            </div>
                        </div>
                        <p class="avatar-hint">
                            Tải ảnh đại diện (không bắt buộc)
                        </p>
                    </div>

                    <!-- Layout 2 cột cho các trường cơ bản -->
                    <div class="form-row">
                        <div class="form-col">
                            <div class="form-group">
                                <label for="name">Họ và tên</label>
                                <input
                                    type="text"
                                    id="name"
                                    v-model="userProfile.name"
                                    placeholder="Nhập họ và tên của bạn"
                                    class="form-input"
                                />
                            </div>
                        </div>
                        <div class="form-col">
                            <div class="form-group">
                                <label for="dateOfBirth">Ngày sinh</label>
                                <input
                                    type="date"
                                    id="dateOfBirth"
                                    v-model="userProfile.dateOfBirth"
                                    :max="maxDate"
                                    class="form-input"
                                />
                            </div>
                        </div>
                    </div>

                    <div class="form-group">
                        <label for="gender">Giới tính</label>
                        <select
                            id="gender"
                            v-model="userProfile.gender"
                            class="form-input"
                        >
                            <option value="">-- Chọn giới tính --</option>
                            <option value="Nam">Nam</option>
                            <option value="Nữ">Nữ</option>
                            <option value="Khác">Khác</option>
                        </select>
                    </div>

                    <!-- Địa chỉ mới - chọn theo đơn vị hành chính -->
                    <div class="address-section">
                        <h3 class="address-section-title">Địa chỉ</h3>

                        <div class="address-grid">
                            <div class="form-group">
                                <label for="province">Tỉnh/Thành phố</label>
                                <select
                                    id="province"
                                    v-model="addressData.province"
                                    class="form-input"
                                    @change="fetchDistricts"
                                    :disabled="loadingProvinces"
                                >
                                    <option :value="null">
                                        -- Chọn Tỉnh/Thành phố --
                                    </option>
                                    <option
                                        v-for="province in provinces"
                                        :key="province.code"
                                        :value="province"
                                    >
                                        {{ province.name }}
                                    </option>
                                </select>
                                <div
                                    v-if="loadingProvinces"
                                    class="loading-indicator"
                                >
                                    <span>Đang tải...</span>
                                </div>
                            </div>

                            <div class="form-group">
                                <label for="district">Quận/Huyện</label>
                                <select
                                    id="district"
                                    v-model="addressData.district"
                                    class="form-input"
                                    @change="fetchWards"
                                    :disabled="
                                        !addressData.province ||
                                        loadingDistricts
                                    "
                                >
                                    <option :value="null">
                                        -- Chọn Quận/Huyện --
                                    </option>
                                    <option
                                        v-for="district in districts"
                                        :key="district.code"
                                        :value="district"
                                    >
                                        {{ district.name }}
                                    </option>
                                </select>
                                <div
                                    v-if="loadingDistricts"
                                    class="loading-indicator"
                                >
                                    <span>Đang tải...</span>
                                </div>
                            </div>
                        </div>

                        <div class="address-grid">
                            <div class="form-group">
                                <label for="ward">Phường/Xã</label>
                                <select
                                    id="ward"
                                    v-model="addressData.ward"
                                    class="form-input"
                                    :disabled="
                                        !addressData.district || loadingWards
                                    "
                                >
                                    <option :value="null">
                                        -- Chọn Phường/Xã --
                                    </option>
                                    <option
                                        v-for="ward in wards"
                                        :key="ward.code"
                                        :value="ward"
                                    >
                                        {{ ward.name }}
                                    </option>
                                </select>
                                <div
                                    v-if="loadingWards"
                                    class="loading-indicator"
                                >
                                    <span>Đang tải...</span>
                                </div>
                            </div>

                            <div class="form-group">
                                <label for="address_detail"
                                    >Địa chỉ chi tiết</label
                                >
                                <input
                                    type="text"
                                    id="address_detail"
                                    v-model="addressData.detail"
                                    placeholder="Nhập số nhà, đường, tòa nhà..."
                                    class="form-input"
                                />
                            </div>
                        </div>

                        <div class="form-group" v-if="userProfile.address">
                            <label class="address-preview-label"
                                >Xem trước địa chỉ đầy đủ:</label
                            >
                            <div class="address-preview">
                                {{ userProfile.address }}
                            </div>
                        </div>
                    </div>

                    <div class="form-actions">
                        <button
                            type="button"
                            class="btn-skip"
                            @click="skipSetup"
                        >
                            Bỏ qua
                        </button>
                        <button
                            type="submit"
                            class="btn-save"
                            :disabled="isLoading"
                        >
                            {{ isLoading ? "Đang xử lý..." : "Lưu thông tin" }}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>

<style scoped>
.profile-setup-page {
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
    padding: 2rem 1rem;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
        Helvetica, Arial, sans-serif;
    color: #333;
}

.setup-container {
    width: 100%;
    max-width: 850px;
    margin: 0 auto;
}

.setup-card {
    background-color: #fff;
    border-radius: 16px;
    box-shadow: 0 10px 30px rgba(149, 157, 165, 0.2);
    padding: 2.8rem;
    transition: all 0.3s ease;
}

.setup-title {
    color: #333;
    font-size: 2.2rem;
    text-align: center;
    margin-bottom: 0.6rem;
    font-weight: 600;
    letter-spacing: -0.5px;
}

.setup-description {
    color: #666;
    text-align: center;
    margin-bottom: 2.5rem;
    font-size: 1.05rem;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
        Helvetica, Arial, sans-serif;
}

.error-message {
    background-color: #ffebee;
    color: #d32f2f;
    padding: 14px;
    border-radius: 10px;
    margin-bottom: 20px;
    font-size: 0.95rem;
    text-align: center;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
        Helvetica, Arial, sans-serif;
    border-left: 4px solid #f44336;
}

.setup-form {
    display: flex;
    flex-direction: column;
}

.avatar-section {
    display: flex;
    flex-direction: column;
    align-items: center;
    margin-bottom: 2.5rem;
}

.avatar-container {
    position: relative;
    width: 140px;
    height: 140px;
    border-radius: 50%;
    overflow: hidden;
    margin-bottom: 1rem;
    box-shadow: 0 6px 16px rgba(0, 0, 0, 0.15);
    border: 4px solid #fff;
    transition: all 0.3s ease;
}

.avatar-container:hover {
    transform: scale(1.02);
    box-shadow: 0 8px 20px rgba(0, 0, 0, 0.2);
}

.avatar-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.avatar-overlay {
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    background-color: rgba(0, 0, 0, 0.65);
    display: flex;
    justify-content: center;
    padding: 12px 0;
    transition: all 0.3s ease;
}

.avatar-container:hover .avatar-overlay {
    background-color: rgba(0, 0, 0, 0.8);
}

.avatar-upload-label {
    color: white;
    cursor: pointer;
    font-size: 0.95rem;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
        Helvetica, Arial, sans-serif;
    display: flex;
    align-items: center;
    gap: 6px;
}

.avatar-upload-label i {
    font-size: 1.1rem;
}

.avatar-upload {
    display: none;
}

.avatar-hint {
    color: #666;
    font-size: 0.95rem;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
        Helvetica, Arial, sans-serif;
    margin-top: 0.4rem;
}

.form-group {
    margin-bottom: 1.8rem;
    position: relative;
}

.form-group label {
    display: block;
    font-weight: 500;
    margin-bottom: 0.6rem;
    color: #333;
    font-size: 1.05rem;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
        Helvetica, Arial, sans-serif;
}

.form-input {
    width: 100%;
    padding: 0.95rem 1rem;
    border: 1px solid #ddd;
    border-radius: 10px;
    font-size: 1rem;
    transition: all 0.3s ease;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
        Helvetica, Arial, sans-serif;
    background-color: #fff;
    color: #333;
}

.form-input:focus {
    border-color: #f86ed3;
    box-shadow: 0 0 0 3px rgba(248, 110, 211, 0.2);
    outline: none;
}

.form-input::placeholder {
    color: #aaa;
    font-size: 0.95rem;
}

.form-input:disabled {
    background-color: #f5f5f5;
    cursor: not-allowed;
    color: #999;
}

select.form-input {
    appearance: none;
    background-image: url("data:image/svg+xml;charset=US-ASCII,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20width%3D%22292.4%22%20height%3D%22292.4%22%3E%3Cpath%20fill%3D%22%23666%22%20d%3D%22M287%2069.4a17.6%2017.6%200%200%200-13-5.4H18.4c-5%200-9.3%201.8-12.9%205.4A17.6%2017.6%200%200%200%200%2082.2c0%205%201.8%209.3%205.4%2012.9l128%20127.9c3.6%203.6%207.8%205.4%2012.8%205.4s9.2-1.8%2012.8-5.4L287%2095c3.5-3.5%205.4-7.8%205.4-12.8%200-5-1.9-9.2-5.5-12.8z%22%2F%3E%3C%2Fsvg%3E");
    background-repeat: no-repeat;
    background-position: right 1rem center;
    background-size: 0.8rem;
    padding-right: 2.5rem;
}

/* Thêm layout 2 cột cho các form-group */
@media (min-width: 768px) {
    .form-row {
        display: flex;
        gap: 2rem;
    }

    .form-col {
        flex: 1;
    }
}

/* Thiết kế cho phần địa chỉ */
.address-section {
    border: 1px solid #eee;
    border-radius: 12px;
    padding: 1.8rem;
    margin-bottom: 2.5rem;
    background-color: #fff; /* Thay đổi từ #f9f9f9 thành #fff */
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.03);
}

.address-section-title {
    font-size: 1.25rem;
    margin-bottom: 1.8rem;
    color: #333;
    text-align: center;
    font-weight: 600;
    letter-spacing: -0.2px;
    position: relative;
    padding-bottom: 0.8rem;
}

.address-section-title::after {
    content: "";
    position: absolute;
    bottom: 0;
    left: 50%;
    transform: translateX(-50%);
    width: 80px;
    height: 3px;
    background: linear-gradient(to right, #f9ceee, #f86ed3);
    border-radius: 3px;
}

/* Layout 2 cột cho form địa chỉ */
@media (min-width: 768px) {
    .address-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 1.8rem;
        margin-bottom: 0.8rem;
    }
}

.loading-indicator {
    position: absolute;
    right: 12px;
    top: calc(50% + 10px);
    color: #666;
    font-size: 0.85rem;
    font-style: italic;
}

.address-preview-label {
    font-weight: 500;
    margin-bottom: 0.6rem;
    color: #333;
    font-size: 1.05rem;
}

.address-preview {
    padding: 1rem 1.2rem;
    background-color: #fff;
    border: 1px solid #ddd;
    border-radius: 10px;
    font-size: 1rem;
    line-height: 1.6;
    color: #333;
    min-height: 60px;
    box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.03);
}

.form-actions {
    display: flex;
    justify-content: space-between;
    margin-top: 2.5rem;
    gap: 1.5rem;
}

.btn-save,
.btn-skip {
    padding: 1rem 1.8rem;
    border-radius: 10px;
    font-size: 1.05rem;
    cursor: pointer;
    transition: all 0.3s ease;
    flex: 1;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto,
        Helvetica, Arial, sans-serif;
    font-weight: 600;
}

.btn-save {
    background-color: #f86ed3;
    border: none;
    color: white;
}

.btn-save:hover {
    background-color: #e05cb6;
    box-shadow: 0 6px 16px rgba(224, 92, 182, 0.3);
    transform: translateY(-2px);
}

.btn-save:active {
    transform: translateY(0);
    box-shadow: 0 3px 8px rgba(224, 92, 182, 0.3);
}

.btn-save:disabled {
    opacity: 0.6;
    cursor: not-allowed;
    transform: none;
    box-shadow: none;
}

.btn-skip {
    background-color: transparent;
    border: 1px solid #ddd;
    color: #666;
}

.btn-skip:hover {
    background-color: #f5f5f5;
    border-color: #ccc;
    color: #333;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.btn-skip:active {
    transform: translateY(1px);
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
}

/* Responsive styles */
@media (max-width: 768px) {
    .setup-card {
        padding: 2.2rem;
        border-radius: 14px;
    }

    .setup-title {
        font-size: 1.8rem;
    }

    .setup-description {
        margin-bottom: 2rem;
    }

    .form-group label,
    .address-preview-label {
        font-size: 1rem;
    }

    .address-section {
        padding: 1.5rem;
    }
}

@media (max-width: 640px) {
    .setup-card {
        padding: 1.8rem;
        border-radius: 12px;
    }

    .form-actions {
        flex-direction: column;
    }

    .btn-save,
    .btn-skip {
        width: 100%;
        padding: 0.9rem 1rem;
    }
}

@media (max-width: 480px) {
    .setup-card {
        padding: 1.5rem;
    }

    .setup-title {
        font-size: 1.6rem;
    }

    .avatar-container {
        width: 120px;
        height: 120px;
        border-width: 3px;
    }

    .form-input {
        padding: 0.85rem;
    }

    .address-section-title {
        font-size: 1.2rem;
    }
}
</style>
