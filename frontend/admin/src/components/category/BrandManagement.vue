<script setup>
import { ref, onMounted, computed, watch } from "vue";
import {
    getBrands,
    createBrand,
    updateBrand,
    activateBrand,
    deactivateBrand,
} from "@/services/brandService";
import emitter from "../../utils/evenBus.js";

// State
const brands = ref([]);
const loading = ref(false);
const showModal = ref(false);
const isEditing = ref(false);
const formData = ref({
    name: "",
    logoFile: null,
    description: "",
});
const hasProductLines = ref(false);
const logoPreview = ref("");
const fileInput = ref(null);
const searchQuery = ref("");
const statusFilter = ref("all"); // Add status filter state: "all", "active", "inactive"
const showStatusModal = ref(false); // For activation/deactivation confirmation
const brandToToggle = ref(null);

// Function to format logo URL
const formatLogoUrl = (logoPath) => {
    if (!logoPath) return null;

    // If logo is already a full URL (starts with http or https) or is base64 data
    if (logoPath.startsWith("http") || logoPath.startsWith("data:"))
        return logoPath;

    // Get base URL from API config
    const apiUrl = import.meta.env.VITE_API_URL || "";
    const baseUrl = apiUrl.includes("/api") ? apiUrl.split("/api")[0] : "";

    // Normalize the path (convert \ to /)
    const normalizedPath = logoPath.replace(/\\/g, "/");

    // Check if path starts with /
    const path = normalizedPath.startsWith("/")
        ? normalizedPath
        : `/${normalizedPath}`;

    return `${baseUrl}${path}`;
};

// Computed properties
const filteredBrands = computed(() => {
    // Giờ đây chỉ lọc theo từ khóa tìm kiếm
    // Việc lọc theo trạng thái đã được thực hiện ở API
    if (!searchQuery.value) return brands.value;

    const query = searchQuery.value.toLowerCase().trim();
    return brands.value.filter(
        (brand) =>
            brand.name.toLowerCase().includes(query) ||
            (brand.description &&
                brand.description.toLowerCase().includes(query))
    );
});

const activeCount = computed(() => {
    return brands.value.filter((brand) => brand.isActive).length;
});

const productLinesCount = computed(() => {
    // Tính tổng số dòng sản phẩm từ tất cả thương hiệu
    console.log("Brands:", brands.value);
    return brands.value.reduce((total, brand) => {
        return total + (brand.productLines?.length || 0);
    }, 0);
});

// Methods
const getBrandProductLinesCount = (brandId) => {
    const brand = brands.value.find((b) => b.id === brandId);
    console.log("Brand found:", brand);
    return brand?.productLines?.length || 0;
};

// Fetch all brands
const fetchBrands = async () => {
    loading.value = true;
    try {
        // Truyền tham số isActive theo bộ lọc được chọn và đảm bảo includeProductLines=true
        const params = {
            includeProductLines: true,
        };
        if (statusFilter.value !== "all") {
            params.isActive = statusFilter.value === "active";
        }
        const response = await getBrands(params);   
        brands.value = response.data.data;
    } catch (error) {
        console.error("Error fetching brands:", error);
        // Hiển thị thông báo lỗi (có thể thêm later)
    } finally {
        loading.value = false;
    }
};

// Open modal to add new brand
const openAddBrandModal = () => {
    isEditing.value = false;
    formData.value = {
        name: "",
        logoFile: null,
        description: "",
    };
    logoPreview.value = "";
    showModal.value = true;
};

// Open modal to edit brand
const editBrand = (brand) => {
    isEditing.value = true;
    formData.value = {
        id: brand.id,
        name: brand.name,
        logoFile: null, // For file upload
        description: brand.description || "",
        existingLogo: brand.logo || "", // Store the existing logo path
    };
    logoPreview.value = brand.logo ? formatLogoUrl(brand.logo) : "";
    showModal.value = true;
};

// Handle file upload
const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (!file) return;

    // Validate file type
    const allowedTypes = [
        "image/jpeg",
        "image/png",
        "image/jpg",
    ];
    if (!allowedTypes.includes(file.type)) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Định dạng tệp không hợp lệ. Vui lòng chọn tệp PNG, JPG.",
        });
        return;
    }

    // File size validation (2MB)
    if (file.size > 2 * 1024 * 1024) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Kích thước tệp quá lớn. Vui lòng chọn tệp nhỏ hơn 2MB.",
        });
        return;
    }

    // Save file for upload
    formData.value.logoFile = file;

    // Create preview
    const reader = new FileReader();
    reader.onload = (e) => {
        logoPreview.value = e.target.result;
    };
    reader.readAsDataURL(file);
};

// Clear logo
const clearLogo = (event) => {
    if (event) {
        event.stopPropagation();
    }
    formData.value.logoFile = null;
    logoPreview.value = "";
    if (fileInput.value) {
        fileInput.value.value = "";
    }
};

// Submit form (create or update)
const submitForm = async () => {
    loading.value = true;

    try {
        const data = new FormData();

        // For edit operations, only include changed fields
        if (isEditing.value) {
            // Only add Name if it has been provided
            data.append("Name", formData.value.name);

            // Only add Logo if a new file was selected
            if (formData.value.logoFile) {
                data.append("Logo", formData.value.logoFile);
            }

            // Always include description field, even if empty
            data.append("Description", formData.value.description || "");
        } else {
            // For create operation, include all required fields
            data.append("Name", formData.value.name);
            if (formData.value.logoFile) {
                data.append("Logo", formData.value.logoFile);
            }
            data.append("Description", formData.value.description || "");
            // New brands are active by default in the backend
        }

        let response;
        if (isEditing.value) {
            response = await updateBrand(formData.value.id, data);
            emitter.emit("show-notification", {
                status: "success",
                message: "Cập nhật thương hiệu thành công!",
            });
        } else {
            response = await createBrand(data);
            emitter.emit("show-notification", {
                status: "success",
                message: "Thêm thương hiệu thành công!",
            });
        }

        // Make sure to refresh the brands list with a fresh API call
        await fetchBrands();

        // Close the modal only after successful data refresh
        closeModal();
    } catch (error) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Có lỗi xảy ra trong quá trình xử lý yêu cầu.",
        });
    } finally {
        loading.value = false;
    }
};

// Close modal
const closeModal = () => {
    showModal.value = false;
    formData.value = {
        name: "",
        logoFile: null,
        description: "",
    };
    logoPreview.value = "";
    if (fileInput.value) {
        fileInput.value.value = "";
    }
};

// Confirm status toggle
const confirmToggleStatus = async (brand) => {
    brandToToggle.value = brand;
    hasProductLines.value = brand.productLines && brand.productLines.length > 0;
    showStatusModal.value = true;
};

// Cancel status toggle
const cancelStatusToggle = () => {
    showStatusModal.value = false;
    brandToToggle.value = null;
    hasProductLines.value = false;
};

// Toggle brand status
const confirmToggleBrandStatus = async () => {
    if (!brandToToggle.value) return;

    loading.value = true;
    try {
        if (brandToToggle.value.isActive) {
            // Deactivate
            await deactivateBrand(brandToToggle.value.id);
            emitter.emit("show-notification", {
                status: "success",
                message: "Vô hiệu hóa thương hiệu thành công!",
            });
        } else {
            // Activate
            await activateBrand(brandToToggle.value.id);
            emitter.emit("show-notification", {
                status: "success",
                message: "Kích hoạt thương hiệu thành công!",
            });
        }

        await fetchBrands();
        showStatusModal.value = false;
        brandToToggle.value = null;
    } catch (error) {
        emitter.emit("show-notification", {
            status: "error",
            message: "Có lỗi xảy ra trong quá trình xử lý yêu cầu.",
        });
    } finally {
        loading.value = false;
    }
};

// Load data when component mounts
onMounted(async () => {
    await fetchBrands();
});

// Watch for status filter changes and reload data
watch(statusFilter, async () => {
    await fetchBrands();
});
</script>

<template>
    <div class="brand-management">
        <div class="section-header">
            <div class="left-section">
                <h2><i class="fas fa-trademark"></i> Quản lý Thương hiệu</h2>
                <p>Quản lý các thương hiệu điện thoại di động</p>
            </div>

            <div class="right-section">
                <div class="search-box">
                    <i class="fas fa-search"></i>
                    <input
                        type="text"
                        v-model="searchQuery"
                        placeholder="Tìm kiếm thương hiệu..."
                    />
                </div>

                <!-- Status Filter -->
                <div class="status-filter">
                    <i class="fas fa-filter"></i>
                    <select v-model="statusFilter">
                        <option value="all">Tất cả trạng thái</option>
                        <option value="active">Đang kích hoạt</option>
                        <option value="inactive">Chưa kích hoạt</option>
                    </select>
                </div>

                <button @click="openAddBrandModal" class="add-button">
                    <i class="fas fa-plus"></i> Thêm thương hiệu
                </button>
            </div>
        </div>

        <!-- Status Cards -->
        <div class="status-cards">
            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-trademark"></i>
                </div>
                <div class="status-content">
                    <h3>{{ brands.length }}</h3>
                    <p>Tổng số thương hiệu</p>
                </div>
            </div>

            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-check-circle"></i>
                </div>
                <div class="status-content">
                    <h3>{{ activeCount }}</h3>
                    <p>Thương hiệu đang kích hoạt</p>
                </div>
            </div>

            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-mobile-alt"></i>
                </div>
                <div class="status-content">
                    <h3>{{ productLinesCount }}</h3>
                    <p>Tổng số dòng sản phẩm</p>
                </div>
            </div>
        </div>

        <!-- Brands table -->
        <div class="data-card">
            <div v-if="loading" class="loading-state">
                <div class="spinner"></div>
                <p>Đang tải dữ liệu...</p>
            </div>
            <div v-else-if="filteredBrands.length === 0" class="empty-state">
                <i class="fas fa-box-open"></i>
                <p>Không có thương hiệu nào</p>
                <button @click="openAddBrandModal" class="action-button">
                    <i class="fas fa-plus"></i> Thêm thương hiệu
                </button>
            </div>
            <table v-else class="data-table">
                <thead>
                    <tr>
                        <th>Thương hiệu</th>
                        <th>Logo</th>
                        <th>Mô tả</th>
                        <th>Trạng thái</th>
                        <th>Số dòng SP</th>
                        <th>Hành động</th>
                    </tr>
                </thead>
                <tbody>
                    <tr
                        v-for="brand in filteredBrands"
                        :key="brand.id"
                        class="data-row"
                    >
                        <td class="name-cell">{{ brand.name }}</td>
                        <td class="logo-cell">
                            <div class="logo-wrapper">
                                <img
                                    v-if="brand.logo"
                                    :src="formatLogoUrl(brand.logo)"
                                    :alt="brand.name"
                                />
                                <div v-else class="no-logo">
                                    <i class="fas fa-image"></i>
                                </div>
                            </div>
                        </td>
                        <td class="description-cell">
                            {{ brand.description || "Không có mô tả" }}
                        </td>
                        <td class="status-cell">
                            <span
                                :class="[
                                    'status-badge',
                                    brand.isActive ? 'active' : 'inactive',
                                ]"
                            >
                                {{
                                    brand.isActive
                                        ? "Đang kích hoạt"
                                        : "Không kích hoạt"
                                }}
                            </span>
                        </td>
                        <td class="count-cell">
                            {{ getBrandProductLinesCount(brand.id) }}
                        </td>
                        <td class="actions-cell">
                            <button
                                @click="editBrand(brand)"
                                class="edit-button"
                                title="Chỉnh sửa"
                            >
                                <i class="fas fa-edit"></i>
                            </button>
                            <button
                                @click="confirmToggleStatus(brand)"
                                :class="[
                                    'status-button',
                                    brand.isActive
                                        ? 'deactivate-button'
                                        : 'activate-button',
                                ]"
                                :title="
                                    brand.isActive ? 'Vô hiệu hóa' : 'Kích hoạt'
                                "
                            >
                                <i
                                    class="fas"
                                    :class="
                                        brand.isActive
                                            ? 'fa-toggle-off'
                                            : 'fa-toggle-on'
                                    "
                                ></i>
                            </button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <!-- Add/Edit Brand Modal -->
        <div v-if="showModal" class="modal-backdrop">
            <div class="modal-container">
                <div class="modal-header">
                    <h3>
                        {{
                            isEditing
                                ? "Chỉnh sửa thương hiệu"
                                : "Thêm thương hiệu mới"
                        }}
                    </h3>
                    <button @click="closeModal" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <form @submit.prevent="submitForm" class="modal-form">
                    <div class="form-group">
                        <label
                            >Tên thương hiệu
                            <span class="required">*</span></label
                        >
                        <input
                            v-model="formData.name"
                            type="text"
                            placeholder="Nhập tên thương hiệu"
                            required
                        />
                    </div>

                    <div class="form-group">
                        <label>Ảnh minh họa</label>
                        <div
                            class="upload-area"
                            @click="() => fileInput && fileInput.click()"
                        >
                            <img
                                v-if="logoPreview"
                                :src="
                                    logoPreview.startsWith('data:')
                                        ? logoPreview
                                        : formatLogoUrl(logoPreview)
                                "
                                alt="Logo Preview"
                                class="image-preview"
                            />
                            <div v-else class="upload-placeholder">
                                <i class="fas fa-cloud-upload-alt"></i>
                                <span>Nhấp để tải lên ảnh (SVG, PNG, JPG)</span>
                            </div>
                            <input
                                type="file"
                                ref="fileInput"
                                class="hidden-input"
                                @change="handleFileChange"
                                accept="image/jpeg,image/png,image/jpg,image/svg+xml"
                            />
                        </div>
                        <div v-if="logoPreview" class="preview-actions">
                            <button
                                type="button"
                                @click="clearLogo"
                                class="remove-button"
                            >
                                <i class="fas fa-trash-alt"></i> Xóa ảnh
                            </button>
                        </div>
                    </div>

                    <div class="form-group">
                        <label>Mô tả</label>
                        <textarea
                            v-model="formData.description"
                            placeholder="Nhập mô tả thương hiệu"
                            rows="3"
                        ></textarea>
                    </div>

                    <div class="form-actions">
                        <button
                            type="button"
                            @click="closeModal"
                            class="cancel-button"
                        >
                            <i class="fas fa-times"></i> Hủy
                        </button>
                        <button
                            type="submit"
                            class="submit-button"
                            :disabled="loading"
                        >
                            <span v-if="loading" class="spinner small"></span>
                            <i
                                v-else
                                class="fas"
                                :class="isEditing ? 'fa-save' : 'fa-plus'"
                            ></i>
                            {{ isEditing ? "Cập nhật" : "Thêm mới" }}
                        </button>
                    </div>
                </form>
            </div>
        </div>

        <!-- Status Toggle Confirmation Modal -->
        <div v-if="showStatusModal" class="modal-backdrop">
            <div class="modal-container warning-modal">
                <div
                    class="modal-header"
                    :class="{ warning: brandToToggle?.isActive }"
                >
                    <h3>
                        {{
                            brandToToggle?.isActive
                                ? "Xác nhận vô hiệu hóa thương hiệu"
                                : "Xác nhận kích hoạt thương hiệu"
                        }}
                    </h3>
                    <button @click="cancelStatusToggle" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <div class="modal-body text-center">
                    <div
                        class="warning-icon"
                        :class="{ 'activate-icon': !brandToToggle?.isActive }"
                    >
                        <i
                            class="fas"
                            :class="
                                brandToToggle?.isActive
                                    ? 'fa-toggle-off'
                                    : 'fa-toggle-on'
                            "
                        ></i>
                    </div>
                    <p class="warning-message">
                        Bạn có chắc chắn muốn
                        {{
                            brandToToggle?.isActive
                                ? "vô hiệu hóa"
                                : "kích hoạt"
                        }}
                        thương hiệu
                        <strong>"{{ brandToToggle?.name }}"</strong>?
                    </p>

                    <!-- PHẦN QUAN TRỌNG: Thông báo tác động -->
                    <div
                        class="warning-details"
                        :class="{
                            'activate-details': !brandToToggle?.isActive,
                        }"
                    >
                        <i class="fas fa-info-circle"></i>
                        <div class="warning-text">
                            <p v-if="hasProductLines" class="warning-count">
                                <span>Thương hiệu này có</span>
                                <strong>{{
                                    getBrandProductLinesCount(brandToToggle?.id)
                                }}</strong>
                                <span>dòng sản phẩm.</span>
                            </p>
                            <p
                                v-if="brandToToggle?.isActive"
                                class="warning-impact"
                            >
                                <strong>Lưu ý:</strong> Vô hiệu hóa thương hiệu
                                sẽ ẩn tất cả dòng sản phẩm và sản phẩm thuộc
                                thương hiệu này.
                            </p>
                            <p v-else class="warning-impact">
                                <strong>Lưu ý:</strong> Kích hoạt thương hiệu sẽ
                                hiển thị tất cả dòng sản phẩm và sản phẩm thuộc
                                thương hiệu này (trừ những dòng sản phẩm đã bị
                                vô hiệu hóa thủ công).
                            </p>
                        </div>
                    </div>
                </div>

                <div class="modal-actions">
                    <button @click="cancelStatusToggle" class="cancel-button">
                        <i class="fas fa-arrow-left"></i> Quay lại
                    </button>
                    <button
                        @click="confirmToggleBrandStatus"
                        :class="
                            brandToToggle?.isActive
                                ? 'deactivate-confirm-button'
                                : 'activate-confirm-button'
                        "
                        :disabled="loading"
                    >
                        <span v-if="loading" class="spinner small"></span>
                        <i
                            v-else
                            class="fas"
                            :class="
                                brandToToggle?.isActive
                                    ? 'fa-toggle-off'
                                    : 'fa-toggle-on'
                            "
                        ></i>
                        {{
                            brandToToggle?.isActive
                                ? "Vô hiệu hóa"
                                : "Kích hoạt"
                        }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.brand-management {
    width: 100%;
}

.section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
}

.left-section h2 {
    font-size: 1.25rem;
    color: #333;
    margin: 0 0 0.25rem 0;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.left-section p {
    color: #666;
    margin: 0;
    font-size: 0.9rem;
}

.right-section {
    display: flex;
    gap: 1rem;
    align-items: center;
}

.search-box {
    position: relative;
    width: 250px;
}

.search-box i {
    position: absolute;
    left: 10px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
}

.search-box input {
    width: 100%;
    padding: 0.6rem 0.6rem 0.6rem 2rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
    transition: all 0.3s;
}

.search-box input:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.add-button {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.6rem 1rem;
    background-color: var(--primary-color);
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.3s;
}

.add-button:hover {
    background-color: #e94e9c;
}

.status-cards {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
    gap: 1rem;
    margin-bottom: 1.5rem;
}

.status-card {
    background-color: white;
    border-radius: 12px;
    padding: 1.25rem;
    display: flex;
    align-items: center;
    gap: 1rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    transition: all 0.3s;
}

.status-card:hover {
    transform: translateY(-3px);
    box-shadow: 0 4px 12px rgba(248, 110, 211, 0.1);
}

.icon-wrapper {
    width: 48px;
    height: 48px;
    border-radius: 12px;
    background-color: #fff5fc;
    color: var(--primary-color);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.25rem;
}

.status-content h3 {
    font-size: 1.5rem;
    margin: 0 0 0.25rem 0;
    color: #333;
}

.status-content p {
    margin: 0;
    color: #666;
    font-size: 0.85rem;
}

.data-card {
    background-color: white;
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    padding: 1.5rem;
    overflow: hidden;
}

.loading-state,
.empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 3rem 0;
    color: #666;
}

.spinner {
    width: 40px;
    height: 40px;
    border: 3px solid rgba(248, 110, 211, 0.1);
    border-top: 3px solid var(--primary-color);
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin-bottom: 1rem;
}

.spinner.small {
    width: 16px;
    height: 16px;
    border-width: 2px;
    margin: 0;
    margin-right: 0.5rem;
}

@keyframes spin {
    0% {
        transform: rotate(0deg);
    }
    100% {
        transform: rotate(360deg);
    }
}

.empty-state i {
    font-size: 3rem;
    color: #ddd;
    margin-bottom: 1rem;
}

.empty-state p {
    margin-bottom: 1rem;
}

.action-button {
    padding: 0.5rem 1rem;
    background-color: var(--primary-color);
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.data-table {
    width: 100%;
    border-collapse: collapse;
    table-layout: fixed;
}

.data-table th {
    text-align: left;
    padding: 1rem;
    color: #666;
    border-bottom: 1px solid #eee;
    font-weight: 600;
    font-size: 0.85rem;
    text-transform: uppercase;
}

.data-table th:nth-child(1) {
    width: 18%;
} /* THƯƠNG HIỆU */
.data-table th:nth-child(2) {
    width: 10%;
} /* LOGO */
.data-table th:nth-child(3) {
    width: 40%;
} /* MÔ TẢ */
.data-table th:nth-child(4) {
    width: 15%;
} /* TRẠNG THÁI */
.data-table th:nth-child(5) {
    width: 8%;
} /* SỐ DÒNG SP */
.data-table th:nth-child(6) {
    width: 9%;
} /* HÀNH ĐỘNG */

.data-row {
    border-bottom: 1px solid #eee;
    transition: background-color 0.2s;
}

.data-row:hover {
    background-color: #f9f9f9;
}

.data-row td {
    padding: 1rem;
    color: #333;
    vertical-align: middle;
}

.name-cell {
    font-weight: 500;
}

.logo-cell {
    width: 10%;
}

.logo-wrapper {
    width: 40px;
    height: 40px;
    border-radius: 8px;
    overflow: hidden;
    background-color: #f9f9f9;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 1px solid #eee;
}

.logo-wrapper img {
    width: 100%;
    height: 100%;
    object-fit: contain;
}

.no-logo {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #ccc;
}

.description-cell {
    max-width: 250px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.status-badge {
    padding: 0.25rem 0.5rem;
    border-radius: 12px;
    font-size: 0.75rem;
    font-weight: 500;
}

.status-badge.active {
    background-color: #e6f7ea;
    color: #22c55e;
}

.status-badge.inactive {
    background-color: #fee2e2;
    color: #ef4444;
}

.count-cell {
    text-align: center;
    font-weight: 500;
}

.actions-cell {
    display: flex;
    gap: 0.5rem;
}

.edit-button,
.status-button {
    width: 32px;
    height: 32px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    border: none;
    cursor: pointer;
    transition: all 0.2s;
}

.edit-button {
    background-color: #e6f7ea;
    color: #22c55e;
}

.edit-button:hover {
    background-color: #22c55e;
    color: white;
}

.activate-button {
    background-color: #e6f7ea;
    color: #22c55e;
}

.activate-button:hover {
    background-color: #22c55e;
    color: white;
}

.deactivate-button {
    background-color: #fee2e2;
    color: #ef4444;
}

.deactivate-button:hover {
    background-color: #ef4444;
    color: white;
}

.activate-confirm-button {
    background-color: #22c55e;
    border: none;
    color: white;
}

.activate-confirm-button:hover {
    background-color: #16a34a;
}

.deactivate-confirm-button {
    background-color: #ef4444;
    border: none;
    color: white;
}

.deactivate-confirm-button:hover {
    background-color: #dc2626;
}

.activate-icon {
    background-color: #e6f7ea;
    color: #22c55e;
}

.activate-details {
    background-color: #e6f7ea;
    border: 1px solid #c8e6c9;
    color: #2e7d32;
}

.warning-details {
    display: flex;
    align-items: flex-start;
    gap: 0.8rem;
    padding: 1.25rem;
    background-color: #ffeeee;
    border: 1px solid #ffcccc;
    border-radius: 8px;
    color: #d32f2f;
    text-align: left;
    font-size: 1rem;
    margin-top: 1.5rem;
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.05);
}

.warning-details.activate-details {
    background-color: #e8f5e9;
    border: 1px solid #c8e6c9;
    color: #2e7d32;
}

.warning-text {
    flex: 1;
}

.warning-count {
    margin: 0 0 0.75rem 0;
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 0.35rem;
}

.warning-impact {
    margin-top: 0.75rem;
    font-size: 1rem;
}

.warning-impact p {
    margin: 0;
    font-weight: 500;
    line-height: 1.5;
}

.warning-details i {
    font-size: 1.5rem;
    margin-top: 0.2rem;
}

/* Modal action buttons */
.modal-actions {
    padding: 1.25rem 1.5rem;
    border-top: 1px solid #eee;
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
}

.deactivate-confirm-button,
.activate-confirm-button {
    min-width: 140px;
    padding: 0.75rem 1.25rem;
    border-radius: 8px;
    font-weight: 500;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    cursor: pointer;
    transition: all 0.2s;
}

.deactivate-confirm-button {
    background-color: #ef4444;
    border: none;
    color: white;
    box-shadow: 0 2px 4px rgba(239, 68, 68, 0.2);
}

.activate-confirm-button {
    background-color: #22c55e;
    border: none;
    color: white;
    box-shadow: 0 2px 4px rgba(34, 197, 94, 0.2);
}

.deactivate-confirm-button:hover {
    background-color: #dc2626;
    box-shadow: 0 4px 6px rgba(239, 68, 68, 0.3);
}

.activate-confirm-button:hover {
    background-color: #16a34a;
    box-shadow: 0 4px 6px rgba(34, 197, 94, 0.3);
}

/* Modal Styles */
.modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
    backdrop-filter: blur(3px);
}

.modal-container {
    background-color: white;
    border-radius: 12px;
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
    width: 100%;
    max-width: 500px;
    max-height: 90vh;
    overflow-y: auto;
}

.modal-header {
    padding: 1.25rem 1.5rem;
    border-bottom: 1px solid #eee;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.modal-header.warning {
    border-bottom-color: #fee2e2;
    color: #ef4444;
}

.modal-header h3 {
    margin: 0;
    font-size: 1.25rem;
    color: #333;
}

.close-button {
    background: none;
    border: none;
    color: #666;
    font-size: 1rem;
    cursor: pointer;
    width: 32px;
    height: 32px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s;
}

.close-button:hover {
    background-color: #f9f9f9;
    color: #333;
}

.modal-form {
    padding: 1.5rem;
}

.form-group {
    margin-bottom: 1.25rem;
}

.form-group label {
    display: block;
    margin-bottom: 0.5rem;
    font-weight: 500;
    color: #333;
}

.form-group input[type="text"],
.form-group textarea {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.95rem;
    transition: all 0.3s;
}

.form-group input[type="text"]:focus,
.form-group textarea:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.required {
    color: #ef4444;
}

.upload-area {
    border: 2px dashed #ddd;
    border-radius: 8px;
    padding: 2rem;
    text-align: center;
    cursor: pointer;
    transition: all 0.3s;
    margin-bottom: 1rem;
}

.upload-area:hover {
    border-color: var(--primary-color);
    background-color: #fff5fc;
}

.upload-placeholder {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.5rem;
    color: #666;
}

.upload-placeholder i {
    font-size: 2rem;
    color: #999;
    margin-bottom: 0.5rem;
}

.upload-placeholder p {
    margin: 0;
    font-weight: 500;
    margin-bottom: 0.25rem;
}

.upload-placeholder span {
    font-size: 0.85rem;
    color: #999;
}

.logo-preview-container {
    position: relative;
    display: inline-block;
}

.logo-preview {
    max-width: 100%;
    max-height: 150px;
    margin: 0 auto;
    display: block;
    border-radius: 4px;
}

.preview-actions {
    display: flex;
    justify-content: center;
    margin-top: 0.75rem;
}

.remove-button {
    background: none;
    border: none;
    color: #ef4444;
    cursor: pointer;
    font-size: 0.9rem;
    display: flex;
    align-items: center;
    gap: 0.25rem;
}

.hidden-input {
    display: none;
}

.form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    margin-top: 1.5rem;
}

.cancel-button,
.submit-button {
    padding: 0.75rem 1.25rem;
    border-radius: 8px;
    font-weight: 500;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
    transition: all 0.2s;
}

.cancel-button {
    background-color: #f9f9f9;
    border: 1px solid #ddd;
    color: #666;
}

.cancel-button:hover {
    background-color: #eee;
}

.submit-button {
    background-color: var(--primary-color);
    border: none;
    color: white;
}

.submit-button:hover {
    background-color: #e94e9c;
}

.submit-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

.modal-body {
    padding: 1.5rem;
}

.text-center {
    text-align: center;
}

.warning-icon {
    width: 64px;
    height: 64px;
    border-radius: 50%;
    background-color: #fee2e2;
    color: #ef4444;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 2rem;
    margin: 0 auto 1.5rem auto;
}

.warning-message {
    font-size: 1.1rem;
    color: #333;
    margin-bottom: 1.5rem;
}

.modal-actions {
    padding: 1.25rem 1.5rem;
    border-top: 1px solid #eee;
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
}

.status-filter {
    position: relative;
    width: 200px;
}

.status-filter i {
    position: absolute;
    left: 10px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
    pointer-events: none;
}

.status-filter select {
    width: 100%;
    padding: 0.6rem 0.6rem 0.6rem 2rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.9rem;
    appearance: none;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%23666' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpolyline points='6 9 12 15 18 9'%3E%3C/polyline%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: right 10px center;
    background-size: 16px;
    transition: all 0.3s;
}

.status-filter select:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

@media (max-width: 768px) {
    .section-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
    }

    .right-section {
        width: 100%;
        flex-direction: column;
    }

    .search-box {
        width: 100%;
    }

    .add-button {
        width: 100%;
        justify-content: center;
    }

    .status-cards {
        grid-template-columns: 1fr;
    }

    .data-table {
        display: block;
        overflow-x: auto;
    }
}
</style>
