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
    return addressService.formatFullAddress(addressData.value);
});

// Parse địa chỉ từ chuỗi ban đầu
const parseAddress = async (addressString) => {
    if (!addressString) return;

    try {
        // Đợi để đảm bảo provinces đã được tải
        if (provinces.value.length === 0) {
            await fetchProvinces();
        }

        // Giả định địa chỉ có định dạng: Chi tiết, Phường/Xã, Quận/Huyện, Tỉnh/Thành phố
        const addressParts = addressString.split(", ");

        if (addressParts.length < 2) {
            // Nếu không đủ thành phần, chỉ lưu vào phần chi tiết
            addressData.value.detail = addressString;
            return;
        }

        // Tìm tỉnh/thành phố (phần cuối cùng của địa chỉ)
        const provinceName = addressParts[addressParts.length - 1];
        const province = provinces.value.find(
            (p) =>
                p.name.toLowerCase() === provinceName.toLowerCase() ||
                provinceName.toLowerCase().includes(p.name.toLowerCase()) ||
                p.name.toLowerCase().includes(provinceName.toLowerCase())
        );

        if (province) {
            addressData.value.province = province;

            // Tải quận/huyện
            await fetchDistricts();

            // Tìm quận/huyện (phần gần cuối)
            const districtName = addressParts[addressParts.length - 2];
            const district = districts.value.find(
                (d) =>
                    d.name.toLowerCase() === districtName.toLowerCase() ||
                    districtName.toLowerCase().includes(d.name.toLowerCase()) ||
                    d.name.toLowerCase().includes(districtName.toLowerCase())
            );

            if (district) {
                addressData.value.district = district;

                // Tải phường/xã
                await fetchWards();

                // Nếu có đủ 3 phần trở lên thì mới có phường/xã
                if (addressParts.length >= 3) {
                    const wardName = addressParts[addressParts.length - 3];
                    const ward = wards.value.find(
                        (w) =>
                            w.name.toLowerCase() === wardName.toLowerCase() ||
                            wardName
                                .toLowerCase()
                                .includes(w.name.toLowerCase()) ||
                            w.name
                                .toLowerCase()
                                .includes(wardName.toLowerCase())
                    );

                    if (ward) {
                        addressData.value.ward = ward;

                        // Nếu còn phần ở đầu thì là chi tiết
                        if (addressParts.length > 3) {
                            addressData.value.detail = addressParts
                                .slice(0, addressParts.length - 3)
                                .join(", ");
                        }
                    } else {
                        // Nếu không tìm thấy phường/xã, còn các phần ở đầu thì là chi tiết
                        addressData.value.detail = addressParts
                            .slice(0, addressParts.length - 2)
                            .join(", ");
                    }
                } else {
                    // Nếu chỉ có 2 phần (tỉnh và quận), không có chi tiết
                    addressData.value.detail = "";
                }
            } else {
                // Nếu không tìm thấy quận/huyện
                addressData.value.detail = addressParts
                    .slice(0, addressParts.length - 1)
                    .join(", ");
            }
        } else {
            // Nếu không tìm thấy tỉnh/thành phố, lưu toàn bộ vào chi tiết
            addressData.value.detail = addressString;
        }
    } catch (error) {
        console.error("Error parsing address:", error);
        // Nếu có lỗi, đặt toàn bộ vào phần chi tiết
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
    // Reset quận/huyện và phường/xã
    addressData.value.district = null;
    addressData.value.ward = null;
    districts.value = [];
    wards.value = [];

    if (addressData.value.province) {
        await fetchDistricts();
    }
};

/**
 * Xử lý khi thay đổi quận/huyện
 */
const handleDistrictChange = async () => {
    // Reset phường/xã
    addressData.value.ward = null;
    wards.value = [];

    if (addressData.value.district) {
        await fetchWards();
    }
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

// Theo dõi sự thay đổi của địa chỉ để emit ra ngoài
watch(
    [
        () => addressData.value.province,
        () => addressData.value.district,
        () => addressData.value.ward,
        () => addressData.value.detail,
    ],
    () => {
        const fullAddress = addressService.formatFullAddress(addressData.value);
        emit("update:address", fullAddress);
    },
    { deep: true }
);

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
    border: 1px solid #eee;
    border-radius: 12px;
    padding: 1.8rem;
    margin-bottom: 2rem;
    background-color: #fff;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.03);
}

.address-section-title {
    font-size: 1.25rem;
    margin-bottom: 1.5rem;
    color: #333;
    text-align: center;
    font-weight: 600;
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
.address-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1.5rem;
    margin-bottom: 0.8rem;
}

.form-group {
    margin-bottom: 1.5rem;
    position: relative;
}

.form-group label {
    display: block;
    font-weight: 500;
    color: #666;
    margin-bottom: 0.5rem;
    font-size: 0.9rem;
}

.form-input {
    width: 100%;
    padding: 0.75rem 1rem;
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 1rem;
    transition: all 0.3s ease;
}

.form-input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.15);
    outline: none;
}

.form-input:disabled {
    background-color: #f5f5f5;
    cursor: not-allowed;
    color: #999;
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
    margin-bottom: 0.5rem;
    color: #333;
    font-size: 0.95rem;
}

.address-preview {
    padding: 1rem 1.2rem;
    background-color: #fff;
    border: 1px solid #ddd;
    border-radius: 6px;
    font-size: 1rem;
    line-height: 1.6;
    color: #333;
    min-height: 60px;
    box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.03);
}

@media (max-width: 768px) {
    .address-grid {
        grid-template-columns: 1fr;
        gap: 1rem;
    }

    .address-section-title {
        font-size: 1.2rem;
    }

    .form-input {
        padding: 0.7rem 0.9rem;
    }
}
</style>
