<script setup>
import { ref, onMounted, computed, watch } from "vue";
import axios from "axios";
import {
    getProductLines,
    getProductLineById,
    createProductLine,
    updateProductLine,
    deleteProductLine as deleteProductLineAPI,
} from "@/services/productLineService.js";
import { getBrands } from "@/services/brandService.js"; // Thêm import getBrands

// State
const productLines = ref([]);
const brands = ref([]);
const products = ref([]);
const loading = ref(false);
const showModal = ref(false);
const showDeleteModal = ref(false);
const isEditing = ref(false);
const formData = ref({
    name: "",
    brandId: null,
    description: "",
    imageFile: null,
    isActive: true,
});
const productLineToDelete = ref(null);
const hasProducts = ref(false);
const imagePreview = ref("");
const fileInput = ref(null);
const searchQuery = ref("");
const statusFilter = ref("all"); // Thêm bộ lọc trạng thái: "all", "active", "inactive"

// API base URL
const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5000/api/v1";

// Function to format image URL
const formatImageUrl = (imagePath) => {
    if (!imagePath) return null;

    // If image is already a full URL (starts with http or https)
    if (imagePath.startsWith("http")) return imagePath;

    // Get base URL from API configuration
    const apiUrl = import.meta.env.VITE_API_URL || "";
    const baseUrl = apiUrl.includes("/api") ? apiUrl.split("/api")[0] : "";

    // Normalize the path (convert \ to /)
    const normalizedPath = imagePath.replace(/\\/g, "/");

    // Check if path starts with /
    const path = normalizedPath.startsWith("/")
        ? normalizedPath
        : `/${normalizedPath}`;

    return `${baseUrl}${path}`;
};

// Computed properties
const filteredProductLines = computed(() => {
    // Chỉ lọc theo từ khóa tìm kiếm - việc lọc theo trạng thái sẽ được xử lý ở backend
    if (!searchQuery.value) return productLines.value;

    const query = searchQuery.value.toLowerCase().trim();
    return productLines.value.filter(
        (pl) =>
            pl.name.toLowerCase().includes(query) ||
            (pl.description && pl.description.toLowerCase().includes(query)) ||
            (pl.brandName && pl.brandName.toLowerCase().includes(query))
    );
});

// Organize product lines by brand
const organizedProductLines = computed(() => {
    const brandMap = {};

    // Create brand map for organizing
    brands.value.forEach((brand) => {
        brandMap[brand.id] = {
            id: brand.id,
            name: brand.name,
            logo: brand.logo,
            productLines: [],
        };
    });

    // Assign product lines to brands
    filteredProductLines.value.forEach((pl) => {
        if (brandMap[pl.brandId]) {
            brandMap[pl.brandId].productLines.push(pl);
        }
    });

    // Convert to array and filter out empty brands
    return Object.values(brandMap).filter((b) => b.productLines.length > 0);
});

const uniqueBrandCount = computed(() => {
    // Ưu tiên sử dụng brandName nếu có, nếu không thì sử dụng brandId
    const uniqueBrands = new Set();

    productLines.value.forEach((pl) => {
        if (pl.brandName) {
            uniqueBrands.add(pl.brandName);
        } else if (pl.brandId) {
            uniqueBrands.add(pl.brandId);
        }
    });

    return uniqueBrands.size;
});

const activeCount = computed(() => {
    return productLines.value.filter((pl) => pl.isActive).length;
});

// Methods
const getProductLineProductsCount = (productLineId) => {
    const productLine = productLines.value.find(
        (pl) => pl.id === productLineId
    );
    return productLine?.products?.length || 0;
};

// Phương thức lấy danh sách brands từ productLines thay vì gọi API
const extractBrandsFromProductLines = () => {
    // Tạo một Set để lưu trữ các brandName duy nhất
    const uniqueBrands = new Set();

    // Tạo một map để lưu trữ thông tin brand
    const brandMap = {};

    // Lặp qua tất cả productLines để thu thập thông tin brand
    productLines.value.forEach((productLine) => {
        if (productLine.brandName) {
            const key = productLine.brandId || productLine.brandName;
            // Nếu productLine có brandName, tạo hoặc cập nhật thông tin brand
            if (!brandMap[key]) {
                brandMap[key] = {
                    id: productLine.brandId,
                    name: productLine.brandName,
                    logo: productLine.brandLogo || null,
                };
                uniqueBrands.add(key);
            }
        }
    });

    // Chuyển đổi map thành mảng brands
    const extractedBrands = Array.from(uniqueBrands).map((id) => brandMap[id]);
    console.log("Extracted brands from product lines:", extractedBrands);
    return extractedBrands;
};

// Fetch all product lines
const fetchProductLines = async () => {
    loading.value = true;
    try {
        // Thêm tham số isActive theo bộ lọc đã chọn
        const params = { includeProducts: true };
        if (statusFilter.value !== "all") {
            params.isActive = statusFilter.value === "active";
        }

        const response = await getProductLines(params);
        console.log("API Response:", response);

        // Lấy productLines từ response.data.productLines
        if (response.data && response.data.productLines) {
            productLines.value = response.data.productLines;
            console.log("Loaded product lines:", productLines.value);
        } else if (Array.isArray(response.data)) {
            // Nếu response.data là một mảng (trường hợp API trả về dạng mảng trực tiếp)
            productLines.value = response.data;
            console.log("Loaded product lines from array:", productLines.value);
        } else {
            console.error("Định dạng dữ liệu không hợp lệ:", response.data);
            productLines.value = [];
        }

        // Sau khi có danh sách productLines, trích xuất brands từ productLines
        brands.value = extractBrandsFromProductLines();
        console.log("Extracted brands from product lines:", brands.value);
    } catch (error) {
        console.error("Error fetching product lines:", error);
        productLines.value = [];
        brands.value = [];
    } finally {
        loading.value = false;
    }
};

// Fetch brands from API directly
const fetchBrands = async () => {
    try {
        const response = await getBrands();
        if (response.data && response.data.brands) {
            brands.value = response.data.brands;
        } else if (Array.isArray(response.data)) {
            brands.value = response.data;
        }
        console.log("Fetched brands from API:", brands.value);
    } catch (error) {
        console.error("Error fetching brands:", error);
    }
};

// Open modal to add new product line
const openAddProductLineModal = async () => {
    loading.value = true;
    try {
        // Fetch brands first to ensure we have the complete list
        await fetchBrands();

        isEditing.value = false;
        formData.value = {
            name: "",
            brandId: null,
            description: "",
            imageFile: null,
            isActive: true,
        };
        imagePreview.value = "";
        showModal.value = true;
    } catch (error) {
        console.error("Error preparing form:", error);
        alert("Có lỗi xảy ra khi chuẩn bị form thêm mới. Vui lòng thử lại.");
    } finally {
        loading.value = false;
    }
};

// Update editProductLine to match brand by name instead of ID
const editProductLine = async (productLine) => {
    loading.value = true;
    try {
        console.log("Editing product line:", productLine);

        // Lưu tên thương hiệu hiện tại trước khi tải danh sách thương hiệu
        const currentBrandName = productLine.brandName;
        console.log("Current brand name:", currentBrandName);

        // Fetch brands first to ensure we have the complete list
        await fetchBrands();

        // Tìm brand trong danh sách brands dựa trên tên thương hiệu
        const matchedBrand = brands.value.find(
            (brand) =>
                brand.name.toLowerCase() === currentBrandName?.toLowerCase()
        );

        console.log("Matched brand by name:", matchedBrand);
        console.log(
            "Available brands:",
            brands.value.map((b) => ({ id: b.id, name: b.name }))
        );

        // Sử dụng brandId từ brand đã tìm thấy (nếu có), nếu không thì giữ nguyên brandId hiện tại
        const brandIdToUse = matchedBrand
            ? matchedBrand.id
            : productLine.brandId;

        isEditing.value = true;
        formData.value = {
            id: productLine.id,
            name: productLine.name,
            brandId: brandIdToUse,
            description: productLine.description || "",
            imageFile: null,
            isActive:
                productLine.isActive !== undefined
                    ? productLine.isActive
                    : true,
        };

        console.log("Form data after setup:", formData.value);

        imagePreview.value = productLine.image || "";
        showModal.value = true;
    } catch (error) {
        console.error("Error preparing form:", error);
        alert("Có lỗi xảy ra khi chuẩn bị form chỉnh sửa. Vui lòng thử lại.");
    } finally {
        loading.value = false;
    }
};

// Handle file upload
const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (file) {
        if (file.size > 1024 * 1024) {
            alert("Kích thước tệp quá lớn. Vui lòng chọn tệp dưới 1MB.");
            return;
        }

        formData.value.imageFile = file;
        // Create preview
        imagePreview.value = URL.createObjectURL(file);
    }
};

// Clear image
const clearImage = () => {
    formData.value.imageFile = null;
    imagePreview.value = "";
    if (fileInput.value) {
        fileInput.value.value = "";
    }
};

// Submit form (create or update)
const submitForm = async () => {
    loading.value = true;

    try {
        const data = new FormData();

        // Thêm dữ liệu cho form data
        data.append("Name", formData.value.name);

        // Đảm bảo brandId là số nguyên và không phải null
        if (formData.value.brandId) {
            data.append("BrandId", parseInt(formData.value.brandId));
        } else {
            console.error("Không thể thêm dòng sản phẩm với brandId null");
            alert("Vui lòng chọn thương hiệu cho dòng sản phẩm");
            loading.value = false;
            return; // Dừng quá trình submit nếu không có brandId
        }

        if (formData.value.imageFile) {
            data.append("Image", formData.value.imageFile);
        }
        if (formData.value.description) {
            data.append("Description", formData.value.description || "");
        }
        data.append("IsActive", formData.value.isActive.toString());

        console.log("Sending form data with brandId:", formData.value.brandId);

        let response;
        if (isEditing.value) {
            // Sử dụng hàm từ service thay vì axios trực tiếp
            response = await updateProductLine(formData.value.id, data);
        } else {
            // Sử dụng hàm từ service thay vì axios trực tiếp
            response = await createProductLine(data);
        }

        console.log("API response:", response);

        // Make sure to refresh the product lines list with a fresh API call
        await fetchProductLines();

        // Close the modal only after successful data refresh
        closeModal();
    } catch (error) {
        console.error("Error submitting form:", error);
        alert(
            "Có lỗi xảy ra khi lưu dòng sản phẩm. Vui lòng kiểm tra và thử lại."
        );
    } finally {
        loading.value = false;
    }
};

// Close modal
const closeModal = () => {
    showModal.value = false;
    formData.value = {
        name: "",
        brandId: null,
        description: "",
        imageFile: null,
        isActive: true,
    };
    imagePreview.value = "";
    if (fileInput.value) {
        fileInput.value.value = "";
    }
};

// Confirm delete
const confirmDelete = async (productLine) => {
    productLineToDelete.value = productLine;
    hasProducts.value = getProductLineProductsCount(productLine.id) > 0;
    showDeleteModal.value = true;
};

// Cancel delete
const cancelDelete = () => {
    showDeleteModal.value = false;
    productLineToDelete.value = null;
    hasProducts.value = false;
};

// Delete product line
const deleteProductLine = async () => {
    if (!productLineToDelete.value) return;

    loading.value = true;
    try {
        await deleteProductLineAPI(productLineToDelete.value.id);

        // Làm mới danh sách sau khi xóa
        await fetchProductLines();

        // Đóng hộp thoại xác nhận
        showDeleteModal.value = false;
        productLineToDelete.value = null;
    } catch (error) {
        console.error("Error deleting product line:", error);
        // Handle error (show notification, etc.)
    } finally {
        loading.value = false;
    }
};

// Watch for statusFilter changes and reload data
watch(statusFilter, async () => {
    await fetchProductLines();
});

// Load data when component mounts
onMounted(async () => {
    await fetchProductLines();
});
</script>
<template>
    <div class="product-line-management">
        <div class="section-header">
            <div class="left-section">
                <h2><i class="fas fa-mobile-alt"></i> Quản lý Dòng sản phẩm</h2>
                <p>Quản lý các dòng sản phẩm theo thương hiệu</p>
            </div>

            <div class="right-section">
                <div class="search-box">
                    <i class="fas fa-search"></i>
                    <input
                        type="text"
                        v-model="searchQuery"
                        placeholder="Tìm kiếm dòng sản phẩm..."
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

                <button @click="openAddProductLineModal" class="add-button">
                    <i class="fas fa-plus"></i> Thêm dòng sản phẩm
                </button>
            </div>
        </div>

        <!-- Status Cards -->
        <div class="status-cards">
            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-mobile-alt"></i>
                </div>
                <div class="status-content">
                    <h3>{{ productLines.length }}</h3>
                    <p>Tổng số dòng sản phẩm</p>
                </div>
            </div>

            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-trademark"></i>
                </div>
                <div class="status-content">
                    <h3>{{ uniqueBrandCount }}</h3>
                    <p>Thương hiệu</p>
                </div>
            </div>

            <div class="status-card">
                <div class="icon-wrapper">
                    <i class="fas fa-check-circle"></i>
                </div>
                <div class="status-content">
                    <h3>{{ activeCount }}</h3>
                    <p>Đang kích hoạt</p>
                </div>
            </div>
        </div>

        <!-- Product Lines hierarchical view -->
        <div class="data-card">
            <div v-if="loading" class="loading-state">
                <div class="spinner"></div>
                <p>Đang tải dữ liệu...</p>
            </div>
            <div
                v-else-if="filteredProductLines.length === 0"
                class="empty-state"
            >
                <i class="fas fa-th-list"></i>
                <p v-if="searchQuery">
                    Không tìm thấy dòng sản phẩm phù hợp với tìm kiếm
                </p>
                <p v-else>Không có dòng sản phẩm nào</p>
                <button @click="openAddProductLineModal" class="action-button">
                    <i class="fas fa-plus"></i> Thêm dòng sản phẩm mới
                </button>
            </div>
            <table v-else class="data-table">
                <thead>
                    <tr>
                        <th>Dòng sản phẩm</th>
                        <th>Ảnh</th>
                        <th>Mô tả</th>
                        <th>Thương hiệu</th>
                        <th>Trạng thái</th>
                        <th>Số SP</th>
                        <th>Hành động</th>
                    </tr>
                </thead>
                <tbody>
                    <tr
                        v-for="productLine in filteredProductLines"
                        :key="productLine.id"
                        class="data-row"
                    >
                        <td class="name-cell">{{ productLine.name }}</td>
                        <td class="logo-cell">
                            <div class="logo-wrapper">
                                <img
                                    v-if="productLine.image"
                                    :src="formatImageUrl(productLine.image)"
                                    :alt="productLine.name"
                                />
                                <div v-else class="no-logo">
                                    <i class="fas fa-mobile-alt"></i>
                                </div>
                            </div>
                        </td>
                        <td class="description-cell">
                            {{ productLine.description || "Không có mô tả" }}
                        </td>
                        <td class="brand-cell">
                            {{
                                productLine.brandName ||
                                brands.find((b) => b.id === productLine.brandId)
                                    ?.name ||
                                "N/A"
                            }}
                        </td>
                        <td class="status-cell">
                            <span
                                :class="[
                                    'status-badge',
                                    productLine.isActive
                                        ? 'active'
                                        : 'inactive',
                                ]"
                            >
                                {{
                                    productLine.isActive
                                        ? "Đang kích hoạt"
                                        : "Không kích hoạt"
                                }}
                            </span>
                        </td>
                        <td class="count-cell">
                            {{ getProductLineProductsCount(productLine.id) }}
                        </td>
                        <td class="actions-cell">
                            <button
                                @click="editProductLine(productLine)"
                                class="edit-button"
                                title="Chỉnh sửa"
                            >
                                <i class="fas fa-edit"></i>
                            </button>
                            <button
                                @click="confirmDelete(productLine)"
                                class="delete-button"
                                title="Xóa"
                            >
                                <i class="fas fa-trash-alt"></i>
                            </button>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <!-- Add/Edit Product Line Modal -->
        <div v-if="showModal" class="modal-backdrop">
            <div class="modal-container">
                <div class="modal-header">
                    <h3>
                        {{
                            isEditing
                                ? "Chỉnh sửa dòng sản phẩm"
                                : "Thêm dòng sản phẩm mới"
                        }}
                    </h3>
                    <button @click="closeModal" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <form @submit.prevent="submitForm" class="modal-form">
                    <div class="form-group">
                        <label
                            >Thương hiệu <span class="required">*</span></label
                        >
                        <div class="select-wrapper">
                            <select
                                v-model="formData.brandId"
                                class="form-select"
                                required
                            >
                                <option :value="null" disabled>
                                    Chọn thương hiệu
                                </option>
                                <option
                                    v-for="brand in brands"
                                    :key="brand.id"
                                    :value="brand.id"
                                >
                                    {{ brand.name }}
                                </option>
                            </select>
                            <i class="fas fa-chevron-down select-icon"></i>
                        </div>
                    </div>

                    <div class="form-group">
                        <label
                            >Tên dòng sản phẩm
                            <span class="required">*</span></label
                        >
                        <input
                            v-model="formData.name"
                            type="text"
                            placeholder="Nhập tên dòng sản phẩm"
                            required
                        />
                    </div>

                    <div class="form-group">
                        <label>Ảnh minh họa</label>
                        <div
                            class="upload-area"
                            @click="$refs.fileInput.click()"
                        >
                            <img
                                v-if="imagePreview"
                                :src="
                                    imagePreview.startsWith('data:') ||
                                    imagePreview.startsWith('blob:')
                                        ? imagePreview
                                        : formatImageUrl(imagePreview)
                                "
                                alt="Preview"
                                class="image-preview"
                            />
                            <div v-else class="upload-placeholder">
                                <i class="fas fa-cloud-upload-alt"></i>
                                <span>Nhấp để tải lên ảnh (SVG, PNG, JPG)</span>
                            </div>
                        </div>
                        <input
                            ref="fileInput"
                            type="file"
                            @change="handleFileChange"
                            accept="image/*"
                            class="hidden-input"
                        />
                        <div v-if="imagePreview" class="preview-actions">
                            <button
                                type="button"
                                @click="clearImage"
                                class="remove-button"
                            >
                                <i class="fas fa-trash-alt"></i> Xóa ảnh
                            </button>
                        </div>
                    </div>

                    <div class="form-group">
                        <label>Trạng thái dòng sản phẩm</label>
                        <div class="toggle-switch-wrapper">
                            <div class="toggle-switch-container">
                                <label class="toggle-switch">
                                    <input
                                        type="checkbox"
                                        v-model="formData.isActive"
                                    />
                                    <span class="toggle-slider"></span>
                                </label>
                                <span class="toggle-status">
                                    {{
                                        formData.isActive
                                            ? "Đang kích hoạt"
                                            : "Không kích hoạt"
                                    }}
                                </span>
                            </div>
                            <div class="toggle-description">
                                {{
                                    formData.isActive
                                        ? "Dòng sản phẩm sẽ hiển thị cho khách hàng trên trang web."
                                        : "Dòng sản phẩm sẽ bị ẩn khỏi trang web."
                                }}
                            </div>
                        </div>
                    </div>

                    <div class="form-group">
                        <label>Mô tả</label>
                        <textarea
                            v-model="formData.description"
                            placeholder="Nhập mô tả dòng sản phẩm"
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

        <!-- Delete Confirmation Modal -->
        <div v-if="showDeleteModal" class="modal-backdrop">
            <div class="modal-container warning-modal">
                <div class="modal-header warning">
                    <h3>Xác nhận xóa dòng sản phẩm</h3>
                    <button @click="cancelDelete" class="close-button">
                        <i class="fas fa-times"></i>
                    </button>
                </div>

                <div class="modal-body text-center">
                    <div class="warning-icon">
                        <i class="fas fa-exclamation-triangle"></i>
                    </div>
                    <p class="warning-message">
                        Bạn có chắc chắn muốn xóa dòng sản phẩm
                        <strong>"{{ productLineToDelete?.name }}"</strong>?
                    </p>
                    <div v-if="hasProducts" class="warning-details">
                        <i class="fas fa-info-circle"></i>
                        <span
                            >Dòng sản phẩm này đang có
                            {{
                                getProductLineProductsCount(
                                    productLineToDelete?.id
                                )
                            }}
                            sản phẩm. Việc xóa có thể ảnh hưởng đến dữ liệu liên
                            quan.</span
                        >
                    </div>
                </div>

                <div class="modal-actions">
                    <button @click="cancelDelete" class="cancel-button">
                        <i class="fas fa-arrow-left"></i> Quay lại
                    </button>
                    <button
                        @click="deleteProductLine"
                        class="delete-confirm-button"
                        :disabled="loading"
                    >
                        <span v-if="loading" class="spinner small"></span>
                        <i v-else class="fas fa-trash-alt"></i> Xóa
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.product-line-management {
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

.no-logo {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #ccc;
}

.brand-name {
    margin: 0;
    font-size: 1.1rem;
    color: #333;
}

/* Data Table Styles */
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
    width: 15%;
} /* DÒNG SẢN PHẨM */
.data-table th:nth-child(2) {
    width: 10%;
} /* ẢNH */
.data-table th:nth-child(3) {
    width: 35%;
} /* MÔ TẢ */
.data-table th:nth-child(4) {
    width: 15%;
} /* THƯƠNG HIỆU */
.data-table th:nth-child(5) {
    width: 12%;
} /* TRẠNG THÁI */
.data-table th:nth-child(6) {
    width: 5%;
} /* SỐ SP */
.data-table th:nth-child(7) {
    width: 8%;
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

.description-cell {
    max-width: 250px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.brand-cell {
    font-weight: 500;
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
.delete-button {
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

.delete-button {
    background-color: #fee2e2;
    color: #ef4444;
}

.delete-button:hover {
    background-color: #ef4444;
    color: white;
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
.form-group textarea,
.form-select {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    font-size: 0.95rem;
    transition: all 0.3s;
}

.form-group input[type="text"]:focus,
.form-group textarea:focus,
.form-select:focus {
    border-color: var(--primary-color);
    box-shadow: 0 0 0 2px rgba(248, 110, 211, 0.1);
    outline: none;
}

.select-wrapper {
    position: relative;
}

.select-icon {
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    color: #666;
    pointer-events: none;
}

.form-select {
    appearance: none;
    padding-right: 2.5rem;
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
}

.image-preview {
    max-width: 100%;
    max-height: 150px;
    margin: 0 auto;
    display: block;
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

.checkbox-label {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
}

.checkbox-label input {
    width: 16px;
    height: 16px;
}

.form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    margin-top: 1.5rem;
}

.cancel-button,
.submit-button,
.delete-confirm-button {
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

.warning-details {
    display: flex;
    align-items: flex-start;
    gap: 0.5rem;
    padding: 1rem;
    background-color: #fee2e2;
    border-radius: 8px;
    color: #ef4444;
    text-align: left;
    font-size: 0.9rem;
}

.modal-actions {
    padding: 1.25rem 1.5rem;
    border-top: 1px solid #eee;
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
}

.delete-confirm-button {
    background-color: #ef4444;
    border: none;
    color: white;
}

.delete-confirm-button:hover {
    background-color: #dc2626;
}

.delete-confirm-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
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

.toggle-switch-wrapper {
    background-color: #f9fafb;
    border: 1px solid #e5e7eb;
    border-radius: 8px;
    padding: 12px 16px;
    margin-top: 8px;
}

.toggle-switch-container {
    display: flex;
    align-items: center;
    margin: 0;
}

.toggle-switch {
    position: relative;
    display: inline-block;
    width: 48px;
    height: 24px;
    margin-right: 12px;
}

.toggle-slider {
    position: absolute;
    cursor: pointer;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: #e2e8f0;
    transition: 0.3s;
    border-radius: 24px;
    box-shadow: inset 0 1px 2px rgba(0, 0, 0, 0.1);
}

.toggle-slider:before {
    position: absolute;
    content: "";
    height: 18px;
    width: 18px;
    left: 3px;
    bottom: 3px;
    background-color: white;
    transition: 0.3s;
    border-radius: 50%;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

input:checked + .toggle-slider {
    background-color: var(--primary-color);
}

input:checked + .toggle-slider:before {
    transform: translateX(24px);
}

.toggle-status {
    font-weight: 600;
    font-size: 0.95rem;
}

input:checked ~ .toggle-status {
    color: var(--primary-color);
}

.toggle-description {
    margin-top: 8px;
    padding-top: 8px;
    border-top: 1px dashed #e5e7eb;
    font-size: 0.85rem;
    color: #6b7280;
    line-height: 1.4;
}

/* Responsive tweaks */
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
