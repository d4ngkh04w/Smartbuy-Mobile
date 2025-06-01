<template>
    <div class="address-section">
        <h3 class="address-section-title">Địa chỉ</h3>

        <div class="address-grid">
            <div class="form-group">
                <label for="province">Tỉnh/Thành phố</label>
                <select
                    id="province"
                    v-model="addressData.province"
                    class="form-input"
                    @change="handleProvinceChange"
                    :disabled="loadingProvinces"
                >
                    <option :value="null">-- Chọn Tỉnh/Thành phố --</option>
                    <option
                        v-for="province in provinces"
                        :key="province.code"
                        :value="province"
                    >
                        {{ province.name }}
                    </option>
                </select>
                <div v-if="loadingProvinces" class="loading-indicator">
                    <span>Đang tải...</span>
                </div>
            </div>

            <div class="form-group">
                <label for="district">Quận/Huyện</label>
                <select
                    id="district"
                    v-model="addressData.district"
                    class="form-input"
                    @change="handleDistrictChange"
                    :disabled="!addressData.province || loadingDistricts"
                >
                    <option :value="null">-- Chọn Quận/Huyện --</option>
                    <option
                        v-for="district in districts"
                        :key="district.code"
                        :value="district"
                    >
                        {{ district.name }}
                    </option>
                </select>
                <div v-if="loadingDistricts" class="loading-indicator">
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
                    @change="emitAddressUpdate"
                    :disabled="!addressData.district || loadingWards"
                >
                    <option :value="null">-- Chọn Phường/Xã --</option>
                    <option
                        v-for="ward in wards"
                        :key="ward.code"
                        :value="ward"
                    >
                        {{ ward.name }}
                    </option>
                </select>
                <div v-if="loadingWards" class="loading-indicator">
                    <span>Đang tải...</span>
                </div>
            </div>

            <div class="form-group">
                <label for="address_detail">Địa chỉ chi tiết</label>
                <input
                    type="text"
                    id="address_detail"
                    v-model="addressData.detail"
                    placeholder="Nhập số nhà, đường, tòa nhà..."
                    class="form-input"
                    @input="emitAddressUpdate"
                />
            </div>
        </div>

        <div class="form-group" v-if="formattedAddress">
            <label class="address-preview-label"
                >Xem trước địa chỉ đầy đủ:</label
            >
            <div class="address-preview">
                {{ formattedAddress }}
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, watch, computed } from "vue";
import addressService from "@/services/addressService";
import emitter from "@/utils/evenBus";

const props = defineProps({
    initialAddress: {
        type: String,
        default: "",
    },
    modelValue: {
        type: String,
        default: "",
    },
});

const emit = defineEmits(["update:address"]);

// Danh sách địa chỉ
const provinces = ref([]);
const districts = ref([]);
const wards = ref([]);

// Loading states cho địa chỉ
const loadingProvinces = ref(false);
const loadingDistricts = ref(false);
const loadingWards = ref(false);

// Thông tin chi tiết địa chỉ
const addressData = ref({
    province: null,
    district: null,
    ward: null,
    detail: "",
});

// Địa chỉ đầy đủ đã được format
const formattedAddress = computed(() => {
    try {
        return addressService.formatFullAddress(addressData.value);
    } catch {
        return "";
    }
});
// Hàm fuzzy matching để xử lý địa chỉ không khớp chính xác
const fuzzyMatch = (str1, str2) => {
    const normalize = (str) =>
        str
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .toLowerCase();
    return (
        normalize(str1).includes(normalize(str2)) ||
        normalize(str2).includes(normalize(str1))
    );
};

// Parse địa chỉ từ chuỗi ban đầu
const parseAddress = async (addressString) => {
    if (!addressString) return;

    try {
        // Đợi để đảm bảo provinces đã được tải
        if (provinces.value.length === 0) {
            await fetchProvinces();
        }

        // Chuẩn hóa địa chỉ
        const normalizedAddress = addressString.trim();
        const parts = normalizedAddress.split(",").map((part) => part.trim());

        // Tìm tỉnh/thành phố (phần cuối)
        if (parts.length > 0) {
            const provincePart = parts[parts.length - 1];
            const province = provinces.value.find((p) =>
                fuzzyMatch(p.name, provincePart)
            );

            if (province) {
                addressData.value.province = province;
                await fetchDistricts();

                // Tìm quận/huyện (phần gần cuối)
                if (parts.length >= 2) {
                    const districtPart = parts[parts.length - 2];
                    const district = districts.value.find((d) =>
                        fuzzyMatch(d.name, districtPart)
                    );

                    if (district) {
                        addressData.value.district = district;
                        await fetchWards();

                        // Tìm phường/xã (phần thứ 3 từ cuối)
                        if (parts.length >= 3) {
                            const wardPart = parts[parts.length - 3];
                            const ward = wards.value.find((w) =>
                                fuzzyMatch(w.name, wardPart)
                            );

                            if (ward) {
                                addressData.value.ward = ward;

                                // Phần còn lại là địa chỉ chi tiết
                                if (parts.length > 3) {
                                    addressData.value.detail = parts
                                        .slice(0, parts.length - 3)
                                        .join(", ");
                                }
                            }
                        }
                    }
                }
            } else {
                // Nếu không tìm thấy tỉnh, đặt toàn bộ vào chi tiết
                addressData.value.detail = addressString;
                emitter.emit("show-notification", {
                    status: "info",
                    message:
                        "Không thể tự động điền địa chỉ. Vui lòng kiểm tra lại.",
                });
            }
        }
    } catch (error) {
        console.error("Error parsing address:", error);
        addressData.value.detail = addressString;
    }
};

/**
 * Lấy danh sách tỉnh/thành phố
 */
const fetchProvinces = async () => {
    try {
        loadingProvinces.value = true;
        provinces.value = await addressService.getProvinces();
        return provinces.value;
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
 * Xử lý khi thay đổi tỉnh/thành phố
 */
const handleProvinceChange = async () => {
    addressData.value.district = null;
    addressData.value.ward = null;
    districts.value = [];
    wards.value = [];

    if (addressData.value.province) {
        await fetchDistricts();
    }

    // Emit địa chỉ mới khi user thay đổi
    emitAddressUpdate();
};

/**
 * Xử lý khi thay đổi quận/huyện
 */
const handleDistrictChange = async () => {
    addressData.value.ward = null;
    wards.value = [];

    if (addressData.value.district) {
        await fetchWards();
    }

    // Emit địa chỉ mới khi user thay đổi
    emitAddressUpdate();
};

// Hàm emit địa chỉ
const emitAddressUpdate = () => {
    const fullAddress = addressService.formatFullAddress(addressData.value);
    emit("update:address", fullAddress);
};

/**
 * Lấy danh sách quận/huyện theo tỉnh đã chọn
 */
const fetchDistricts = async () => {
    if (!addressData.value.province) {
        districts.value = [];
        return;
    }

    try {
        loadingDistricts.value = true;
        districts.value = await addressService.getDistrictsByProvince(
            addressData.value.province.code
        );
        return districts.value;
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
        return;
    }

    try {
        loadingWards.value = true;
        wards.value = await addressService.getWardsByDistrict(
            addressData.value.district.code
        );
        return wards.value;
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

// Khởi tạo địa chỉ khi component được tạo
onMounted(async () => {
    await fetchProvinces();
    if (props.initialAddress) {
        await parseAddress(props.initialAddress);
    }
});

// Phơi bày phương thức reset cho component cha
const resetAddress = () => {
    addressData.value = {
        province: null,
        district: null,
        ward: null,
        detail: "",
    };
};

defineExpose({ resetAddress });
</script>

<style scoped>
.address-section {
    border: 1px solid #eaeaea;
    border-radius: 12px;
    padding: 2rem;
    margin-bottom: 2rem;
    background-color: #fff;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
    transition: all 0.3s ease;
}

.address-section:hover {
    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.08);
}

.address-section-title {
    font-size: 1.3rem;
    margin-bottom: 1.8rem;
    color: #2c3e50;
    text-align: left;
    font-weight: 600;
    position: relative;
    padding-bottom: 0.8rem;
    display: flex;
    align-items: center;
}

.address-section-title::before {
    content: "";
    display: inline-block;
    width: 4px;
    height: 20px;
    background: linear-gradient(to bottom, #f86ed3, #a18af5);
    border-radius: 2px;
    margin-right: 12px;
}

/* Layout 2 cột cho form địa chỉ */
.address-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1.8rem;
    margin-bottom: 1rem;
}

.form-group {
    margin-bottom: 1.8rem;
    position: relative;
}

.form-group label {
    display: block;
    font-weight: 500;
    color: #4a5568;
    margin-bottom: 0.6rem;
    font-size: 0.95rem;
}

.form-input {
    width: 100%;
    padding: 0.85rem 1.2rem;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    font-size: 1rem;
    transition: all 0.3s ease;
    background-color: #f8fafc;
    color: #2d3748;
}

.form-input:hover {
    border-color: #cbd5e0;
    background-color: #fff;
}

.form-input:focus {
    border-color: #a18af5;
    box-shadow: 0 0 0 3px rgba(161, 138, 245, 0.2);
    outline: none;
    background-color: #fff;
}

.form-input:disabled {
    background-color: #f1f5f9;
    cursor: not-allowed;
    color: #94a3b8;
}

.loading-indicator {
    position: absolute;
    right: 12px;
    top: 38px;
    color: #64748b;
    font-size: 0.85rem;
    font-style: italic;
    display: flex;
    align-items: center;
}

.loading-indicator::before {
    content: "";
    display: inline-block;
    width: 14px;
    height: 14px;
    border: 2px solid #e2e8f0;
    border-top-color: #a18af5;
    border-radius: 50%;
    margin-right: 6px;
    animation: spin 0.8s linear infinite;
}

@keyframes spin {
    to {
        transform: rotate(360deg);
    }
}

.address-preview-label {
    font-weight: 500;
    margin-bottom: 0.8rem;
    color: #2d3748;
    font-size: 0.95rem;
    display: flex;
    align-items: center;
}

.address-preview {
    padding: 1.2rem 1.5rem;
    background-color: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    font-size: 1rem;
    line-height: 1.6;
    color: #2d3748;
    min-height: 60px;
    transition: all 0.3s ease;
}

.address-preview:hover {
    background-color: #fff;
    border-color: #cbd5e0;
}

/* Responsive design */
@media (max-width: 768px) {
    .address-section {
        padding: 1.5rem;
    }

    .address-grid {
        grid-template-columns: 1fr;
        gap: 1.2rem;
    }

    .address-section-title {
        font-size: 1.2rem;
        margin-bottom: 1.5rem;
    }

    .form-input {
        padding: 0.75rem 1rem;
    }
}

/* Animation for select elements */
select.form-input {
    appearance: none;
    background-image: url("data:image/svg+xml;charset=UTF-8,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3e%3cpolyline points='6 9 12 15 18 9'%3e%3c/polyline%3e%3c/svg%3e");
    background-repeat: no-repeat;
    background-position: right 1rem center;
    background-size: 1em;
    padding-right: 2.5rem;
}
</style>
